# UI-WEB-006 — 디자이너의 태국어 문구 수정 반영

2026-11-10 Asia/Seoul · Claude 단독 · 앞선 작업 미커밋 보존. 커밋/push/배포 빌드 없음.

## 원본

LOGO-SPLIT-001 확인 때 두 02 섹션을 기존 내보내기 설정으로 다시 받은 `../logo_split/after_browser_export.zip`(84개)을 원본으로 썼다(`download/`, `zip_entries.json`). Figma는 수정하지 않았다.

## 감지 (`changed.json`)

현재 프로그램 PNG와 보이는 픽셀(알파 곱셈 후) 기준으로 비교했다.

| 요소 | 차이 | 판단 |
|---|---|---|
| 태국어 영상 종료 문구 `VideoEndPopupText_th` | 30,238px | 오탈자 수정("น ำ"→"นำ") — **교체** |
| 태국어 선택 화면 버튼 안내 `ButtonHelpBox_selectall_th` | 2,991px | 문구·배치 수정 — **교체** |
| 그 밖의 버튼 안내 6개, 영어 타이틀 | 2~23px, 최대 19 | 같은 원본을 다시 렌더링한 잡음 수준. 미교체 |

두 요소의 화면 위치는 최신 프레임에서도 같아(593,899 / 447,438, `located.json`) Lang JSON은 바꾸지 않았다. 바이트 그대로 복사, meta/GUID 유지, `before/` 백업(`applied.json`).

## 검증

Unity Editor Play `runtime.json` 4항목 PASS: 태국어 선택 화면(블록 4개, 안내 표시)과 실제 영상 끝 seek 후 종료 팝업에서 새 이미지 표시. `runtime_contact.png` 직접 확인. 콘솔 오류 0. Player 재빌드/실장비 미수행.
