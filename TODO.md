# TODO

현재 프로젝트의 할 일은 Codex와 Claude가 함께 관리한다. 작업 전 [AGENTS.md](AGENTS.md)와 [HANDOFF](docs/agents/HANDOFF.md)를 읽고 담당·범위를 등록한다. 형식: `- [ ] 할 일 — 작업 ID / 담당 / 검증 계획`. 완료 시 실제 검증과 완료일을 적는다. 인수인계 상세는 HANDOFF에 유지한다.

아래 기존 완료 항목은 이전 템플릿 이력이다. 과거 담당자·검증자를 보존하며 현재 프로젝트의 검증 결과로 간주하지 않는다.

## 진행 중

- [ ] 대상 PC에 Desktop/NSM_DNA_02 복사 후 NSM_DNA_02.exe 직접 실행·재실행 시 RFID 연결·이미지 생성·방화벽 허용 확인, 이전 설치의 RfidSettings.json 복사 — DEPLOY-SELF-001 후속 / 담당 미정. 이 PC Player 3회 실행으로 자동 구동과 포트 재수신은 확인. docs/validation/deploy_self/README.md.

- [ ] 새Windows10/11x64·실제RTX3060에서 동봉Installers 설치/재부팅·Run_Thai02.cmd/Player입력·RFID·생성시간/장시간 현장검수 — COMFY-PORTABLE-001 후속. 동봉환경3실생성/이동경로·설치서명·4모델해시는 현PC검증완료.

- [ ] 실제 LAN-754(KA118) 네 대에서 클라이언트 접속·방화벽·전원 차단/재접속, UidBurst 수신 공백(기본100ms)과 보고 간격·확인 주기(기본1초) 검수 및 기존 잘린값의 F10 재등록 — RFID-STABLE-001·RFID-PRESENCE-001·RFID-LISTEN-001 후속 / 담당 미정. 고정16바이트 기본값 폐기, 루프백83검사/Windows빌드/배포 완료. [RFID_SETUP](docs/RFID_SETUP.md).

- [ ] RTX3060 12GB 새 Klein640/4 경로의 예열1+20회15초 이내 실측 및 Windows Player/실물입력·장시간 검수 — STYLE-QUALITY-003 후속. 현PC80이미지/앱37검사 완료, API 중앙3.3805초·앱3.78~4.04초. 과거 Turbo5~12초 추정을 새 모델의3060 실측처럼 재사용하지 않는다. docs/validation/comfy_style3/README.md.

- [ ] RFID-BLOCKS-001 최종 직접 마우스/F9·광고→영상0.5초 암전 후보 실제 조작, Windows Player 빌드/실행, 문서 최신화 — Codex 인계 대기 / 사용자 Escape로 Computer Use 중단. 구현/기존33·83·8 검증 상세와 현재 한계는 HANDOFF.
## 할 일

- [ ] 설치PC에서 추가 입력 표본과 Windows Player 실장비 품질 검수 — COMFY-QUALITY-002 / 담당 미정 / STYLE-QUALITY-003의10종×4연령×2조건(6색)80개 직접 검수·원본 구조/알파/중앙 검사 완료. 다른 랜덤시드/새 설정에 무결점 보장을 확장하지 않는다.
- [ ] 안내 PNG의 고정 언어 표기 갱신 필요 여부 검토 — UI-LANG-002 / 담당 미정 / 실제20개 블록 표시·10물건 연결은 RFID 구현/COMFY-RESTORE-001에서 확인. 전체 TH·EN 장비/디자인 확인은 별도.
- [ ] Windows Player/설치 PC에서 물리버튼·RFID, 두 영상 전체 길이·음량, 포커스 복구와 장시간 자원 검증 — DEPLOY-UI-001 / 담당 미정 / 에디터 검증과 구분한 실제 장비 기록.

## 완료

- [x] 광고 화면 효과 시작 3초 뒤 입력 허용·버튼 안내 페이드 — UI-ADHINT-001 / Claude 단독 /2026-11-05 Play34 PASS. Player 재빌드/실제 버튼 후속. docs/validation/ad_hint/README.md.
- [x] 전역 오디오 매니저와 화면 전환 confirm·최초 화면 복귀 home — AUDIO-001 / Claude 단독 /2026-11-05 Play32 PASS, 믹서 출력 확인. Player 재빌드/실제 버튼/현장 음량 후속. docs/validation/audio/README.md.

- [x] 블록 인식 화면 안내 연출: 3초마다 빈 칸 중 무작위 글자 튕김·초과 인식 시 기존 블록 진동 반복 — UI-ATTN-001 / Claude 단독 /2026-11-05 Edit139·Play64+칸 회귀152 PASS. Player 재빌드/실물 리더·버튼/장시간 후속. docs/validation/ui_attention/README.md.

- [x] 웹 최신 UI 적용: 그룹 칸 1개·용도별 칸 표시·그룹 칸 축소 이동·빈 칸 안내·그룹/선택 배경 — UI-SLOTS-002 / Claude 단독 /2026-11-05 Edit135·Play131+전체흐름48 PASS. Player 재빌드/실물 리더·버튼/장시간 후속. docs/validation/ui_slots/README.md.

- [x] 동일 카드값 순환의 고정16바이트 오정렬 원인 재현/가변 UID 처리 — RFID-STABLE-001 / Codex 단독 /2026-11-02 전체 Edit130·새 실소켓16/기존67 PASS. 4/7/10/16바이트·분할/반복·교체/제거/재접속·F10저장·생성고정, Windows3씬 오류0/기존경고5. Desktop DLL2/framing 적용 및 USB업데이트, 보호7파일SHA동일. 실제장비 공백/UID계약·Player화면/장시간 후속. docs/validation/rfid_stable/README.md.

- [x] 흰색/다른색 물체 내부의 잘못된 투명화 보완 — ALPHA-OPAQUE-001 / Codex 단독 /2026-11-02 10종·6색·4연령 포함100실생성물 최종마스크/새파이프라인4개·원본RGB/외곽알파/실제구멍 검증 PASS, 기본노드6색 손상주입12케이스/워크플로9테스트 PASS. 바탕화면JSON2개/USB패치 적용, C#·모델·입력/RFID 미수정. 새PC/3060·Player화면 후속. docs/validation/alpha_opaque/README.md.

- [x] 주기(기본 1초, 별도 JSON)마다 최근 인식값으로 블록 유지/교체/제거 판단 — RFID-PRESENCE-001 / Claude 단독 /2026-11-02 Edit118·루프백 소켓 Play67 PASS. 실제 리더 보고 간격/Player 후속. docs/validation/rfid_presence/README.md.

- [x] RFID 연결 방향 반전: 앱이 TCP10123 수신, 리더 출발지 IP로 칸 자동 연결, 미등록 IP 거부, F10 수신 상태 표시 — RFID-LISTEN-001 / Claude 단독 /2026-11-02 Edit114·루프백 소켓 Play55 PASS. 실제 리더/방화벽/Player 후속. docs/validation/rfid_listen/README.md.

- [x] Desktop/Thai_02에 독립Comfy/Python/Torch·현재4모델·공식NVIDIA/VC++설치파일·경로보정실행기 동봉 — COMFY-PORTABLE-001 / Codex 단독 /2026-11-02 약20.3GB, 모델4SHA/설치서명2/기존파일5보존·GPU/경로이동·3실생성480RGBA PASS. 기존빌드/서버보존, 새PC/3060후속. docs/validation/comfy_portable/README.md.

- [x] 실제4리더 IP121/122/123/124 TCP10123·16바이트 전체HEX·F10 조회/20블록 JSON매핑 저장/재로드 — RFID-TCP-MAP-001 / Codex 단독 /2026-11-02 Edit112·루프백TCP4대/Play44 PASS, Windows최종배포 오류0·경고0. 실제장비 제거신호/주기·Player마우스/장시간 후속. docs/validation/rfid_tcp/README.md.

- [x] 광고 내부 효과 확대/가시성 보강·둥근 박스 마스킹·폭죽 광고요소 최상단 — AD-FX-005 / Codex 단독 /2026-11-02 실제 렌더3색조합47·취소/영상수명8 PASS, 실제생성 미리보기/홈 PASS. Windows배포 빌드 오류0·기존경고5. Player 육안/실물/장시간 후속. docs/validation/ad_fx5/README.md.

- [x] 광고 연출의 규칙성 완화·생성 두 HEX 색상 전달/유지 및 지정 운영 로그 메시지/Comfy 제외·Windows 강제 종료 감지 — AD-FX-004 / LOG-003 / Codex 단독 /2026-11-02 Play39+수명8+최종컨페티14, 로컬로그15/Windows종료5/최종Release강제종료 PASS·실제서버HTTP200/ok:true. 최종배포 빌드 오류0·기존경고5, 실물/장시간 후속. docs/validation/ad_fx4/README.md·content_log/README.md.

- [x] Desktop GUI 없이 서버 직접 실행/기존 서버 재사용 검증 및 준비 전 RedButton·숫자1 차단/타이틀 안내 숨김 — COMFY-AUTOSTART-002 / Codex 단독 /2026-11-02 Edit91·Play21·배포 Player11 PASS, 오류/경고0 배포빌드. 추가 GUI 입력검수는 사용자 Escape로 중단, 실물장비/다른PC 후속. docs/validation/comfy_autostart/README.md.

- [x] 레퍼런스 기반 귀여운2D 생성·다색/연령80개 직접검수·원본 불완전 출력 차단 — STYLE-QUALITY-003 / Codex 단독 /2026-11-01 Edit66/Python8/실제앱27+실패·취소 fixture10 PASS·실제 Unity1440 프롬프트 일치.3060/Player/설치PC 후속은 별도. docs/validation/comfy_style3/README.md.

- [x] 단순 일러스트/인식 두 색/빠른 단일 생성 적용 — COMFY-FAST-002 / Codex 단독 /2026-10-30 Turbo4/512,최종61/7/앱16 PASS·10종2시드/대표4연령 검토.3060 실측은 별도 후속.

- [x] 최종480×480 제품 알파경계 정중앙 보정·비율/투명여백·측면고정해제/자연스러운사선 시점 — COMFY-FRAMING-001 / Codex 단독 구현·검증 / 2026-10-30 Edit58/58·Python7/7·실생성Play8/8 PASS·중심(240,239.5), 이전PNG2개(240,240). 미커밋·Player/장비/전 조합품질 후속. docs/validation/comfy_framing/README.md.
- [x] ComfyUI 새 투명 일러스트 복원·가이드/ControlNet 제거·첨부 스타일 반영·현재10물건/원에서추출한6색·전선택패널 생성입력잠금/취소·실패재시도 — COMFY-RESTORE-001 / Codex 단독 구현·검증 / 2026-10-30 최종 Edit49/49·Python7/7·Play31·종료/시간초과9 PASS·실생성17.58초/알파검사·396파일동일. 미커밋, Player/실장비/전체품질 후속. docs/validation/comfy_restore/README.md.
- [x] 2_new의 언어별 가이드/요소 적용·기존 이름/GUID 유지·가이드/중복원본 정리, 영상 이후/기능/연출 보존 — UI-REFRESH-002(UI-REFRESH-001 대체완료) / Codex 단독 구현·검증 / 2026-10-29 PNG24개 교체·8화면 확인·실행52항목/정적24슬롯 PASS·291파일 해시 동일·정리 후 세 씬 누락참조0. 미커밋·Player/실장비 후속. docs/validation/ui_refresh2/README.md.
- [x] 원형 파동 시작 축소·광고 박스 내부 최대 크기 제한 — AD-FX-003 / Codex 단독 구현·검증 / 2026-10-29 시작120/최대480, Play28 PASS·실제정점620개가 광고 박스16px 안쪽 유지. 기존 연출/입력/영상 회귀·199파일 및 미저장 Idle 보존. 미커밋·Player/실장비 후속.
- [x] 광고 물건 최전면·느린 반복 아우라·큰 버스트·ConfettiFullscreen 자연 종료 및 단발 완료 후 입력 복구 — AD-FX-002 / Codex 단독 구현·검증 / 2026-10-29 최종Play26 PASS, 컨페티 관측5.855초, 일시정지/아우라 다회 반복·물건4지점 픽셀 불변·영상 암전/첫프레임0·취소/재진입 정리. 원본195파일·미저장 Idle 보존, 미커밋·Player/장비 후속. 이전 AD-FX-001의 전체1.8초 종료 요구를 대체.
- [x] 광고에 Item의 두 ParticleImage 묶음 적용, Demo/Prefabs19개 검토,2초 이내 종료·종료 전 버튼 입력 차단 — AD-FX-001 / Codex 단독 구현·검증 / 2026-10-29 최종 Play21항목 PASS, 실시간1.814초, 화면 캡처 확인. 취소/비활성/재진입·일시정지·영상암전/첫프레임0 확인. 원본195파일 해시 동일·미저장 Item 디자인 보존. 미커밋, Player/장비 검증 후속.
- [x] 실제0번 프레임 렌더까지 광고 유지·불투명 영상 인계·즉시 연속재생, 실패/취소/최종프레임 해제 시 광고 복원 — VIDEO-FIRST-001 / Codex 단독 구현·검증 / 2026-10-29 최종 첫프레임/미디어41·기존UI/영상58 PASS. TH/EN3회 첫표시0·미덮임0, 실제 첫화면 캡처 확인. 미커밋·설치PC/Player 후속. 상세 HANDOFF.
- [x] 그룹→배치의 네 기존 칸 동시 이동/도착 인계·암전 없는 교차페이드, 이동 중/후 인식 해제 시 배치 화면 유지·빈칸/계속차단 — UI-SLOTS-001 / Codex 단독 구현·검증 / 2026-10-29 이동·연출·입력53·기존UI/영상58 PASS, 화면캡처 직접 확인. 미커밋·실장비/Player 후속. 상세 HANDOFF.
- [x] 공통 UI 연출·입력 잠금/역전·취소 정리, 네 칸 동시페이드·신규배치만OutBack·우측/광고이미지 페이드, 필수블록 완료 시 계속안내/짧은 진행 허용 — UI-MOTION-001 / UI-GATE-001 / Codex 단독 구현·검증 / 2026-10-29 EditMode27/27·연출/입력39·UI회귀58·색칠회귀18 PASS.5초홈 유지, 미커밋·실장비/Player 후속. 상세 HANDOFF.
- [x] Figma 좌표의 이미지 UI, 언어 JSON/페이드, 타이틀2초 언어토글·짧은 현재언어 시작·전역5초홈, 두 언어 영상과 무색칠 일반 블록칸 — UI-LANG-001 / Codex 단독 구현·검증 / 2026-10-29 컴파일오류0·EditMode27/27·UI실행58·색칠회귀18 PASS. 미커밋, 실장비/Player 검증 후속. 상세 HANDOFF.

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
