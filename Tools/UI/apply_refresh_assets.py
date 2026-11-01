"""Install supplied exports at the existing paths/GUIDs and update only the pre-video language entries."""
import hashlib
import json
from pathlib import Path
import shutil

ROOT = Path(__file__).resolve().parents[2]
REPORT = ROOT/'docs/validation/ui_refresh2/asset_mapping.json'

def main():
    report=json.loads(REPORT.read_text(encoding='utf-8'))
    assert len(report['entries'])==24
    audit=[]
    for language,suffix in [('th','Th'),('en','En')]:
        path=ROOT/f'Assets/StreamingAssets/Lang_{suffix}.json'
        doc=json.loads(path.read_text(encoding='utf-8-sig'))
        before_video=next(e.copy() for e in doc['images'] if e['id']=='video.end')
        for row in [r for r in report['entries'] if r['language']==language]:
            source,target=ROOT/row['source'],ROOT/row['target']
            assert source.is_file() and target.is_file() and (Path(str(target)+'.meta')).is_file()
            assert row['opaqueRmse']<15, row
            meta=Path(str(target)+'.meta');meta_hash=hashlib.sha256(meta.read_bytes()).hexdigest()
            old_hash=hashlib.sha256(target.read_bytes()).hexdigest()
            shutil.copyfile(source,target)
            assert hashlib.sha256(meta.read_bytes()).hexdigest()==meta_hash
            entry=next(e for e in doc['images'] if e['id']==row['id'])
            entry.update({k:row[k] for k in ['x','y','width','height','figmaNode']})
            audit.append(dict(id=row['id'],language=language,source=row['source'],target=row['target'],oldHash=old_hash,newHash=hashlib.sha256(target.read_bytes()).hexdigest(),metaHash=meta_hash))
        assert next(e for e in doc['images'] if e['id']=='video.end')==before_video
        path.write_text(json.dumps(doc,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
    (REPORT.parent/'replacement_audit.json').write_text(json.dumps(audit,ensure_ascii=False,indent=2),encoding='utf-8')
    print('Replaced 24 PNGs at their original paths, preserved all meta files, and kept video entries unchanged.')

if __name__=='__main__':main()
