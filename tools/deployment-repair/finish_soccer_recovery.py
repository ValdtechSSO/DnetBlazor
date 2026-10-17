#!/usr/bin/env python3
"""Finish the validated recovery without recreating the now healthy worker."""
import datetime
import fcntl
import json
import os
from pathlib import Path
import recover_soccer_worker as recovery

BACKUP = Path('/home/docker/.proxy-recovery-backups/20261017T144744Z-462764')

def finish():
    recovery.require(os.geteuid() == 0 and os.uname().nodename == 'srv438597', 'Unexpected host')
    with recovery.LOCK.open('a') as lock:
        fcntl.flock(lock, fcntl.LOCK_EX)
        registry_bytes = recovery.REGISTRY.read_bytes()
        state = json.loads(registry_bytes)
        entry = state['allocations'][recovery.RESERVATION_KEY]
        recovery.require(entry.get('container_name') == recovery.WORKER_NAME and entry.get('ip') == '192.168.0.208'
                         and entry.get('application') == 'laliguilla' and entry.get('environment') == 'prod', 'Unexpected reservation')
        worker = recovery.query('docker', 'inspect', recovery.WORKER_NAME)[0]
        app = recovery.query('docker', 'inspect', recovery.APP_NAME)[0]
        proxy = recovery.query('docker', 'inspect', recovery.PROXY_NAME)[0]
        recovery.WORKER_IP = entry['ip']
        recovery.wait_running(recovery.WORKER_NAME, entry['ip'], worker['Image'])
        original = (BACKUP / 'worker.compose.original').read_text()
        recovery.require(recovery.WORKER_FILE.read_text() == recovery.replacement(original, __import__('yaml')),
                         'Installed Compose differs from the network-only patch')
        before = recovery.query('docker', 'compose', '-p', 'docker', '--project-directory', str(recovery.ROOT),
                                '-f', str(BACKUP / 'worker.compose.original'), 'config', '--format', 'json')
        service = before['services'][recovery.WORKER_SERVICE]
        recovery.require(service['image'] == worker['Config']['Image'], 'Worker image changed')
        image = recovery.query('docker', 'image', 'inspect', service['image'])[0]
        recovery.require(image['Id'] == worker['Image'], 'Worker no longer uses the original image')
        parse = lambda values: dict(value.split('=', 1) for value in values or [])
        expected = parse(image['Config'].get('Env'))
        expected.update(service.get('environment') or {})
        recovery.require(parse(worker['Config'].get('Env')) == expected, 'Worker environment values changed')
        recovery.require(app['Config']['Image'] == recovery.RELEASE_IMAGE
                         and app['Config']['Labels'].get('org.opencontainers.image.revision') == recovery.RELEASE_COMMIT,
                         'DnetBlazor image differs from the validated release')
        recovery.require(not app['State']['Running'] and proxy['State']['Running'], 'Unexpected application/proxy state')
        network = recovery.query('docker', 'network', 'inspect', recovery.NETWORK)[0]
        recovery.require(not any(e.get('IPv4Address', '').split('/')[0] == '192.168.0.3'
                                 for e in network.get('Containers', {}).values()), 'DnetBlazor IP is occupied again')
        recovery.require(not recovery.run('ss', '-H', '-lnt', '( sport = :5001 )').strip(), 'DnetBlazor port is occupied')
        recovery.run('docker', 'start', app['Id'])
        recovery.wait_running(recovery.APP_NAME, '192.168.0.3', app['Image'])
        recovery.verify_proxy()
        current_worker = recovery.wait_running(recovery.WORKER_NAME, entry['ip'], worker['Image'])
        current_proxy = recovery.query('docker', 'inspect', recovery.PROXY_NAME)[0]
        recovery.require(current_worker['Id'] == worker['Id'] and current_worker['RestartCount'] == worker['RestartCount'],
                         'Worker changed or restarted during final verification')
        recovery.require(current_proxy['Id'] == proxy['Id'] and current_proxy['State']['Running'], 'Proxy changed')
        recovery.require(recovery.REGISTRY.read_bytes() == registry_bytes, 'Central registry changed unexpectedly')
        entry['state'] = 'active'
        entry['verified_at_utc'] = datetime.datetime.now(datetime.timezone.utc).isoformat()
        recovery.allocation.atomic_json(recovery.REGISTRY, state)
        result = {'status': 'verified', 'worker_ip': entry['ip'], 'worker_image': worker['Config']['Image'],
                  'worker_environment': 'unchanged values', 'worker_restart_count': current_worker['RestartCount'],
                  'dnetblazor_revision': recovery.RELEASE_COMMIT, 'dnetblazor_http_https': 200,
                  'proxy': 'unchanged', 'reservation': 'active', 'backup': str(BACKUP)}
        (BACKUP / 'result.json').write_text(json.dumps(result, indent=2)+'\n')
        print(json.dumps(result, indent=2))

if __name__ == '__main__':
    try:
        finish()
    except Exception as error:
        message = str(error) if type(error) is ValueError else type(error).__name__
        raise SystemExit('Final verification stopped: '+message)
