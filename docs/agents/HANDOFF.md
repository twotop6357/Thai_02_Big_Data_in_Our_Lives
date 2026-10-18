# HANDOFF — Codex·Claude 공동 인수인계

**두 에이전트 모두 작업 전 현재 상태·담당 표·최신 기록을 반드시 읽는다.** 작업 재개·인계·컨텍스트 축약 후에도 다시 읽는다. 규칙은 [AGENTS.md](../../AGENTS.md)와 [개발 기준](../DEVELOPMENT_GUIDELINES.md)을 따른다. 외부 `handoff` 스킬은 필요하지 않다.

## 현재 상태

- 마지막 갱신: 2026-10-18 (Asia/Seoul), Codex, INIT-001.
- 최신 작업: 사용자 추가 에셋과 공동 개발 지침을 초기 개발 준비 커밋으로 정리.
- 현재 브랜치 / 기준 HEAD: `main` / `079f50a` (INIT-001 시작 기준). INIT-001 결과는 이 기록을 포함하는 `chore: add editor assets and initial development setup` 커밋에서 확인한다.
- 다음 행동: 다음 담당자가 이 문서와 실제 Git 상태를 확인하고 사용자 기능 요청에 맞춰 작업 ID와 범위를 등록한다. Product Name 등 배포 식별자는 추후 결정한다.
- 미해결 치명적 장애: 이번 세션에서 발견한 항목 없음. 런타임 검증으로 장애 부재를 확인한 뜻은 아니다. [TroubleShooting](../TroubleShooting.md) 참조.

## 진행 중 작업 / 파일 소유권

| 작업 ID | 담당 | 상태 | 수정 범위 | 다음 담당 / 행동 |
|---|---|---|---|---|
| INIT-001 | Codex | 완료 | Assets/Resources/_Temp 및 메타, Assets/vFolders 및 메타, Assets/vHierarchy 및 메타, URP 렌더러, 기존 공동 지침 문서와 이 HANDOFF의 커밋 | 소유권 해제; 다음 작업 전 실제 HEAD 재확인 |
| DOC-001 | Codex | 완료 | AGENTS.md, CLAUDE.md, docs/DEVELOPMENT_GUIDELINES.md, docs/agents/HANDOFF.md, docs/TroubleShooting.md, TODO.md, README.md | 다음 작업자가 최신 상태 확인 후 새 작업 등록 |

`진행 중` 행의 파일은 다른 에이전트가 편집하지 않는다. 이 표는 원자적 잠금이 아니므로 기본적으로 순차 인계한다. 작업 종료 시 상태를 바꾸고 더 이상 소유하지 않음을 명시한다. 위 완료 작업의 파일 소유권은 해제되었다.

## 현재 프로젝트 확인값

2026-10-18에 파일로 확인한 값이며 다음 작업 시 다시 확인한다.

- remote: `origin https://github.com/twotop6357/Thai_02_Big_Data_in_Our_Lives.git` (fetch/push).
- Unity: `2022.3.62f3`. 빌드 등록 씬: `Assets/Scenes/0_Idle.unity` 하나.
- Company Name: `MyCompany`; Product Name: `Unity-Startup-Template`; Standalone Identifier: `com.Unity-Technologies.com.unity.template.urp-blank`. 배포 전 프로젝트 값 결정 필요.
- GameManager: 무입력 이벤트 구독으로 현재 씬 재로드. GameLifetimeScope에서 등록. 실제 화면 흐름·씬 내 중복 루트는 아직 실행 검증하지 않음.
- Settings: 무입력 꺼짐/20초, 60 FPS, 포커스 대기/재시도 각 3초, alwaysOnTop false, apiUrl 비어 있음.
- PC 종료: 모든 요일 비활성화, `-s -f -t 45`. 실제 종료 테스트 금지 조건 유지.
- R3: manifest와 NuGet 모두 1.3.1 표기. 실제 DLL 중복/Player 호환은 미검증.

## 보존할 기존 로컬 변경

아래는 DOC-001 시작 전부터 존재한 변경의 기록이다. INIT-001에서 사용자 요청으로 URP 렌더러·vFolders·vHierarchy를 커밋에 포함했다. `.vscode/`와 자동 생성 `.slnx`는 로컬 파일로 남겨 두었다. 다음 작업자는 실제 Git 상태를 다시 확인한다.

- 수정: `Assets/Settings/URP-Performant-Renderer.asset`.
- 미추적: `.vscode/`, `Assets/vFolders.meta`, `Assets/vFolders/`, `Assets/vHierarchy.meta`, `Assets/vHierarchy/`, `Thai_02_Big_Data_in_Our_Lives.slnx`.

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
