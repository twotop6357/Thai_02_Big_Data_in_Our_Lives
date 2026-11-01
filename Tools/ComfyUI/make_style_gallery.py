"""Compose review images from unchanged, actual Unity-validated generated PNGs."""
import json
from pathlib import Path
from PIL import Image,ImageDraw,ImageFont
ROOT=Path(__file__).resolve().parents[2];OUT=ROOT/'docs/validation/comfy_style3';FOLDER=OUT/'final_accepted'
report=json.loads((FOLDER/'report.json').read_text());catalog=json.loads((ROOT/'Assets/StreamingAssets/BlockGeneration/BlockCatalog.json').read_text(encoding='utf-8'))
products=[p['id'] for p in catalog['products']];names=['가방','신발','모자','선글라스','헤드폰','상의','자전거','텀블러','손목시계','우산'];ages=['child','student','working_adult','senior'];alabels=['아동','학생','직장인','시니어'];cn={'red':'빨강','blue':'파랑','green':'초록','white':'흰색','black':'검정','yellow':'노랑'}
font=lambda size:boldfont(size)
def boldfont(size):return ImageFont.truetype('C:/Windows/Fonts/malgunbd.ttf',size)
normal=lambda size:ImageFont.truetype('C:/Windows/Fonts/malgun.ttf',size)
def paste(canvas,r,xy,size):
 image=Image.open(FOLDER/'framed'/r['local_image']).convert('RGBA');base=Image.new('RGBA',image.size,'white');base.alpha_composite(image);canvas.paste(base.convert('RGB').resize((size,size),Image.Resampling.LANCZOS),xy)
for batch,filename,title in [(0,'age_comparison.png','귀엽고 부드러운 2D 물건 · 전체 연령 비교'),(1,'color_comparison.png','같은 그림체 · 다양한 색 블록 조합 비교')]:
 cell=372;left=154;top=160;canvas=Image.new('RGB',(left+4*cell+18,top+10*cell+36),'#f3f2f7');d=ImageDraw.Draw(canvas)
 d.text((26,18),title,font=font(36),fill='#29334a');d.text((28,73),'10종 × 4연령 · 실제 생성 PNG / 앱에서 원본 검사·중앙 배치',font=normal(21),fill='#626c7b')
 for col,label in enumerate(alabels):d.text((left+col*cell+cell/2,125),label,font=font(26),fill='#34435b',anchor='mm')
 for row,(p,label) in enumerate(zip(products,names)):
  y=top+row*cell;d.text((18,y+157),label,font=font(24),fill='#34435b')
  for col,age in enumerate(ages):
   r=next(r for r in report['results'] if r['pass_id']==batch and r['selection']['product']==p and r['selection']['group']==age);x=left+col*cell;s=r['selection']
   d.rounded_rectangle((x,y,x+360,y+360),radius=12,fill='white');paste(canvas,r,(x+9,y+4),342)
   color=cn[s['color1']]+' / '+cn[s['color2']];d.text((x+180,y+346),color,font=normal(17),fill='#687389',anchor='mm')
 canvas.save(OUT/filename)
# A predetermined product/age/color selection, not per-cell seed selection. Both complete sheets remain available.
choices=[('bags','child',1),('shoes','student',1),('hats','senior',0),('sunglasses','child',1),('headphones','student',1),('tops','senior',1),('bicycles','student',1),('tumblers','working_adult',1),('wristwatches','student',0),('umbrellas','child',1)]
canvas=Image.new('RGB',(1800,960),'#f3f2f7');d=ImageDraw.Draw(canvas);d.text((28,18),'레퍼런스의 부드러운 색면과 귀여운 형태를 반영한 실제 생성 결과',font=font(32),fill='#29334a');d.text((30,66),'전체 80개 중 물건·연령·색 조합 예시 · 투명 배경은 흰색으로 표시',font=normal(21),fill='#626c7b')
layout=[]
for i,(p,a,batch) in enumerate(choices):
 r=next(r for r in report['results'] if r['pass_id']==batch and r['selection']['product']==p and r['selection']['group']==a);s=r['selection'];x=18+(i%5)*356;y=112+(i//5)*416
 d.rounded_rectangle((x,y,x+344,y+396),radius=14,fill='white');paste(canvas,r,(x+8,y+5),328)
 label=names[products.index(p)]+' · '+alabels[ages.index(a)];color=cn[s['color1']]+' / '+cn[s['color2']]
 d.text((x+172,y+348),label,font=font(22),fill='#34435b',anchor='mm');d.text((x+172,y+378),color,font=normal(19),fill='#687389',anchor='mm');layout.append({'image':r['local_image'],'selection':s,'box':[x,y,x+344,y+396]})
canvas.save(OUT/'quality_preview.png');(OUT/'preview_layout.json').write_text(json.dumps(layout,ensure_ascii=False,indent=2),encoding='utf-8')
canvas=Image.new('RGB',(1420,570),'#f3f2f7');d=ImageDraw.Draw(canvas)
labels=['사용자 레퍼런스','이전 결과 · 범용 Turbo','수정 결과 · 레퍼런스 기반 2D']
for i,label in enumerate(labels):d.text((i*470+235,35),label,font=font(24),fill='#34435b',anchor='mm');d.rectangle((i*470+8,65,i*470+462,548),fill='white')
ref=Image.open(ROOT/'docs/validation/comfy_shoepose/reference.png').convert('RGB');ref.thumbnail((430,430));ref=ref.resize((402,320));canvas.paste(ref,(34,135))
old=Image.open(ROOT/'docs/validation/comfy_age_design/approved/framed/shoes_student_1206502.png').convert('RGBA');bg=Image.new('RGBA',old.size,'white');bg.alpha_composite(old);canvas.paste(bg.convert('RGB').resize((440,440)),(485,82))
r=next(r for r in report['results'] if r['selection']['product']=='shoes' and r['selection']['group']=='student' and r['pass_id']==0);paste(canvas,r,(955,82),440)
canvas.save(OUT/'reference_comparison.png')
print('Composed preview, original-reference comparison and both complete 40-case sheets.')
