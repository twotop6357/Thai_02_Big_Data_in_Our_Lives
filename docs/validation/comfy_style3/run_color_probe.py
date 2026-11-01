import sys,asyncio,json
from pathlib import Path
sys.path.insert(0,'Tools/ComfyUI')
from iterate_style_quality import graph
from probe_acceleration import render
out=Path('docs/validation/comfy_style3/color_probe');out.mkdir(exist_ok=False)
cases=[('shoes','working_adult','black','white'),('shoes','working_adult','green','white'),('headphones','senior','yellow','black'),('wristwatches','student','blue','white'),('hats','child','yellow','black'),('hats','senior','green','white'),('bags','child','green','white'),('tops','student','white','black')]
async def main():
 results=[]
 for i,(p,a,f,s) in enumerate(cases):
  api,selection=graph('klein_ref',p,a,f,s,seed=1211101+i)
  tag=p+'_'+a+'_'+f+'_'+s;r=await render(api,tag,out);r['selection']=selection;results.append(r)
  (out/'report.json').write_text(json.dumps({'results':results,'completed':len(results)==len(cases)},ensure_ascii=False,indent=2),encoding='utf-8')
asyncio.run(main())
