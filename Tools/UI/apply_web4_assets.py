"""Apply reviewed, original PNG exports from the browser's bilingual UI ZIP.

comparison.json records the screenshot-to-element matches. Keep the four-reader
board and its animated slot positions; only replace static artwork/layout data.
"""
import hashlib
import json
import shutil
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT / 'docs/validation/ui_web4'


def main():
    rows = json.loads((OUT / 'comparison.json').read_text(encoding='utf-8'))
    baseline = json.loads((OUT / 'before_hashes.json').read_text(encoding='utf-8'))
    documents = {}
    for language in ('en', 'th'):
        path = ROOT / f'Assets/StreamingAssets/Lang_{language.title()}.json'
        documents[language] = json.loads(path.read_text(encoding='utf-8-sig'))
    applied = []
    for row in rows:
        target = ROOT / row['target']
        # Refuse to overwrite an intervening edit, including another worker's UI.
        assert hashlib.sha256(target.read_bytes()).hexdigest() == baseline[row['target']], target
        if not row['equal']:
            backup = OUT / 'before' / row['target']
            backup.parent.mkdir(parents=True, exist_ok=True)
            shutil.copy2(target, backup)
            shutil.copyfile(OUT / 'download' / f"{row['index']:02}.png", target)
        entry = next(e for e in documents[row['language']]['images'] if e['id'] == row['id'])
        rect = {key: row[key] for key in ('x', 'y', 'width', 'height')}
        if row['id'] in ('group.title', 'group.subtitle'):
            # Web moved these down for a single slot. Retain the existing header
            # band so it does not overlap the four-reader board at y=274.
            rect['y'] = entry['y']
        entry.update(rect)
        applied.append(dict(language=row['language'], id=row['id'], target=row['target'],
                            replaced=not row['equal'], appliedRect=rect,
                            retainedGroupHeaderY=row['id'] in ('group.title', 'group.subtitle')))
    for language, document in documents.items():
        path = ROOT / f'Assets/StreamingAssets/Lang_{language.title()}.json'
        relative = path.relative_to(ROOT).as_posix()
        assert hashlib.sha256(path.read_bytes()).hexdigest() == baseline[relative], path
        backup = OUT / 'before' / relative
        backup.parent.mkdir(parents=True, exist_ok=True)
        shutil.copy2(path, backup)
        path.write_text(json.dumps(document, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')
    (OUT / 'applied.json').write_text(json.dumps(applied, ensure_ascii=False, indent=2), encoding='utf-8')
    print('Replaced', sum(a['replaced'] for a in applied), 'original PNGs; updated two language documents.')


if __name__ == '__main__':
    main()
