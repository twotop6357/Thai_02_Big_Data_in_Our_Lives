# 블록 시뮬레이터와 결과 이미지 연결

## 현재 상태 (2026-10-28 / COLOR-001)

ComfyUI 연동을 종료하고 로컬 선화 색칠을 연결했다. 나잇대·물품별 흰 원본 PNG와 두 영역 마스크로 선택 색상을 합성하며 검정 외곽선·명암·알파를 보존한다. 네 블록을 인식하거나 변경하면 자동 처리한다. 에디터의 Apply colors / Retry 및 Cancel coloring으로 재시도/취소할 수 있다. GenerateImage()는 로컬 색칠 진입점이다. 서버·AI 모델은 필요 없다. [이미지/마스크/색상 교체 방법](COLORING.md).

Idle의 언어 선택(숫자1 단일=Thai, 빠른2회=English), Game/Result 연결, Idle 외 씬5초 유지 복귀 및 무입력 복귀는 유지한다. Result는 실제 이미지가 제공되어야 이동할 수 있다. 이전 서버 상태 라벨은 비활성 ImageGenerationStatusText로 보존하고 서버 문구/컴포넌트를 제거했다.

## 에디터에서 블록 선택

Game에서 숫자2=Group, 3=Product, 4=Color1, 5=Color2. 키를 누르면 목록을 표시하고 클릭으로 인식한다. 같은 키를 다시 누르면 인식을 해제한다. 숫자패드도 지원한다. 선택값은 SelectedOptionsGroup의 각 OptionText에 반영한다. 에디터 시뮬레이터는 BlockExperienceController.enableEditorKeyboard로 끌 수 있다.

| 블록 | 값 |
|---|---|
| Group | Child, Student, Working Adult, Senior |
| Product | Bags, Shoes, Hats, Sunglasses, Headphones, Tops, Bicycles, Tumblers, Wristwatches, Phone Cases, Umbrellas, Equipment |
| Color1 / Color2 | Black, White, Blue, Red, Green, Yellow |

Equipment 화면 이름에 robots/keyboards/laptops는 노출하지 않는다. BlockColoring.json에서 나잇대별 외형을 지정하므로 색상 변경 시 물품 외형이 유지된다. 카탈로그/프롬프트 데이터와 CreatePrompt()는 기존 데이터로 보존했으며 현재 색칠에는 사용하지 않는다. 포함된48종은 전시용 최종 삽화로 교체 가능한 기하학 선화 샘플이다.

## 설정·선택 기록

- Assets/StreamingAssets/BlockGeneration/BlockCatalog.json: 블록 목록·레이블·향후 활용 가능한 프롬프트 데이터.
- 같은 폴더 BlockSettings.json: maxJournalFiles=30, maxJournalMegabytes=4. 서버 URL·실행 경로·워크플로는 없다.
- 같은 폴더 BlockColoring.json: 나잇대·물품별 원본/마스크 PNG 경로와 색상 RGB. Coloring/Samples에48쌍의 기본 샘플.
- 비동기 로드 후 IsReady=true가 된다. 설정 누락/오류는 영문 오류와 초기화 실패 안내로 드러나며 서버 대기 상태로 들어가지 않는다.

분석용 이벤트는 Application.persistentDataPath/BlockData/blocks_*.jsonl에 기록한다. 공통 schema_version/event_id/session_id/sequence/timestamp_utc/event_type/data, session_started/session_ended, block_recognized/block_removed 이벤트와 슬롯·선택값·revision·언어를 보존한다. coloring_completed에는 선택값/revision/확정물품/원본·마스크 경로/두 RGB/결과크기를 기록한다. 새 generation_* 이벤트는 생성하지 않는다. 과거 JSONL은 그대로 두며 일반 보존 한도는 계속 적용한다.

기본 파일당4MiB/최대30개, 오래된 blocks_*.jsonl부터 순환 삭제, 대기128개 한도·직렬 비동기 쓰기는 유지한다. 영구 분석 데이터는 별도로 수집/백업해야 한다. 강제 전원 차단 시 마지막 줄과 session_ended는 보장하지 않는다.

## 로컬 색칠과 결과 연결

BlockExperienceController.TryDisplayResult(Texture2D texture, int selectionRevision)는 메인 스레드에서 사용한다. 요청 시작 시 Selection.Revision을 저장하고 화면/세션 수명에 맞게 호출자가 작업을 취소해야 한다. 블록 미완성·선택 변경·화면 종료·씬 전환 중 결과는 false로 거부한다. false면 텍스처 소유권은 호출자에게 있고 호출자가 해제한다. true면 컨트롤러/매니저가 소유하며 호출자가 파괴하지 않는다.

성공 시 실제 텍스처 크기로 Sprite를 만들어 ResultImage에 할당한 후 활성화하고 TextGroup을 숨긴다. GameManager가 텍스처를 인수하면 PressButtonText를 표시한다. 짧은 버튼 입력→2_Result에서 같은 텍스처, Idle 복귀 시 자원을 해제한다. LocalBlockColoring은 최대2048px PNG와 같은 크기의 마스크를 읽어 픽셀 계산을 작업 스레드에서 수행한다. 파일 누락/손상/마스크 오류는 원본 예외와 영문 상태로 드러나고 재시도할 수 있다. 결과 이미지를 디스크에 누적 저장하지 않는다.

색칠 작업은 선택/화면/세션 수명 토큰으로 취소하며 늦은 결과를 표시하지 않는다. 타이머는 색칠 중에도 계속 동작하며 Pause하지 않는다.

## 보관한 이전 구현

ComfyUI 전용 소스/설정/워크플로/ControlGuides/메타/보관 테스트는 Tools/ComfyUI/legacy_runtime/Assets에 옮겼다. Tools 전체는 Unity 컴파일·Player 배포 대상이 아니다. 이전 개발 도구·모델 manifest·검증 자료도 과거 기록으로 보존한다. 설치된 ComfyUI/외부 모델/기존 생성물·실행 중 서버를 삭제하거나 종료하지 않았다. 앱은 기존 서버 이미지도 더 이상 자동 삭제하지 않는다.

현재 검증은 [HANDOFF](agents/HANDOFF.md)의 COLOR-001 기록을 따른다. 과거 ComfyUI 생성 시험은 현재 동작의 검증으로 사용하지 않는다.
