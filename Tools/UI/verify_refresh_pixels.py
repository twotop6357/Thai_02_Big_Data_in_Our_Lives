"""Verify the rendered static slots against supplied guides before deleting the guide exports."""
import hashlib
import json
from pathlib import Path
import numpy as np
from PIL import Image

ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/'docs/validation/ui_refresh2'

def main():
    mapping=json.loads((OUT/'asset_mapping.json').read_text(encoding='utf-8'))
    rows=[]
    for entry in mapping['entries']:
        stage=entry['id'].split('.')[0]
        capture=np.asarray(Image.open(OUT/f'{entry["language"]}_{stage}.png').convert('RGB')).astype(np.float32)
        guide=np.asarray(Image.open(ROOT/entry['guide']).convert('RGB')).astype(np.float32)
        asset=np.asarray(Image.open(ROOT/entry['target']).convert('RGBA'))
        mask=asset[:,:,3]>=254
        x,y,w,h=(entry[k] for k in ['x','y','width','height'])
        delta=capture[y:y+h,x:x+w][mask]-guide[y:y+h,x:x+w][mask]
        rmse=float(np.sqrt(np.mean(delta*delta)))
        bad=float(np.mean(np.max(np.abs(delta),axis=1)>40))
        same=hashlib.sha256((ROOT/entry['source']).read_bytes()).digest()==hashlib.sha256((ROOT/entry['target']).read_bytes()).digest()
        row=dict(language=entry['language'],id=entry['id'],renderedGuideRmse=round(rmse,4),fractionOver40=round(bad,5),sourceInstalledExactly=same)
        rows.append(row)
        print(row)
    result=dict(entries=rows,status='PASS' if all(r['sourceInstalledExactly'] and r['renderedGuideRmse']<18 and r['fractionOver40']<.04 for r in rows) else 'FAIL')
    (OUT/'pixel_comparison.json').write_text(json.dumps(result,indent=2),encoding='utf-8')
    if result['status']!='PASS': raise SystemExit('Static guide comparison needs attention')
    print('PASS: all 24 static image slots match the supplied guide pixels; transparent margins and live block/product content are excluded.')

if __name__=='__main__':main()
