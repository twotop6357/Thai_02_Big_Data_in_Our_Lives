"""Compare isolated candidate graphs; never change the active app configuration or interrupt jobs."""
import argparse
import asyncio
import json
import time
import uuid
import urllib.parse
from pathlib import Path
import aiohttp
from benchmark import http
from build_workflow import build


async def render(api, tag, folder):
    if (folder / (tag + '.png')).exists():
        raise FileExistsError('Preserve earlier evidence; select a new output folder or tag.')
    server = 'http://127.0.0.1:8188'
    q = http(server, '/queue')
    if q.get('queue_running') or q.get('queue_pending'):
        raise RuntimeError('Shared server is busy; no request submitted.')
    client = str(uuid.uuid4())
    stages = []
    started = time.perf_counter()
    async with aiohttp.ClientSession() as session:
        async with session.ws_connect(server + '/ws?clientId=' + client) as ws:
            queued = http(server, '/prompt', {'prompt': api, 'client_id': client})
            pid = queued['prompt_id']
            print(tag, pid, flush=True)
            async with asyncio.timeout(180):
                async for msg in ws:
                    if msg.type != aiohttp.WSMsgType.TEXT:
                        continue
                    event = json.loads(msg.data)
                    data = event.get('data', {})
                    if data.get('prompt_id') != pid:
                        continue
                    if event['type'] == 'execution_error':
                        raise RuntimeError(data)
                    if event['type'] == 'executing':
                        stages.append({'node': data.get('node'), 'seconds': round(time.perf_counter() - started, 3)})
                        if data.get('node') is None:
                            break
    history = http(server, '/history/' + pid)[pid]
    im = history['outputs']['7']['images'][0]
    png = http(server, '/view?' + urllib.parse.urlencode(im), binary=True)
    folder.mkdir(parents=True, exist_ok=True)
    (folder / (tag + '.png')).write_bytes(png)
    result = {'tag': tag, 'seconds': round(time.perf_counter() - started, 3), 'stages': stages,
              'api': api, 'prompt_id': pid, 'server_image': im}
    print(tag, result['seconds'], flush=True)
    return result


async def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--output', type=Path, default=Path('docs/validation/comfy_light/probes2'))
    parser.add_argument('--report', type=Path, help='Replay one exact API graph from a historical probe report')
    parser.add_argument('--tag', default='active')
    parser.add_argument('--seed', type=int)
    args = parser.parse_args()
    if args.report:
        cases = [x for x in json.loads(args.report.read_text()) if x['tag'] == args.tag]
        if len(cases) != 1:
            parser.error('--tag must identify exactly one saved candidate')
        api = cases[0]['api']
    else:
        api = build('student', 'shoes', 'red', 'blue', seed=1206501)[0]
    if args.seed is not None:
        if not 0 <= args.seed < 2**64:
            parser.error('--seed must be an unsigned 64-bit integer')
        for item in api.values():
            if item['class_type'] == 'KSampler':
                item['inputs']['seed'] = args.seed
    api['7']['inputs']['filename_prefix'] = 'Thai02/Light/' + args.tag
    result = await render(api, args.tag, args.output)
    (args.output / 'report.json').write_text(json.dumps([result], indent=2))


if __name__ == '__main__':
    asyncio.run(main())
