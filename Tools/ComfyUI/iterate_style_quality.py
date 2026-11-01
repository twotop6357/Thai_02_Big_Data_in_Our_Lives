"""Isolated local pipeline experiments. Does not mutate the active app configuration."""
import argparse,asyncio,copy,hashlib,json,urllib.request,urllib.error,uuid
from pathlib import Path
from build_workflow import build,read_json
from probe_acceleration import render
ROOT=Path(__file__).resolve().parents[2]
SERVER='http://127.0.0.1:8188'
STYLE='cute polished 2D mobile game item illustration, softly rounded proportions, broad clean color shapes, gentle pastel cel shading, subtle colored edges, smooth finished highlights'
NEG='photograph, photorealistic, semi-realistic, realistic, 3d render, glossy plastic render, sketch, unfinished, rough lineart, black outlines, hatching, messy lines, noise, grunge, intricate texture, text, watermark, logo, person, human, face, mannequin, hands, feet, wearing, holding, collage, grid, multiple views, duplicates, cropped, cut off, missing parts, broken, malformed, deformed, disconnected parts, floating parts, scenery, pedestal'

def upload_reference():
 p=ROOT/'docs/validation/comfy_shoepose/reference.png';data=p.read_bytes();name='style_user_'+hashlib.sha256(data).hexdigest()[:16]+'.png';folder='Thai02Style3'
 try:
  with urllib.request.urlopen(SERVER+'/view?filename='+name+'&subfolder='+folder+'&type=input',timeout=20) as r:old=r.read()
  if hashlib.sha256(old).digest()!=hashlib.sha256(data).digest():raise ValueError('Immutable reference differs')
 except urllib.error.HTTPError as e:
  if e.code!=404:raise
  boundary=uuid.uuid4().hex
  body=b''.join([f'--{boundary}\r\nContent-Disposition: form-data; name="{k}"\r\n\r\n{v}\r\n'.encode() for k,v in [('subfolder',folder),('type','input'),('overwrite','false')]])
  body+=f'--{boundary}\r\nContent-Disposition: form-data; name="image"; filename="{name}"\r\nContent-Type: image/png\r\n\r\n'.encode()+data+f'\r\n--{boundary}--\r\n'.encode()
  req=urllib.request.Request(SERVER+'/upload/image',data=body,headers={'Content-Type':'multipart/form-data; boundary='+boundary})
  with urllib.request.urlopen(req,timeout=20) as r:result=json.load(r)
  if result.get('name')!=name or result.get('subfolder')!=folder:raise ValueError(result)
 return folder+'/'+name

def graph(mode,product,age,first='red',second='blue',seed=1206503,size=640):
 api,_,selection=build(age,product,first,second,seed=seed)
 options=read_json('presets.json');item=options['products'][product];design=next(v for v in item['ageVariants'] if v['groupId']==age)
 subject=design['prompt'].replace('simple flat illustration of ','')
 if mode=='klein_ref':
  fixes={('shoes','working_adult'):'one classic laceless penny loafer, a slip-on dress shoe with a wide strap across the vamp, smooth moccasin toe and low flat heel, no shoelaces or sneaker panels',('shoes','senior'):'one comfortable slip-on walking shoe, wide rounded toe and elastic instep, no shoelaces',('headphones','senior'):'one wireless neckband earphones, U-shaped neckband with two thin connected cables ending in two small in-ear earbuds',('wristwatches','student'):'one rectangular digital sports wristwatch with a chunky band and a clear digital LCD display reading 12:30, entire watch and both band ends',('wristwatches','working_adult'):'one rounded square smartwatch with a complete silicone wristband, simple clock face and three small colorful app dots on the screen'}
  subject=fixes.get((product,age),subject)
 positive=f'{STYLE}. {subject}. {options["color_options"][first]["prompt"]} {design["primaryPart"]}, {options["color_options"][second]["prompt"]} {design["secondaryPart"]}. One complete standalone object, fully visible, modest size in the middle with wide white margins, plain white background.'
 def node(kind,**inputs):return {'class_type':kind,'inputs':inputs}
 api['2']['inputs']['text']=positive
 api['3']['inputs']['text']=NEG
 if mode.startswith('anime'):
  # Keep archived exploration runnable after the production builder switches model families.
  for key in ('30','31','32','33','34','35','36','37','38','39'):api.pop(key,None)
  api['1']=node('CheckpointLoaderSimple',ckpt_name='animagine-xl-4.0-opt.safetensors')
  api['2']['inputs']['clip']=['1',1]
  api['3']=node('CLIPTextEncode',text=NEG,clip=['1',1])
  api['4']=node('EmptyLatentImage',width=size,height=size,batch_size=1)
  api['6']['inputs']['vae']=['1',2]
  api['2']['inputs']['text']='masterpiece, best quality, very aesthetic, '+positive
  model=['1',0]
  if mode=='anime_hyper':
   api['30']=node('LoraLoaderModelOnly',model=model,lora_name='Hyper-SDXL-8steps-CFG-lora.safetensors',strength_model=1.0);model=['30',0]
  api['5']=node('KSampler',model=model,positive=['2',0],negative=['3',0],latent_image=['4',0],seed=seed,steps=8 if mode=='anime_hyper' else 16,cfg=5.5,sampler_name='euler_ancestral' if mode!='anime_hyper' else 'euler',scheduler='normal' if mode!='anime_hyper' else 'sgm_uniform',denoise=1.0)
  del api['28'];del api['29']
 else:
  api['1']=node('UNETLoader',unet_name='flux-2-klein-4b-fp8.safetensors',weight_dtype='default')
  api['30']=node('CLIPLoader',clip_name='qwen_3_4b.safetensors',type='flux2',device='default')
  api['31']=node('VAELoader',vae_name='flux2-klein-vae-bf16.safetensors')
  api['2']['inputs']['clip']=['30',0]
  api['4']=node('EmptyFlux2LatentImage',width=size,height=size,batch_size=1)
  api['29']=node('Flux2Scheduler',steps=4,width=size,height=size)
  api['28']=node('KSamplerSelect',sampler_name='euler')
  cond=['2',0]
  if mode=='klein_ref':
   api['2']['inputs']['text']=(subject+'. Main color '+options['color_options'][first]['hex']+' ('+options['color_options'][first]['prompt']+') on '+design['primaryPart']+'. Accent color '+options['color_options'][second]['hex']+' ('+options['color_options'][second]['prompt']+') on '+design['secondaryPart']+'. Match ONLY the reference picture art style: cute polished 2D illustration, friendly rounded shapes, broad clean color blocks, gentle pastel cel shading and small smooth highlights, subtle colored edges. Keep the specified object identity, construction and colors. One complete standalone object, about 70 percent of the canvas, entire object visible with wide empty margins. Plain white background, without a card, frame, writing, scenery or surrounding decorations. Entirely hand-drawn flat illustration, not a photo or 3D render.')
   api['32']=node('LoadImage',image=upload_reference())
   api['33']=node('ImageScale',image=['32',0],upscale_method='lanczos',width=384,height=304,crop='disabled')
   api['34']=node('VAEEncode',pixels=['33',0],vae=['31',0])
   api['35']=node('ReferenceLatent',conditioning=cond,latent=['34',0]);cond=['35',0]
  api['36']=node('RandomNoise',noise_seed=seed)
  api['37']=node('BasicGuider',model=['1',0],conditioning=cond)
  api['5']=node('SamplerCustomAdvanced',noise=['36',0],guider=['37',0],sampler=['28',0],sigmas=['29',0],latent_image=['4',0])
  api['6']['inputs']['vae']=['31',0]
  # Unused negative encode must not access an SDXL CLIP output.
  del api['3']
 api['7']['inputs']['filename_prefix']='Thai02/Style3/'+mode
 selection['positive_prompt']=api['2']['inputs']['text']
 return api,selection

async def main():
 parser=argparse.ArgumentParser();parser.add_argument('--output',type=Path,required=True);parser.add_argument('--modes',nargs='+',default=['anime_hyper','anime_plain','klein_text','klein_ref']);parser.add_argument('--products',nargs='+',default=['shoes','bags']);parser.add_argument('--age',default='student');parser.add_argument('--seed',type=int,default=1206503);parser.add_argument('--size',type=int,default=640);args=parser.parse_args()
 args.output.mkdir(parents=True,exist_ok=False);results=[]
 for mode in args.modes:
  for product in args.products:
   age='working_adult' if product=='bags' and args.age=='student' else args.age
   api,selection=graph(mode,product,age,seed=args.seed,size=args.size)
   tag=mode+'_'+product+'_'+age
   result=await render(api,tag,args.output);result['selection']=selection;results.append(result)
   (args.output/'report.json').write_text(json.dumps(results,ensure_ascii=False,indent=2),encoding='utf-8')
 if results:
  from PIL import Image,ImageDraw
  w=360;canvas=Image.new('RGB',(w*len(args.products),400*len(args.modes)),'#f5f3f6');d=ImageDraw.Draw(canvas)
  for i,r in enumerate(results):
   im=Image.open(args.output/(r['tag']+'.png'));bg=Image.new('RGBA',im.size,'white');bg.alpha_composite(im);x=i%len(args.products)*w;y=i//len(args.products)*400;canvas.paste(bg.convert('RGB').resize((350,350)),(x,y));d.text((x+5,y+354),r['tag'],fill='#333');d.text((x+5,y+374),str(r['seconds'])+' sec',fill='#333')
  canvas.save(args.output/'comparison.jpg')
if __name__=='__main__':asyncio.run(main())
