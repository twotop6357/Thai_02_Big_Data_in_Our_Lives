import json,sys,hashlib
from pathlib import Path
sys.path.insert(0,'Tools/ComfyUI')
from benchmark import run
from build_workflow import build
from verify_visual_matrix import config_hash
source=Path('docs/validation/comfy_style3/neutral_final');out=Path('docs/validation/comfy_style3/final_accepted');out.mkdir(exist_ok=False)
old=json.loads((source/'report.json').read_text());assert old['completed']
report={'task':'STYLE-QUALITY-003','config_sha256':config_hash(),'results':[],'completed':False}
for item in old['results']:
 s=item['selection'];api=build(s['group'],s['product'],s['color1'],s['color2'],seed=s['seed'])[0];digest=hashlib.sha256(json.dumps(api,sort_keys=True).encode()).hexdigest();name=item['local_image']
 if digest==item['api_sha256']:
  data=(source/name).read_bytes();assert hashlib.sha256(data).hexdigest()==item['png_sha256'];(out/name).write_bytes(data);r=dict(item,reused_from=str(source/name))
 else:
  r=run('http://127.0.0.1:8188',s['group'],s['product'],s['color1'],s['color2'],'quality',s['seed'],180,out/name)
  r.update(pass_id=item['pass_id'],api_sha256=digest,png_sha256=hashlib.sha256((out/name).read_bytes()).hexdigest())
 report['results'].append(r)
 (out/'report.json').write_text(json.dumps(report,ensure_ascii=False,indent=2),encoding='utf-8')
report.update(completed=True,new_generations=sum('reused_from' not in r for r in report['results']))
(out/'report.json').write_text(json.dumps(report,ensure_ascii=False,indent=2),encoding='utf-8')
print('Final 80-case matrix ready, graph-identical reuse verified.')
