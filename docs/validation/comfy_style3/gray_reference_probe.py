import asyncio,sys,json
from pathlib import Path
sys.path.insert(0,'Tools/ComfyUI');from build_workflow import build
from probe_acceleration import render
from PIL import Image,ImageDraw
out=Path('docs/validation/comfy_style3/gray_reference_probe');out.mkdir(exist_ok=False)
cases=[('shoes','student','blue','white',1211312),('bags','child','green','white',1211301),('shoes','student','red','blue',1211212),('hats','child','yellow','black',1211401),('wristwatches','working_adult','white','black',1211329),('headphones','senior','yellow','black',1211335)]
async def main():
 rows=[]
 for i,(p,a,f,s,seed) in enumerate(cases):
  api,_,selection=build(a,p,f,s,seed=seed)
  api['38']={'class_type':'ImageToMask','inputs':{'image':['32',0],'channel':'green'}}
  api['39']={'class_type':'MaskToImage','inputs':{'mask':['38',0]}}
  api['33']['inputs']['image']=['39',0]
  r=await render(api,str(i),out);r['selection']=selection;rows.append(r)
  (out/'report.json').write_text(json.dumps({'results':rows,'completed':len(rows)==len(cases)},ensure_ascii=False,indent=2))
 canvas=Image.new('RGB',(960,680),'#f5f3f8');draw=ImageDraw.Draw(canvas)
 for i,r in enumerate(rows):
  im=Image.open(out/(r['tag']+'.png'));bg=Image.new('RGBA',im.size,'white');bg.alpha_composite(im);canvas.paste(bg.convert('RGB').resize((310,310)),((i%3)*320,(i//3)*340));draw.text(((i%3)*320,(i//3)*340+312),r['selection']['product']+' '+r['selection']['color1']+'/'+r['selection']['color2'],fill='black')
 canvas.save(out/'comparison.jpg')
asyncio.run(main())
