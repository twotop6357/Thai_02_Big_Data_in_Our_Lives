# 이미지 UI와 언어별 배치

2026-11-05 UI-SLOTS-002(최신): 그룹 화면을 보드 504×504·칸 424×424 하나로 바꾸고, 선택 화면 네 칸을 용도별 빈 칸 이미지로 교체했다. 칸·보드·선택 배경도 언어 JSON 바인딩(`keepObjectName`)으로 관리한다. 아래 UI-WEB-004의 "기존4칸 유지"는 이 작업으로 대체됐다. [원본 ZIP·검증](validation/ui_slots/README.md).

2026-11-05 UI-WEB-004: 열린 Figma 웹의 2번 TH/EN UI를 직접 ZIP으로 받아 일괄 비교하고 PNG15개 교체·영상 좌상단 홈 안내2개를 추가했다. 파란 버튼/연령 제목/태국어 타이틀/선택 강조/종료 줄 간격·언어별 배치를 반영했다. 기존4칸/이동연출·입력/영상 동작은 보존했다. `video.home`는 실제 영상 프레임 표시 때만 나타나고 종료·실패·취소·복귀 때 숨긴다. [원본 ZIP·비교·검증·적용 화면](validation/ui_web4/README.md).

2026-11-02 UI-NEW-003: 사용자가 제공한 NEW의 인트로 배경1개·영어/태국어 버튼 안내8개를 기존 PNG에 덮어썼다. 기존 파일명/meta/GUID/참조를 유지했고 모든 이미지 크기가 같아 씬/JSON/코드·연출은 수정하지 않았다. 교체와 양언어8화면 확인 후 NEW 폴더/meta를 삭제했다. [대응표와 실제 화면](validation/ui_new3/README.md).

2026-10-30 / COMFY-RESTORE-001: 로컬 자동색칠을 ComfyUI 생성으로 복원했다. 현재 물건10종/실제 블록 표시를 유지하고,6색은 블록 PNG 원의 HEX/RGB를 사용한다. 생성 중 두 선택 패널의 계속 안내·모든 버튼을 차단하며 성공/실패/취소 후 복구한다. 아래 과거 색칠 언급은 이 조건으로 대체한다. 배치·이미지·영상 연출은 유지한다. [현재 운영](BLOCK_SIMULATOR.md).

## 기준

Figma [Big Data in Our Lives](https://www.figma.com/design/Nq9j1gdpFVxXnDlv8wGqna/?node-id=0-1)의 TH/EN 10개 화면을 2026-10-29에 읽었다. 프레임은 모두1920×1080이다. 안내 이미지는 글자·그림자의 실제 `absoluteRenderBounds`를 사용한다. 원본 경계와 렌더 경계는 [좌표 기록](validation/ui/figma_geometry.json)에 있으며 프로젝트의 PNG28개 크기는 이 렌더 크기와 래스터 반올림1픽셀 이내로 일치한다.

최신 적용(UI-REFRESH-002): 사용자가 제공한 `2_new`의1920×1080 언어별 가이드8장과 새 요소24개를 기존 이름/GUID로 반영했다. 두 언어의 `idle.help`는 x307/y879/1315×173으로 동일하다. 그룹 보드와 네 칸은7px 위로 이동했다. 이 새 가이드가 이전 UI-POS-001의 x514.81787/y840.08301을 대체한다. 영상 화면 이후의 이미지/위치/기능은 유지했다. [최신 매핑·검증·정리 기록](validation/ui_refresh2/README.md).

배경/제목/부제/안내/보드는 프로젝트에 제공된 이미지를 재사용한다. 가이드의 예시 Student를 고정 배경으로 넣지 않는다. **블록 칸 프레임에는 선택 틴트·색상 견본을 칠하지 않는다.** 그룹/선택 화면의8개 칸은 같은 FrameImage 원본과 흰색 Image.color를 유지한다. BlockContent에는 실제 리더가 인식한 블록 이미지를 표시하고 미인식 때 비운다. 선택한 두 색상은 추천 물품 합성에 사용한다.

관람객 UI의 `ImageUI` 아래에는 TMP/UI.Text 컴포넌트가 없다. 배치 단계의 계속 안내는 필수 블록 인식 후에만 표시하며, 연령 단계는1개·물품/색상 단계는4개 기준이다. 부족한 동안 짧은1 입력은 진행하지 않고5초 홈은 유지한다. 기존 블록 선택 카탈로그·자동색칠·결과 텍스처 전달·취소·익명 저널을 유지한다. 개발용 시뮬레이터 목록/진단은 에디터에서만 표시한다.

## 화면과 공통 좌표

아래 x/y는 화면 왼쪽 위에서 시작한다. Unity는 같은 좌표를 Top Left 앵커와 `(x,-y)`로 표시한다. CanvasScaler 기준1920×1080, Match0.5다.

| 요소 | x | y | 너비 | 높이 |
|---|---:|---:|---:|---:|
| 연령 선택 보드 | 636 | 274 | 648 | 615 |
| 연령 칸 좌상 / 우상 | 690 / 968 | 316 | 262 | 262 |
| 연령 칸 좌하 / 우하 | 690 / 968 | 596.5 | 262 | 262 |
| 물품·색상 전체 보드 | 207 | 281 | 1507 | 597 |
| 물품 칸 좌상 / 우상 | 268 / 536 | 320 | 252 | 252 |
| 물품 칸 좌하 / 우하 | 268 / 536 | 588 | 252 | 252 |
| 언어별 추천 물품 보드 | 816 | 331 | 838 | 493 |
| 추천 결과 보드 | 461 | 320 | 998 | 537 |
| 영상 종료 팝업 | 263 | 203 | 1394 | 674 |

언어별 제목·부제·버튼 안내·완료 안내의 실제 좌표와 크기는 JSON을 따른다. 생성 물품 이미지는 보드 안에서 종횡비를 보존한다. 영상은 화면 전체에 비율을 유지해 표시하고 종료 때 마지막 프레임 위에 어두운 전체 오버레이와 안내 팝업을 표시한다.

## 이미지와 영상 교체

- `Assets/StreamingAssets/Lang_En.json`, `Lang_Th.json`: 같은15개 `images[].id`에 언어별 `sprite`·`x`·`y`·`width`·`height`를 지정한다. `figmaNode`는 출처이며 런타임에서 외부 Figma를 호출하지 않는다. `video`는 Resources 기준 확장자 없는 `Videos/Video_en` / `Videos/Video_th`다.
- `Assets/UI/ExhibitionUIAssets.asset`: Sprite ID와 실제 Sprite 참조 목록. 기존 PNG를 같은 경로에서 교체하면 GUID를 유지한다. 새 PNG를 추가하면 Sprite(2D and UI), Single, mipmap off, NPOT None, alpha transparency, Clamp를 설정하고 라이브러리에 ID와 참조를 등록한다. JSON에서 그 ID를 사용한다. Background는 언어 페이드 대상이 아니다.
- `LocalizedImageSet.bindings`: 화면의 고정 UI ID와 Image 연결. 같은 JSON에서 다른 언어 이미지와 위치·크기를 함께 바꾼다. 언어 페이드는 공통 UIMotion의0.25초 InOutSine을 사용한다. 실제 언어 모드 변경은 GameManager가 타이틀에서만 허용한다. 화면·팝업·카드·입력 잠금은 [공통 연출 규칙](UI_MOTION.md)을 따른다.
- `ExhibitionUIAssets.defaultThai/defaultEnglish`: 마지막 에디터 구성 시의 정상 JSON 사본. 외부 JSON 누락·문법/스키마·참조 오류에는 원본 예외를 영문 로그에 남기고 이 사본으로 두 언어를 복구한다. 수정한 JSON을 운영 기본으로 확정하려면 이 사본도 갱신한다.
- `ExhibitionUIBuilder.Build()`는 세 씬의 Canvas UI를 다시 작성하는 명시적인 제작 도구다. 런타임에는 실행하지 않는다. Inspector로 조정한 UI를 재작성하므로 필요할 때 변경을 보관하고 실행한다. PNG 원본과 GUID는 유지하며 UI importer와 에셋 라이브러리·기본 JSON 사본을 갱신한다. 일반적인 이미지/JSON 교체에는 씬 재작성이 필요 없다.

예:

```json
{
  "id": "idle.subtitle",
  "sprite": "Subtitle_en",
  "x": 98.83252,
  "y": 466.74414,
  "width": 677.85107,
  "height": 88.4375,
  "figmaNode": "20:352"
}
```

PNG에 그려진 문구는 제공된 원본 그대로다. UI-NEW-003의 초기 안내는 **한 번 눌러 시작 / 2초 유지로 Thai↔English 전환**으로 갱신돼 실제 버튼 동작과 일치한다. 그룹/배치/결과 안내의 홈 유지 문구도 새 이미지를 사용한다. 안내 문구를 바꾸려면 해당 PNG를 같은 이름으로 교체한다.

## 영상과 종료 책임

ResultVideoController는 결과 화면의 짧은 입력을 받아 현재 언어 JSON의 영상을 준비한다. 화면/세션 토큰을 연결하고 준비·재생에 시간 제한을 둔다. 영상 중만 타이머를 Pause하고 정상 종료·실패·취소·비활성화·홈 복귀에서 Resume한다. 실패 후 짧은 입력은 재시도,5초 유지는 홈이다. 정상 종료 후 마지막 프레임/클립은 팝업 배경용으로 화면이 소유하며 홈/비활성화에서 해제한다. RenderTexture는 `ExhibitionVideo`라는 이름으로 소유권을 구분한다.

영상 준비 중에는 기존 광고를 불투명하게 유지하고 영상 RawImage를 비활성화한다. `isPrepared` 이후에도 `frameReady`의 실제0번 프레임을 기다린다. 숨겨진 상태에서0번 프레임의 RenderTexture 쓰기 완료를 기다린 뒤, 광고와 영상을 같은 프레임에서 불투명하게 교체한다.0번 프레임을 한 렌더 프레임 표시한 즉시 재생·오디오를 재개하므로 시작 부분이 페이드 중 지나가거나 멈춘 화면으로 남지 않는다. 영상 진입에는 패널 페이드를 적용하지 않는다. 준비 중 오디오를 임시 음소거하고 시작 후 원래 음소거/프레임 드롭 설정을 복원한다. 실제 재생 후에는 매 프레임 콜백을 해제한다.

첫 프레임 준비 실패나 재생 오류에는 광고를 먼저 복원하고 영상 RawImage를 끈 다음 기존 팝업을 표시한다. 빈 RenderTexture·흰 RawImage·카메라 Skybox를 실패 배경으로 사용하지 않는다. 취소·홈·화면 종료에서도 영상 이미지를 먼저 끈 뒤 자원을 해제한다. 첫 프레임/연속 진행/화면 덮임 검증은 [video_first_frame.json](validation/ui/video_first_frame.json), 기존 UI/영상 회귀는 [video_first_ui_regression.json](validation/ui/video_first_ui_regression.json)을 참조한다.

완료 팝업에서는 `SetBlockPresent(0..3,false)`로 네 블록 제거를 확인해 홈으로 이동한다. 실제 장비 연결은 후속이다. 에디터 숫자2/3/4/5가 제거를 모의하며, 짧은 버튼 입력 또는5초 홈도 복구 경로다. 영상 없이 Result 씬만 직접 실행하는 방식은 유효 세션이 아니므로 Idle부터 시작한다.

버튼 API와 설정은 [BUTTON_INPUT](BUTTON_INPUT.md), 실행 증거는 [runtime.json](validation/ui/runtime.json)과 같은 폴더의 실제 화면 PNG를 참조한다. 영상 종료 검증은 디코더 재생 후 끝부분으로 이동해 종료 이벤트를 확인한 것으로, 두 영상 전체 길이의 연속 시청 검증을 대신하지 않는다. Windows Player/설치 PC/실물 버튼·RFID/장시간 검증은 별도로 수행한다.
