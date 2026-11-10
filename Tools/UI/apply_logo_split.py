"""LOGO-SPLIT-001: take the NSM logo out of the screen backgrounds.

The backgrounds come from the web: each screen frame was duplicated, the duplicate kept only the background
layers (logo and every element the program draws itself removed), exported with 1x PNG and deleted again.
The originals were not edited. The logo is the designer's own 1x PNG export of
"Thai_National_Science_Museum_logo_2 1" (135x116, web position 1746,40), placed by Assets/Editor/LogoAuthoring.cs.
Bytes are copied unchanged; existing .meta/GUIDs are kept.
Run once from the repository root: python -I Tools/UI/apply_logo_split.py
"""
import hashlib
import json
import shutil
from pathlib import Path

import numpy as np
from PIL import Image

ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT / 'docs/validation/logo_split'
DOWNLOAD = OUT / 'download'
BOX = (1746, 40, 1746 + 135, 40 + 116)
TARGETS = {
    'Assets/UI/Backgrounds/Intro_bg.png': 'bg_idle.png',
    'Assets/UI/Backgrounds/SelectGroupPanel_bg.png': 'bg_group.png',
    'Assets/UI/Backgrounds/SelectAllPanel_bg.png': 'bg_selection_en.png',
    'Assets/UI/Backgrounds/RecommendedAdPanel_bg.png': 'bg_ad.png',
    'Assets/UI/UIs_En/SelectAll/SelectionBackground_en.png': 'bg_selection_en.png',
    'Assets/UI/UIs_Th/SelectAll/SelectionBackground_th.png': 'bg_selection_th.png',
}
LOGO = 'Assets/UI/UIs_Common/Logo/NSMLogo.png'


def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def main():
    log = []
    outside = np.ones((1080, 1920), bool)
    outside[BOX[1]:BOX[3], BOX[0]:BOX[2]] = False
    for target, source in TARGETS.items():
        path = ROOT / target
        backup = OUT / 'before' / target
        backup.parent.mkdir(parents=True, exist_ok=True)
        if not backup.exists():
            shutil.copy2(path, backup)
        new = np.asarray(Image.open(DOWNLOAD / source).convert('RGB')).astype(float)
        old = np.asarray(Image.open(backup).convert('RGB')).astype(float)
        diff = np.abs(new - old).max(-1)
        # Everything except the logo box must be the same picture as before.
        if diff[outside].max() > 16:
            raise SystemExit(f'{target}: {source} differs outside the logo box (max {diff[outside].max()})')
        shutil.copyfile(DOWNLOAD / source, path)
        log.append(dict(target=target, source=source, before=sha(backup), after=sha(path),
                        outside_logo_rmse=round(float(np.sqrt((diff[outside] ** 2).mean())), 3),
                        outside_logo_max=int(diff[outside].max())))
    logo = ROOT / LOGO
    logo.parent.mkdir(parents=True, exist_ok=True)
    shutil.copyfile(DOWNLOAD / 'logo.png', logo)
    log.append(dict(target=LOGO, after=sha(logo), source='web Thai_National_Science_Museum_logo_2 1 (135x116 at 1746,40)'))
    (OUT / 'applied.json').write_text(json.dumps(log, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')
    print(json.dumps(log, ensure_ascii=False, indent=1))


if __name__ == '__main__':
    main()
