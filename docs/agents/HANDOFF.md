# HANDOFF — Codex·Claude 공동 인수인계

**두 에이전트 모두 작업 전 현재 상태·담당 표·최신 기록을 반드시 읽는다.** 작업 재개·인계·컨텍스트 축약 후에도 다시 읽는다. 규칙은 [AGENTS.md](../../AGENTS.md)와 [개발 기준](../DEVELOPMENT_GUIDELINES.md)을 따른다. 외부 `handoff` 스킬은 필요하지 않다.

## 현재 상태

- 마지막 갱신: 2026-10-18 (Asia/Seoul), Codex, COMMIT-001.
- 현재 작업: AI-001 인계 대기 — ComfyUI 실행 가능한 기본 워크플로 작성. 서브컬처 모바일게임 아이템 화풍의 일관성 및 대상 GPU 검수는 미완료.
- AI 구성: SDXL Base 1.0 / 내부 768 / 출력 480 / 16 steps / dpmpp_2m karras. 실제 측정 장비는 RTX 5070 Ti Laptop 12GB. RTX 3060 20초 충족 미검증.
- 최신 작업: 4입력 프롬프트 매핑, 표준 노드 UI/API JSON, 모델 해시 확인 도구, 측정 도구와 실제 4회 결과를 저장. 이미지 품질 실패도 validation에 함께 기록. Unity 생성 API/실시간 프리뷰 연결은 구현하지 않음.
- 현재 브랜치: `main`. FLOW-001/LOG-001/AI-001 및 사용자 씬·폰트·설정 변경은 COMMIT-001 기록을 포함한 커밋에 함께 저장. 실제 해시는 git log -1로 확인한다. 아래 미커밋 표기는 당시 작업 시점의 기록이다.
- 다음 행동: Idle에서 Play해 버튼 동작을 확인할 수 있다. 후속 UI 지역화는 GameManager.CurrentLanguage/LanguageCode에 연결하고 태국어 문구·폰트를 준비한다. Windows Player·실제 Arduino·장시간 검증은 남아 있다.
- 미해결 치명적 장애: 이번 세션에서 발견한 항목 없음. 런타임 검증으로 장애 부재를 확인한 뜻은 아니다. [TroubleShooting](../TroubleShooting.md) 참조.

## 진행 중 작업 / 파일 소유권

| 작업 ID | 담당 | 상태 | 수정 범위 | 다음 담당 / 행동 |
|---|---|---|---|---|
| AI-001 | Codex | 인계 대기 | Tools/ComfyUI, docs/COMFYUI_SETUP.md, HANDOFF | 소유권 해제; 화풍 승인·품질 안정화 및 3060 실측 필요; 미커밋 |
| LOG-001 | Codex | 완료 | GameManager.cs, EnglishDebugLogHandler.cs 및 메타, AGENTS.md, docs/BUTTON_INPUT.md, CHANGELOG.md, HANDOFF | 소유권 해제; 미커밋 |
| FLOW-001 | Codex | 완료 | GameManager, 신규 Input/설정/시뮬레이터 스크립트 및 메타·테스트, 0_Idle, Settings.json, EditorBuildSettings, AGENTS, CHANGELOG, docs/BUTTON_INPUT.md, HANDOFF | 파일 소유권 해제; 미커밋 |
| INIT-001 | Codex | 완료 | Assets/Resources/_Temp 및 메타, Assets/vFolders 및 메타, Assets/vHierarchy 및 메타, URP 렌더러, 기존 공동 지침 문서와 이 HANDOFF의 커밋 | 소유권 해제; 다음 작업 전 실제 HEAD 재확인 |
| DOC-001 | Codex | 완료 | AGENTS.md, CLAUDE.md, docs/DEVELOPMENT_GUIDELINES.md, docs/agents/HANDOFF.md, docs/TroubleShooting.md, TODO.md, README.md | 다음 작업자가 최신 상태 확인 후 새 작업 등록 |

`진행 중` 행의 파일은 다른 에이전트가 편집하지 않는다. 이 표는 원자적 잠금이 아니므로 기본적으로 순차 인계한다. 작업 종료 시 상태를 바꾸고 더 이상 소유하지 않음을 명시한다. 위 완료 작업의 파일 소유권은 해제되었다.

## 현재 프로젝트 확인값

2026-10-18에 파일로 확인한 값이며 다음 작업 시 다시 확인한다.

- remote: `origin https://github.com/twotop6357/Thai_02_Big_Data_in_Our_Lives.git` (fetch/push).
- Unity: `2022.3.62f3`. 빌드 등록 씬: `Assets/Scenes/0_Idle.unity`, `Assets/Scenes/1_Game.unity`.
- Company Name: `MyCompany`; Product Name: `Unity-Startup-Template`; Standalone Identifier: `com.Unity-Technologies.com.unity.template.urp-blank`. 배포 전 프로젝트 값 결정 필요.
- GameManager: GameLifetimeScope의 기존 주입 등록 유지. 단일/이중 입력으로 Thai/English 세션을 만들고 Game 진입, Idle 외 씬 5초 유지 및 타임아웃은 Idle 이동. 수동 로그/타임아웃 분리. 반복 복귀 후 매니저/시뮬레이터 각 1개 검증.
- Settings: COMMIT-001 직전 실제 파일은 무입력 켜짐/30초, 버튼 이중 입력 간격 0.4초, 60 FPS, 포커스 대기/재시도 각 3초, alwaysOnTop true, apiUrl 비어 있음. 사용자 변경 보존; 이전 실행 검증 당시 설정과 다를 수 있음.
- PC 종료: 모든 요일 비활성화, `-s -f -t 45`. 실제 종료 테스트 금지 조건 유지.
- R3: manifest와 NuGet 모두 1.3.1 표기. 실제 DLL 중복/Player 호환은 미검증.

## 보존할 기존 로컬 변경

아래는 DOC-001 시작 전부터 존재한 변경의 기록이다. INIT-001에서 사용자 요청으로 URP 렌더러·vFolders·vHierarchy를 커밋에 포함했다. `.vscode/`와 자동 생성 `.slnx`는 로컬 파일로 남겨 두었다. 다음 작업자는 실제 Git 상태를 다시 확인한다.

- 수정: `Assets/Settings/URP-Performant-Renderer.asset`.
- 미추적: `.vscode/`, `Assets/vFolders.meta`, `Assets/vFolders/`, `Assets/vHierarchy.meta`, `Assets/vHierarchy/`, `Thai_02_Big_Data_in_Our_Lives.slnx`.

FLOW-001 시작 시 사용자 변경: `_TempFont.asset`, `0_Idle.unity` UI, `Settings.json`의 resetTime 30, TMP Settings, vFolders Data, 신규 `1_Game.unity`와 메타. 기존 내용은 보존했다. 이 작업은 Idle에 시뮬레이터를 추가하고 언어 안내 한 곳을 수정했으며 Settings에는 새 간격 필드만 추가했다. GameScene 내용은 수정하지 않았다.

## 기록 방법

현재 상태와 담당 표를 함께 갱신하고 아래 최신 기록 맨 위에 항목을 추가한다. 날짜는 `YYYY-MM-DD HH:mm +09:00` 또는 날짜+시간대, 작업 ID는 중복 없이 쓴다. 커밋 해시가 없으면 미커밋으로 표기한다. 검증은 실제 수행자와 방법을 적는다. 확인하지 않은 상대 에이전트의 승인을 적지 않는다.

```markdown
### [일시 / 시간대] 작업 ID · 담당 → 다음 담당 · 상태
- 요청/목표:
- 기준 브랜치/HEAD, 커밋 여부:
- 변경 파일/핵심 결정:
- 검증: 수행자, 환경, 시나리오/실행 방법, 결과, 증거 위치.
- 미검증/위험/차단:
- 다음 행동/파일 소유권 해제 여부:
- 장애: TroubleShooting 항목 링크 또는 이번 작업에서 발견 없음.
```

## 최신 작업 기록

### [2026-10-18 / Asia/Seoul] COMMIT-001 · Codex → 다음 작업자 · 완료

- 요청: 지금까지 변경사항 커밋. 기준 main / 0e14d8f, 현재 프로젝트 origin 확인. push 요청은 없어 로컬 커밋만 수행.
- 범위: 사용자 Idle/Game 씬·폰트·TMP·vFolders 설정, 버튼 입력/씬 연결 및 테스트, 영문 로그, Settings/빌드 씬, AGENTS/CHANGELOG/기능 문서/HANDOFF, ComfyUI 워크플로·도구·검증 샘플. 이전 FLOW/LOG/AI의 미커밋 내용을 이번 커밋에 포함.
- 현 설정: useInactivityTimer=true, resetTime=30, alwaysOnTop=true. 실제 파일 값으로 보존하고 현재 상태/CHANGELOG에 기록. 과거 false 설정에서의 검증과 구분.
- 제외: 로컬 .vscode 및 자동 생성 slnx, ignored ComfyUI results/references/generated/캐시 및 모델 가중치. 삭제하지 않고 로컬 보존. validation의 4개 샘플은 검증 증거로 포함.
- 검증: ComfyUI Python 테스트4개 재통과. Unity 컴파일/9개 EditMode 및 버튼 실행 검증은 FLOW/LOG의 실제 과거 기록이며 이번 커밋 준비에서 재실행하지 않음. 목표3060/운영 이미지 품질/Windows Player/장시간 미검증 유지. 기존 Unity YAML·vFolders의 공백 경고는 내용 변경 방지를 위해 일괄 포맷하지 않음.
- 파일 소유권 해제. 새 치명적 장애 없음. 커밋 해시는 이 기록을 포함한 git log에서 확인.

### [2026-10-18 / Asia/Seoul] AI-001 · Codex → 다음 작업자 · 인계 대기

- 요청: 그룹·물품·두 색상으로 480×480 생성, RTX 3060 12GB에서 약 20초 목표. 추가 요청으로 굵은 선의 단순 만화보다 서브컬처 모바일게임 아이템 화풍을 지향.
- 기준: main / 0e14d8f. 기존 사용자·FLOW/LOG 변경 보존, 이번 변경 미커밋. 원본 저장소/패키지 수정·push 없음.
- 변경: Tools/ComfyUI의 model/presets, UI/API/선택 JSON 생성기, 해시 검증 다운로드 도구, 측정 도구, 테스트, validation 이미지 4장/JSON 및 docs/COMFYUI_SETUP.md. 현재 SDXL Base 단독, 내부768→480, 16steps CFG6 dpmpp_2m/karras. 그룹은 연령 추정 대신 디자인 테마로 전달; 색상은 제품별 부위에 매핑.
- 실제 환경: localhost:8188 ComfyUI 0.37.0 / torch2.12.1+cu130 / RTX5070Ti Laptop12GB. SDXL 기존 파일의 공식 SHA 일치 확인. DreamShaper8은 내려받아 비교했으나 기본값에서 제외하고 모델 폴더에 그대로 보존.
- 검증: Codex가 실제 서버에서 최종 프롬프트로 워밍업1+4회 생성 및 480PNG 확인. 워밍업7.193초, 본 측정6.533/6.867/6.703/6.967초, 중앙값6.785초. 오프라인 테스트4개 통과(입력2880조합, 그래프 링크, 입력 검증, 프로필). 설치된 object_info와 노드/필수 입력/UI 출력 슬롯 일치 확인. UI 그래프 frontend 시각 로딩은 미검증. ComfyUI 사용자 워크플로 Thai_02_product_480_mobile_game.json에 신규 저장(기존 파일 덮어쓰지 않음).
- 품질: 학생 가방은 채색 일러스트 방향이지만 갈색 장식 추가; 어린이 선글라스는 렌즈 반사에 검정이 섞임. 직장인 가방은 3D 제품 렌더처럼 보여 미달, 노인 선글라스는 인물 등장으로 실패. 성공 예만 남기지 않고 validation/current_pc.json 및 4장에 모두 보존. visual_acceptance_verified=false. 속도 충족을 품질 통과로 해석하지 말 것.
- 비교: 더 강한2D 가중치도 복수 물품·인물·글자 문제가 있었음. img2img 참조는 원래 색을 유지하여 가변색상 기본 구성으로 채택하지 않음. 비교 결과는 ignored results/references 경로에 로컬 보존. 모델 전반 성능 순위로 일반화하지 않음.
- 미검증/다음 행동: RTX3060 실제20회+Unity 동시 실행·장시간·프리뷰 수신, 전체 조합 이미지 품질, cap/sneakers/watch, Unity 요청/취소 연결 미구현. 승인 화풍에 맞는 스타일 모델/LoRA와 구조 제약을 추가 비교해야 운영 품질 승인 가능. 현재는 실행 가능한 시험 구성. Claude 교차 검증 없음.
- 문서/상태: 이번 도구·문서 범위 검증 완료. 저장소 전체 diff 검사에서는 기존 Idle YAML/vFolders의 trailing whitespace가 있어 무관한 변경은 손대지 않음. 신규 치명적 장애 없음. 모든 파일 소유권 해제.

### [2026-10-18 / Asia/Seoul] LOG-001 · Codex → 다음 작업자 · 완료

- 요청: 모든 프로그램 Debug 로그를 영문으로 작성하고 Thai/English 중 어떤 언어로 시작했는지 표시.
- 기준: `main` / `0e14d8f`, FLOW-001 등 기존 미커밋 작업 보존. 이번 변경도 미커밋.
- 변경: GameManager에서 씬 로드 성공 후 `[GameManager] Game started. Language: Thai (th).` 또는 `[GameManager] Game started. Language: English (en).`을 1회 출력. 실패/Idle 복귀에서는 시작 로그를 출력하지 않음. AGENTS의 영문 로그 규칙, BUTTON_INPUT 사용법, CHANGELOG 갱신.
- 로그 점검: Assets/Scripts의 기존 직접 Debug 메시지는 영문. 공통 Template Runtime Reporter의 한글 경고 한 곳 발견. PackageCache 수정 대신 `Assets/Scripts/App/EnglishDebugLogHandler.cs`로 정확히 해당 레거시 문자열만 영문 매핑. 일반 포맷·로그 수준·context·원본 예외 전달을 보존하며 임의 메시지 번역/필터링은 하지 않음. OS/외부 예외 원문은 진단 보존 대상.
- 검증: 신규 스크립트 강제 임포트 및 컴파일 확인. Unity 실행에서 Input System 가상 digit1 키로 Thai/English 각각 진입, Game 로드 후 각 로그 정확히 1회, Idle 복귀 시 추가 시작 로그 없음. Reporter 경고 영문 매핑 및 Debug.LogFormat의 인자 보존 확인. 최종 Console error 0, Game started 로그 2개(각 언어 1개). 스크립트 메타 존재·diff 형식 검사 통과.
- 도구 참고: 초기 if_dirty/scripts 갱신에서는 새 파일이 임포트되지 않아 검증 실패. force/all 및 AssetDatabase.Refresh로 실제 타입/메타 생성 확인 후 재검증. MCP execute_code가 같은 비동기 검증을 중복 시작한 정황(포맷 로그 중복 및 상호 간섭 실패)이 있어 SessionState 실행 가드를 추가한 최종 검증에서 PASS. 제품 코드 결함으로 분류하지 않음.
- 미검증: Windows Player, Domain Reload 비활성 모드, 모든 외부 패키지/OS 동적 예외 문구의 언어까지 보장하는 검증은 미수행. 이번 작업에서 새 치명적 장애 발견 없음.
- 다음 행동: 향후 Debug/프로젝트 로거 메시지는 AGENTS의 영문 규칙 준수. 추가 비영문 의존성 메시지가 발견되면 패키지 직접 수정 없이 프로젝트 호환 계층 검토. 파일 소유권 해제, 커밋/푸시 없음.

### [2026-10-18 / Asia/Seoul] FLOW-001 · Codex → 다음 작업자 · 완료

- 요청: Idle의 버튼 1회=태국어, 빠른 2회=영어로 Game 진입; Idle 외 모든 씬에서 5초 유지로 Idle 복귀; 간격 JSON 설정; 실제 Arduino 대신 숫자 1, Idle 별도 컴포넌트 On/Off.
- 기준: `main` / `0e14d8f`, 미커밋. 실제 구현은 `0_Idle`/`1_Game` 경로 사용. 사용자 씬/폰트/설정 변경을 보존.
- 변경: `GameManager` 세션 언어·비동기 씬 전환 가드·세션 취소 토큰·미디어 정지·타이머 재개·수동 로그 분리. `ButtonInputSettings`, `KeyboardButtonSimulator`, 순수 `ExhibitionButtonGesture`/전용 asmdef와 Editor 테스트 추가. Idle의 `ButtonInputSimulator`는 DontDestroyOnLoad로 유지되며 복귀 시 복제본 제거. Build Settings에 Game 등록. AGENTS의 오래된 복귀 설명 갱신, CHANGELOG와 [사용법](../BUTTON_INPUT.md) 작성.
- 설정: `buttonDoublePressIntervalSeconds` 기본 0.4초, 0.1~2초 범위 밖은 기본값. 시작 시 OnSettingsLoaded 이후 로드하며 공통 Settings 타입 확장 한계로 프로젝트 필드를 동일 JSON에서 한 번 더 읽음. 누락 기본값 0.4 및 음수 거부를 Unity JsonUtility로 확인.
- 단위 검증: Unity 2022.3.62f3 EditMode `Exhibition.Input.Tests` 9/9 통과(job `6650d85fcb7a44828a2c42d0ca87feba`). 단일 대기, 이중 입력과 정확한 경계, 늦은 두 번째 입력, 5초 유지, 놓으면 초기화, 전환 후 release latch, 사용자 간격, Reset 취소.
- 실행 검증 1: Input System 가상 키보드로 실제 digit1/numpad1 상태 이벤트 전달. 단일 Thai/Game, 이중 English/Game, 짧은 유지 무시, timeScale=0에서 5초 Idle, 세션 취소·선택 초기화, 계속 누른 복귀 키로 재시작 방지, 반복 복귀 후 매니저/시뮬레이터 각 1개, 추가 활성 씬에서 복귀, 시뮬레이터 Off 입력 무시 확인.
- 실행 검증 2: 대기 중 시뮬레이터 Off가 단일 선택 취소, 포커스 콜백 상실이 유지 입력 취소, 무입력 이벤트로 Idle 복귀·Pause 해제·세션 취소·수동 이벤트 0회, 중복 수동 요청 시 이벤트 1회 확인. 포커스는 콜백 모의 호출이며 실제 OS 포커스 테스트는 아님.
- 최종 회귀: 80ms 직접 API 주입 smoke에서 진입 확인이 실패하여 재확인. 설정 준비 대기·프레임 동기화된 실제 키보드 경로는 통과. 비활성 MonoBehaviour에도 포커스 콜백이 올 수 있어 키보드 Off 상태에서 다른 어댑터의 입력을 초기화하지 않도록 isActiveAndEnabled 가드 추가. 최종 직접 API 주입+꺼진 시뮬레이터 포커스 상실 모의 호출에서 Thai/Game 진입·중복 수동 복귀 정상 통과. 수정 후 컴파일/Console error 0. MCP 응답 시간 초과가 한 번 있었으나 Unity SessionState의 최종 결과 PASS를 별도로 확인.
- 컴파일: 최종 소스 재컴파일 후 Console error 0. 스크립트/테스트 메타 짝, 문서 링크 및 변경 코드 형식 검사 통과. MCP 재연결 포트 경고만 있었고 기능 오류 없음.
- 미검증: Windows Player 빌드/설치 PC·실제 키보드 수동 조작·실제 Arduino·API 서버 수신 로그·실제 미디어 재생 취소·GC/장시간 구동은 미검증. 타임아웃은 이벤트 주입으로 검증했으며 실제 무입력 30초 대기는 현재 비활성 설정이라 수행하지 않음.
- 경계: 언어 **선택 상태**만 제공, Game 기존 영문 텍스트의 자동 태국어 번역은 구현 범위에 포함하지 않음. 미래 콘텐츠 작업은 SessionCancellationToken 및 화면 수명 토큰을 사용해야 함. 직접 Game에서 Play 시 Idle 시뮬레이터는 없음.
- 다음: Idle에서 Play하여 사용. 시뮬레이터는 Play 전 Idle 오브젝트/컴포넌트, Play 중 DontDestroyOnLoad 원본에서 On/Off. 커밋/푸시 없음, 파일 소유권 해제. 치명적 장애 발견 없음.

### [2026-10-18 / Asia/Seoul] INIT-001 · Codex → 다음 작업자 · 완료

- 요청/목표: 사용자가 추가한 에셋과 초기 개발 준비 상태를 로컬 커밋으로 보존.
- 기준: `main` / `079f50a`. 결과 커밋은 이 기록 포함 커밋(`chore: add editor assets and initial development setup`). 원격 push 없음.
- 변경: vFolders·vHierarchy 에디터 도구와 메타, NanumGothic.ttf 및 TMP `_TempFont`와 메타, 사용자 URP 렌더러 변경, DOC-001 공동 지침 문서 포함. Resources/_Temp는 이름과 무관하게 사용자가 추가한 폰트 에셋이므로 보존.
- 제외: `.vscode/` 로컬 편집기 설정, 자동 생성 `Thai_02_Big_Data_in_Our_Lives.slnx`. 파일 삭제 없음.
- 검증: Codex가 신규 Assets 86개 파일의 에셋/메타 짝, 두 도구의 Editor 전용 asmdef, 신규 50MB 초과 파일 없음, 주요 비밀 패턴 검사(일치 없음), LFS 폰트/PDF 3개 객체의 존재와 SHA-256 확인. diff 검토 완료.
- 형식 검사: staged diff에 Unity 생성 YAML/메타 및 서드파티 원본의 후행 공백 경고 있음. 원본 보존을 위해 일괄 포맷하지 않음. 지침 문서는 별도 형식 검사.
- 미검증: Unity 컴파일·Play·Windows 빌드·폰트 표시·실제 에디터 도구 동작 미수행. 초기 상태 보존 커밋이며 실행 검증 통과를 의미하지 않음.
- 다음 행동: 다음 작업자가 실제 HEAD/변경 사항 확인 후 기능 개발. 파일 소유권 해제.
- 장애: 이번 정적 확인에서 치명적 문제 발견 없음.

### [2026-10-18 / Asia/Seoul] DOC-001 · Codex → 다음 작업자 · 완료

- 요청/목표: Unity 안정성·GC·Codex/Claude 협업 및 치명적 장애 기록 지침 작성.
- 기준: `main` / `079f50a`. 미커밋.
- 변경: 루트 AGENTS 신설, CLAUDE를 공통 문서 진입 안내로 갱신, 개발 기준과 TroubleShooting 신설, 이 문서를 공동 인수인계로 확장. TODO의 과거 담당 제한을 공동 규칙으로 변경하고 README에 진입 링크 추가.
- 결정: 공통 기준은 AGENTS/개발 기준에 유지하고 CLAUDE는 연결만 한다. 기본 순차 인계, 작업 전 최신 기록과 diff 필수 확인, 수명별 취소 및 측정에 근거한 성능 개선을 적용한다.
- 검증: Codex가 실제 저장소·remote·상태·코드·설정과 기존 문서를 확인. 문서 상대 링크, 필수 섹션, 기존 인수인계 기록 보존, `git diff --check`, 변경 범위 확인 완료. 런타임/설정 변경 없음.
- 미검증: Unity 컴파일·Play·Windows 빌드·장비·API·장시간·Profiler 측정 미수행(문서 작업). Claude 교차 검토 미수행.
- 다음 행동: 사용자 구체적 기능 요청에 맞춰 새 작업 등록. DOC-001 파일 소유권 해제.
- 장애: 이번 작업에서 발견 없음. 아래 템플릿 기록은 현재 검증 결과가 아님.

## 보존 기록 — 이전 템플릿 이력

아래 내용은 복사되어 온 과거 Claude→Antigravity 기록이다. 원문을 보존하며, 현재 프로젝트의 컴파일·실행·빌드 검증으로 사용하지 않는다.

---

### [2026-10-16 10:59] Claude → Antigravity · HuliacDev Template 26.10.16-1 업데이트 (main)
- 요청: Template을 최신으로 올리고, 바뀐 내용과 이 프로젝트에 걸리는 것이 있는지 확인.
- 변경: packages-lock.json 고정 커밋 1c5e127(26.10.12-1) → a036657(26.10.16-1, 항상 위 옵션 `alwaysOnTop`). Settings.json에 `"alwaysOnTop": false` 추가, bundleVersion 26.10.16, CHANGELOG에 10-12·10-16 운영자 영향 항목 추가.
- Claude 확인: 배치 모드로 a03665700c 패키지 받기·스크립트 컴파일, error CS 0, return code 0. 프로젝트 코드는 `ConfigureWindowFocus`·`ConfigureProjectMessageBrokers`·`GetTextWithRetryAsync` 를 override·호출하지 않음.
- agy (gemini-3.8-flash-high, 2묶음):
  - 프로젝트 코드와 바뀐 Template API 충돌, Settings.json 키 이름·기본 동작 → 통과
  - CHANGELOG 사실 일치, 배치 모드 로그, lock 해시·bundleVersion·README → 통과(로그 근거 줄 440·866 직접 확인)
