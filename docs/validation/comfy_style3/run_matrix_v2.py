import sys,json,asyncio,hashlib
from pathlib import Path
sys.path.insert(0,'Tools/ComfyUI')
from iterate_style_quality import graph
from probe_acceleration import render
from build_workflow import read_json
out=Path('docs/validation/comfy_style3/matrix_v2');out.mkdir(exist_ok=False)
products=list(read_json('presets.json')['products']);ages=['child','student','working_adult','senior'];cases=[]
for group,age in enumerate(ages):
 for product in products:
  cases.append({'product':product,'age':age,'first':'red','second':'blue','seed':1211001+group})
(out/'planned_cases.json').write_text(json.dumps(cases,indent=2))
async def main():
 results=[]
 for case in cases:
  api,s=graph('klein_ref',case['product'],case['age'],case['first'],case['second'],case['seed'])
  tag=case['product']+'_'+case['age'];r=await render(api,tag,out);r['selection']=s;r['api_sha256']=hashlib.sha256(json.dumps(api,sort_keys=True).encode()).hexdigest();results.append(r)
  (out/'report.json').write_text(json.dumps({'results':results,'completed':len(results)==len(cases)},ensure_ascii=False,indent=2),encoding='utf-8')
asyncio.run(main())
