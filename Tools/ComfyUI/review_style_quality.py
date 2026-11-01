"""Contact sheets and transparent-foreground diagnostics for actual local generations."""
import argparse,json
from pathlib import Path
import numpy as np
from PIL import Image,ImageDraw,ImageFont

def review(folder):
 data=json.loads((folder/'report.json').read_text(encoding='utf-8'));rows=data['results'] if isinstance(data,dict) else data
 products=['bags','shoes','hats','sunglasses','headphones','tops','bicycles','tumblers','wristwatches','umbrellas'];ages=['child','student','working_adult','senior']
 labels=['가방','신발','모자','선글라스','헤드폰','상의','자전거','텀블러','손목시계','우산'];alabels=['아동','학생','직장인','시니어']
 cell=320;left=140;top=90;font=ImageFont.truetype('C:/Windows/Fonts/malgun.ttf',24)
 canvas=Image.new('RGB',(left+4*cell,top+10*cell),'#f4f3f8');draw=ImageDraw.Draw(canvas)
 for col,label in enumerate(alabels):draw.text((left+col*cell+cell/2,40),label,font=font,fill='#2e3145',anchor='mm')
 diagnostics=[]
 for i,p in enumerate(products):
  draw.text((15,top+i*cell+135),labels[i],font=font,fill='#2e3145')
  for j,a in enumerate(ages):
   r=next((r for r in rows if r['selection']['product']==p and r['selection']['group']==a),None)
   if r is None:continue
   name=r['tag']+'.png' if 'tag' in r else r['local_image'];im=Image.open(folder/name).convert('RGBA');pix=np.array(im);mask=pix[:,:,3]>=64;yy,xx=np.where(mask)
   diag={'product':p,'age':a,'file':name,'opaque_fraction':float(mask.mean()),'empty':not len(xx)}
   if len(xx):
    diag.update(bounds=[int(xx.min()),int(yy.min()),int(xx.max()+1),int(yy.max()+1)],touches_edge=bool(xx.min()<3 or yy.min()<3 or xx.max()>=477 or yy.max()>=477))
   diagnostics.append(diag)
   bg=Image.new('RGBA',im.size,'white');bg.alpha_composite(im);canvas.paste(bg.convert('RGB').resize((310,310)),(left+j*cell,top+i*cell))
 canvas.save(folder/'contact_sheet.jpg',quality=95)
 canvas.crop((0,0,canvas.width,top+5*cell)).save(folder/'review_top.jpg',quality=95)
 canvas.crop((0,top+5*cell,canvas.width,canvas.height)).save(folder/'review_bottom.jpg',quality=95)
 (folder/'alpha_diagnostics.json').write_text(json.dumps(diagnostics,indent=2))
 print('Reviewed',len(diagnostics),'outputs; edge hits',sum(r.get('touches_edge',False) for r in diagnostics))
if __name__=='__main__':
 p=argparse.ArgumentParser();p.add_argument('folder',type=Path);review(p.parse_args().folder)
