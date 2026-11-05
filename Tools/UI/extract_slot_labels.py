"""Split the label out of each web slot PNG so only the words can bounce.

The web PNGs bake the label into an opaque flat fill and stay untouched. This writes a same-size
transparent label per slot and one flat cover patch that hides the baked label at runtime.
Cover + label over the original must reproduce the original; the script fails otherwise.
"""
import json
from pathlib import Path

import numpy as np
from PIL import Image

ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT / 'docs/validation/ui_attention'
FILL = np.array([230, 244, 255], dtype=float)
COVER_INSET = .12  # Must match SlotLabelCover's anchors in 1_Game.
SLOTS = ['UIs_En/GroupSelect/GroupSlot_en', 'UIs_Th/GroupSelect/GroupSlot_th', 'UIs_En/SelectAll/ProductSlot_en',
         'UIs_Th/SelectAll/ProductSlot_th', 'UIs_En/SelectAll/ColorSlot_en', 'UIs_Th/SelectAll/ColorSlot_th']


def main():
    report = []
    for slot in SLOTS:
        source = ROOT / 'Assets/UI' / (slot + '.png')
        original = np.array(Image.open(source).convert('RGBA')).astype(float)
        size = original.shape[0]
        low, high = round(size * COVER_INSET), size - round(size * COVER_INSET)
        inner = original[low:high, low:high, :3]
        if (original[low:high, low:high, 3] != 255).any():
            raise SystemExit(f'{slot}: the fill under the label is not opaque')
        # Darkest inner pixel is the solid text color; everything else is an antialiased mix with the fill.
        distance = ((inner - FILL) ** 2).sum(axis=2)
        text = inner[np.unravel_index(distance.argmax(), distance.shape)]
        direction = FILL - text
        alpha = np.clip(((FILL - inner) * direction).sum(axis=2) / (direction * direction).sum(), 0, 1)
        label = np.zeros_like(original)
        label[..., :3] = text
        label[low:high, low:high, 3] = np.round(alpha * 255)
        target = source.with_name(source.stem.replace('Slot_', 'SlotLabel_') + '.png')
        Image.fromarray(label.astype(np.uint8), 'RGBA').save(target)
        rebuilt = FILL + (text - FILL) * (label[low:high, low:high, 3:] / 255)
        error = float(np.abs(rebuilt - inner).max())
        report.append(dict(slot=slot, size=size, text=text.tolist(), max_channel_error=round(error, 2),
                           covered_outside_label=int((distance > 0).sum())))
        if error > 3:
            raise SystemExit(f'{slot}: label split differs from the original by {error}')
    cover = ROOT / 'Assets/UI/UIs_Common/SelectGroupPanel/SlotLabelCover.png'
    Image.new('RGBA', (4, 4), (230, 244, 255, 255)).save(cover)
    OUT.mkdir(parents=True, exist_ok=True)
    (OUT / 'label_split.json').write_text(json.dumps(report, indent=2) + '\n', encoding='utf-8')
    entries(('en', 'th'))
    print(json.dumps(report, indent=1))


def entries(languages):
    labels = [('group.slotLabel', 'GroupSlotLabel', 424), ('selection.groupSlotLabel', 'GroupSlotLabel', 252),
              ('selection.productSlotLabel', 'ProductSlotLabel', 252), ('selection.colorSlot1Label', 'ColorSlotLabel', 252),
              ('selection.colorSlot2Label', 'ColorSlotLabel', 252)]
    for language in languages:
        path = ROOT / f'Assets/StreamingAssets/Lang_{language.title()}.json'
        document = json.loads(path.read_text(encoding='utf-8-sig'))
        images = [e for e in document['images'] if not e['id'].endswith('Label')]
        # Labels stretch over their frame; these values only satisfy the document schema.
        for image_id, sprite, size in labels:
            images.append(dict(id=image_id, sprite=f'{sprite}_{language}', x=0, y=0, width=size, height=size, figmaNode='derived'))
        document['images'] = images
        path.write_text(json.dumps(document, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')


if __name__ == '__main__':
    main()
