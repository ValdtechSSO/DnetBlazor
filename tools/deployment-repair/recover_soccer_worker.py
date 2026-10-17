#!/usr/bin/env python3
"""Apply the reserved soccer worker IP and start the released DnetBlazor container.

This is an explicitly authorized maintenance operation using a central reservation.
It never pulls images, restarts dependencies, or removes other endpoints.
"""

import argparse
import copy
import datetime
import fcntl
import hashlib
import http.client
import ipaddress
import json
import os
from pathlib import Path
import re
import shlex
import socket
import ssl
import stat
import subprocess
import tempfile
import time
from urllib.parse import urlsplit



ROOT = Path('/home/docker')
WORKER_FILE = ROOT / 'll-soccer-prod-docker-compose.yaml'
WORKER_SERVICE = 'soccerintegrationworkerprod'
WORKER_NAME = 'soccer-integration-worker-prod'
WORKER_IP = None
RESERVATION_KEY = 'laliguilla/soccer-integration-worker/prod'
PROXY_NAME = 'docker-webproxy-1'
APP_NAME = 'docker-blazorclient-1'
RELEASE_COMMIT = 'bf96fea6171ebbcb7a70f90ea367a0435f0d9fe9'
RELEASE_IMAGE = 'ghcr.io/valdtechsso/dnetblazor@sha256:e23cbcff4c5ec99b1e2ebc0b23a1cfe21394ef86f31792902068b2ac0d963ff0'
NETWORK = 'docker_datalnet'
REGISTRY = ROOT / 'proxy-endpoint-reservations.json'
LOCK = ROOT / '.proxy-endpoint-allocation.lock'


def require(condition, message):
    if not condition:
        raise ValueError(message)


def run(*args):
    result = subprocess.run(args, capture_output=True, text=True, timeout=90)
    require(result.returncode == 0, f'Required command failed: {args[0]} (exit {result.returncode}); output withheld')
    return result.stdout


def query(*args):
    return json.loads(run(*args))


def address(value):
    return str(ipaddress.ip_address(str(value).split('/')[0]))


def compose(path):
    return query('docker', 'compose', '-p', 'docker', '-f', str(path), 'config', '--format', 'json')


def check_targets(config):
    for cluster in config.get('ReverseProxy', {}).get('Clusters', {}).values():
        for destination in cluster.get('Destinations', {}).values():
            require(urlsplit(destination['Address']).hostname != WORKER_IP,
                    'Reserved worker IP is now referenced by a deployed YARP destination')


def check_available(baseline, worker, proxy, containers, yaml, scratch):
    expected_refs = {'Docker: ' + WORKER_NAME}
    require(baseline.get('schema_version') == 1 and baseline.get('network') == NETWORK,
            'Invalid historical baseline')
    require(set(baseline['blocked_ips'].get('192.168.0.9', [])) == expected_refs and WORKER_IP not in baseline['blocked_ips'],
            'Baseline does not confirm historical ownership or blocks the new address')
    registry_bytes = REGISTRY.read_bytes()
    registry = json.loads(registry_bytes)
    require(registry.get('schema_version') == 1 and isinstance(registry.get('allocations'), dict),
            'Invalid central reservation registry')
    for entry in registry['allocations'].values():
        require(isinstance(entry, dict) and 'ip' in entry, 'Incomplete central reservation')
        if address(entry['ip']) == WORKER_IP:
            require(entry.get('application') in ('laliguilla', 'soccer')
                    and entry.get('container_name') == WORKER_NAME
                    and entry.get('environment') == 'prod', 'Reserved worker IP reserved by another owner')

    network_ids = run('docker', 'network', 'ls', '-q', '--no-trunc').split()
    networks = query('docker', 'network', 'inspect', *network_ids)
    require(len(networks) == len(network_ids), 'Incomplete Docker network inventory')
    require(any(network['Name'] == NETWORK for network in networks), 'Missing proxy network')
    for network in networks:
        for config in network['IPAM'].get('Config') or []:
            candidates = [config.get('Gateway')]
            candidates.extend((config.get('AuxiliaryAddresses') or config.get('AuxAddress') or {}).values())
            require(not any(value and address(value) == WORKER_IP for value in candidates),
                    'Reserved worker IP reserved by network IPAM')
        for endpoint in (network.get('Containers') or {}).values():
            require(not endpoint.get('IPv4Address') or address(endpoint['IPv4Address']) != WORKER_IP,
                    'Reserved worker IP already has a Docker endpoint')

    files = set(ROOT.glob('*.yaml')) | set(ROOT.glob('*.yml'))
    files |= set(ROOT.glob('*/*.yaml')) | set(ROOT.glob('*/*.yml'))
    for container in containers:
        labels = container['Config'].get('Labels') or {}
        files.update(Path(path) for path in labels.get('com.docker.compose.project.config_files', '').split(',') if path)
        for attachment in (container['NetworkSettings'].get('Networks') or {}).values():
            candidates = [attachment.get('IPAddress'), (attachment.get('IPAMConfig') or {}).get('IPv4Address')]
            require(not any(value and address(value) == WORKER_IP for value in candidates),
                    'Reserved worker IP already occupied or requested by a container')
    for interface in query('ip', '-j', 'addr', 'show'):
        for item in interface.get('addr_info', []):
            require(item.get('local') != WORKER_IP, 'Reserved worker IP assigned to host interface')
    for line in run('ss', '-H', '-lntu').splitlines():
        fields = line.split()
        require(len(fields) >= 6, 'Incomplete host socket inventory')
        local = fields[4].rpartition(':')[0].strip('[]').split('%')[0]
        require(local != WORKER_IP, 'Reserved worker IP bound by a host socket')

    compose_count = 0
    for path in sorted(files):
        require(path.is_file(), 'A required Compose file is missing')
        raw = path.read_text()
        significant = [line.strip() for line in raw.splitlines() if line.strip() and not line.lstrip().startswith('#')]
        if significant and significant[0].split()[0].upper() in ('FROM', 'ARG'):
            continue
        parsed = yaml.safe_load(raw)
        if not isinstance(parsed, dict) or 'services' not in parsed:
            continue
        require(not parsed.get('include'), 'Compose include requires an expanded inventory')
        compose_count += 1
        for name, service in parsed['services'].items():
            require(isinstance(service, dict) and not service.get('extends'), 'Unsupported Compose service or extends')
            attachments = service.get('networks') or {}
            for attachment in attachments.values() if isinstance(attachments, dict) else []:
                if isinstance(attachment, dict) and attachment.get('ipv4_address'):
                    candidate = address(attachment['ipv4_address'])
                    require(candidate != WORKER_IP or (path.resolve() == WORKER_FILE.resolve() and name == WORKER_SERVICE),
                            'Reserved worker IP declared for another Compose owner')
        for network in (parsed.get('networks') or {}).values():
            for config in ((network or {}).get('ipam') or {}).get('config', []):
                candidates = [config.get('gateway')]
                candidates.extend((config.get('aux_addresses') or {}).values())
                require(not any(value and address(value) == WORKER_IP for value in candidates),
                        'Reserved worker IP reserved in Compose IPAM')
    require(WORKER_FILE in files and compose_count > 0, 'Incomplete Compose inventory')

    folder = proxy['Config'].get('WorkingDir') or '/app'
    for filename in ('appsettings.json', 'appsettings.Production.json'):
        target = scratch / filename
        run('docker', 'cp', proxy['Id'] + ':' + folder + '/' + filename, str(target))
        check_targets(json.loads(target.read_text()))
    for variable in proxy['Config'].get('Env') or []:
        key, _, value = variable.partition('=')
        if key.lower().startswith('reverseproxy__clusters__') and key.lower().endswith('__address'):
            require(urlsplit(value).hostname != WORKER_IP, 'Worker IP referenced by a YARP environment override')
    print(json.dumps({'revalidated_reserved_ip': WORKER_IP, 'containers_checked': len(containers),
                      'networks_checked': len(networks), 'compose_files_checked': compose_count,
                      'registry_sha256': hashlib.sha256(registry_bytes).hexdigest()}), flush=True)
    return registry_bytes


def replacement(raw, yaml):
    original = yaml.safe_load(raw)
    worker = original['services'][WORKER_SERVICE]
    require(worker.get('networks') == ['datalnet'], 'Expected the diagnosed dynamic worker network; refuse unrelated edits')
    match = re.search(r'(?ms)^  soccerintegrationworkerprod:[ \t]*\n.*?(?=^  [^ \n]|^[^ \n]|\Z)', raw)
    require(match is not None, 'Cannot locate worker Compose block unambiguously')
    old = match.group()
    new, count = re.subn(r'(?m)^    networks:[ \t]*\n      - datalnet[ \t]*\n',
                        f'    networks:\n      datalnet:\n        ipv4_address: "{WORKER_IP}"\n', old)
    require(count == 1, 'Cannot patch only the worker network declaration')
    result = raw[:match.start()] + new + raw[match.end():]
    expected = copy.deepcopy(original)
    expected['services'][WORKER_SERVICE]['networks'] = {'datalnet': {'ipv4_address': WORKER_IP}}
    require(yaml.safe_load(result) == expected, 'Compose patch changed unexpected settings')
    return result


def atomic_replace(path, contents, metadata):
    descriptor, temporary = tempfile.mkstemp(dir=path.parent, prefix='.proxy-worker-recovery-')
    try:
        os.fchmod(descriptor, stat.S_IMODE(metadata.st_mode))
        os.fchown(descriptor, metadata.st_uid, metadata.st_gid)
        with os.fdopen(descriptor, 'wb') as stream:
            stream.write(contents)
            stream.flush()
            os.fsync(stream.fileno())
        os.replace(temporary, path)
        directory = os.open(path.parent, os.O_RDONLY)
        try:
            os.fsync(directory)
        finally:
            os.close(directory)
    finally:
        if os.path.exists(temporary):
            os.unlink(temporary)


def wait_running(name, expected_ip, image_id):
    for attempt in range(20):
        container = query('docker', 'inspect', name)[0]
        attachment = container['NetworkSettings']['Networks'].get(NETWORK) or {}
        if container['State']['Running'] and attachment.get('IPAddress') == expected_ip:
            require(container['Image'] == image_id, 'Recovery changed the deployed image')
            if name == WORKER_NAME:
                require((attachment.get('IPAMConfig') or {}).get('IPv4Address') == WORKER_IP,
                        'Restored worker IP is not static')
            return container
        time.sleep(1)
    raise ValueError('Container did not become active on its expected IP')


def verify_proxy():
    context = ssl.create_default_context()
    for attempt in range(15):
        try:
            with socket.create_connection(('127.0.0.1', 443), timeout=3) as connection:
                with context.wrap_socket(connection, server_hostname='console.valdtech.com'):
                    pass
            connection = http.client.HTTPConnection('127.0.0.1', 80, timeout=3)
            try:
                connection.request('GET', '/', headers={'Host': 'console.valdtech.com'})
                response = connection.getresponse()
                require(response.status in (307, 308), 'Proxy HTTP redirect failed')
                require(response.getheader('Location', '').startswith('https://console.valdtech.com/'),
                        'Proxy redirect location differs from expected host')
            finally:
                connection.close()
            connection = http.client.HTTPConnection('127.0.0.1', 5001, timeout=5)
            try:
                connection.request('GET', '/Dialog')
                response = connection.getresponse()
                require(response.status == 200 and b'id="app"' in response.read(), 'DnetBlazor HTTP check failed')
            finally:
                connection.close()
            with socket.create_connection(('127.0.0.1', 443), timeout=5) as tcp:
                with context.wrap_socket(tcp, server_hostname='www.datalnet.com') as tls:
                    tls.sendall(b'GET /Dialog HTTP/1.1\r\nHost: www.datalnet.com\r\nConnection: close\r\n\r\n')
                    response = http.client.HTTPResponse(tls)
                    response.begin()
                    require(response.status == 200 and b'id="app"' in response.read(), 'DnetBlazor HTTPS route failed')
            return
        except OSError:
            if attempt == 14:
                raise
            time.sleep(1)


def recover(baseline_path, apply):
    global WORKER_IP
    try:
        import yaml
    except ImportError:
        raise ValueError('Python PyYAML is required for the complete Compose inventory') from None
    require(os.geteuid() == 0 and os.uname().nodename == 'srv438597', 'Recovery only allowed as root on VPS srv438597')
    with LOCK.open('a') as lock, tempfile.TemporaryDirectory(prefix='proxy-worker-revalidation-') as directory:
        fcntl.flock(lock, fcntl.LOCK_EX)
        scratch = Path(directory)
        reservation = json.loads(REGISTRY.read_text())['allocations'][RESERVATION_KEY]
        require(reservation.get('application') == 'laliguilla' and reservation.get('container_name') == WORKER_NAME
                and reservation.get('environment') == 'prod' and reservation.get('network') == NETWORK, 'Unexpected reservation owner')
        WORKER_IP = address(reservation['ip'])
        require(ipaddress.ip_address(WORKER_IP) in ipaddress.ip_network('192.168.0.0/24')
                and 200 < int(WORKER_IP.rsplit('.', 1)[1]) < 255, 'Expected the user-authorized high address')
        ids = run('docker', 'ps', '-aq', '--no-trunc').split()
        containers = query('docker', 'inspect', *ids)
        require(len(containers) == len(ids) and {c['Id'] for c in containers} == set(ids), 'Incomplete container inventory')
        by_name = {c['Name'].lstrip('/'): c for c in containers}
        worker, proxy, app = by_name[WORKER_NAME], by_name[PROXY_NAME], by_name[APP_NAME]
        labels = worker['Config']['Labels']
        require(labels.get('com.docker.compose.project') == 'docker'
                and labels.get('com.docker.compose.service') == WORKER_SERVICE
                and labels.get('com.docker.compose.project.config_files') == str(WORKER_FILE), 'Unexpected worker ownership')
        require(worker['State']['Running'] and proxy['State']['Running'] and not app['State']['Running'], 'Expected active worker/proxy and inactive DnetBlazor container')
        require(app['Config']['Image'] == RELEASE_IMAGE, 'DnetBlazor image differs from published release')
        require(app['Config']['Labels'].get('org.opencontainers.image.revision') == RELEASE_COMMIT, 'DnetBlazor revision differs from validated release')
        require(worker['NetworkSettings']['Networks'][NETWORK]['IPAddress'] == '192.168.0.3', 'Worker no longer occupies DnetBlazor IP')
        require(not worker['NetworkSettings']['Networks'][NETWORK].get('IPAMConfig'), 'Worker no longer has diagnosed dynamic network')
        require(proxy['Config']['Labels'].get('com.docker.compose.project') == 'docker'
                and proxy['Config']['Labels'].get('com.docker.compose.service') == 'webproxy', 'Unexpected proxy ownership')
        original = WORKER_FILE.read_bytes()
        metadata = WORKER_FILE.stat()
        patched = replacement(original.decode(), yaml).encode()
        before = compose(WORKER_FILE)
        require(before['services'][WORKER_SERVICE]['image'] == worker['Config']['Image'], 'Worker Compose image drift detected')
        image = query('docker', 'image', 'inspect', worker['Config']['Image'])[0]
        require(image['Id'] == worker['Image'], 'Worker image tag no longer identifies the deployed image')
        config_hash = run('docker', 'compose', '-p', 'docker', '-f', str(WORKER_FILE), 'config', '--hash', WORKER_SERVICE).split()
        require(len(config_hash) == 2 and config_hash[0] == WORKER_SERVICE
                and config_hash[1] == labels.get('com.docker.compose.config-hash'), 'Worker Compose has drifted from the deployed settings')
        # Validate the exact proposed file with the same working directory before replacing anything.
        proposed = scratch / WORKER_FILE.name
        proposed.write_bytes(patched)
        after = query('docker', 'compose', '-p', 'docker', '--project-directory', str(ROOT),
                      '-f', str(proposed), 'config', '--format', 'json')
        expected = copy.deepcopy(before)
        expected['services'][WORKER_SERVICE]['networks'] = {'datalnet': {'ipv4_address': WORKER_IP}}
        require(after == expected, 'Resolved Compose changes more than the worker IP')
        registry_bytes = check_available(json.loads(Path(baseline_path).read_text()), worker, proxy, containers, yaml, scratch)
        require(not run('ss', '-H', '-lnt', '( sport = :5001 )').strip(),
                'A host process is listening on DnetBlazor TCP 5001')
        if not apply:
            print('Recovery preview passed; no services or Compose files changed.', flush=True)
            return
        require(WORKER_FILE.read_bytes() == original and REGISTRY.read_bytes() == registry_bytes,
                'Compose or registry changed during revalidation')
        stamp = datetime.datetime.now(datetime.timezone.utc).strftime('%Y%m%dT%H%M%SZ')
        backup = ROOT / '.proxy-recovery-backups' / (stamp + '-' + str(os.getpid()))
        backup.mkdir(parents=True, mode=0o700)
        os.chmod(backup.parent, 0o700)
        os.chmod(backup, 0o700)
        backup_file = backup / 'worker.compose.original'
        with backup_file.open('xb') as stream:
            os.chmod(backup_file, 0o600)
            stream.write(original)
            stream.flush()
            os.fsync(stream.fileno())
        backup_directory = os.open(backup, os.O_RDONLY)
        try:
            os.fsync(backup_directory)
        finally:
            os.close(backup_directory)
        print('Restoring worker network; original Compose backup: ' + str(backup_file), flush=True)
        rollback = backup / 'rollback.sh'
        rollback.write_text('#!/bin/sh\nset -eu\n'
                            'exec 9>>/home/docker/.proxy-endpoint-allocation.lock\nflock -x 9\n'
                            f'docker stop {shlex.quote(APP_NAME)} || true\n'
                            f'cp -- {shlex.quote(str(backup_file))} {shlex.quote(str(WORKER_FILE))}\n'
                            f'docker compose -p docker -f {shlex.quote(str(WORKER_FILE))} up -d --no-deps --no-build --pull never --force-recreate {shlex.quote(WORKER_SERVICE)}\n')
        rollback.chmod(0o700)
        atomic_replace(WORKER_FILE, patched, metadata)
        run('docker', 'compose', '-p', 'docker', '-f', str(WORKER_FILE), 'up', '-d', '--no-deps',
            '--no-build', '--pull', 'never', '--force-recreate', WORKER_SERVICE)
        restored_worker = wait_running(WORKER_NAME, WORKER_IP, worker['Image'])
        require(restored_worker['Config']['Env'] == worker['Config']['Env'], 'Worker environment changed during recovery')
        require(restored_worker['HostConfig']['PortBindings'] == worker['HostConfig']['PortBindings'],
                'Worker port publications changed during recovery')
        time.sleep(5)
        stabilized_worker = wait_running(WORKER_NAME, WORKER_IP, worker['Image'])
        require(stabilized_worker['RestartCount'] == restored_worker['RestartCount'], 'Worker restarted during recovery verification')
        require(stabilized_worker['Config']['Env'] == worker['Config']['Env'], 'Worker environment changed')
        current_proxy = query('docker', 'inspect', PROXY_NAME)[0]
        require(current_proxy['Id'] == proxy['Id'] and current_proxy['State']['Running'], 'Proxy changed during recovery')
        print('Worker active at its reserved IP; starting the validated DnetBlazor image.', flush=True)
        run('docker', 'start', app['Id'])
        restored_app = wait_running(APP_NAME, '192.168.0.3', app['Image'])
        require(restored_app['Id'] == app['Id'], 'Unexpected DnetBlazor recreation')
        verify_proxy()
        require(REGISTRY.read_bytes() == registry_bytes, 'Central reservations changed unexpectedly during recovery')
        result = {'status': 'recovered', 'verified_at_utc': datetime.datetime.now(datetime.timezone.utc).isoformat(),
                  'worker': {'name': WORKER_NAME, 'ip': WORKER_IP, 'image': worker['Config']['Image'],
                             'image_id': restored_worker['Image'], 'id': restored_worker['Id']},
                  'app': {'name': APP_NAME, 'ip': '192.168.0.3', 'id': restored_app['Id'], 'image_id': restored_app['Image'], 'revision': RELEASE_COMMIT},
                  'proxy': {'name': PROXY_NAME, 'id': proxy['Id'], 'status': 'unchanged'},
                  'http_redirect': 'verified', 'tls_sni': 'www.datalnet.com and console.valdtech.com verified',
                  'dnetblazor_http_https': '200 with application shell',
                  'central_reservations': 'unchanged', 'backup': str(backup_file), 'rollback': str(rollback)}
        (backup / 'result.json').write_text(json.dumps(result, indent=2) + '\n')
        print(json.dumps(result, indent=2), flush=True)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--baseline', required=True)
    parser.add_argument('--apply', action='store_true')
    args = parser.parse_args()
    try:
        recover(args.baseline, args.apply)
    except Exception as error:
        # Exceptions may originate in YAML, JSON or Compose containing secrets.
        if isinstance(error, ValueError) and not isinstance(error, json.JSONDecodeError):
            message = str(error) if error.__class__ is ValueError else error.__class__.__name__
        else:
            message = error.__class__.__name__
        print('Soccer worker recovery stopped: ' + message, flush=True)
        return 1
    return 0


if __name__ == '__main__':
    raise SystemExit(main())
