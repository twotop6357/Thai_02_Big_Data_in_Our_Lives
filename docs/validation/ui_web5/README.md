# UI-WEB-005 / AUDIO-002 — 태국어 폰트 변경 적용, 인식·언어 효과음, 30초 복귀 확인

2026-11-06 Asia/Seoul · Claude 단독 구현·검증 · main/60d397e + 기존 미커밋(ProjectSettings preloadedAssets 1줄, 사용자 오디오 2개) 보존. 커밋/push/배포 빌드 없음. 교차 검증 없음.

## 원본과 다운로드

Figma MCP 없이 사용자가 띄운 웹에서 직접 받았다.

- `browser_export.zip`: `02. Big Data in our lives_ UI (TH)/(En)` 두 섹션을 선택해 File → Export로 받은 84개(PNG 79·PDF 5). `download/NN.png`, `zip_entries.json`에 원래 이름. 웹 레이어 메모: 태국어 폰트 Inter → **Noto Sans Thai**, Berlin Sans FB Demi → **Futura 100 THA**.
- `extra/group_slot_th_Frame351.png`: 위 ZIP에 없는 태국어 그룹 칸(424×424). 임시 1x PNG 내보내기 설정을 추가해 받고 바로 지웠다.
- `extra/selection_slots_th_browser_export.zip`(+풀린 PNG 3개): 태국어 제품 칸·색 칸 2개. 같은 방식(받은 뒤 설정 삭제). 현재 에셋과 **픽셀 동일**(이미 Futura 100 THA)이라 교체하지 않았다.
- 원본 Figma 파일은 임시 내보내기 설정 추가/삭제 외에 수정하지 않았다.

## 감지 방법

`zip_match.json`/`fuzzy.json`: 새 PNG를 현재 Assets PNG 전체와 이전 웹 ZIP(ui_web4·ui_slots)에 픽셀 비교. 영어 요소는 전부 동일(최대 RMSE 0.18, 강한 차이 픽셀 0)이고 안내 2개만 문구가 바뀌었다. 태국어 텍스트 요소는 모두 달랐다. 새/이전 태국어 전체 프레임의 변경 영역 격자 비교로 ZIP에 없는 그룹 칸 글자 변경도 찾았다. 추천 상품 상자·선택 배경·보드는 변경 없음.

## 적용 (`Tools/UI/apply_web5_assets.py`, 결과 `applied.json`, 원본 `before/`)

웹 원본 바이트를 그대로 복사했고 기존 PNG의 meta/GUID는 유지했다.

| 대상 | 내용 |
|---|---|
| 태국어 UI 14개 | 타이틀/부제/버튼 안내(4화면), 종료 안내, 영상 좌상단 안내. 새 폰트로 크기가 바뀌어 `located.json`(전체 프레임 안 위치를 불투명 픽셀 매칭)의 좌표로 `Lang_Th.json` 갱신 |
| 태국어 그룹 칸 | `GroupSlot_th` 교체, 튕김용 `GroupSlotLabel_th`를 기존 분리 알고리즘으로 재생성(복원 오차 ≤3 검사 통과) |
| 태국어 블록 16개 | `Assets/UI/Blocks/ThaiBlocks`와 `StreamingAssets/BlockArtwork/Images/th` 양쪽 교체. 웹의 빨강 중복 1개는 미사용 |
| 태국어 연령 블록 4개 **신규** | นักเรียน/เด็ก/ผู้สูงอายุ/วัยทำงาน → `Student/Child/Senior/WorkingAdultBlock_Thai.png` 양쪽 추가. `RfidBlockAuthoring.ConfigureArtwork()`로 카탈로그 thaiSprite 연결, `blocks_th.json` 경로 변경. 이전에는 태국어 모드에서 영어 연령 이미지가 보였다. 252×252(영어 연령은 261×261)이며 칸에 비율 유지로 표시된다 |
| 영어 안내 2개 | 그룹/선택 화면 "Continue" → "continue", 902→896. 웹 박스 좌표(X 518.5, Y 904, 884×136)와 모든 안내의 중심 966 규칙으로 x 518, y 899 |

`ExhibitionUIAssets`의 기본 사본(defaultThai/defaultEnglish)을 새 JSON으로 갱신. 씬·프리팹·스크립트 참조 변경 없음.

## 효과음 (AUDIO-002)

`AudioManager`에 `PlaySelected`·`PlayLanguage` 추가(Resources/Audios/selected·language, 사용자 파일 수정 없음).

- `language`: 타이틀에서 언어 전환 길게 누름이 받아들여질 때 1회.
- `selected`: 블록 화면(그룹·선택)에서 리더가 새 카드를 받아들였을 때(빈 칸→카드, 다른 카드로 교체). 그룹 우선 규칙으로 거부된 카드, 같은 카드 반복 보고, 제거, 생성 중 잠금, 결과/광고 화면은 소리 없음. 한 프레임에 여러 카드가 들어오면 1회.
- 그룹 화면에서 비그룹 카드로 자동 진행될 때는 `selected`와 전환 `confirm`이 함께 난다(사용자 미확정 기본값).

## 30초 무입력 복귀 확인

정상 동작, **코드 수정 없음**. 블록 없는 블록 화면에서 마지막 활동 뒤 정확히 30.0초에 Idle로 복귀하고 `home`이 재생됐다. 대기 중 Input System 이벤트 0건. 이전 기록의 54~104초 지연은 대기 중 PC 입력 이벤트 때문(CHECK-RFID-TIMEOUT). 설치 PC에서 입력 장치가 계속 신호를 보내면 복귀가 늦어질 수 있는 위험과 그에 대한 사용자 결정 대기는 그대로다.

## 실제 검증

Unity 2022.3.62f3 / Windows Editor / Game 1920×1080. 컴파일 오류 0(기존 CS4014 경고 4). `runtime.json` **60항목 PASS**(`Tools/UI/verify_web5_runtime.cs`): 양언어 바인딩 스프라이트, 언어음 2회, 인식음 조건 6개, 태국어 Student 블록, 30초 복귀/home. 스크린샷 `idle_th`·`group_th`·`selection_full_th`·`group_empty_th/en`·`idle_en`·`selection_group_only_en`, 모음 `runtime_contact.png`를 직접 확인했다. 최종 콘솔 오류 0, Play 중지/0_Idle.

## 미수행

Windows Player 재빌드/배포(기존 배포본은 이전 리소스), 실제 RFID 리더·버튼, 스피커 청취/현장 음량, 영상 전체, 장시간. 결과 화면 이후 흐름은 이번에 다시 돌리지 않았다(리소스 교체만 해당).
