"""Apply the slot elements of the browser's batch ZIP without re-encoding them.

ZIP order was checked by eye: 0/8 board, 1/9 People, 2/5 Product, 3/4 and 6/7 Color.
The web's group title and subtitle positions replace the earlier four-slot header band.
"""
import json
import shutil
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT / 'docs/validation/ui_slots'
DOWNLOAD = OUT / 'download'
FILES = {
    0: 'Assets/UI/UIs_Common/SelectGroupPanel/BoardImage.png',
    1: 'Assets/UI/UIs_En/GroupSelect/GroupSlot_en.png',
    9: 'Assets/UI/UIs_Th/GroupSelect/GroupSlot_th.png',
    2: 'Assets/UI/UIs_En/SelectAll/ProductSlot_en.png',
    5: 'Assets/UI/UIs_Th/SelectAll/ProductSlot_th.png',
    3: 'Assets/UI/UIs_En/SelectAll/ColorSlot_en.png',
    6: 'Assets/UI/UIs_Th/SelectAll/ColorSlot_th.png',
}
# Web top-left positions. English group content sits 1px lower than Thai in the design.
SLOTS = {
    'en': [('group.board', 'BoardImage', 708, 355, 504, 504), ('group.slot', 'GroupSlot_en', 748, 395, 424, 424)],
    'th': [('group.board', 'BoardImage', 708, 354, 504, 504), ('group.slot', 'GroupSlot_th', 748, 394, 424, 424)],
}
SELECTION = [('selection.groupSlot', 'GroupSlot', 268, 320), ('selection.productSlot', 'ProductSlot', 536, 320),
             ('selection.colorSlot1', 'ColorSlot', 268, 588), ('selection.colorSlot2', 'ColorSlot', 536, 588)]


def main():
    backup = OUT / 'before/BoardImage.png'
    backup.parent.mkdir(parents=True, exist_ok=True)
    if not backup.exists():
        shutil.copy2(ROOT / FILES[0], backup)
    for index, target in FILES.items():
        shutil.copyfile(DOWNLOAD / f'{index:02}.png', ROOT / target)
    rows = json.loads((ROOT / 'docs/validation/ui_web4/comparison.json').read_text(encoding='utf-8'))
    for language in ('en', 'th'):
        path = ROOT / f'Assets/StreamingAssets/Lang_{language.title()}.json'
        shutil.copy2(path, OUT / 'before' / path.name)
        document = json.loads(path.read_text(encoding='utf-8-sig'))
        images = [e for e in document['images'] if not e['id'].endswith(('Slot', 'Slot1', 'Slot2')) and e['id'] != 'group.board']
        for row in rows:
            if row['language'] == language and row['id'] in ('group.title', 'group.subtitle'):
                entry = next(e for e in images if e['id'] == row['id'])
                entry.update({key: row[key] for key in ('x', 'y', 'width', 'height')})
        for image_id, sprite, x, y, width, height in SLOTS[language]:
            images.append(dict(id=image_id, sprite=sprite, x=x, y=y, width=width, height=height, figmaNode='web'))
        for image_id, sprite, x, y in SELECTION:
            images.append(dict(id=image_id, sprite=f'{sprite}_{language}', x=x, y=y, width=252, height=252, figmaNode='web'))
        document['images'] = images
        path.write_text(json.dumps(document, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')
    print('Applied', len(FILES), 'original PNGs and', len(SELECTION) + 2, 'slot entries per language.')


if __name__ == '__main__':
    main()
