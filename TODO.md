# TODO

<!-- 이 프로젝트에서 해야 할 일. 사용자가 적고, Claude가 작업하면서 갱신한다(Antigravity는 읽기만 한다). -->
<!-- 형식: - [ ] 할 일 — 담당: Claude / 검증: Antigravity -->
<!-- 끝나면 [x]로 바꾸고 완료일을 붙여 "완료"로 옮긴다. 오래된 완료 항목은 지워도 된다(git 기록에 남는다). -->

## 진행 중

## 할 일

## 완료

- [x] HuliacDev Template 패키지 26.10.12-1 → 26.10.16-1 업데이트(packages-lock.json 고정 커밋 1c5e127 → a036657, 창을 항상 위로 두는 `alwaysOnTop` 옵션 추가 — 기본 꺼짐이라 기존 동작과 같음), Settings.json에 `"alwaysOnTop": false` 추가, Player Settings Version 26.10.16, CHANGELOG에 빠져 있던 26.10.12-1의 운영자 영향 변경도 함께 기록 — 담당: Claude / 검증: Antigravity (완료: 2026-10-16)
- [x] README 사용 방법을 "템플릿 폴더를 하나 clone해 두고 Assets·Packages·ProjectSettings·.gitignore·.gitattributes·CLAUDE.md만 새 폴더로 복사 → Unity Hub로 열기 → Player Settings의 Product Name 변경" 방식으로 바꿈(clone→upstream·fork 안내는 뺌). 복사한 6개로 파일 1,080개가 템플릿과 같게 들어가고 LFS 파일 없이 push되는 것을 임시 폴더에서 확인함 — 담당: Claude / 검증: Antigravity (완료: 2026-10-13)
- [x] README 맨 앞에 clone·fork·ZIP 사용 방법을 넣고 전체를 간단하게 다시 씀. clone 후 새 저장소로 push하는 순서는 임시 폴더에서 직접 시험함(LFS hook이 있으면 예전 커밋의 LFS 파일이 없어 push가 막히므로 `lfs.allowincompletepush` 설정을 넣음) — 담당: Claude / 검증: Antigravity (완료: 2026-10-13)
- [x] 남은 서드파티 LFS 파일 28개(NuGet 아이콘, DOTween 에디터 이미지, TextMesh Pro 이미지·폰트·PDF)도 LFS에서 빼고, README에 Git 설치 필요 안내 추가 — `.gitattributes` 끝에 서드파티 폴더만 예외로 두어 프로젝트 자체 이미지·영상·폰트는 계속 LFS. `git archive` 결과물(ZIP과 같음)에 LFS 포인터 0개, Unity 배치 모드로 처음부터 열어 컴파일 에러 0 확인 — 담당: Claude / 검증: Antigravity (완료: 2026-10-13)
- [x] DLL 126개를 Git LFS에서 빼서 일반 git 바이너리로 커밋(`.gitattributes` 의 `*.dll lfs` → `*.dll binary`, 내용은 LFS 원본과 SHA-256 동일) — GitHub "Download ZIP"으로 받으면 DLL이 LFS 포인터로 들어가 DOTween·R3·ZLogger 등 스크립트 에러가 나던 문제. 이전 커밋은 그대로 LFS — 담당: Claude / 검증: Antigravity (완료: 2026-10-13)
- [x] HuliacDev Template 패키지 26.10.10-1 → 26.10.10-2 업데이트(packages-lock.json 고정 커밋 36ec69a → 8afd2a8, Settings.json을 읽지 못하면 비활동 복귀 90초 대체 설정, JsonLoader 취소 전달·원자적 저장, ConsecutiveClickCounter·ApiRetryUtil.GetTextWithRetryAsync 추가 — 이 프로젝트 코드는 바뀐 API를 쓰지 않음) — 담당: Claude / 검증: Antigravity (완료: 2026-10-10)
- [x] HuliacDev Template 패키지 26.10.9-1 → 26.10.10-1 업데이트(packages-lock.json 고정 커밋 640d05e → 36ec69a, 창 포커스 복구 WindowFocusRestorer·F 키 토글 추가, Reporter 메모리 누수 수정), Settings.json에 `focusRestoreDelay`·`focusRestoreRetryInterval`(3초) 추가, Player Settings Version 26.10.10 — 담당: Claude / 검증: Antigravity (완료: 2026-10-10)

- [x] HuliacDev Template 패키지 26.9.25-3 → 26.10.9-1 업데이트(packages-lock.json 고정 커밋 b4547f3 → 640d05e, VideoManager 영상 RenderTexture 깊이 버퍼 제거 — 이 프로젝트는 해당 API를 쓰지 않음), Player Settings Version 26.10.9 — 담당: Claude / 검증: Claude(Antigravity 한도 초과로 대신 검증) (완료: 2026-10-09)
- [x] MCP for Unity 패키지 10.2.0 → 10.3.0 업데이트(packages-lock.json 고정 커밋 갱신), Player Settings Version 26.10.4 — 담당: Claude / 검증: Antigravity (완료: 2026-10-04)
- [x] README를 현재 구조·패키지에 맞게 갱신 — 담당: Claude / 검증: Claude(Antigravity 중단으로 대신 검증) (완료: 2026-10-02)
- [x] Test Framework를 프리릴리스(2.0.1-pre.18)에서 정식 버전으로 변경 — 담당: Claude / 검증: Claude(Antigravity 중단으로 대신 검증) (완료: 2026-10-02)
- [x] 0_Idle 씬에서 효과 없는 Directional Light·Global Volume 제거 — 담당: Claude / 검증: Claude(Antigravity 중단으로 대신 검증) (완료: 2026-10-02)
- [x] MCP용 Roslyn DLL을 Editor 전용으로 바꿔 플레이어 빌드에서 제외 — 담당: Claude / 검증: Claude(Antigravity 중단으로 대신 검증) (완료: 2026-10-02)
