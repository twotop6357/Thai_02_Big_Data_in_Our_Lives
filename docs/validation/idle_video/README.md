# IDLE-VIDEO-001 — 타이틀 배경 영상 c02 반복 재생

2026-11-10 Asia/Seoul · Claude 단독 구현·검증 · 앞선 작업 미커밋 보존. 커밋/push/배포 빌드 없음.

## 동작

- 사용자가 추가한 `Assets/Resources/Videos/c02.mp4`(1920×1080, 4초, 30fps, 오디오 없음, 트랜스코딩 없음 — 기존 영상과 같은 설정)를 타이틀 배경으로 무한 반복 재생한다. 소리는 내지 않는다.
- 0_Idle `ImageUI` 순서: `Intro_bg`(정지 배경) → **`IntroVideo`**(RawImage+VideoPlayer+`IdleVideoBackground`) → `NSMLogo` → 문구(`LocalizedImages`) → 블록 제거 팝업. 로고·문구·팝업은 영상 위에 그려진다.
- 첫 프레임이 디코딩될 때까지 영상 이미지를 숨겨 정지 배경이 보이고, 재생 오류 시에도 정지 배경으로 남는다(영문 오류 로그).
- 렌더 텍스처(1920×1080)는 타이틀 진입 때 만들고 떠날 때 해제한다. 첫 프레임 알림만 받고 이후 프레임 콜백은 끈다(반복 할당 없음).
- 작성: `Assets/Editor/IdleVideoAuthoring.cs` `Configure()`(씬 변경은 추가만). `ExhibitionUIBuilder` 타이틀 재작성도 같은 함수를 호출한다. 이때 Unity의 가짜 null 때문에 `GetComponent() ?? AddComponent()`가 동작하지 않는 것을 발견해 이 파일과 `LogoAuthoring`·`TitleBlockPopupAuthoring`을 `TryGetComponent`로 고쳤다.

## 실제 검증

Unity 2022.3.62f3 / Windows Editor. `runtime.json` **9항목 PASS**: 첫 프레임 표시·재생·반복·무음, 형제 순서, 13초 동안 반복 3회, 영어 전환 중 계속 재생, 블록 팝업 아래에서 재생, 타이틀의 렌더 텍스처 1개, 시작 버튼 정상, 타이틀을 떠나면 해제(0개), 홈 복귀 시 다시 재생(1개). `idle_th/en.png`, `idle_popup_en.png`, `runtime_contact.png` 직접 확인. 콘솔 오류 0.

## 이후 변경

IDLE-LOOP-001(2026-11-11)에서 루프 지점 튐을 고치려고 c02를 재인코딩하고 `isLooping` 대신 플레이어 2개 교대 재생으로 바꿨다. 렌더 텍스처는 타이틀에서 4개(영상 2개·확인용 1×1 2개)다. `Intro_bg`는 영상 첫 프레임으로 다시 만들었다. 상세는 [TITLE_VIDEO](../../TITLE_VIDEO.md).

## 미수행

Windows Player 재빌드/배포(대상 PC의 영상 디코딩 성능·반복 이음새 육안 확인 포함), 장시간 반복 재생.
