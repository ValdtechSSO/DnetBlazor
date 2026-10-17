#!/usr/bin/env python3
"""Reserve a high address for the soccer worker, as explicitly requested by the user."""
import datetime
import fcntl
import ipaddress
import json
import os
from pathlib import Path
import allocate_proxy_ips as allocation

KEY = 'laliguilla/soccer-integration-worker/prod'
OWNER = 'soccer-integration-worker-prod'

def reserve(baseline, collect):
    if baseline.get('schema_version') != 1 or baseline.get('subnet') != str(allocation.SUBNET):
        raise ValueError('Invalid conservative baseline')
    with allocation.LOCK.open('a') as lock:
        fcntl.flock(lock, fcntl.LOCK_EX)
        state = json.loads(allocation.REGISTRY.read_text())
        if state.get('schema_version') != 1 or not isinstance(state.get('allocations'), dict):
            raise ValueError('Invalid central reservation registry')
        used, _ = collect()
        used.update(baseline['blocked_ips'])
        entries = state['allocations']
        for entry in entries.values():
            used.add(allocation.address(entry['ip']))
        if KEY in entries:
            entry = entries[KEY]
            if (entry.get('container_name') != OWNER or entry.get('application') != 'laliguilla'
                    or entry.get('environment') != 'prod' or entry.get('network') != 'docker_datalnet'):
                raise ValueError('Existing reservation has another owner')
            return {'status': 'already-reserved', 'allocation': entry}
        high = [int(ipaddress.IPv4Address(ip)) & 255 for ip in used
                if ipaddress.IPv4Address(ip) in allocation.SUBNET
                and 200 < (int(ipaddress.IPv4Address(ip)) & 255) < 255]
        last = max(high, default=200)
        if last >= 254:
            raise ValueError('No address remains after the last high address')
        target = f'192.168.0.{last+1}'
        if target in used:
            raise ValueError('Requested address is occupied or reserved')
        entry = {'application': 'laliguilla', 'service': 'soccer-integration-worker',
                 'environment': 'prod', 'container_name': OWNER, 'ip': target,
                 'network': 'docker_datalnet', 'internal_port': None, 'host_port': None,
                 'state': 'reserved', 'deploy_requested': True,
                 'created_at_utc': datetime.datetime.now(datetime.timezone.utc).isoformat(),
                 'policy_exception': 'User explicitly requested an IP after the last address above 200 on 2026-10-17'}
        entries[KEY] = entry
        allocation.atomic_json(allocation.REGISTRY, state)
        return {'status': 'reserved', 'last_high_octet': last, 'allocation': entry}

if __name__ == '__main__':
    if os.geteuid() != 0 or os.uname().nodename != 'srv438597':
        raise SystemExit('Run only as root on srv438597')
    baseline = json.loads(Path('network-reservations.baseline.json').read_text())
    try:
        result = reserve(baseline, lambda: allocation.collect_live(Path('/home/docker'), 'docker_datalnet'))
    except Exception as error:
        message = str(error) if type(error) is ValueError else type(error).__name__
        raise SystemExit('Reservation refused: '+message)
    print(json.dumps(result, indent=2))
