"""Build original geometric SAMPLE line art + two-region masks. Requires Pillow.

These are replaceable developer samples, not approved exhibition artwork.
Only Coloring/Samples is written; a custom BlockColoring.json is never overwritten.
Run from any directory. No ComfyUI, model, server or network dependency.
"""
import json
from pathlib import Path
from PIL import Image, ImageDraw

ROOT = Path(__file__).resolve().parents[2] / 'Assets/StreamingAssets/BlockGeneration'
SCALE = 2
SIZE = 480
PRIMARY, SECONDARY = (255, 0, 0, 255), (0, 255, 0, 255)


class Canvas:
    def __init__(self):
        self.base = Image.new('RGBA', (SIZE*SCALE, SIZE*SCALE))
        self.mask = Image.new('RGBA', self.base.size, (0, 0, 0, 255))
        self.b = ImageDraw.Draw(self.base)
        self.m = ImageDraw.Draw(self.mask)

    def coords(self, values):
        return tuple(round(v*SCALE) for v in values)

    def box(self, xy, region=1, radius=12):
        xy = self.coords(xy)
        self.b.rounded_rectangle(xy, radius=radius*SCALE, fill='white', outline='black', width=3*SCALE)
        self.m.rounded_rectangle(xy, radius=radius*SCALE, fill=PRIMARY if region == 1 else SECONDARY)

    def oval(self, xy, region=1):
        xy = self.coords(xy)
        self.b.ellipse(xy, fill='white', outline='black', width=3*SCALE)
        self.m.ellipse(xy, fill=PRIMARY if region == 1 else SECONDARY)

    def poly(self, points, region=1):
        points = [self.coords(p) for p in points]
        self.b.polygon(points, fill='white')
        self.b.line(points+[points[0]], fill='black', width=3*SCALE, joint='curve')
        self.m.polygon(points, fill=PRIMARY if region == 1 else SECONDARY)

    def line(self, points, width=3):
        self.b.line([self.coords(p) for p in points], fill='black', width=width*SCALE, joint='curve')

    def save(self, stem):
        stem.parent.mkdir(parents=True, exist_ok=True)
        self.base.resize((SIZE, SIZE), Image.Resampling.LANCZOS).save(str(stem)+'_base.png')
        # Bilinear avoids ringing; the compositor tolerates one byte of boundary rounding.
        self.mask.resize((SIZE, SIZE), Image.Resampling.BILINEAR).save(str(stem)+'_mask.png')


def draw(kind, age):
    c = Canvas()
    r = [30, 16, 5, 22][age]
    if kind == 'bags':
        if age < 2:
            c.box((190, 75, 290, 150), 2, r)
            c.box((115, 140, 160, 365), 2, 18); c.box((320, 140, 365, 365), 2, 18)
            c.box((140, 115, 340, 400), 1, r)
            c.box((165, 255, 315, 375), 2, r)
            c.line([(155, 200), (325, 200)])
            if age == 0: c.oval((213, 150, 267, 190), 2)
        else:
            c.box((175, 85, 305, 240), 2, r)
            c.box((100, 185, 380, 380), 1, r)
            c.box((115, 195, 365, 265), 2, r)
            c.box((225, 240, 255, 290), 1, 4)
            if age == 3: c.box((130, 290, 350, 355), 2, r)
    elif kind == 'shoes':
        height = [215, 170, 230, 200][age]
        c.poly([(75, 325), (115, 290), (145, height), (205, height-10), (260, 290), (370, 320), (410, 350), (395, 380), (80, 380)])
        c.box((75, 350, 405, 385), 2, 14)
        if age == 2: c.poly([(190, 245), (225, 240), (300, 315), (270, 325)], 2)
        else:
            for i in range(2 if age in (0, 3) else 4):
                c.box((163+i*22, 230+i*20, 218+i*22, 245+i*20), 2, 4)
        c.line([(110, 330), (170, 315)])
    elif kind == 'hats':
        if age == 0:
            c.oval((135, 95, 345, 330)); c.box((120, 260, 360, 330), 2, 20); c.oval((217, 65, 263, 110), 2)
        else:
            c.oval((130, 120, 350, 335)); c.box((130, 255, 350, 315), 2, 7)
            if age == 1: c.oval((215, 285, 410, 345), 2)
            else: c.oval((75 if age == 3 else 95, 285, 405 if age == 3 else 385, 345), 2)
            c.line([(240, 125), (240, 250)])
    elif kind == 'sunglasses':
        c.poly([(80, 210), (55, 150), (100, 145), (135, 210)], 1)
        c.poly([(345, 210), (380, 145), (425, 150), (400, 210)], 1)
        c.box((190, 205, 290, 230), 1, 8)
        for x in (80, 270):
            if age in (0, 3):
                c.oval((x, 175, x+130, 315)); c.oval((x+14, 190, x+116, 300), 2)
            else:
                c.box((x, 185, x+130, 300), 1, r); c.box((x+14, 200, x+116, 285), 2, r)
    elif kind == 'headphones':
        c.oval((115, 80, 365, 385), 2)
        # Clear the headband opening to genuine transparency, not white background.
        c.b.ellipse(c.coords((145, 115, 335, 375)), fill=(0, 0, 0, 0), outline='black', width=3*SCALE)
        c.m.ellipse(c.coords((145, 115, 335, 375)), fill=(0, 0, 0, 255))
        c.b.rectangle(c.coords((155, 240, 325, 390)), fill=(0, 0, 0, 0))
        c.m.rectangle(c.coords((155, 240, 325, 390)), fill=(0, 0, 0, 255))
        c.box((95, 225, 165, 365), 1, r); c.box((315, 225, 385, 365), 1, r)
        c.box((143, 240, 178, 350), 2, 10); c.box((302, 240, 337, 350), 2, 10)
        if age == 1: c.line([(110, 275), (150, 315)])
        if age == 3: c.box((345, 275, 370, 310), 2, 8)
    elif kind == 'tops':
        sleeve = [155, 145, 160, 100][age]
        c.poly([(175, 105), (115, 130), (65, 220), (115, 255), (145, 220), (145, 395), (335, 395), (335, 220), (365, 255), (415, 220), (365, 130), (305, 105), (275, sleeve), (205, sleeve)])
        c.poly([(175, 105), (205, sleeve), (275, sleeve), (305, 105), (287, sleeve+18), (193, sleeve+18)], 2)
        c.box((145, 370, 335, 395), 2, 1)
        if age == 0: c.oval((215, 225, 265, 275), 2)
        elif age in (2, 3):
            c.line([(240, 175), (240, 365)])
            c.box((265, 205, 310, 245), 2, 3)
        else: c.poly([(155, 235), (325, 235), (325, 260), (155, 260)], 2)
    elif kind == 'bicycles':
        for x in (65, 295):
            c.oval((x, 270, x+120, 390), 2)
            c.b.ellipse(c.coords((x+12, 282, x+108, 378)), fill=(0, 0, 0, 0), outline='black', width=2*SCALE)
            c.m.ellipse(c.coords((x+12, 282, x+108, 378)), fill=(0, 0, 0, 255))
            c.line([(x+60, 283), (x+60, 377)], 1); c.line([(x+13, 330), (x+107, 330)], 1)
        c.poly([(125, 330), (188, 220), (230, 330)], 1)
        c.poly([(194, 220), (306, 220 if age != 3 else 280), (230, 330)], 1)
        c.poly([(300, 200), (312, 197), (361, 329), (349, 333)], 1)
        c.box((161, 197, 215, 214), 2, 5)
        c.line([(190, 214), (195, 245)], 4)
        c.line([(305, 203), (295, 177), (344, 177)], 5)
        c.oval((214, 314, 246, 346), 2); c.line([(230, 330), (253, 350), (270, 350)])
        if age == 0: c.oval((78, 370, 112, 404), 2)
        if age == 1: c.box((138, 240, 170, 260), 2, 5)
        if age == 3: c.box((325, 207, 379, 260), 2, 5)
    elif kind == 'tumblers':
        width = [100, 120, 115, 145][age]; left = 240-width/2
        if age == 3: c.box((300, 180, 360, 300), 2, 20)
        c.box((left, 145, left+width, 395), 1, r)
        c.box((left-10, 110, left+width+10, 160), 2, 12)
        c.box((left+3, 245, left+width-3, 290), 2, 6)
        if age == 0: c.box((230, 65, 245, 115), 2, 6)
        if age == 1: c.line([(left+20, 175), (left+20, 220)])
    elif kind == 'wristwatches':
        c.box((205, 65, 275, 415), 1, r)
        if age in (1, 2): c.box((157, 150, 323, 320), 2, r)
        else: c.oval((150, 145, 330, 325), 2)
        c.oval((172, 167, 308, 303), 1)
        c.line([(240, 235), (215, 200)], 4); c.line([(240, 235), (280, 220)], 4)
        if age == 3:
            for x,y in [(240, 180), (240, 290), (185, 235), (295, 235)]: c.oval((x-4,y-4,x+4,y+4), 2)
        c.box((328, 215, 345, 250), 2, 3)
    elif kind == 'phone_cases':
        w = [190, 180, 170, 200][age]; left = (480-w)/2
        c.box((left, 70, left+w, 415), 2, r)
        c.box((left+12, 82, left+w-12, 403), 1, r)
        c.box((left+25, 98, left+90, 178), 2, 12)
        c.oval((left+35, 110, left+58, 133), 1); c.oval((left+57, 145, left+80, 168), 1)
        if age == 0: c.oval((210, 245, 270, 305), 2)
        if age == 1: c.poly([(left+15, 350), (left+w-15, 250), (left+w-15, 280), (left+15, 380)], 2)
        if age == 3: c.box((210, 220, 270, 330), 2, 20)
    elif kind == 'umbrellas':
        c.box((234, 195, 246, 375), 2, 5)
        c.box((210, 360, 255, 405), 2, 14)
        top = [100, 80, 65, 115][age]
        points = [(65, 240), (100, 175), (165, 120), (240, top), (315, 120), (380, 175), (415, 240), (345, 225), (275, 245), (205, 225), (135, 245)]
        c.poly(points)
        c.poly([(240, top), (165, 120), (135, 245), (205, 225)], 2)
        c.poly([(240, top), (315, 120), (345, 225), (275, 245)], 2)
    elif kind == 'robots':
        c.box((160, 90, 320, 195), 1, r); c.box((140, 210, 340, 335), 1, r)
        for x in (90, 350): c.box((x, 220, x+40, 330), 2, r)
        for x in (155, 265): c.box((x, 345, x+60, 410), 2, r)
        for x in (190, 265): c.oval((x, 120, x+25, 150), 2)
        c.box((175, 245, 305, 305), 2, r)
        c.line([(240, 65), (240, 90)], 5)
    elif kind == 'keyboards':
        c.box((55, 160, 425, 340), 1, r)
        for row in range(4):
            for col in range(10): c.box((72+col*34, 177+row*31, 99+col*34, 201+row*31), 2, 3)
        c.box((150, 307, 330, 326), 2, 3)
    elif kind == 'laptops':
        c.box((105, 90, 375, 280), 1, r); c.box((120, 105, 360, 265), 2, r)
        c.poly([(105, 280), (375, 280), (410, 390), (70, 390)], 1)
        for row in range(3):
            for col in range(9): c.box((119+col*27, 292+row*19, 140+col*27, 306+row*19), 2, 2)
        c.box((205, 355, 275, 380), 2, 5)
    return c


def main():
    blocks = json.loads((ROOT/'BlockCatalog.json').read_text(encoding='utf-8-sig'))
    templates = []
    for age, group in enumerate(blocks['groups']):
        for product in blocks['products']:
            kind = product['id']
            if kind == 'equipment': kind = ['robots', 'laptops', 'keyboards', 'laptops'][age]
            relative = f"Coloring/Samples/{group['id']}_{kind}"
            draw(kind, age).save(ROOT/relative)
            templates.append(dict(groupId=group['id'], productId=product['id'], resolvedProductId=kind,
                                  baseImage=relative+'_base.png', maskImage=relative+'_mask.png'))
    palette = dict(black='#000000', white='#FFFFFF', blue='#2068E8', red='#E83030', green='#27AD55', yellow='#FFD52A')
    catalog = ROOT/'BlockColoring.json'
    if not catalog.exists():
        catalog.write_text(json.dumps(dict(schemaVersion=1, colors=[dict(id=k, hex=v) for k,v in palette.items()],
                                           templates=templates), indent=2)+'\n', encoding='utf-8')
    print(f'Built {len(templates)} sample base/mask pairs. Existing custom catalog preserved.')


if __name__ == '__main__':
    main()
