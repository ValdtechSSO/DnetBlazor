#!/usr/bin/env python3
"""Reserve new proxy backends on the VPS under a shared lock. Fail closed."""

import argparse
import datetime
import fcntl
import ipaddress
import json
import os
from pathlib import Path
import re
import subprocess
import tempfile


SUBNET = ipaddress.ip_network('192.168.0.0/24')
REGISTRY = Path('/home/docker/proxy-endpoint-reservations.json')
LOCK = Path('/home/docker/.proxy-endpoint-allocation.lock')


def command(args):
    result = subprocess.run(args, text=True, capture_output=True, timeout=60)
    if result.returncode:
        raise ValueError(f'Cannot complete required query: {args[0]} (exit {result.returncode})')
    return result.stdout


def address(value):
    return str(ipaddress.IPv4Address(str(value).split('/')[0]))


def destination_ips(config):
    result = set()
    from urllib.parse import urlsplit
    for cluster in config.get('ReverseProxy', {}).get('Clusters', {}).values():
        for destination in cluster.get('Destinations', {}).values():
            host = urlsplit(destination['Address']).hostname
            try:
                result.add(address(host))
            except ValueError:
                # DNS destinations are covered by the container and Compose inventory.
                pass
    return result


def published_ports(values):
    result = set()
    for value in values:
        if isinstance(value, dict):
            token = str(value.get('published', ''))
            if not token:
                continue
        else:
            parts = str(value).split('/')[0].rsplit(':', 2)
            if len(parts) < 2:
                continue
            token = parts[-2]
        if not re.fullmatch(r'\d+(?:-\d+)?', token):
            raise ValueError('Unresolved or unsupported published port in Compose')
        lo, hi = (map(int, token.split('-')) if '-' in token else (int(token), int(token)))
        if not 1 <= lo <= hi <= 65535:
            raise ValueError('Invalid port range in Compose')
        result.update(range(lo, hi+1))
    return result


def collect_live(compose_root, network_name):
    """Keep only address/port metadata; never print environment or secret values."""
    used_ips, ports = set(), {22, 80, 443}
    networks = json.loads(command(['docker', 'network', 'inspect', network_name]))
    configs = networks[0]['IPAM'].get('Config') or []
    if not any(c.get('Subnet') == str(SUBNET) for c in configs):
        raise ValueError('Unexpected Docker subnet; refuse allocation')
    for config in configs:
        if config.get('Gateway'):
            used_ips.add(address(config['Gateway']))
        for ip in (config.get('AuxiliaryAddresses') or config.get('AuxAddress') or {}).values():
            used_ips.add(address(ip))
    for endpoint in (networks[0].get('Containers') or {}).values():
        if endpoint.get('IPv4Address'):
            used_ips.add(address(endpoint['IPv4Address']))
    ids = command(['docker', 'ps', '-aq']).split()
    containers = json.loads(command(['docker', 'inspect', *ids])) if ids else []
    compose_files = set(compose_root.glob('*.yml')) | set(compose_root.glob('*.yaml'))
    compose_files |= set(compose_root.glob('*/*.yml')) | set(compose_root.glob('*/*.yaml'))
    proxy_found = False
    for container in containers:
        cfg = container.get('Config') or {}
        settings = container.get('NetworkSettings') or {}
        for net in (settings.get('Networks') or {}).values():
            for ip in [net.get('IPAddress'), (net.get('IPAMConfig') or {}).get('IPv4Address')]:
                if ip:
                    used_ips.add(address(ip))
        for bindings in ((container.get('HostConfig') or {}).get('PortBindings') or {}).values():
            ports.update(int(binding['HostPort']) for binding in bindings or [] if binding['HostPort'])
        labels = cfg.get('Labels') or {}
        compose_files.update(Path(p) for p in labels.get('com.docker.compose.project.config_files', '').split(',') if p)
        if 'dnetwebproxy' in cfg.get('Image', '').lower() and container.get('State', {}).get('Running'):
            proxy_found = True
            folder = cfg.get('WorkingDir') or '/app'
            for filename in ['appsettings.json', 'appsettings.Production.json']:
                config = json.loads(command(['docker', 'exec', container['Id'], 'cat', folder+'/'+filename]))
                used_ips.update(destination_ips(config))
            for env in cfg.get('Env') or []:
                key, _, value = env.partition('=')
                if key.lower().startswith('reverseproxy__clusters__') and key.lower().endswith('__address'):
                    used_ips.update(destination_ips({'ReverseProxy': {'Clusters': {'env': {
                        'Destinations': {'env': {'Address': value}}}}}}))
    if not proxy_found:
        raise ValueError('Cannot inspect the running production proxy')
    for interface in json.loads(command(['ip', '-j', 'addr', 'show'])):
        used_ips.update(address(a['local']) for a in interface.get('addr_info', []) if a.get('family') == 'inet')
    for line in command(['ss', '-H', '-lntu']).splitlines():
        parts = line.split()
        if len(parts) < 6:
            raise ValueError('Cannot parse required host socket inventory')
        local, _, port = parts[4].rpartition(':')
        if not port.isdigit():
            raise ValueError('Non-numeric port in socket inventory')
        ports.add(int(port))
        try:
            used_ips.add(address(local.strip('[]').split('%')[0]))
        except ValueError:
            pass
    try:
        import yaml
    except ImportError:
        yaml = None
    for path in sorted(compose_files):
        if not path.is_file():
            raise ValueError(f'Referenced Compose unavailable: {path}')
        text = path.read_text()
        significant = [s.strip() for s in text.splitlines() if s.strip() and not s.lstrip().startswith('#')]
        if significant and significant[0].split()[0].upper() in ('FROM', 'ARG') and any(s.startswith('FROM ') for s in significant):
            continue
        try:
            parsed = yaml.safe_load(text) if yaml else json.loads(command([
                'docker', 'compose', '-f', str(path), 'config', '--format', 'json']))
        except Exception as exc:
            raise ValueError(f'Cannot inventory Compose: {path}') from exc
        if not isinstance(parsed, dict) or 'services' not in parsed:
            continue
        for service in parsed['services'].values():
            if not isinstance(service, dict):
                raise ValueError(f'Invalid Compose service: {path}')
            nets = service.get('networks') or {}
            for net in nets.values() if isinstance(nets, dict) else []:
                if isinstance(net, dict) and net.get('ipv4_address'):
                    used_ips.add(address(net['ipv4_address']))
            ports.update(published_ports(service.get('ports') or []))
        for net in (parsed.get('networks') or {}).values():
            for config in (net or {}).get('ipam', {}).get('config', []):
                if config.get('gateway'):
                    used_ips.add(address(config['gateway']))
                used_ips.update(address(ip) for ip in (config.get('aux_addresses') or {}).values())
    return used_ips, ports


def plan(application, service, environments, internal_port, publish, used_ips, used_ports, allocations):
    """Reserve a complete n/n+100 pair, including an undeployed counterpart."""
    requested_environments = set(environments)
    environments = ['prod', 'test']
    occupied = set(used_ips)
    ports = set(used_ports)
    for key, allocation in allocations.items():
        ip = address(allocation['ip'])
        if ip in occupied and any(other['ip'] == ip for other_key, other in allocations.items() if other_key != key):
            raise ValueError('Duplicate IP in central allocation registry')
        occupied.add(ip)
        if allocation.get('host_port'):
            ports.add(int(allocation['host_port']))
    keys = [f'{application}/{service}/{env}' for env in environments]
    if any(key in allocations for key in keys):
        raise ValueError('Service already reserved; inspect its existing allocation instead of requesting another')
    candidates = [(n, n+100) for n in range(3, 100)]
    selected = next((numbers for numbers in candidates if all(f'192.168.0.{n}' not in occupied for n in numbers)), None)
    if selected is None:
        raise ValueError('No complete prod/test pair available inside the environment ranges')
    result = {}
    for key, env, number in zip(keys, environments, selected):
        deploy_requested = env in requested_environments
        publish_requested = publish and deploy_requested
        host_port = next((p for p in range(5400, 6000) if p not in ports), None) if publish_requested else None
        if publish_requested and host_port is None:
            raise ValueError('No unreserved published port in 5400-5999')
        if host_port:
            ports.add(host_port)
        name = f'{application}-{service}-{env}'
        result[key] = {'application': application, 'service': service, 'environment': env,
                       'ip': f'192.168.0.{number}', 'internal_port': internal_port if deploy_requested else None,
                       'host_port': host_port, 'container_name': name, 'cluster_id': name,
                       'network': 'docker_datalnet', 'state': 'candidate', 'deploy_requested': deploy_requested,
                       'created_at_utc': datetime.datetime.now(datetime.timezone.utc).isoformat()}
    return result


def atomic_json(path, value):
    fd, name = tempfile.mkstemp(dir=path.parent, prefix='.'+path.name+'.')
    try:
        with os.fdopen(fd, 'w') as file:
            json.dump(value, file, indent=2, ensure_ascii=False)
            file.write('\n')
            file.flush()
            os.fsync(file.fileno())
        os.replace(name, path)
        directory = os.open(path.parent, os.O_RDONLY)
        try:
            os.fsync(directory)
        finally:
            os.close(directory)
    finally:
        if os.path.exists(name):
            os.unlink(name)


def reserve(registry, lock, collect, baseline, application, service, environments, internal_port, publish=False, preview=False):
    with lock.open('a') as file:
        fcntl.flock(file, fcntl.LOCK_EX)
        state = json.loads(registry.read_text()) if registry.exists() else {'schema_version': 1, 'allocations': {}}
        if state.get('schema_version') != 1 or not isinstance(state.get('allocations'), dict):
            raise ValueError('Unsupported or invalid central registry; refuse allocation')
        used_ips, used_ports = collect()
        used_ips.update(baseline['blocked_ips'])
        used_ports.update(baseline['blocked_host_ports'])
        result = plan(application, service, environments, internal_port, publish,
                      used_ips, used_ports, state['allocations'])
        if not preview:
            for allocation in result.values():
                allocation['state'] = 'reserved'
            state['allocations'].update(result)
            atomic_json(registry, state)
        return {'status': 'preview-not-reserved' if preview else 'reserved', 'allocations': result}


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--application', required=True)
    parser.add_argument('--service', required=True)
    parser.add_argument('--environments', nargs='+', choices=['prod', 'test'], required=True,
                        help='Environments to deploy; both IPs are always reserved as an n/n+100 pair')
    parser.add_argument('--port', type=int, required=True)
    parser.add_argument('--publish', action='store_true', help='Also reserve distinct host TCP ports')
    parser.add_argument('--preview', action='store_true', help='Inspect live state without persisting reservations')
    parser.add_argument('--baseline', type=Path, default=Path(__file__).resolve().parents[1]/'network-reservations.baseline.json')
    args = parser.parse_args()
    if os.uname().nodename != 'srv438597' or os.geteuid() != 0:
        parser.error('Run as root on VPS srv438597; local copies cannot allocate centrally')
    for name in [args.application, args.service]:
        if not re.fullmatch(r'[a-z][a-z0-9-]{0,47}', name):
            parser.error('Application and service must be lowercase slugs, maximum 48 characters')
    if not 1 <= args.port <= 65535:
        parser.error('Internal port must be in 1-65535')
    try:
        baseline = json.loads(args.baseline.read_text())
        if baseline.get('schema_version') != 1 or baseline.get('subnet') != str(SUBNET):
            raise ValueError('Invalid conservative baseline')
        result = reserve(REGISTRY, LOCK, lambda: collect_live(Path('/home/docker'), 'docker_datalnet'),
                         baseline, args.application, args.service, args.environments, args.port,
                         args.publish, args.preview)
    except (ValueError, OSError, subprocess.TimeoutExpired) as exc:
        parser.exit(1, f'Allocation refused: {exc}\n')
    print(json.dumps(result, indent=2))


if __name__ == '__main__':
    main()
