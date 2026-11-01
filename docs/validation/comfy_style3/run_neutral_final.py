"""Predeclared final real-generation acceptance matrix for the active graph."""
import json,sys,hashlib,time
from pathlib import Path
sys.path.insert(0,'Tools/ComfyUI')
from benchmark import run
from build_workflow import read_json
from verify_visual_matrix import config_hash,case_hash
out=Path('docs/validation/comfy_style3/neutral_final');out.mkdir(exist_ok=False)
products=list(read_json('presets.json')['products']);ages=['child','student','working_adult','senior'];pairs=[('green','white'),('yellow','black'),('blue','white'),('black','yellow'),('white','black'),('red','green')]
cases=[]
for pass_id in range(2):
 for ai,age in enumerate(ages):
  for pi,product in enumerate(products):
   first,second=('red','blue') if pass_id==0 else pairs[(pi+ai)%len(pairs)]
   cases.append({'pass_id':pass_id,'product':product,'age':age,'first':first,'second':second,'seed':1211201+pass_id*100+ai*10+pi})
(out/'planned_cases.json').write_text(json.dumps(cases,indent=2));fingerprint=config_hash();report={'task':'STYLE-QUALITY-003','config_sha256':fingerprint,'results':[],'completed':False}
for i,c in enumerate(cases):
 if config_hash()!=fingerprint:raise RuntimeError('Active configuration changed; stopped before another job.')
 name=f"{c['product']}_{c['age']}_{c['first']}_{c['second']}_{c['seed']}.png"
 result=run('http://127.0.0.1:8188',c['age'],c['product'],c['first'],c['second'],'quality',c['seed'],180,out/name)
 result['pass_id']=c['pass_id'];result['png_sha256']=hashlib.sha256((out/name).read_bytes()).hexdigest()
 from build_workflow import build
 result['api_sha256']=hashlib.sha256(json.dumps(build(c['age'],c['product'],c['first'],c['second'],seed=c['seed'])[0],sort_keys=True).encode()).hexdigest()
 report['results'].append(result);report['completed']=i==len(cases)-1
 (out/'report.json').write_text(json.dumps(report,ensure_ascii=False,indent=2),encoding='utf-8')
 print(f'ACCEPTANCE OUTPUT {i+1}/{len(cases)}',flush=True)
