# UI-IDLEPOP-001 — 타이틀 화면 블록 제거 팝업

2026-11-09 Asia/Seoul · Claude 단독 구현·검증 · main/60d397e + UI-WEB-005/AUDIO-002 미커밋 보존. 커밋/push/배포 빌드 없음. 교차 검증 없음.

## 원본

Figma MCP 없이 웹에서 받았다. 2번 프로젝트 TH/En 섹션 사이 최상위 `Group 149`(1394×674) = `Rectangle 11` + 영어 문구 "Please remove all the cube from the Center board to Start." 태국어판은 없다.

- `browser_export.zip`(`download/`, `zip_entries.json`): 두 자식 레이어에 디자이너가 넣어 둔 1x PNG 설정으로 받음. `Rectangle 11`은 기존 `VideoEndPopup.png`와 픽셀 동일 → 재사용. 문구 1197×159는 `Assets/UI/UIs_Common/IdlePanel/TitleBlockPopupText.png`로 바이트 그대로 추가.
- `group149_composite.png`: 문구 위치 확인용으로 그룹에 임시 1x PNG 설정을 넣어 받고 바로 지움. 문구는 팝업 안 (98, 260) → 화면 (361, 463).

## 동작

- 타이틀 화면에서 리더 위에 등록된 블록이 하나라도 있으면 팝업을 띄우고, 모두 빠지면 닫는다. 언어와 관계없이 같은 이미지.
- 연출은 영상 종료 팝업과 같은 `UIPopupPresentation`: 회색 배경(5B5B5B 80%) 페이드 후 카드 등장, 닫을 때 역순(약 0.56초).
- 팝업 요청부터 닫힘 연출이 끝날 때까지 `Presentation.LockInput()`으로 버튼 입력(시작·언어 전환)을 막는다. 잠글 때와 풀 때 버튼 상태를 초기화하므로 누르고 있던 버튼은 한 번 떼야 인식된다.
- 화면 전환 중에는 띄우지 않고, 타이틀 등장이 끝난 뒤 판단한다.

### 실제 블록 존재 기록 (RfidReaderHub)

기존 세션 상태(`Readers`)로는 판단할 수 없어 별도 기록 `HasPresentBlocks`/`ReportPresence`를 추가했다.

- TCP 리더는 카드 UID가 **바뀔 때만** 보고한다. 홈 복귀 때 허브가 세션을 비우면 보드에 남은 카드는 다시 보고되지 않는다.
- 타이틀에서는 그룹 우선 규칙 때문에 제품·색 카드가 세션에서 무시된다.
- 그래서 `RfidTcpReaders`가 카드 변화마다 규칙 판단 전에 존재를 보고하고, 허브는 홈 복귀(세션 초기화) 때 존재 기록을 유지한다. 실제 제거(`RemoveReader`), F9 전체 지우기(`ClearReaders`), F9 드래그(`MoveReader`)는 기록도 갱신한다. 규칙 때문에 내부에서 세션만 비우는 경우는 기록을 지우지 않는다. 미등록 카드는 블록으로 세지 않는다.
- 세션 인식 규칙(그룹 우선, 생성 중 잠금, 홈 후 남은 카드 재생 안 함)은 바꾸지 않았다.

## 작성

`Assets/Editor/TitleBlockPopupAuthoring.cs` `Configure()`로 0_Idle에 `ImageUI/TitleBlockOverlay/PopupContent/{PopupFrame, TitleBlockPopupText}` 추가, ImageUI에 `UIPopupPresentation`·`TitleBlockPopup` 연결. 씬 diff는 추가만(삭제 0). `ExhibitionUIBuilder`의 타이틀 재작성도 같은 함수를 호출한다.

## 실제 검증

Unity 2022.3.62f3 / Windows Editor / Game 1920×1080. `runtime.json` **25항목 PASS**(`Tools/UI/verify_title_block_popup.cs`): 빈 보드 무팝업, 그룹 카드 팝업, 팝업 중 짧게 누름 시작 안 됨·길게 누름 언어 안 바뀜(양언어), 그룹 우선으로 무시된 제품 카드도 팝업, 하드웨어 경로 존재 보고, 마지막 카드 제거 시 닫힘·입력 복구, 미등록 카드 무팝업, 빠른 놓기/빼기 반복, 비운 뒤 시작, 게임 중 카드를 둔 채 홈 복귀 시 팝업, 영어 동일 이미지. 블록은 허브 API로 넣었다(실제 TCP 리더 아님).

- 첫 실행에서 최초 Play 시작 시 GameManager가 아직 없어 컴포넌트가 꺼지는 문제를 발견해 지연 조회로 고친 뒤 재실행했다.
- `popup_th.png`·`popup_en.png`·`popup_contact.png` 직접 확인. 웹 합성본과 불투명 픽셀 RMSE 2.76.
- EditMode **139/139 PASS**. 콘솔 오류 0. 경고: 의도한 미등록 카드 1건, TCP 10123 AddressAlreadyInUse(다른 Unity 인스턴스가 포트 사용 중, 이번 실행은 실제 소켓 미시험).
- Play 중지/0_Idle.

## 미수행

Windows Player 재빌드/배포, 실제 RFID 리더(실제 UID 변화 보고 경로)·물리 버튼, 영상/결과 이후 전체 흐름 재실행(verify_slots_flow 미실행), 장시간.
