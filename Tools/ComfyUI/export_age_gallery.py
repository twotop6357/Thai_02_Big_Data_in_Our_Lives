"""Generate the current ten-product/four-age comparison without changing production settings."""
import hashlib
import json
from pathlib import Path
from benchmark import run
from build_workflow import read_json
from verify_visual_matrix import case_hash, config_hash

ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT / 'docs/validation/comfy_age_all'
SOURCE = ROOT / 'docs/validation/comfy_fast2/final'
AGES = ('child', 'student', 'working_adult', 'senior')
SEED = 1206501


def prepare_framing():
    """Run the exact pure-array production helper outside Unity when the Editor connector is offline."""
    from PIL import Image
    report = json.loads((OUT / 'report.json').read_text())
    if not report['completed'] or len(report['results']) != 40:
        raise ValueError('All forty outputs are required.')
    (OUT / 'raw_inputs').mkdir(exist_ok=True)
    (OUT / 'raw_outputs').mkdir(exist_ok=True)
    for result in report['results']:
        path = OUT / result['local_image']
        if hashlib.sha256(path.read_bytes()).hexdigest() != result['png_sha256']:
            raise ValueError('Source output changed: ' + path.name)
        with Image.open(path) as image:
            if image.size != (480, 480) or image.mode != 'RGBA':
                raise ValueError('Expected native 480 RGBA output.')
            # Unity GetPixels32 uses bottom-to-top rows.
            (OUT / 'raw_inputs' / (path.stem + '.rgba')).write_bytes(image.transpose(Image.Transpose.FLIP_TOP_BOTTOM).tobytes())
    helper = ROOT / 'Assets/Scripts/Blocks/Data/GeneratedImageFraming.cs'
    source = helper.read_text(encoding='utf-8')
    source = source[source.index('namespace Exhibition.Blocks'):]
    shim = '''using System;
using System.Threading;
using UnityEngine;
namespace UnityEngine {
    public struct Color32 {
        public byte r,g,b,a;
        public Color32(byte red,byte green,byte blue,byte alpha) {r=red;g=green;b=blue;a=alpha;}
    }
}
'''
    host = '''
public static class AgeGalleryFrameHost {
    public static void Run(string input,string output) {
        var bytes=System.IO.File.ReadAllBytes(input);
        if(bytes.Length!=480*480*4) throw new Exception("Unexpected RGBA byte length.");
        var pixels=new Color32[480*480];
        for(int i=0;i<pixels.Length;i++) pixels[i]=new Color32(bytes[i*4],bytes[i*4+1],bytes[i*4+2],bytes[i*4+3]);
        var framed=Exhibition.Blocks.GeneratedImageFraming.Center(pixels,480,480);
        for(int i=0;i<framed.Length;i++) {bytes[i*4]=framed[i].r;bytes[i*4+1]=framed[i].g;bytes[i*4+2]=framed[i].b;bytes[i*4+3]=framed[i].a;}
        System.IO.File.WriteAllBytes(output,bytes);
    }
}
'''
    (OUT / 'framing_host.cs').write_text(shim + source + host, encoding='utf-8')
    (OUT / 'framing_method.json').write_text(json.dumps({
        'source': str(helper.relative_to(ROOT)), 'source_sha256': hashlib.sha256(helper.read_bytes()).hexdigest(),
        'execution': 'Exact C# array helper compiled by PowerShell Add-Type; four-byte Color32 data shim, matching Unity row order.',
        'reason': 'Unity MCP transport unavailable; no Editor or scene mutation.'}, indent=2))


def compose():
    from PIL import Image, ImageDraw, ImageFont
    import numpy as np
    report = json.loads((OUT / 'report.json').read_text())
    products = list(read_json('presets.json')['products'])
    if config_hash() != report['config_sha256']:
        raise ValueError('Active generation settings changed during export.')
    names = ['가방', '신발', '모자', '선글라스', '헤드폰', '상의', '자전거', '텀블러', '손목시계', '우산']
    ages = ['아동', '학생', '직장인', '시니어']
    title = ImageFont.truetype('C:/Windows/Fonts/malgunbd.ttf', 46)
    header = ImageFont.truetype('C:/Windows/Fonts/malgunbd.ttf', 34)
    normal = ImageFont.truetype('C:/Windows/Fonts/malgun.ttf', 26)
    small = ImageFont.truetype('C:/Windows/Fonts/malgun.ttf', 22)
    left, top, cell, gap = 196, 202, 492, 16
    canvas = Image.new('RGB', (2248, 5330), '#F3F5F9')
    draw = ImageDraw.Draw(canvas)
    draw.text((36, 24), '연령별 물건 일러스트 모아보기', font=title, fill='#17243A')
    draw.text((38, 92), '10종 × 4연령 · 입력 색상: 빨강 / 파랑 · 물건별 동일 시드로 비교', font=normal, fill='#526075')
    for col, age in enumerate(ages):
        x = left + col * (cell + gap)
        draw.rounded_rectangle((x, 144, x + cell, 190), radius=12, fill='#E5EAF3')
        draw.text((x + cell/2, 165), age, font=header, fill='#253953', anchor='mm')
    (OUT / 'framed').mkdir(exist_ok=True)
    cells = []
    for row, (product, label) in enumerate(zip(products, names)):
        y = top + row * (cell + gap)
        draw.text((40, y + 201), f'{row+1:02d}', font=small, fill='#8290A4')
        draw.text((40, y + 239), label, font=header, fill='#253953')
        for col, age in enumerate(AGES):
            item = next(r for r in report['results'] if r['selection']['product'] == product and r['selection']['group'] == age)
            path = OUT / item['local_image']
            raw = (OUT / 'raw_outputs' / (path.stem + '.rgba')).read_bytes()
            im = Image.frombytes('RGBA', (480, 480), raw).transpose(Image.Transpose.FLIP_TOP_BOTTOM)
            alpha = np.array(im)[:, :, 3]
            yy, xx = np.where(alpha >= 8)
            if not len(xx) or abs((xx.min()+xx.max()+1)/2-240)>1 or abs((yy.min()+yy.max()+1)/2-240)>1:
                raise ValueError('Framing check failed: ' + path.name)
            im.save(OUT / 'framed' / path.name)
            x = left + col * (cell + gap)
            draw.rounded_rectangle((x, y, x + cell, y + cell), radius=14, fill='white', outline='#DDE3EC', width=2)
            bg = Image.new('RGBA', (480, 480), 'white'); bg.alpha_composite(im)
            canvas.paste(bg.convert('RGB').resize((440, 440), Image.Resampling.LANCZOS), (x + 26, y + 6))
            variants = read_json('presets.json')['products'][product].get('ageVariants', [])
            caption = next((v['label'] for v in variants if v['groupId'] == age), '')
            if caption:
                draw.text((x + cell/2, y + 468), caption, font=small, fill='#526075', anchor='mm')
            cells.append({'product': product, 'age': age, 'source': path.name, 'row': row, 'column': col})
    draw.text((38, 5290), '현재 SDXL-Turbo 4단계 설정 · 투명 배경은 비교용 흰색으로 표시', font=small, fill='#627187')
    canvas.save(OUT / 'all_products_by_age.png')
    (OUT / 'layout.json').write_text(json.dumps({'size': list(canvas.size), 'cells': cells, 'count': len(cells)}, ensure_ascii=False, indent=2), encoding='utf-8')
    # Smaller halves are internal layout QA, while the delivered image includes all forty cells.
    canvas.crop((0, 0, canvas.width, 2742)).resize((1124, 1371)).save(OUT / 'qa_top.jpg')
    canvas.crop((0, 2742, canvas.width, canvas.height)).resize((1124, 1294)).save(OUT / 'qa_bottom.jpg')
    print(f'Composed {len(cells)} cells: {canvas.size}', flush=True)


def main():
    OUT.mkdir(parents=True, exist_ok=False)
    source = SOURCE
    prior = json.loads((source / 'report.json').read_text())
    fingerprint = config_hash()
    report = {'task': 'AGE-DESIGN-002' if OUT.name != 'comfy_age_all' else 'AGE-GALLERY-001', 'config_sha256': fingerprint, 'seed': SEED,
              'colors': ['red', 'blue'], 'age_order': AGES, 'results': [], 'completed': False}
    for product in read_json('presets.json')['products']:
        for age in AGES:
            if config_hash() != fingerprint:
                raise RuntimeError('Production configuration changed; stopped before submitting another job.')
            digest = case_hash(product, age, SEED)
            name = f'{product}_{age}_{SEED}.png'
            old = next((r for r in prior['results'] if r.get('api_sha256') == digest
                        and r.get('local_image') == name and (source / name).exists()), None)
            if old:
                (OUT / name).write_bytes((source / name).read_bytes())
                result = dict(old, reused_from=str((source / name).relative_to(ROOT)))
            else:
                result = run('http://127.0.0.1:8188', age, product, 'red', 'blue', 'quality',
                             SEED, 180, OUT / name)
            result.update(api_sha256=digest, local_image=name,
                          png_sha256=hashlib.sha256((OUT / name).read_bytes()).hexdigest())
            report['results'].append(result)
            (OUT / 'report.json').write_text(json.dumps(report, ensure_ascii=False, indent=2), encoding='utf-8')
            print(f"READY {len(report['results'])}/40 {product}/{age}", flush=True)
    report['completed'] = True
    report['generated_count'] = sum('reused_from' not in r for r in report['results'])
    (OUT / 'report.json').write_text(json.dumps(report, ensure_ascii=False, indent=2), encoding='utf-8')


if __name__ == '__main__':
    import argparse
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--output', type=Path, default=OUT)
    parser.add_argument('--source', type=Path, default=SOURCE, help='Reuse only outputs whose complete API graph hash matches')
    parser.add_argument('--seed', type=int, default=SEED)
    parser.add_argument('--prepare-framing', action='store_true')
    parser.add_argument('--compose', action='store_true')
    args = parser.parse_args()
    OUT = args.output.resolve()
    SOURCE = args.source.resolve()
    SEED = args.seed
    if args.prepare_framing:
        prepare_framing()
    elif args.compose:
        compose()
    else:
        main()
