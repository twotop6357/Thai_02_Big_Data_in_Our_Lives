"""Match the supplied opaque PNG pixels to their local full-screen guides. No image bytes are edited."""
import json
from pathlib import Path
import numpy as np
from PIL import Image

ROOT = Path(__file__).resolve().parents[2]
NEW = ROOT / 'Assets/UI/2_new'
OUT = ROOT / 'docs/validation/ui_refresh2'
GUIDES = {
    'th': {'idle': '태국어타이틀가이드.png', 'group': '태국어그룹선택창가이드.png', 'selection': '태국어모든선택창가이드.png', 'result': '태국어광고창가이드.png'},
    'en': {'idle': '영어타이틀가이드.png', 'group': '영어그룹선택가이드.png', 'selection': '영어모든선택가이드.png', 'result': '영어광고창가이드.png'},
}
FRAME_IDS = {'th': dict(zip(GUIDES['th'], ['27:525','27:538','27:558','27:579'])), 'en': dict(zip(GUIDES['en'], ['27:644','27:656','27:676','27:697']))}

def pixels(path):
    return np.asarray(Image.open(path).convert('RGBA'))

def match(guide, template, region):
    h, w = template.shape[:2]
    x0, y0, x1, y1 = region
    x0, y0 = max(0, x0), max(0, y0)
    x1, y1 = min(guide.shape[1]-w, x1), min(guide.shape[0]-h, y1)
    ys, xs = np.nonzero(template[:, :, 3] >= 254)
    if len(xs) < 30:
        raise ValueError('Not enough opaque source pixels')
    rng = np.random.default_rng(492)
    probes = rng.choice(len(xs), min(128, len(xs)), replace=False)
    oy, ox = np.mgrid[y0:y1+1, x0:x1+1]
    scores = np.zeros(ox.shape, np.float32)
    for i in probes:
        delta = guide[oy+ys[i], ox+xs[i], :3].astype(np.float32)-template[ys[i], xs[i], :3]
        scores += np.sum(delta*delta, axis=2)
    candidates = np.argpartition(scores.ravel(), min(20, scores.size-1))[:min(20, scores.size)]
    dense = rng.choice(len(xs), min(40000, len(xs)), replace=False)
    ty, tx = ys[dense], xs[dense]
    target = template[ty, tx, :3].astype(np.float32)
    best = None
    for index in candidates:
        iy, ix = np.unravel_index(index, scores.shape)
        x, y = int(ox[iy, ix]), int(oy[iy, ix])
        delta = guide[ty+y, tx+x, :3].astype(np.float32)-target
        rmse = float(np.sqrt(np.mean(delta*delta)))
        if best is None or rmse < best['opaqueRmse']:
            best = dict(x=x, y=y, width=w, height=h, opaqueRmse=round(rmse, 4))
    return best

def main():
    report = {'referenceResolution': [1920,1080], 'entries': [], 'common': []}
    for language, folder in [('th','Thai'), ('en','English')]:
        doc = json.loads((ROOT/f'Assets/StreamingAssets/Lang_{"Th" if language=="th" else "En"}.json').read_text(encoding='utf-8-sig'))
        by_id = {entry['id']:entry for entry in doc['images']}
        for stage, filename in GUIDES[language].items():
            guide_path = next(NEW.rglob(filename))
            guide = pixels(guide_path)
            parts = [p for p in (NEW/folder).iterdir() if p.is_dir() and (('Title_' in p.name or 'Idle_' in p.name) if stage=='idle' else p.name.startswith({'group':'GroupSelect','selection':'SelectAll','result':'Ad'}[stage]))]
            assert len(parts)==1, (language,stage,parts)
            for source in sorted(parts[0].glob('*.png')):
                role = 'help' if '버튼' in source.name else 'subtitle' if '서브' in source.name else 'title'
                entry = by_id[f'{stage}.{role}']
                old = list((ROOT/'Assets/UI').rglob(entry['sprite']+'.png'))
                assert len(old)==1, old
                region = ((0,0,180,100) if stage=='idle' else (0,0,1920,170)) if role=='title' else ((0,390,500,610) if stage=='idle' else (0,175,1920,275)) if role=='subtitle' else (0,850,1920,930)
                found = match(guide, pixels(source), region)
                row = dict(language=language,id=entry['id'],source=str(source.relative_to(ROOT)).replace('\\','/'),target=str(old[0].relative_to(ROOT)).replace('\\','/'),guide=str(guide_path.relative_to(ROOT)).replace('\\','/'),figmaNode=FRAME_IDS[language][stage],previous=entry,**found)
                report['entries'].append(row)
                print(language,entry['id'],found,flush=True)
            if language=='th':
                common = {'group': [('GroupBoard','Assets/UI/UIs_Common/SelectGroupPanel/BoardImage.png',(580,250,690,300))], 'selection': [('CenterBoard','Assets/UI/UIs_Common/SelectAllPanel/CenterBoard_selectallPanel.png',(180,260,240,300)),('ProductBoard','Assets/UI/UIs_Th/SelectAll/RecommendedProductImageBox_selectall_th.png',(790,315,850,350))], 'result': [('AdvertisementBoard','Assets/UI/UIs_Common/RecommendedAdPanel/AdBoard.png',(440,300,480,340))]}.get(stage,[])
                for name,path,region in common:
                    found=match(guide,pixels(ROOT/path),region)
                    report['common'].append(dict(name=name,source=path,**found))
                    print(name,found,flush=True)
    OUT.mkdir(parents=True,exist_ok=True)
    (OUT/'asset_mapping.json').write_text(json.dumps(report,ensure_ascii=False,indent=2),encoding='utf-8')

if __name__=='__main__': main()
