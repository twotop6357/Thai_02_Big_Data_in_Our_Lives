"""Create original geometric edge guides (not finished product illustrations). Requires Pillow."""
from pathlib import Path
from PIL import Image, ImageDraw
ROOT=Path(__file__).resolve().parents[2]/'Assets/StreamingAssets/BlockGeneration/ControlGuides'
ROOT.mkdir(parents=True,exist_ok=True)
for kind in ['bags','shoes','hats','sunglasses','headphones','tops','bicycles','tumblers','wristwatches','phone_cases','umbrellas','robots','keyboards','laptops']:
 im=Image.new('RGB',(1024,1024),'black');d=ImageDraw.Draw(im)
 def line(points,w=3):d.line(points,fill='white',width=w,joint='curve')
 def box(xy,r=15):d.rounded_rectangle(xy,radius=r,outline='white',width=3)
 def oval(xy):d.ellipse(xy,outline='white',width=3)
 def poly(points):line(points+[points[0]])
 if kind=='bags':
  box((290,230,730,840),65);box((340,530,680,780),30);d.arc((405,140,615,320),180,360,fill='white',width=3);line([(405,230),(405,210)]);line([(615,210),(615,230)]);box((250,370,290,780));box((730,370,770,780));line([(320,370),(700,370)])
 elif kind=='shoes':
  poly([(170,735),(250,690),(310,470),(430,430),(530,610),(650,660),(800,695),(850,750),(815,805),(200,805),(160,780)]);line([(170,760),(820,765)]);line([(310,470),(360,670),(650,700)]);
  for i in range(5):line([(350+i*26,575+i*17),(410+i*26,560+i*17)])
 elif kind=='hats':
  d.arc((250,250,720,750),180,360,fill='white',width=3);line([(250,500),(250,630),(650,660),(860,650),(780,710),(620,710),(250,630)]);line([(720,500),(720,655)]);line([(490,250),(490,630)]);d.arc((300,280,650,720),180,300,fill='white',width=3)
 elif kind=='sunglasses':
  oval((185,340,465,650));oval((555,340,835,650));oval((205,360,445,630));oval((575,360,815,630));line([(465,450),(510,430),(555,450)]);line([(190,410),(145,330),(125,335)]);line([(830,410),(875,330),(895,335)])
 elif kind=='headphones':
  d.arc((265,170,755,850),180,360,fill='white',width=4);d.arc((310,225,710,845),180,360,fill='white',width=4);box((220,470,365,765),45);box((655,470,800,765),45);box((245,495,340,740),30);box((680,495,775,740),30);line([(270,515),(285,420)]);line([(750,515),(735,420)])
 elif kind=='tops':
  poly([(385,225),(315,245),(180,365),(255,470),(310,425),(310,820),(715,820),(715,425),(770,470),(845,365),(710,245),(640,225),(605,290),(420,290)]);line([(385,225),(420,290),(605,290),(640,225)]);line([(330,790),(695,790)])
 elif kind=='bicycles':
  oval((140,550,420,830));oval((620,550,900,830));oval((153,563,407,817));oval((633,563,887,817));line([(280,690),(410,470),(480,690),(280,690),(670,470),(410,470)]);line([(480,690),(670,470),(760,690)]);line([(410,470),(390,410),(340,410),(435,410)]);line([(670,470),(645,390),(750,380),(770,400)]);oval((450,660,510,720));line([(480,690),(520,720),(545,720)])
 elif kind=='tumblers':
  box((355,280,670,825),45);oval((355,235,670,310));oval((370,250,655,295));d.arc((355,780,670,860),0,180,fill='white',width=3);line([(390,330),(390,770)])
 elif kind=='wristwatches':
  box((425,155,595,335),15);box((425,685,595,865),15);oval((300,300,720,720));oval((330,330,690,690));line([(510,510),(440,450)]);line([(510,510),(590,470)]);box((720,465,755,535),5)
  for x,y in [(510,345),(675,510),(510,675),(345,510)]:oval((x-5,y-5,x+5,y+5))
 elif kind=='phone_cases':
  box((310,160,715,865),60);box((330,180,695,845),45);box((355,205,490,380),25);oval((375,225,420,270));oval((430,225,475,270));oval((375,290,420,335))
 elif kind=='umbrellas':
  poly([(170,460),(225,350),(350,245),(510,180),(670,245),(795,350),(850,460),(710,425),(580,460),(440,425),(305,460)]);line([(510,180),(440,425)]);line([(510,180),(580,460)]);line([(510,180),(305,460)]);line([(510,180),(710,425)]);line([(510,445),(510,780)]);d.arc((405,730,510,855),0,180,fill='white',width=4);line([(405,795),(405,760)])
 elif kind=='robots':
  box((350,175,675,405),45);box((310,430,715,690),40);box((175,445,300,685),25);box((725,445,850,685),25);box((325,710,465,865),20);box((560,710,700,865),20);line([(300,500),(310,500)]);line([(715,500),(725,500)]);line([(465,405),(465,430)]);line([(560,405),(560,430)]);oval((400,250,465,315));oval((560,250,625,315));box((370,490,655,620),15)
 elif kind=='keyboards':
  box((145,325,880,735),25);box((165,345,860,715),15)
  for row in range(5):
   for col in range(12):box((180+col*55,365+row*60,225+col*55,410+row*60),4)
 elif kind=='laptops':
  poly([(260,175),(780,175),(780,585),(260,585)]);box((280,195,760,565),6);poly([(260,585),(780,585),(850,820),(190,820)]);line([(200,795),(840,795)]);box((435,715,605,775),7)
  for row in range(4):
   for col in range(10):box((290+col*44,610+row*22,325+col*44,627+row*22),2)
 im.save(ROOT/f'guide_{kind}.png')
print('Created 14 original geometric control guides:',ROOT)
