"""Apply the web ZIP originals for the Thai font change (UI-WEB-005).

Inputs live in docs/validation/ui_web5: browser_export.zip (download/NN.png), located.json (element
positions found inside the exported full frames) and extra/ (group slot exported on its own).
Bytes are copied unchanged. Existing .meta files are kept so GUIDs and Sprite references survive.
Run once from the repository root: python -I Tools/UI/apply_web5_assets.py
"""
import hashlib
import json
import shutil
from pathlib import Path

import numpy as np
from PIL import Image

ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT / 'docs/validation/ui_web5'
DOWNLOAD = OUT / 'download'
STREAM_TH = 'Assets/StreamingAssets/BlockArtwork/Images/th/'

# ZIP index -> block name. 12 is a duplicate of red (3) in the web board and is not used.
THAI_BLOCKS = {0: 'Bag', 1: 'Black', 2: 'Sunglasses', 3: 'Red', 4: 'Green', 5: 'Shoes', 6: 'White', 7: 'WristWatch',
               8: 'Hat', 9: 'Blue', 10: 'Tumbler', 11: 'Headphones', 13: 'Bicycle', 14: 'Yellow', 15: 'Umbrella', 16: 'Top'}
# New in the web: Thai age blocks. Previously Thai mode reused the English images.
THAI_AGES = {17: ('student', 'Student'), 18: ('child', 'Child'), 19: ('senior', 'Senior'), 20: ('working_adult', 'WorkingAdult')}
# English help boxes: only "Continue" -> "continue". Same 966 centre as every web help box, y unchanged.
ENGLISH = [(67, 'group.help', 'Assets/UI/UIs_En/GroupSelect/ButtonHelpBox_groupselectPanel_en.png'),
           (72, 'selection.help', 'Assets/UI/UIs_En/SelectAll/ButtonHelpBox_selectallPanel_en.png')]
GROUP_SLOT = ('extra/group_slot_th_Frame351.png', 'Assets/UI/UIs_Th/GroupSelect/GroupSlot_th.png')
FILL = np.array([230, 244, 255], dtype=float)
COVER_INSET = .12  # Same split as Tools/UI/extract_slot_labels.py.


def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def replace(source, target, log, new=False):
    target_path = ROOT / target
    if target_path.exists():
        backup = OUT / 'before' / target
        backup.parent.mkdir(parents=True, exist_ok=True)
        if not backup.exists():
            shutil.copy2(target_path, backup)
    elif not new:
        raise SystemExit(f'Missing target {target}')
    before = sha(target_path) if target_path.exists() else None
    shutil.copyfile(source, target_path)
    log.append(dict(source=source.relative_to(OUT).as_posix(), target=target, before=before, after=sha(target_path), new=new))


def split_group_label(log):
    """Regenerate GroupSlotLabel_th from the new GroupSlot_th, same algorithm as extract_slot_labels.py."""
    source = ROOT / GROUP_SLOT[1]
    original = np.array(Image.open(source).convert('RGBA')).astype(float)
    size = original.shape[0]
    low, high = round(size * COVER_INSET), size - round(size * COVER_INSET)
    inner = original[low:high, low:high, :3]
    if (original[low:high, low:high, 3] != 255).any():
        raise SystemExit('GroupSlot_th: the fill under the label is not opaque')
    distance = ((inner - FILL) ** 2).sum(axis=2)
    text = inner[np.unravel_index(distance.argmax(), distance.shape)]
    direction = FILL - text
    alpha = np.clip(((FILL - inner) * direction).sum(axis=2) / (direction * direction).sum(), 0, 1)
    label = np.zeros_like(original)
    label[..., :3] = text
    label[low:high, low:high, 3] = np.round(alpha * 255)
    target = source.with_name('GroupSlotLabel_th.png')
    backup = OUT / 'before' / target.relative_to(ROOT)
    backup.parent.mkdir(parents=True, exist_ok=True)
    if not backup.exists():
        shutil.copy2(target, backup)
    Image.fromarray(label.astype(np.uint8), 'RGBA').save(target)
    rebuilt = FILL + (text - FILL) * (label[low:high, low:high, 3:] / 255)
    error = float(np.abs(rebuilt - inner).max())
    if error > 3:
        raise SystemExit(f'GroupSlot_th: label split differs from the original by {error}')
    log.append(dict(source='derived from ' + GROUP_SLOT[1], target=target.relative_to(ROOT).as_posix(),
                    after=sha(target), max_channel_error=round(error, 2), text=text.tolist()))


def update_layout(language, rects):
    path = ROOT / f'Assets/StreamingAssets/Lang_{language.title()}.json'
    backup = OUT / 'before' / path.relative_to(ROOT)
    backup.parent.mkdir(parents=True, exist_ok=True)
    if not backup.exists():
        shutil.copy2(path, backup)
    document = json.loads(path.read_text(encoding='utf-8-sig'))
    for image_id, rect in rects.items():
        entry = next(e for e in document['images'] if e['id'] == image_id)
        entry.update(rect)
    path.write_text(json.dumps(document, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')


def main():
    log = []
    located = [row for row in json.loads((OUT / 'located.json').read_text(encoding='utf-8')) if row['target']]
    thai_rects = {}
    for row in located:
        replace(DOWNLOAD / f"{row['index']:02}.png", row['target'], log)
        thai_rects[row['id']] = {k: row[k] for k in ('x', 'y', 'width', 'height')}
    english_rects = {}
    for index, image_id, target in ENGLISH:
        replace(DOWNLOAD / f'{index:02}.png', target, log)
        width, height = Image.open(DOWNLOAD / f'{index:02}.png').size
        english_rects[image_id] = dict(x=966 - width // 2, y=899, width=width, height=height)
    replace(OUT / GROUP_SLOT[0], GROUP_SLOT[1], log)
    split_group_label(log)
    for index, name in THAI_BLOCKS.items():
        for folder in ('Assets/UI/Blocks/ThaiBlocks/', STREAM_TH):
            replace(DOWNLOAD / f'{index:02}.png', f'{folder}{name}Block_Thai.png', log)
    for index, (_, name) in THAI_AGES.items():
        for folder in ('Assets/UI/Blocks/ThaiBlocks/', STREAM_TH):
            replace(DOWNLOAD / f'{index:02}.png', f'{folder}{name}Block_Thai.png', log, new=True)
    update_layout('th', thai_rects)
    update_layout('en', english_rects)
    # Thai age blocks now point at their own images.
    blocks = ROOT / 'Assets/StreamingAssets/BlockArtwork/blocks_th.json'
    backup = OUT / 'before' / blocks.relative_to(ROOT)
    if not backup.exists():
        shutil.copy2(blocks, backup)
    document = json.loads(blocks.read_text(encoding='utf-8-sig'))
    ages = {block_id: name for block_id, name in THAI_AGES.values()}
    for entry in document['blocks']:
        if entry['kind'] == 'group' and entry['id'] in ages:
            entry['image'] = f"Images/th/{ages[entry['id']]}Block_Thai.png"
    blocks.write_text(json.dumps(document, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')
    (OUT / 'applied.json').write_text(json.dumps(dict(files=log, thai=thai_rects, english=english_rects),
                                                 ensure_ascii=False, indent=2) + '\n', encoding='utf-8')
    print('Applied', len(log), 'files; updated Lang_Th/Lang_En and blocks_th.json.')


if __name__ == '__main__':
    main()
