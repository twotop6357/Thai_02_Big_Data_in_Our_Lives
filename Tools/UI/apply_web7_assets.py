"""UI-WEB-007: 태국어 폰트 변경으로 글 길이가 바뀐 문구 6개를 웹 일괄 ZIP 원본으로 교체한다.

입력은 docs/validation/ui_web7/download/NN.png(browser_export.zip을 순서대로 푼 파일)이다.
새 요소를 같은 ZIP의 전체 화면 프레임 안에서 불투명 픽셀 기준으로 찾아 Lang_Th.json 좌표를 갱신한다.
PNG 바이트는 그대로 복사하고 기존 .meta는 유지해 GUID와 Sprite 참조를 보존한다.
저장소 루트에서 한 번 실행: python -I Tools/UI/apply_web7_assets.py
"""
import hashlib
import json
import shutil
import sys
from pathlib import Path

import cv2
import numpy as np
from PIL import Image

ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT / 'docs/validation/ui_web7'
DOWNLOAD = OUT / 'download'
# 위치를 찾을 때 이전 좌표에서 상하좌우로 더 살펴볼 범위(픽셀).
SEARCH_MARGIN = 160
# 불투명 픽셀의 평균 채널 오차가 이 값을 넘으면 잘못 찾은 것으로 보고 중단한다(0~255).
MAX_RMSE = 12

# (요소 ZIP 인덱스, 전체 프레임 ZIP 인덱스, Lang JSON id, 대상 PNG)
ELEMENTS = [
    (39, 36, 'group.title', 'Assets/UI/UIs_Th/GroupSelect/Title_groupselect_th.png'),
    (40, 36, 'group.subtitle', 'Assets/UI/UIs_Th/GroupSelect/Subtitle_groupselect_th.png'),
    (49, 46, 'selection.title', 'Assets/UI/UIs_Th/SelectAll/Title_selectall_th.png'),
    (54, 51, 'result.title', 'Assets/UI/UIs_Th/RecommendedAd/Title_recommendedAd_th.png'),
    (55, 51, 'result.subtitle', 'Assets/UI/UIs_Th/RecommendedAd/Subtitle_recommendedAd_th.png'),
    (59, 58, 'video.end', 'Assets/UI/UIs_Th/Video/VideoEndPopupText_th.png'),
]
LANG_TH = ROOT / 'Assets/StreamingAssets/Lang_Th.json'


def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def backup(target):
    """처음 실행할 때만 원래 파일을 before/에 보관한다."""
    copy = OUT / 'before' / target.relative_to(ROOT)
    copy.parent.mkdir(parents=True, exist_ok=True)
    if not copy.exists():
        shutil.copy2(target, copy)


def locate(element_index, frame_index, previous):
    """새 요소가 전체 프레임 안 어디에 그려졌는지 불투명 픽셀만 비교해 찾는다."""
    element = np.asarray(Image.open(DOWNLOAD / f'{element_index:02}.png').convert('RGBA'))
    frame = np.asarray(Image.open(DOWNLOAD / f'{frame_index:02}.png').convert('RGB'))
    height, width = element.shape[:2]
    left = max(0, previous['x'] - SEARCH_MARGIN)
    top = max(0, previous['y'] - SEARCH_MARGIN)
    right = min(frame.shape[1], previous['x'] + max(width, previous['width']) + SEARCH_MARGIN)
    bottom = min(frame.shape[0], previous['y'] + max(height, previous['height']) + SEARCH_MARGIN)
    window = frame[top:bottom, left:right].astype(np.float32)
    opaque = element[..., 3] == 255
    if opaque.sum() < 100:
        raise SystemExit(f'Element {element_index} has too few opaque pixels to locate.')
    mask = np.repeat(opaque[..., None], 3, axis=2).astype(np.float32)
    scores = cv2.matchTemplate(window, element[..., :3].astype(np.float32), cv2.TM_SQDIFF, mask=mask)
    _, _, best, _ = cv2.minMaxLoc(scores)
    x, y = left + best[0], top + best[1]
    patch = frame[y:y + height, x:x + width].astype(float)
    rmse = float(np.sqrt(((patch - element[..., :3])[opaque] ** 2).mean()))
    if rmse > MAX_RMSE:
        raise SystemExit(f'Element {element_index}: best match RMSE {rmse:.1f} exceeds {MAX_RMSE}.')
    return dict(x=int(x), y=int(y), width=int(width), height=int(height)), round(rmse, 3)


def main():
    document = json.loads(LANG_TH.read_text(encoding='utf-8-sig'))
    entries = {entry['id']: entry for entry in document['images']}
    log = []
    for element_index, frame_index, image_id, target in ELEMENTS:
        entry = entries[image_id]
        previous = {k: entry[k] for k in ('x', 'y', 'width', 'height')}
        rect, rmse = locate(element_index, frame_index, previous)
        target_path = ROOT / target
        backup(target_path)
        before = sha(target_path)
        shutil.copyfile(DOWNLOAD / f'{element_index:02}.png', target_path)
        entry.update(rect)
        log.append(dict(index=element_index, frame=frame_index, id=image_id, target=target, before=before,
                        after=sha(target_path), previousRect=previous, rect=rect, opaqueRmse=rmse))
    backup(LANG_TH)
    # 기존 파일과 같은 CRLF 줄바꿈으로 저장한다.
    with open(LANG_TH, 'w', encoding='utf-8', newline='\r\n') as stream:
        stream.write(json.dumps(document, ensure_ascii=False, indent=2) + '\n')
    (OUT / 'applied.json').write_text(json.dumps(log, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')
    sys.stdout.reconfigure(encoding='utf-8')
    for row in log:
        print(row['id'], row['previousRect'], '->', row['rect'], 'rmse', row['opaqueRmse'])


if __name__ == '__main__':
    main()
