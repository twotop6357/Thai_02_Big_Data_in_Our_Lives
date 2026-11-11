# HANDOFF — Codex·Claude 공동 인수인계

**두 에이전트 모두 작업 전 현재 상태·담당 표·최신 기록을 반드시 읽는다.** 작업 재개·인계·컨텍스트 축약 후에도 다시 읽는다. 규칙은 [AGENTS.md](../../AGENTS.md)와 [개발 기준](../DEVELOPMENT_GUIDELINES.md)을 따른다. 외부 `handoff` 스킬은 필요하지 않다.

## 현재 상태

- UI-WEB-007 완료 / Claude 단독 / 2026-11-11 Asia/Seoul / 기준 main/f65c232. 웹 2번 TH/EN 일괄 ZIP(86개)으로 태국어 폰트 변경에 따른 문구 6개(그룹 제목·부제, 선택 제목, 결과 제목·부제, 영상 종료) 원본 교체와 Lang_Th.json·defaultThai 좌표 갱신. 그 밖의 요소·영어는 변경 없음. Play23 PASS·콘솔 오류0. 새 영상 종료 문구의 "น ำ" 띄어 보임은 웹 원본 그대로라 디자이너 확인 필요. 16:26 재내보내기 결과 PNG81개 동일(PDF5만 다름, 미사용)로 추가 변경 없음. Player 재빌드/실장비 미수행. 기능·문서 커밋 완료, 사용자 변경(ProjectSettings·AdConfetti 프리팹)은 미커밋 유지. 소유권 해제. docs/validation/ui_web7/README.md.

- IDLE-VIDEO-001 완료(미커밋) / Claude 단독 / 2026-11-10 Asia/Seoul. 타이틀 배경 c02.mp4 무음 무한 반복, 로고·문구·팝업은 위. Play9 PASS. 소유권 해제. docs/validation/idle_video/README.md.

- LOGO-SPLIT-001 완료(미커밋) / Claude 단독 / 2026-11-10 Asia/Seoul. 배경 6개를 로고 없는 웹 원본(복제본 방식)으로 교체, NSMLogo 이미지를 4개 배경 위에 배치. Play12 PASS·로고 밖 화면 동일. Figma 원본 보존 확인(섹션 폭 자동확장 2194로 복구). 디자이너의 태국어 문구 수정 2건은 UI-WEB-006으로 반영. 소유권 해제. docs/validation/logo_split/README.md.

- TIMEOUT-HOLD-001 완료(미커밋) / Claude 단독 / 2026-11-09 Asia/Seoul. 그룹·선택 화면에 블록이 하나라도 있으면 무입력 복귀 보류, 마지막 제거 후 30초. Play12·EditMode139 PASS. 결과·광고 화면 규칙은 그대로. 소유권 해제.

- UI-IDLEPOP-001 완료(미커밋) / Claude 단독 구현·검증 / 2026-11-09 Asia/Seoul / main/60d397e+UI-WEB-005 등 미커밋 보존. 웹 2번 Group 149(영어 단일) 블록 제거 팝업을 타이틀에 추가: 등록 블록이 하나라도 리더 위에 있으면 영상 종료 팝업과 같은 연출로 표시하고 버튼 입력 차단. 허브에 세션과 별개인 실제 존재 기록 추가(홈 복귀 후 남은 카드·그룹 우선으로 무시된 카드 포함). Play25·EditMode139 PASS. Player/실제 리더·버튼 미수행. 소유권 해제. docs/validation/idle_popup/README.md.

- UI-WEB-005 / AUDIO-002 완료(미커밋) / Claude 단독 구현·검증 / 2026-11-06 Asia/Seoul / main/60d397e+기존 변경(ProjectSettings preloaded 1줄·사용자 mp3 2개) 보존. 웹 2번 TH/EN ZIP으로 태국어 폰트 변경(Noto Sans Thai·Futura 100 THA) 감지·원본 교체 31개+태국어 연령 블록4 신규, 영어 안내2("continue"). 블록 화면 새 인식 selected·타이틀 언어 전환 language. 블록 없는 화면 30.0초 복귀 정상(코드 수정 없음). Play60 PASS·콘솔 오류0. Player재빌드/실제 리더·버튼/청취 미수행. 소유권 해제. docs/validation/ui_web5/README.md.

- COMMIT-005 완료 / Claude 단독 / 2026-11-05 Asia/Seoul / 기준 main/3852ee6. 사용자 지시로 전체 변경을 이 기록을 포함한 작업별 로컬 커밋으로 보존(해시는 git log 확인), push 없음. 여러 작업이 함께 고친 파일은 hunk로 나누지 않고 파일 단위로 한 커밋에 넣었다(예: GameManager는 오디오 커밋, BlockExperienceView는 칸 UI 커밋). 중간 커밋의 개별 컴파일·빌드는 미검증이며 마지막 커밋의 트리만 작업 트리와 같다. 커밋 전후 Unity 재실행 없음. Tools/UI/__pycache__ 1개는 제외. 운영 로그 URL의 uid가 포함된 Settings.json도 지시대로 커밋했다. 아래 미커밋 표기는 당시 상태이며 Player·실장비 후속은 그대로 남는다. 소유권 해제.

- DEPLOY-NSM-001 완료(미커밋) / Claude 단독 / 2026-11-05 Asia/Seoul / main/3852ee6+기존 변경 보존. Desktop/NSM_DNA_02에 StreamingAssets 복구(174 SHA 일치, RFID 카드 비어 있음)와 Run_NSM_DNA_02.cmd·Launcher 추가. 대상 PC는 ComfyRuntime 이동·RfidSettings.json 복사 후 실행. 수동 절차 실패 원인 미확정. 실제 런타임 이동/동봉 서버/대상 PC 미검증. 소유권 해제. docs/validation/deploy_nsm/README.md.

- UI-ADHINT-001 완료(미커밋) / Claude 단독 구현·검증 / 2026-11-05 Asia/Seoul / main/3852ee6+기존 변경 보존. 광고 효과 시작 3초 뒤 입력 허용(폭죽은 자연 종료까지 재생), 그 시점에 버튼 안내 0.25초 페이드. Play34 PASS. 구 verify_ad_highlight*/ad_fx* 미재실행(폭죽 종료까지 잠금 전제). Player재빌드/실제 버튼 미수행. 소유권 해제. docs/validation/ad_hint/README.md.

- AUDIO-001 완료(미커밋) / Claude 단독 구현·검증 / 2026-11-05 Asia/Seoul / main/3852ee6+기존 변경 보존. GameManager.Audio로 접근하는 전역 AudioManager, 화면 전환 confirm·최초 화면 복귀 home(시간 초과 포함). Play32 PASS, 믹서 출력 확인(스피커 청취 안 함). ADHINT 이전에 실행. Player재빌드/실제 버튼/현장 음량 미수행. 소유권 해제. docs/validation/audio/README.md.

- UI-ATTN-001 완료(미커밋) / Claude 단독 구현·검증 / 2026-11-05 Asia/Seoul / main/3852ee6+기존 변경 보존. 그룹·선택 화면 공통: 블록 변화 없이 3초마다 빈 칸 중 무작위 한 칸의 글자만 튕김, 초과 인식(그룹·제품2+/색3+)이면 표시된 해당 블록이 즉시 좌우 진동 후 유지 시 3초마다 반복. 입력 잠금 없음. 글자는 원본 PNG 유지+파생 글자 레이어. Edit139·Play64+칸 회귀152 PASS. Player재빌드/실제 리더·버튼/장시간 미수행. 소유권 해제. docs/validation/ui_attention/README.md.

- UI-SLOTS-002 완료(미커밋) / Claude 단독 구현·검증 / 2026-11-05 Asia/Seoul / main/3852ee6+기존 변경 보존. 블록을 리더 위치와 무관하게 용도별 칸(좌상 그룹·우상 제품·좌하우하 색)에 표시, 주/보조색은 화면 칸 순서. 그룹 화면은 칸 1개(424), 전환 시 그 칸만 0.5초에 252로 줄며 이동. 빈 칸/보드/그룹·선택 배경은 웹 일괄 ZIP 2개 원본. Edit135·Play131+전체흐름48 PASS. Player재빌드/실제 리더·버튼/전체영상/장시간 미수행. 소유권 해제. docs/validation/ui_slots/README.md.

- UI-WEB-004 완료(미커밋) / Codex 단독 구현·검증 / 2026-11-05 Asia/Seoul / main/3852ee6+기존 변경 보존. Figma MCP 없이 웹의 TH/EN2번 UI64개 ZIP+영상안내2개 추가ZIP 직접다운. PNG15교체/meta보존+영상좌상단 홈안내2추가, JSON/기본사본·Result씬/컨트롤러의 안내 표시만·Builder 갱신. 기존4칸/이동·입력/미디어/효과 보존. Unity98·실패/재시도/취소10·정적26/영상안내2픽셀 PASS, 신규바인딩포함30참조 확인. 기존688중666SHA동일, 의도21변경 외 Reporter build_info자동갱신1. Play중지/Idle dirtyfalse, 소유권 해제. Player재빌드/실장비/전체영상/장시간 미수행. docs/validation/ui_web4/README.md.

- RENAME-001 완료(미커밋) / Codex 단독 구현·검증 / 2026-11-04 Asia/Seoul / main/3852ee6 + 기존 미커밋 보존. Product Name NSM_DNA_02, Windows 빌드 스크립트/에디터 저장 경로의 파일명 NSM_DNA_02.exe 적용. Unity 재조회·설정 단일줄 diff·형식 검사 PASS, Play/빌드는 사용자 지시로 미실행. Company/식별자·3씬·패키지·작업 폴더/원격명 보존. 소유권 해제. 기존 배포본은 그대로이며 다음 빌드부터 적용.

- RFID-STABLE-001 수정/자동검증 완료(미커밋) / Codex 단독 /2026-11-02 Asia/Seoul / main/3852ee6+기존변경보존. 사용자 LAN-754(KA118)/7바이트 반복 확인. FixedLength16이7종 잘못된UID로순환하는원인재현 후 UidBurst 기본값(수신공백/정확반복단일화·새값2회확인), F10길이표시. Edit130/실소켓새16+기존67 PASS, Windows3씬 오류0/기존경고5. DesktopDLL2/JSONframing 적용·원본백업·보호7SHA/매핑보존, USB RFID_Update.zip 98252바이트. Play중지/Idle dirtyfalse/최종콘솔오류0, Claude기능·Comfy/패키지/씬보존, 소유권해제. 실제장비의100ms공백조건/보고간격·정확한UID계약·Player화면/장시간은미검증. docs/validation/rfid_stable/README.md.

- RFID-PRESENCE-001 구현/자동검증 완료(미커밋) / Claude 단독 /2026-11-02 Asia/Seoul / main/3852ee6+기존변경보존. 사용자 지시: 리더에 유지 감지가 없으므로 앱이 주기(기본1초, StreamingAssets/RFID/RfidPresenceSettings.json)마다 최근값을 확인해 같으면 유지/다르면 교체/없으면 제거. absentTimeoutMilliseconds 제거, 잘못된 프레임은 즉시 제거하지 않고 값 없음으로 취급. Edit118/루프백 소켓 Play67 PASS(1000ms에서 인식99/교체515/제거1015ms), 콘솔15건은 의도한 실패 주입. ALPHA-OPAQUE-001과 사용자 허용 하에 병행(파일 겹침 없음), Play중지/Idle, 씬 변경 없음, 소유권 해제. 미검증: 실제 리더 보고 간격(주기는 이보다 길어야 깜빡임 없음)·실장비·방화벽·Player 재빌드. 사용자 결정 대기: 생성 중 뺀 블록이 생성 후 화면에 남는 기존 SNAPSHOT 규칙 유지 여부. docs/RFID_SETUP.md·validation/rfid_presence/README.md.

- ALPHA-OPAQUE-001 수정/검증 완료(미커밋) / Codex 단독 /2026-11-02 Asia/Seoul / main/3852ee6+기존변경보존. 흰배경/BiRefNet 오분리 재현 후 제거용 배경문구만 중간회색으로 변경하고 기본노드 RGB재질보호/내부알파 보완. 10물건·6색·4연령 포함100실생성물 최종마스크 재실행+최종전체파이프라인 새4개, 원본RGB/외곽알파/실제구멍 보존 PASS. 기본노드 손상주입12/단위9 PASS. Desktop/Thai_02 JSON2개 원본백업 후 적용·USB패치7647바이트 준비, 기존빌드/Settings/RFID5해시 보존. 기존모델/스타일/색/연령/샘플러 및 C#/씬/RFID변경 보존, 소유권해제. 프로그램 완전종료 후 재시작 필요. 실제3060/새PC·Player화면/장시간 후속. docs/validation/alpha_opaque/README.md.

- RFID-LISTEN-001 구현/자동검증 완료(미커밋) / Claude 단독 /2026-11-02 Asia/Seoul / main/3852ee6+기존 변경 보존. 사용자 지시로 RFID 연결 방향을 반전: Unity가 TCP10123 수신, 리더가 클라이언트로 접속, 출발지 IP121/122/123/124로 좌상/우상/좌하/우하 자동 연결(미등록 IP 거부, IP 자동 칸 배정은 물리 위치 보존을 위해 미적용). JSON version2/listenPort, 이전 version1 거부. 같은 IP 새 접속이 이전 연결을 대체, keepalive 적용. 리더 미접속 칸의 F9 블록은 이제 유지. Edit114/실제 루프백 소켓 Play55 PASS, 콘솔 오류/경고9건은 의도한 실패 주입. Play중지/Idle, 씬 변경 없음, 소유권 해제. 미검증: 실제 리더(클라이언트 모드 설정 필요)·방화벽 인바운드·전원 차단 감지 시간·재접속 후 카드 재보고 여부·Player 재빌드(기존 배포본은 이전 방식). docs/RFID_SETUP.md·validation/rfid_listen/README.md.

- COMFY-PORTABLE-001 패키징/현PC검증 완료(미커밋) / Codex 단독 /2026-11-02 Asia/Seoul / main/3852ee6+기존변경보존. Desktop/Thai_02 약20.3GB 독립Python/Torch/Comfy·현재4모델·공식NVIDIA/VC++설치파일·Run/Check_Runtime/안내동봉. 모델4SHA/설치서명2/기존빌드설정5SHA보존·격리GPU/한글공백이동/3실생성480RGBA PASS. 8189테스트중지/기존17356유지(유휴모델캐시만해제), 임시트리는삭제차단으로workspaceTemp에보존이동/배포본제외. [TS-20261102-01](../TroubleShooting.md#TS-20261102-01) 해결·소유권해제. 실제새Windows/3060·Player조작/현장후속. docs/validation/comfy_portable/README.md.

- RFID-TCP-MAP-001 구현/자동검증 완료(미커밋) / Codex 단독 /2026-11-02 Asia/Seoul / main/3852ee6+기존 변경 보존. 사용자 TCP10123/HEX·16바이트 예시를 전체 카드값으로 매핑, IP121/122/123/124를 좌상/우상/좌하/우하 연결. F10 실시간/JSON매핑·자동재접속·생성고정/그룹우선 보존. Edit112·실제루프백4TCP/Play44 PASS, 최종Windows3씬 배포 오류0·경고0·동봉JSON SHA동일. 씬/원본아트/vendor/패키지 diff0, 빌드추가 preloaded1개만 원복. Play중지/Idle·dirty false, 소유권해제. 실제제거HEX/보고주기·실장비/Player마우스/장시간 후속. docs/RFID_SETUP.md·validation/rfid_tcp/README.md.

- AD-FX-005 완료(미커밋) / Codex 단독 /2026-11-02 Asia/Seoul / main/3852ee6+기존 변경 보존. 내부 빛살/링/별 크기·농도·노출 보강, 실제 박스 Sprite를 따른 둥근 숨김 마스크로 밖을 가림. 전체폭죽 전면Canvas32766으로 광고이미지/제목/안내보다 위, 전환검정32767 유지. 실제렌더3색47·수명/영상8·실제Comfy새가방/홈 PASS, 최종사진직접확인. Windows3씬 배포오류0·기존경고5. 씬/효과프리팹/vendor/패키지 diff0, 사용자입력보존/빌드preloaded추가1개원복. Play중지/Idle·dirty false, 소유권해제. docs/validation/ad_fx5/README.md, Player육안/장비/장시간 후속.

- AD-FX-004 / LOG-003 완료(미커밋) / Codex 단독 /2026-11-02 Asia/Seoul / main/3852ee6+기존 변경 보존. 광고 별/파동/빛살/컨페티의 균일성 완화·생성 HEX2개를 결과와 함께 전달/홈 해제, 이후 RFID로 재색칠 안 함. 실제3색조합 Play39·수명/영상8·최종컨페티14 PASS. 지정운영URL·정확한 로그문구·Comfy 제외·독립 Windows 종료감시 적용, 로컬계약15/Windows종료5/최종Release강제종료 PASS, 실제서버HTTP200/ok:true. 최종3씬 배포BuildOptions.None 오류0·기존경고5, 빌드 동봉설정/스크립트 SHA확인. 씬/효과프리팹/vendor/패키지 미수정, 입력 등 기존변경 보존. 소유권 해제·실물/설치PC/장시간 후속, docs/validation/ad_fx4/README.md·content_log/README.md.

- COMFY-AUTOSTART-002 적용/자동검증 완료(미커밋) / Codex 단독 /2026-11-02 Asia/Seoul / main/3852ee6+기존 변경 보존. Desktop GUI 없이 Python 직접실행·기존서버 재사용 검증, Editor자동실행 true. 설정+서버 준비 전 RedButton/숫자1 차단·타이틀 도움Image 숨김 및 준비전 누름 버리기. Edit91/Play21/배포 Player11 PASS, 실제3씬 Windows BuildOptions.None 오류0·경고0, listen TCP/UDP0. GUI타이틀 표시 확인 후 추가 입력검수는 사용자Escape로 중단, Computer Use 추가 조작 없음. 기존8188 PID17356 보존, 테스트 Python 정리. 상세 docs/validation/comfy_autostart/README.md, 소유권 해제·실물/다른PC/전체Player흐름 후속.

- UI-NEW-003 완료(미커밋) / Codex 단독 /2026-11-02 Asia/Seoul / main/3852ee6+기존 변경 보존. NEW의 Intro_bg1개/TH·EN4화면 안내8개를 기존 파일명에 바이트 덮어쓰기,9개 meta/GUID/라이브러리 연결 유지. 크기 동일해 씬/JSON/기능 미수정. 양언어8화면 직접검토·실행18 PASS·콘솔 오류0. NEW참조0 확인 후 Unity에셋삭제로 폴더/meta 제거,452보호파일SHA동일(Reporter 자동생성build_info1개 별도기록). Play중지/Idle·dirty false, 소유권 해제. docs/validation/ui_new3/README.md.

- RFID-SNAPSHOT-001 완료(미커밋) / Codex 단독 /2026-11-02 Asia/Seoul / main/3852ee6+기존 변경 보존. 생성 시 리더4개 입력 lease 고정, 인식해제/교체/이동/일반clear를 UI·선택·이벤트·요청에 반영하지 않고 버림. 종료 후 재생 없음, 이후 새 신호는 처리. 명시적 취소/오류/시간초과/화면종료 lease해제·홈 강제초기화 보존. Edit91/실제Play45 PASS(실제생성+RFID노이즈/양패널/수명). 기존82파일SHA동일. Play중지/Idle·dirty false, fixture33924종료. 의도한HTTP400로그2건 외 오류 미발견, Player/실장비 미수행. 소유권 해제, docs/validation/rfid_snapshot/README.md.

- BLOCK-ART-LOADING-001 완료(미커밋) / Codex 단독 /2026-11-02 Asia/Seoul / main/3852ee6+기존 변경 보존. StreamingAssets/BlockArtwork 언어별 JSON20항목씩/외부36이미지·시작/홈 재로드·오류별 기본값/자원해제 적용. 완성 배치 후 버튼으로 생성, 배경 없는 붉은 분홍색 회전 링(선두/꼬리 RGB동일·꼬리 알파0→1) 표시/모든 종료 정리. Edit88·자원/생성실행47·최종붉은링9·외부이미지 Thai/English/Thai432(480슬롯) PASS. 원본아트/씬/패키지/입력/생성값82SHA동일. 최종 콘솔 오류0, Play중지/Idle·dirty false, fixture종료. Player/실장비 미수행. 소유권 해제. docs/BLOCK_RESOURCES.md·validation/block_art_loading/README.md.

- GROUP-FIRST-001 완료(미커밋) / Codex 단독 /2026-11-02 Asia/Seoul / main/3852ee6+기존 변경 보존. 최초 그룹 전 물건/색 무시, 정확히1그룹 뒤 새 비그룹 입력은 기존 버튼 Continue/슬롯 이동 연출로 자동 진행. 2그룹/그룹 제거/홈은 대기 요청 취소, 배치 단계 진입 후 그룹 제거·자유 재배치 유지. Edit76/76·실제Play101 PASS(네 연령/물건·색/중복·제거·교체·이동·홈/실제 생성잠금 포함). 최종 생성 Image ready, 콘솔 오류0, 검증 중 불변파일83SHA동일. Play중지/Idle·dirty false, 실장비/Player 미수행. 소유권 해제, docs/validation/group_first/README.md.

- BLOCK-SUFFIX-001 완료(미커밋) / Codex 단독 /2026-11-02 Asia/Seoul / main/3852ee6+기존 변경 보존. EnglishBlocks20개 *_Eng.png, ThaiBlocks16개 *_Thai.png로 변경·PNG/GUID36/36 보존. authoring/동기화·검증 도구 경로 갱신. Unity 컴파일 오류0·카탈로그36참조/언어별 조회·팔레트 --check PASS. 런타임 로직/씬/입력/생성값 미수정, Play중지/Idle·dirty false. Play/Player 재실행 미수행. 소유권 해제, suffix_rename.json 참조.

- THAI-BLOCKS-001 완료(미커밋) / Codex 단독 /2026-11-02 Asia/Seoul / main/3852ee6+사용자/RED-BUTTON 변경 보존. ThaiBlocks16개를 영문 대응 이름으로 정리·Sprite 연결. 두 선택 화면/F9 목록·리더·드래그가 현재 언어를 사용하고 연령4종은 기존 공용 fallback. 원PNG/GUID36/36 보존, 영문20개 PNG/meta는 HEAD 원본과 동일. 실제 Thai→English→Thai20종/480이미지슬롯 확인·426항목 PASS, Edit19/Python8·6색동일 검사 PASS, 양언어 실화면/F9 직접 검토. 씬·패키지·생성/입력 설정 미수정. Play중지/Idle·dirty false. Player/실물장비 미수행, 소유권 해제. docs/validation/thai_blocks/README.md.

- RED-BUTTON-001 완료(미커밋) / Codex 단독 / 2026-11-02 Asia/Seoul / main/3852ee6. KeyboardButtonSimulator에서 Input Manager RedButton + 숫자1/숫자패드1 유지 상태를 병합. 사용자 InputManager 매핑 SHA 보존. Unity 컴파일·제스처13/13 및 실제 어댑터 Play11항목 PASS, 최종 콘솔 오류0·매니저1개/Idle·Play중지. 실물 joystick button 0 누름과 Player 검증은 미수행. docs/validation/red_button/README.md. 소유권 해제.

- COMMIT-004 완료 / Codex 단독 / 2026-11-02 Asia/Seoul / 기준 main/37723c7. 현재 전체 변경 3404경로를 이 기록을 포함한 로컬 커밋으로 보존(해시는 git log 확인). UI·영상·RFID 후보·Klein 생성/검증 및 사용자 에셋 포함. ignored 로컬 캐시·모델·생성물은 그대로 보존, push 없음. 아래 과거 미커밋 표기는 당시 상태이며 RFID 인계 대기/Player·3060·실장비 검수는 미완료 유지. 소유권 해제.

- STYLE-QUALITY-003 적용/검수 완료(미커밋) / Codex 단독 / 2026-11-01 Asia/Seoul / main/37723c7+기존 미커밋 보존. 기본값은 Klein4B FP8/640/4steps/Euler/Flux2Scheduler + 원본 사용자 그림의 회색 스타일 참조 + BiRefNet/480RGBA. 최종10종×4연령×2조건(6색)80개 직접 검수·실제Unity 원본/중앙/알파 검사80 PASS, 실제 앱 새4개/27항목·실패/취소 fixture10항목·Edit66/Python8·Unity/Python1440조합 PASS. 현PC5070Ti Laptop API 중앙3.3805/최대3.936초, 앱3.78~4.04초. 모든 랜덤시드 무결점/자동스타일분류 보장은 하지 않으며3060/Player/실장비는 미검증. 잘림·큰 분리 부품·희미함은 표시 전 거부. docs/validation/comfy_style3/README.md 및 quality_preview.png. 씬/패키지/ProjectSettings 시작SHA동일, 원본아트 보존. Play중지/0_Idle·dirty false, 검증용Editor는 종료도구 패턴 차단으로 열어둠/우회 없음. fixture종료·소유권 해제. 아래Turbo/AGE 기록은 과거 단계다.

- AGE-DESIGN-002 구현/실생성 검증 완료(미커밋) / Codex 단독 / 2026-10-30 Asia/Seoul / main/37723c7+기존 미커밋 보존. 10종×4연령의 별도 형태/색 부위 40개 적용(모자: 유치원모자/비니/볼캡/중절모). 최종2시드80개 실제 출력·중앙/여백/알파/해시 확인·육안 검토, Python8 PASS·원본C# 오프라인 컴파일/1440조합 Python 일치 PASS. 대표40개는 docs/validation/comfy_age_design/preview.png. 단일 Turbo4/512 유지. Unity 연결 오류로 새 Editor/Play/Player 검증은 미수행, 일부 시드의 중복/잘림은 남음. 소유권 해제, 카탈로그 재로드(다음 Game 진입)부터 반영.

- AGE-GALLERY-001 완료(미커밋) / Codex 단독 / 2026-10-30 Asia/Seoul / main/37723c7+기존 미커밋 보존. 현재 Turbo4의 전체 10물건×4연령 40개를 2248×5330 PNG 한 장으로 제공. 빨강/파랑·시드1206501 고정, 동일 API 기존16장+추가생성24장. 40셀/조합/PNG 무결성 및 배치 직접 검토 완료. docs/validation/comfy_age_all/all_products_by_age.png. 활성 모델/프롬프트/앱/씬 변경 없음, 소유권 해제.

- COMFY-FAST-002 적용/현PC 검증 완료(미커밋): 사용자 최신 기준의 단순 일러스트·두 색·속도 우선으로 SDXL-Turbo512/4steps·CFG1·SDTurboScheduler를 활성 기본값에 적용. 추가 생성/가이드/ControlNet/어댑터 제거. 최종API20회 중앙1.750/최대2.541초,실제앱 첫6.242/다른 연령·색 반복2.647초(5070Ti Laptop). Edit61/61·Python7/7·앱16항목 PASS,10종2시드/가방·신발4연령·실제프레이밍26/26 확인. 원본31보존목록 중 의도된Framing1개 외30SHA동일. 3060 예열5~12초는 계획 추정/실측 아님,Player/실장비 후속. Codex 단독/소유권 해제,main/37723c7+기존 미커밋 보존,Play종료/0_Idle·dirty false. docs/validation/comfy_fast2/README.md.



- COMFY-LIGHT-001은 이전 품질 엄격 기준의 비교/인계 기록이다. 사용자의 최신 간단한 일러스트·속도 우선 지시로 COMFY-FAST-002가 대체했으며,이전34초 품질 복귀는 현재 기본값이 아니다. 원본 비교 자료는 docs/validation/comfy_light에 보존.





- COMFY-SHOEPOSE-001의 구도/색/연령 요구는 최신 COMFY-FAST-002에서10종2시드·대표4연령/두 색 전달로 재검증했다. 현재 가이드/ControlNet은 사용하지 않으며 실제 원본블록 PNG/GUID는 보존한다.
- COMFY-FRAMING-001 완료 (2026-10-30 Asia/Seoul, 미커밋): Comfy 다운로드 후 가시 알파 경계의 중심을480×480의(240,240)에 보정·최소24px 투명여백/비율 유지·필요할 때만 축소. 작업스레드/취소/실패 텍스처 해제, 빈 이미지 실패. 측면 강제를 해제하고 자연스러운 사선 시점 허용. Edit58/58·Python7/7·실생성Play8/8 PASS/17.84초·중심(240,239.5), 이전PNG2개 모두(240,240). 기존 컨트롤러/매니저/입력View·씬/패키지8파일 해시 동일. Codex 단독 담당/검증·소유권 해제. Play종료/Idle·dirty false, Windows Player/실장비/전 조합 품질 후속. 서버 원본은 보존하고 앱 수신 최종 텍스처에 적용.
- COMFY-RESTORE-001 완료 (2026-10-30 Asia/Seoul, 미커밋): 사용자 추가 참고이미지에 맞춘 새 투명배경 일러스트로 ComfyUI 복원. SDXL Base1.0/1024/28steps·CFG6.5·DPM++2M·Karras + BiRefNet/480RGBA, 가이드 업로드/ControlNet/로컬 색칠 미사용. 현재 물건10종/블록20개 연결 유지,6색 원 내부 HEX/RGB를 요청 메타데이터/저널에 보존하고 대응 색 설명으로 모델 조건 지정. 두 선택 패널의 생성 중 모든 버튼(5초 유지 포함)/계속 안내 차단, 성공·실패·취소·시간 초과·비활성·홈에서 입력/타이머 복구 및 새 입력 재시도. 최종 EditMode49/49·Python7/7·Play31·종료/시간초과9 PASS, 실제생성17.58초/투명픽셀154591·모서리알파0 확인. 397파일 중 승인된 View1개 외396해시 동일. Codex 단독 담당/검증·소유권 해제. Play 종료/0_Idle·dirty false, Player/실장비/전 조합 품질 후속. 기존 RFID 인계대기 유지.
- 마지막 갱신: 2026-11-02 (Asia/Seoul), Codex, UI-NEW-003 완료(미커밋). 기존 RFID/그룹/언어/입력/외부아트·로딩 변경 보존, RFID 인계 대기·3060/Player 장비검수 후속.
- UI-REFRESH-002 완료: 2_new의 언어별가이드8개/요소24개를 영상 전4화면에 적용. 기존이름/GUID/meta24개 보존, 양언어 타이틀안내x307/y879/1315×173·그룹보드/칸7px 상향·언어JSON/기본사본 동기화. 실제양언어52항목/정적이미지24슬롯 대조 PASS·8화면 직접 확인, 프로그램/연출/효과/영상 포함291파일 해시 동일. 영상/종료 하위 직렬화 전후 동일. 참조0 확인 후 가이드8장/중복원본24개와2_new/meta 삭제, 정리 후3씬누락스크립트/Sprite0. main/37723c7+기존미커밋 보존, Codex 단독 구현/검증·소유권 해제. Play종료/0_Idle. Player/실장비/장시간 후속, RFID 인계대기 범위는 미완료 유지.
- UI-REFRESH-001 완료(002로 대체): Figma MCP 한도 차단은 사용자의 로컬 가이드/PNG 제공으로 해결됨. 당시 조회자료는 docs/validation/ui_refresh에 보존, 실제 적용/최신검증은 docs/validation/ui_refresh2. 영상 이후 변경 없음이라는 최신 지시로8화면만 갱신했다.
- AD-FX-003 완료: 원형 파동 시작 지름455→120, 최대805→480으로 제한해 광고 박스 안에 유지. Play28 PASS/반복 한 주기 이상 실제정점620개가 박스16px 안쪽 유지·최대지름 관측477.469. 씬/실행컨트롤러/컨페티/vendor 포함199파일 해시 동일·미저장 Idle 상태 보존, Play종료. 기준main/37723c7+기존미커밋 보존, Codex 단독 구현/검증·소유권 해제. 수정은 ProductRing 프리팹 설정·Editor작성도구·기존검증/자료·문서/CHANGELOG/TODO/HANDOFF. 물건 최전면/느린 반복/입력 유지, Player/실장비 후속.
- AD-FX-002 완료: 사용자 후속 지시로 물건 최전면·초당10도 반복 아우라·큰 별 버스트·ConfettiFullscreen 프로젝트 복사본 추가. 아우라는 광고를 떠날 때 정리하며, 단발 입장/컨페티 완료까지 입력잠금 유지(컨페티2초 제한 해제). 최종Play26 PASS/컨페티 관측5.855초·일시정지에서 다회 반복·물건4곳 픽셀 불변·영상암전/첫프레임0·취소/재진입 검증. vendor195파일 SHA256 동일, Play종료·원래 미저장 Idle 루트 상태 동일. main/37723c7 + 기존미커밋 보존, Codex 단독 구현/검증·소유권 해제. Player/실장비/장시간 후속, RFID 인계대기 범위는 미완료 유지. 아래 AD-FX-001의 전체1.8초 종료 조건은 이번 지시로 대체됨.
- AD-FX-001 완료: 미저장 Scroll/Item의 두 ParticleImage 묶음(자식 링 포함3컴포넌트)을 전용 AdItemEmphasis 프리팹으로 복사, 광고 물건 주변의 금빛 빛살·링·별 단발 효과로 연결. 실시간1.8초·종료 전 공통 입력 잠금·취소/비활성/재진입 정리. 최종 Play21항목 PASS·실측1.814초, 일시정지/중복요청·버튼무시·해제·광고→영상 암전/첫프레임0 확인. Demo/Prefabs19개 검토·vendor195파일 해시 동일·원본 Item 디자인 값 보존. Play 종료·미저장 Scroll 원래 씬으로 복원. Codex 단독 담당/검증, main/37723c7 + 기존미커밋 보존·소유권 해제. Player/실장비/장시간은 후속, RFID 인계대기 전체 완료로 간주하지 않음.
- 인계 대기(미완료): 사용자 물리 Escape로 Computer Use 중단 통지가 도착하여 화면 조작을 중단함. 최종 후보는 F9/드래그를 Update의 직접 마우스 입력으로 변경(신/구 입력 병합·F9 래치·화면 크기 보정), 배경0.55, 광고→영상 암전/해제 각0.5초 재추가. 마지막 확인은 Play 실행 중/Idle/frame171/시뮬레이터 준비됨. 최신 입력+암전 후보의 실제 조작 재검증과 Windows Player 빌드/실행은 아직 필요. 기본 EditMode33/33, 이전 실행83항목, 직접 InputSystem 마우스8항목 PASS 기록은 있으나 최종 후보 전체 통과로 간주하지 않음. GUI GameView.SendEvent 방식의 초기 포인터 검증은 실패했고 직접 마우스 경로로 교체 후8항목 통과. 다음 담당이 새로 상태/씬 확인 후 인수할 수 있도록 소유권 해제.
- 추가 지시: 테스트 UI 배경을 더 투명하게 조정. RfidSimulator의 panelOpacity 기본0.55(기존0.97), 리더칸 보조배경0.12(기존0.28). 블록그림/글자 가독성 유지, 인스펙터에서 배경 불투명도 조정 가능.
- 사용자 확정: 네 물리칸은 종류 제한 없이 자유 배치, 첫 그룹 화면은 기존처럼 그룹1개 조건 유지. 배치 화면만 그룹1·물건1·색2의 정확한 조합을 요구. 색 두 개는 리더 번호 오름차순으로 주색/보조색을 결정. F9는 테스트창만 숨기고 점유 상태는 유지, 홈에서는 세션 초기화.
- 진행 중: Assets/UI/Blocks의20개 원본(그룹4·물건10·색6) 매핑, 이미지 없는 phone_cases/equipment 선택항목·색칠설정 제외, 네 물리 리더별 독립 인식 모델과 유효 종류 조합을 기존 선택/색칠/버튼에 연결. F9 표시/숨김·드래그앤드롭 테스트 UI는 Player에도 포함하고 씬 전환 중 유지. main/37723c7 + 기존미커밋·사용자 신규PNG/GUID 보존, Codex 단독 담당/검증. 범위: Blocks/Data 및 컨트롤러·신규 RFID hub/시뮬레이터/아트맵·BlockExperienceView·필요한 Editor/Idle·Game 연결·BlockCatalog/BlockColoring JSON·Tests/Tools 검증·관련 문서/CHANGELOG/TODO/HANDOFF. 소유권 충돌 없음. 리더 위치/첫 그룹 화면 조건은 사용자 옵션 응답 대기, 독립 작업 진행. 세션/홈·언어·영상0번프레임 유지, 패키지 변경/임의커밋 없음.
- VIDEO-FIRST-001 완료: 광고를 유지하며 실제 frameReady0과 RenderTexture 쓰기 완료를 기다린 뒤 영상에 불투명하게 인계.0번 프레임 한 렌더 후 즉시 재생/오디오 복원, 영상 진입 페이드 제거. 실패/취소/비활성화에는 광고를 복원하고 RawImage를 끈 뒤 자원 정리. 기준 main/37723c7 + 기존미커밋 보존, Codex 단독 구현·검증/소유권 해제. ResultVideoController·Tools/UI 검증·UI_MOTION/IMAGE_UI/CHANGELOG/TODO/HANDOFF 변경, 씬/authoring/원본영상·언어/설정·블록/색칠/매니저 미변경.
- 최신 영상 검증: Unity2022.3.62f3 컴파일오류0, 최종 첫 프레임/실패/재진입41항목 PASS, 기존 UI/영상회귀58항목 PASS. Thai·English 재시도·English 반복3회 모두 첫 Canvas표시0, frame8까지 정상 진행, 준비/인계의 미덮임 렌더콜백0. 실제 첫 화면 캡처 직접 확인. 원본/설정/선택·색칠·입력58파일 SHA256 동일. 증거 docs/validation/ui/video_first*.json/PNG. 최종 비활성화 보강 후41항목 재실행, UI58은 해당 보강 직전 수행. Player/설치PC·전체길이 시청/장시간 검증은 후속.
- UI-SLOTS-001 후속 명시 승인: 이동 중/이동 후 그룹 인식이 해제돼도 이전 패널로 돌아가지 않는다. 배치 화면을 유지하며 선택 모델의 해당 슬롯을 미인식(null/빈칸)으로 유지하고, 재인식 전 계속 안내/짧은 진행만 차단. 기존5초홈 유지. 이 조건 변경도 BlockExperienceView 및 검증/문서 범위에 포함.
- UI-SLOTS-001 완료: 그룹→배치에서 기존 네 칸이 불투명하게 함께0.5초 InOutSine 이동·크기보간, 페이지 내용은 동시 교차페이드. 목적지 칸은 이동 중 비활성이고 도착 순간 인계. 그룹 미인식으로 자동 복귀하던 분기 제거, 현재 배치 슬롯을 비우고 계속 입력만 차단. 기준 main/37723c7 + 기존 미커밋 보존, Codex 단독 구현·검증, 소유권 해제. 씬/원본에셋/설정·색칠·세션·영상/다른 씬 전환 미변경.
- 최신 추가 검증: Unity2022.3.62f3 컴파일오류0, 이동/연출/입력53항목 PASS, 기존 양언어UI/영상회귀58항목 PASS. TH/EN 정확한 도착 모서리·이동 중 목적지 비활성·그룹 제거 후 배치 유지·재인식·홈요청 후 정리, 화면캡처4개 직접 확인. 원본/설정/선택·색칠·입력58파일 SHA256 동일. 증거 docs/validation/ui/slot_transfer*.json/PNG. 아래 motion* 검증은 이전 작업 기록이며 그룹 제거 시 이전 패널 복귀 동작은 이번 지시로 대체함.
- 연출 완료: 공통 unscaled 연출/입력잠금·씬암전·패널순차페이드·팝업배경후OutBack·안내InOutSine·역전/취소 정리 연결. 기존 선택/색칠/영상/언어/홈·설정·JSON/위치는 보존하고 아래 사용자 추가 승인 입력 조건만 변경. 기준 main/37723c7 + UI-LANG/POS 미커밋, Codex 단독 구현·검증, 소유권 해제.
- 최신 연출 정정 완료: 네 블록 칸은 크기 유지·동시 페이드, 실제 신규 블록 배치에서만 해당 칸 OutBack. 우측 추천 이미지와 광고 이미지도 페이드만 사용. 안내 팝업의 배경0.25초→OutBack0.3초 유지.
- UI-GATE-001 완료: 배치 단계의 필수 블록 인식 완료 전에는 계속 안내를 숨기고 짧은 물리1 입력의 진행을 차단. Group은 연령1개, ProductAndColors는4개 모두 기준. 블록 제거 시 안내 숨김/진행 재차단, 재인식 시 복구. 기존5초 홈 보존.
- 최신 정정: 영어 타이틀 idle.help 위치를 태국어 기준 x=514.81787/y=840.08301로 수정하고 기본영어 JSON 사본 동기화. 실제2초 입력의 TH→EN→TH 페이드 왕복에서 동일좌표 확인, title_en.png 갱신. 영어 이미지 크기 유지, 씬/스크립트 재작성 없음.
- 현재 작업: UI-LANG-001 구현·검증 완료, main/37723c7 기준 미커밋/push 없음. Figma TH/EN10화면의1920×1080 및28개 이미지 렌더좌표를 읽어 세 씬에 적용. 모든 관람객 텍스트는 이미지이며 일반 빈 프레임은 블록 인식/색상 선택에도 원본색 유지. 실제 블록 아트는 추후 연결할 BlockContent 영역만 마련.
- 버튼/영상: 짧은 입력은 현재 언어로 시작, 타이틀2초 유지 Thai↔English 페이드 전환, 모든 단계5초 유지 홈. 홈에서도 언어 모드 유지. 추천 결과→현재 언어 영상→종료 안내/블록 제거 홈, 실패 재시도·준비/재생 취소·타이머 Resume 확인. 상세 [IMAGE_UI](../IMAGE_UI.md)·[BUTTON_INPUT](../BUTTON_INPUT.md).
- 이미지 구성(최신): Klein4B FP8/640/4steps·Flux2Scheduler 단일 생성 + 원본 사용자 스타일의 회색 ReferenceLatent + BiRefNet/480RGBA. ControlNet/블록형태가이드/LoRA/IPAdapter/2차 생성 없음. 잘림/큰분리/희미함 원본 거부 후 전경/중앙보정·10물건/원RGB/연령·전선택패널 입력잠금/취소 복구 유지.
- 최신 검증: Unity2022.3.62f3 컴파일오류0, EditMode27/27 PASS, 연출/필수블록 안내·입력39항목 PASS, UI 회귀58항목 PASS, 색칠 회귀18항목 PASS. 원본/설정/선택·색칠·입력58파일 SHA256 동일. 세 씬 누락스크립트0/텍스트컴포넌트0,8슬롯untinted·Hint페이드/빈BlockContent 및Game안내 초기비활성 확인. 증거 docs/validation/ui/motion*.json. 영상은 재생 후 끝부분 seek로 종료 이벤트 확인했으며 전체 길이 시청·WindowsPlayer·실제버튼/RFID·장시간/Profiler는 미검증.
- 현재 브랜치: `main`. FLOW-001/LOG-001/AI-001 및 사용자 씬·폰트·설정 변경은 COMMIT-001 기록을 포함한 커밋에 함께 저장. 실제 해시는 git log -1로 확인한다. 아래 미커밋 표기는 당시 작업 시점의 기록이다.
- 다음 행동(최신): Windows Player/설치PC·실물버튼/RFID·전체10물건×색×시드의 스타일/단일제품/색/알파 품질·영상 전체길이·장시간/Profiler 검수. 최종 Comfy 연결/샘플 및 20블록 아트 매핑은 확인 완료. 기존 RFID 직접입력/F9 후속은 인계 대기 유지. Play 종료/Idle, 파일 소유권 해제.
- 미해결 치명적 장애: 이번 세션에서 발견한 항목 없음. 런타임 검증으로 장애 부재를 확인한 뜻은 아니다. [TroubleShooting](../TroubleShooting.md) 참조.

## 진행 중 작업 / 파일 소유권

| 작업 ID | 담당 | 상태 | 수정 범위 | 다음 담당 / 행동 |
|---|---|---|---|---|
| UI-WEB-007 | Claude 단독 | 완료 | 태국어 UI PNG6(그룹 제목·부제, 선택 제목, 결과 제목·부제, 영상 종료 문구)·Lang_Th.json·ExhibitionUIAssets 기본사본·Tools/UI apply/verify_web7·validation/ui_web7·.gitignore(__pycache__)·CHANGELOG/HANDOFF | 소유권 해제. Play23 PASS. 영상 종료 문구 "น ำ" 디자이너 확인·Player 재빌드 후속 |
| IDLE-VIDEO-001 | Claude 단독 | 완료(미커밋) | 신규 UI/IdleVideoBackground·Editor/IdleVideoAuthoring·0_Idle 추가만·Builder 1줄·Logo/TitleBlockPopupAuthoring TryGetComponent 수정·validation/idle_video·CHANGELOG/HANDOFF | 소유권 해제. Play9 PASS. Player/장시간 후속 |
| TIMEOUT-HOLD-001 | Claude 단독 | 완료(미커밋) | BlockExperienceController(LateUpdate 보류·생성 해제·OnDisable 해제)·validation/rfid_timeout/hold_runtime*.json·BUTTON_INPUT/CHANGELOG/HANDOFF | 소유권 해제. Play12·EditMode139 PASS. 실제 리더/Player 후속 |
| UI-IDLEPOP-001 | Claude 단독 | 완료(미커밋) | 신규 UI/TitleBlockPopup·Editor/TitleBlockPopupAuthoring·UIs_Common/IdlePanel PNG1, RfidReaderHub(존재 기록)·RfidTcpReaders(존재 보고)·ExhibitionUIBuilder 1줄·0_Idle(추가만)·Tools/UI/verify_title_block_popup·validation/idle_popup·BUTTON_INPUT/CHANGELOG/HANDOFF | 소유권 해제. Play25·EditMode139 PASS. Player/실제 리더·버튼 후속 |
| UI-WEB-005 / AUDIO-002 | Claude 단독 | 완료(미커밋) | 태국어 UI PNG14·그룹 칸+파생 글자·태국어 블록16 교체/연령4 신규(UI·StreamingAssets 양쪽)·영어 안내2·Lang_Th/En·blocks_th.json·RfidBlockArtwork·ExhibitionUIAssets 기본사본·AudioManager/GameManager/BlockExperienceView·Tools/UI apply/verify_web5·validation/ui_web5·BUTTON_INPUT/CHANGELOG/HANDOFF | 소유권 해제. Play60 PASS, 30초 복귀 정상(수정 없음). Player/실장비/청취 후속 |
| DEPLOY-NSM-001 | Claude 단독 | 완료(미커밋) | Tools/ComfyUI 실행기 신규·Desktop/NSM_DNA_02 StreamingAssets 복구+실행기 배치·validation/deploy_nsm·CHANGELOG/HANDOFF. C#/씬 무수정, 재빌드 없음 | 소유권 해제. 임시 경로 실행 시험 PASS, 대상 PC/실제 런타임 후속 |
| AUDIO-001 | Claude 단독 | 완료(미커밋) | 신규 Core/AudioManager·GameManager·BlockExperienceView·ResultVideoController·Tools/Audio·validation/audio·BUTTON_INPUT/CHANGELOG/TODO/HANDOFF. Resources/Audios 사용자 WAV는 수정하지 않음 | 소유권 해제. Play32 PASS, Player/실장비/현장 음량 후속 |
| UI-ADHINT-001 | Claude 단독 | 완료(미커밋) | AdProductHighlight·ResultVideoController·2_Result 참조 1개·Editor Builder·Tools/UI·validation/ad_hint·UI_MOTION/AD_PRODUCT_HIGHLIGHT/CHANGELOG/TODO/HANDOFF | 소유권 해제. Play34 PASS, Player/실장비 후속 |
| UI-ATTN-001 | Claude 단독 | 완료(미커밋) | RfidReaderSelection·신규 BlockAttentionSchedule·BlockExperienceView·Editor Builder/UIMotionAuthoring·1_Game·언어JSON/ExhibitionUIAssets·칸 글자 PNG 신규·테스트·Tools/UI·validation/ui_attention·UI_MOTION/CHANGELOG/TODO/HANDOFF | 소유권 해제. Edit139/Play64+칸 회귀152 PASS, Player/실장비 후속 |
| UI-SLOTS-002 | Claude 단독 | 완료(미커밋) | RfidReaderSelection·BlockExperienceController·BlockExperienceView·UIBlockSlotTransfer·LocalizedImageSet·Editor Builder/UIMotionAuthoring·1_Game·언어JSON/ExhibitionUIAssets·칸/배경 PNG·테스트·Tools/UI·validation/ui_slots·문서/CHANGELOG/TODO/HANDOFF | 소유권 해제. Edit135/Play131+48 PASS, Player/실장비 후속 |
| UI-WEB-004 | Codex 단독 | 완료(미커밋) | UI PNG15교체/2추가·언어JSON/기본사본·영상안내 표시/씬/Builder·전용검증·IMAGE_UI/CHANGELOG/HANDOFF | 소유권 해제. Unity98/수명10·픽셀28 PASS, Player/실장비 후속 |
| RENAME-001 | Codex 단독 | 완료(미커밋) | Product Name·빌드 스크립트 출력명·Unity 로컬 빌드 경로·CHANGELOG/HANDOFF | 소유권 해제. Unity 재조회/정적 diff PASS. 사용자 지시로 빌드 미실행, 다음 빌드부터 적용 |
| RFID-STABLE-001 | Codex 단독 | 수정/자동검증 완료(미커밋) | RfidConfiguration/RfidTcpReaders/RfidMappingPanel·RFID JSON·RfidConfigurationTests·Tools/RFID·검증/운영문서/HANDOFF | 소유권해제. Edit130/실소켓83·Windows빌드·Desktop/USB 적용. 실리더 공백/UID계약/Player 후속 |
| RFID-PRESENCE-001 | Claude 단독 | 구현/자동검증 완료(미커밋) | RfidTcpReaders·RfidConfiguration·RfidMappingPanel·RFID JSON2개·RfidConfigurationTests·verify_tcp_mapping·validation/rfid_presence·RFID_SETUP/CHANGELOG/TODO/HANDOFF | 소유권 해제. Edit118/루프백Play67 PASS, 실제 리더 보고 간격·실장비/Player 후속 |
| ALPHA-OPAQUE-001 | Codex 단독 | 완료(미커밋) | 워크플로빌더/알파 기본노드·API/UI/inputs·Catalog배경문구·진단/검증/배포도구·validation/alpha_opaque·COMFYUI_SETUP/CHANGELOG/TODO/HANDOFF | 소유권해제. 100최종마스크/새전체4·합성12·단위9 PASS, Desktop JSON2개/USB패치7647바이트 적용. 실제3060/Player 후속 |
| RFID-LISTEN-001 | Claude 단독 | 구현/자동검증 완료(미커밋) | RfidTcpReaders·RfidConfiguration·RfidMappingPanel·RfidSettings.json·RfidConfigurationTests·Tools/RFID/verify_tcp_mapping.cs·validation/rfid_listen·RFID_SETUP/BUTTON_INPUT/BLOCK_SIMULATOR/CHANGELOG/TODO/HANDOFF | 소유권 해제. Edit114/루프백Play55 PASS, 실제 리더·방화벽·Player 재빌드 후속 |
| COMFY-PORTABLE-001 | Codex 단독 | 패키징/현PC검증 완료(미커밋) | Desktop/Thai_02 runtime/모델/준비물·실행/검증도구·운영문서 | 소유권 해제.4모델SHA/서명2/이동/GPU/3실생성 PASS, 새Windows/3060·Player후속 |
| RFID-TCP-MAP-001 | Codex 단독 | 구현/자동검증 완료(미커밋) | TCP/HEX·JSON/F10·Hub/Simulator 연결·테스트/검증/운영문서 | 소유권 해제. Edit112/루프백TCP4대 Play44/배포PASS, 실제제거신호·장비/Player마우스/장시간 후속 |
| AD-FX-005 | Codex 단독 | 완료(미커밋) | 광고효과 가시성/마스킹/전면폭죽·검증·운영문서 | 소유권 해제. 렌더47/수명8/실제생성·빌드 PASS, Player육안/장비 후속 |
| AD-FX-004 / LOG-003 | Codex 단독 | 완료(미커밋) | 광고효과/색상결과전달·로그 확장/설정·검증/운영문서 | 소유권 해제. Play39+8+14/로컬로그15/Windows종료5/Release강제종료·운영서버응답 검증, 실물/장시간 후속 |
| COMFY-AUTOSTART-002 | Codex 단독 | 적용/자동검증 완료(미커밋) | Launcher·Scope·GameManager·LocalizedImageSet·Editor자동실행 설정·검증/운영문서 | 소유권 해제. Edit91/Play21/배포Player11 PASS, 추가GUI입력검수 사용자Escape로 중단 |
| UI-NEW-003 | Codex 단독 | 완료(미커밋) | Intro_bg·버튼안내8PNG·NEW삭제·검증/IMAGE_UI/CHANGELOG/HANDOFF | 소유권 해제. 9개해시/meta·양언어8화면/실행18 PASS, NEW제거 |
| RFID-SNAPSHOT-001 | Codex 단독 | 완료(미커밋) | 리더데이터/Hub·Controller·테스트/검증·운영문서 | 소유권 해제. Edit91/실제Play45 PASS, Player/실장비 후속 |
| BLOCK-ART-LOADING-001 | Codex 단독 | 완료(미커밋) | 전용언어JSON/이미지·로더/Hub/View/Simulator/Controller·원형UI·테스트/운영문서/검증 | 소유권 해제. Edit88/실행47/최종링9/언어432 PASS, Player/실장비 후속 |
| GROUP-FIRST-001 | Codex 단독 | 완료(미커밋) | RfidReaderSelection/Hub·BlockExperienceView·테스트/검증·운영문서/HANDOFF | 소유권 해제. Edit76/실제Play101 PASS, Player/실장비 후속 |
| BLOCK-SUFFIX-001 | Codex 단독 | 완료(미커밋) | 영문/태국어36파일명·authoring/도구 경로·검증/문서 | 소유권 해제. PNG/GUID36/36·컴파일/카탈로그36참조·팔레트 PASS |
| THAI-BLOCKS-001 | Codex 단독 | 완료(미커밋) | ThaiBlocks·ArtworkCatalog/asset·View/Simulator·authoring·경로 동기화 도구·검증/문서 | 소유권 해제. Thai/English/Thai20종·480슬롯/426항목·Edit19/Python8 PASS, Player/실물 후속 |
| RED-BUTTON-001 | Codex 단독 | 완료(미커밋) | KeyboardButtonSimulator.cs, BUTTON_INPUT/CHANGELOG/HANDOFF, Tools/Input 및 입력 검증 자료 | 소유권 해제. main/3852ee6, 제스처13/Play11 PASS, 실물 RedButton/Player 후속 |
| COMMIT-004 | Codex 단독 | 완료 | 전체 현재 Git 변경 3404경로 및 HANDOFF | 기준 main/37723c7. 로컬 커밋만 수행, 소유권 해제. Python8/8·JSON/meta/GUID·LFS1597객체 확인. 해시는 git log 확인, 장비/Player 후속 유지 |
| STYLE-QUALITY-003 | Codex 단독 | 완료(적용/표본검수, 미커밋) | Klein/스타일참조/graph·Catalog/Selection/Controller/Client/Framing·Tests·검증/운영문서 | 소유권 해제. 최종80/실앱37/Edit66/Python8 PASS,3060/Player/실장비 후속 |
| AGE-DESIGN-002 | Codex 단독 | 완료(구현/실생성, 미커밋) | 연령형태 Catalog/Selection·C#/Python검증·기본API/UI·gallery·검증/문서 | 소유권 해제. 2시드80개 확인, 새 Editor/Play·Player/시드 품질 후속 |
| AGE-GALLERY-001 | Codex 단독 | 완료(미커밋) | 신규 export_age_gallery.py·comfy_age_all 산출물·HANDOFF | 소유권 해제. 40개 전체 비교 PNG 제공, 사용자 품질 검토 후속 |
| COMFY-FAST-002 | Codex 단독 | 완료(적용/현PC검증) | Turbo 모델/presets/graph/API/UI·Catalog/Controller·전경선택/테스트·검증/문서 | 소유권 해제,3060/Windows Player·실물입력/장시간 후속 |
| COMFY-LIGHT-001 | Codex 단독 | FAST-002로 대체 | 모델/참조/워크플로·Data/Controller·검증/문서 | 품질 감사 완료·가속 후보 불채택·품질 모드 복구. 소유권 해제,3060 속도/전체 품질 후속 |
| COMFY-SHOEPOSE-001 | Codex | COMFY-LIGHT-001 통합 인계 | 구도/참조/스타일·반복 검증 | 최신 품질 감사/복구 기준을 따른다. 전체 품질 승인 미완료·소유권 해제 |
| COMFY-FRAMING-001 | Codex | 완료 | Comfy 다운로드 중앙보정/Data·테스트·시점 프롬프트/워크플로·검증/문서 | 미커밋·소유권 해제.58/7/실생성8 PASS·중심(240,239.5), 기존PNG2개(240,240). Player/장비/전체품질 후속 |
| COMFY-RESTORE-001 | Codex | 완료 | Comfy 서비스·컨트롤러/DI/View·BlockSelection/색상카탈로그·모델/워크플로·검증·운영문서/CHANGELOG/TODO/HANDOFF | 미커밋·소유권 해제. 최종49/7/31/9 PASS·17.58초 실제생성/알파확인. Player/실물버튼/RFID·전 조합/시드 품질 후속 |
| UI-REFRESH-002 | Codex | 완료 | 언어UI24PNG·Lang JSON/라이브러리·3씬배치·LocalUIRefreshAuthoring/Builder좌표·Tools검증·2_new정리·문서/CHANGELOG/TODO/HANDOFF | 소유권 해제·미커밋. 52/24 PASS·291파일보존·가이드/중복원본삭제·3씬참조정상. Player/실장비 후속 |
| UI-REFRESH-001 | Codex | 완료 | docs/validation/ui_refresh 조회자료·TODO/HANDOFF | 002의 로컬수입본 적용으로 대체완료. 과거조회자료는 보존 |
| AD-FX-003 | Codex | 완료 | ProductRing프리팹 설정·Editor작성도구·기존연출 검증도구/자료·문서/CHANGELOG/TODO/HANDOFF | 소유권 해제·미커밋. 시작120/최대480·Play28 PASS/정점620개 경계내·199파일/미저장 Idle 보존. Player/실장비 후속 |
| AD-FX-002 | Codex | 완료 | AdProductHighlight/ResultVideoController 효과훅·AdItemEmphasis/AdConfettiFullscreen프리팹·2_Result·Editor작성도구·신규verify_ad_highlight_loop·문서/CHANGELOG/TODO/HANDOFF | 소유권 해제·미커밋. 최종Play26 PASS/컨페티5.855초, 원본195파일·미저장 Idle 보존. Player/실장비 후속 |
| AD-FX-001 | Codex | 완료 | AdProductHighlight·Editor작성도구·AdItemEmphasis프리팹/메타·2_Result·UIScenePresentation/ResultVideoController 효과연결·Tools/UI/verify_ad_highlight·검증/문서/CHANGELOG/TODO/HANDOFF | 미커밋·소유권 해제. Play21 PASS/1.814초·원본195파일 및 Item 디자인 보존, Play종료/미저장 Scroll 복원. Player/실장비 후속 |
| RFID-BLOCKS-001 | Codex | 인계 대기 | Blocks/Data·BlockExperienceController·신규 RFID 허브/Player 드래그UI/아트매핑·BlockExperienceView·ResultVideoController/UIPresentationTransitions·Editor/Idle/Game 연결·Catalog/Coloring JSON·Tests/Tools·문서/CHANGELOG/TODO/HANDOFF | 미커밋·소유권 해제. 사용자Esc로 화면조작 중단. 최종 직접입력/암전·Player 빌드/검증·문서 최종갱신 필요 |
| VIDEO-FIRST-001 | Codex | 완료 | ResultVideoController·신규 Tools/UI/verify_video_first_frame·verify_runtime 검증·UI_MOTION/IMAGE_UI/CHANGELOG/TODO/HANDOFF | 소유권 해제·미커밋. 첫 프레임41/기존UI58 PASS, TH/EN3회 모두 첫표시0·미덮임0. Player/설치PC 후속 |
| UI-SLOTS-001 | Codex | 완료 | BlockExperienceView·신규 UIBlockSlotTransfer(+meta)·UIScenePresentation 숨김요청보존, Tools/UI 검증·UI_MOTION/BUTTON_INPUT/CHANGELOG/TODO/HANDOFF | 소유권 해제·미커밋. 이동/입력53·UI/영상58 PASS, 원본/설정58파일동일. 인식해제 후 배치 유지. Player/실물장비 후속 |
| UI-GATE-001 | Codex | 완료 | BlockExperienceView의 필수블록/안내페이드·짧은입력가드, BlockExperienceController의 안내표시 소유권 이관, UIMotionAuthoring/기존Game씬 연결, 검증·UI문서/CHANGELOG/HANDOFF/TODO | 소유권 해제·미커밋. 필수블록 인식·제거·재인식/짧은입력무시/5초홈 PASS. 실물버튼/RFID 후속 |
| UI-MOTION-001 | Codex | 완료 | 신규 UI 연출/공통전환 컴포넌트·도구/검증, GameManager의 연출 전후 훅·버튼잠금, BlockExperienceView·ResultVideoController·LocalizedImageSet 시각 연출, BlockExperienceController 에디터 입력잠금, 기존3씬, UI문서·CHANGELOG·검증/HANDOFF/TODO | 소유권 해제·미커밋. main/37723c7 + 기존미커밋 보존. EditMode27/27·연출/게이트39·UI58·색칠18 PASS. 외부 패키지 수정/업데이트·커밋/push 없음 |
| UI-POS-001 | Codex | 완료 | Lang_En.json의 idle.help x/y, ExhibitionUIAssets.asset 기본영어 JSON, IMAGE_UI·CHANGELOG·검증/HANDOFF | main/37723c7 + 기존 미커밋 보존. 두 언어 실제 페이드 왕복·좌표 일치 PASS. 이미지크기 유지. 미커밋, 소유권 해제. Play 종료/Idle |
| UI-LANG-001 | Codex | 완료 | GameManager·ExhibitionButtonGesture·입력테스트, BlockExperienceController, 신규 UI/언어/영상 스크립트·Editor 구성도구·설정 JSON·에셋매핑, 0_Idle/1_Game/2_Result, Tools/UI·Coloring 검증기, BUTTON_INPUT·BLOCK_SIMULATOR·IMAGE_UI·CHANGELOG·TODO·검증/HANDOFF | main/37723c7 기준 미커밋. 소유권 해제.27/27·UI58·색칠18 PASS. 기존 URP 3파일·ProjectSettings 변경 보존. 사용자 신규 Assets/UI·Resources/Videos 원본/GUID 보존, 필요한 UI importer 설정만 적용. 패키지 갱신·커밋/push 없음 |
| COMMIT-003 | Codex | 완료 | 전체 현재 Git 변경(색칠/ComfyUI 제거·보관/사용자 AssetKits·IDE설정 포함), HANDOFF | main/092fb12 기준. 전체508경로 커밋. 소유권 해제. 해시는 git log -1, push 없음 |
| COLOR-001 | Codex | 완료 | BlockExperienceController, 신규 Data/BlockColoringCatalog·BlockColorizer·LocalBlockColoring, Tests/Blocks 색칠 테스트, StreamingAssets/BlockGeneration/Coloring 원본·마스크·설정, Tools/Coloring 샘플 제작·실행검증, BLOCK_SIMULATOR·COLORING·CHANGELOG·검증/HANDOFF | main/092fb12 + REMOVE-COMFY-001 미커밋 기준. 담당/검증 Codex 단독. 26/26·Play18개 PASS. 소유권 해제, 미커밋. 전시용 삽화 교체/배포·장비 검수 후속 |
| REMOVE-COMFY-001 | Codex | 완료 | GameLifetimeScope, BlockExperienceController/블록 설정 로더, Comfy 서비스/상태UI/보관정책 및 전용테스트·StreamingAssets의 Tools 아카이브, Idle 씬, 문서/검증/HANDOFF/CHANGELOG | main/092fb12 + QUALITY-001 미커밋 기준. 소유권 해제, 미커밋. 대체 백엔드 후속; 외부 설치/모델/생성물 미수정 |
| QUALITY-001 | Codex | 완료 | 런타임/Tools ComfyUI 워크플로·카탈로그·모델/alpha 설정, 필요 시 Unity 수신 검사·테스트, 검증/운영 문서/HANDOFF/CHANGELOG | main/092fb12 기준 미커밋, 소유권 해제. AssetKits 및 Claude 후속 기능 보존. 3060/Player/장시간·잔여 품질 검수 후속 |
| COMFYLAUNCH-001 | Claude | 완료 | 신규 Blocks/ComfyServerLauncher·ComfyStatusPresenter(+메타), ComfyImageClient 설정 필드, GameLifetimeScope, StreamingAssets ComfyUISettings.json, 0_Idle ComfyUIStatusText 연결(사용자 미커밋 Idle 변경 보존), COMFYUI_SETUP/CHANGELOG/HANDOFF | 소유권 해제; main/ceef831 기준 로컬 커밋(git log -1); Windows Player 빌드에서 실제 자동 실행 확인 필요 |
| RESULTSCENE-001 | Claude | 완료 | ExhibitionButtonGesture(+테스트), GameManager, BlockExperienceController, 신규 Assets/Scripts/Result, 1_Game PressButtonText 참조/초기 상태, 2_Result 표시 컴포넌트, EditorBuildSettings, BUTTON_INPUT/BLOCK_SIMULATOR/CHANGELOG/HANDOFF | 소유권 해제; main/9d45652 기준 로컬 커밋(git log -1); 2_Result는 사용자 미추적 씬에 컴포넌트만 추가; AssetKits 미수정 |
| LOG-002 | Claude | 완료 | GameManager, KeyboardButtonSimulator, GameLifetimeScope, BlockExperienceController, ComfyImageClient, GeneratedImageRetention의 Debug.Log 추가만, HANDOFF | 소유권 해제; 로컬 커밋 완료(push 없음); Unity 에디터 컴파일·Play 로그 확인 필요 |
| COMMIT-002 | Codex | 완료 | BLOCK/RETENTION/RESULT의 소스·씬·설정·테스트·문서·검증 JSON, 관련 사용자 폰트/SO | 로컬 커밋만 수행; 파일 소유권 해제; 해시는 git log -1 |
| RESULT-001 | Codex | 완료 | BlockExperienceController, Game ResultImage/TextGroup 참조/초기 상태, BLOCK_SIMULATOR/CHANGELOG/HANDOFF, validation/result_ui.json | 소유권 해제; main/50cbe9b 및 기존 미커밋 보존; 실제 생성·취소·실패 UI 검증 |
| RETENTION-001 | Codex | 완료 | 이미지 정리 정책/루트 서비스·테스트, GameLifetimeScope, BlockExperienceController/ComfyUISettings, BLOCK_SIMULATOR/HANDOFF/CHANGELOG, validation/retention.json | 소유권 해제; main/50cbe9b 및 BLOCK-001 미커밋 기반; 설정 기본1000장/512MiB 복구 확인 |
| BLOCK-001 | Codex | 완료 | Assets/Scripts/Blocks 및 메타, StreamingAssets/BlockGeneration, Tests/Blocks, Game 씬 추가 연결, 검증 JSON, BLOCK_SIMULATOR/COMFYUI_SETUP/CHANGELOG/HANDOFF | 소유권 해제; main/50cbe9b 기준 미커밋; 실제 RFID·Player·전체 물품 품질 후속 |
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

### [2026-11-10 / Asia/Seoul] IDLE-VIDEO-001 · Claude → 다음 작업자 · 완료(미커밋)

- 요청: 사용자가 추가한 Resources/Videos/c02.mp4를 타이틀 배경으로, 로고 등은 위에, 무한 반복.
- 변경: IntroVideo(RawImage+VideoPlayer+IdleVideoBackground)를 Intro_bg 다음에 배치(정지 배경은 첫 프레임 전·오류 시 대체). RT는 진입 생성/이탈 해제, 오디오 None, 첫 프레임 콜백만. 작성기 3개의 `GetComponent() ??` 가짜 null 문제를 TryGetComponent로 수정.
- 검증: Play9 PASS(반복 3회/13초, 순서, 팝업 아래, RT 해제·재생성), 콘솔 오류0. Player/장시간/실PC 디코딩 미수행. docs/validation/idle_video/README.md.

### [2026-11-10 / Asia/Seoul] UI-WEB-006 · Claude → 다음 작업자 · 완료(미커밋)

- 요청: LOGO-SPLIT-001에서 감지한 Figma 변경 반영.
- 변경: VideoEndPopupText_th(태국어 오탈자 수정), ButtonHelpBox_selectall_th(문구·배치) 원본 바이트 교체, meta 유지. 위치 동일해 JSON 무변경. 나머지 차이(≤23px)는 렌더 잡음으로 미교체. Figma 무수정.
- 검증: Play4 PASS(선택 화면 안내, 영상 종료 팝업), 콘솔 오류0. Player/실장비 미수행. docs/validation/ui_web6/README.md.

### [2026-11-10 / Asia/Seoul] LOGO-SPLIT-001 · Claude → 다음 작업자 · 완료(미커밋)

- 요청: 우상단 로고를 배경에서 분리해 별도 이미지로 배치. Figma는 원본 훼손 금지, 필요 시 복제→필요 요소만 남김→내보내기→복제본 삭제.
- 변경: 배경 6개(Intro/SelectGroupPanel/SelectAllPanel/RecommendedAdPanel_bg, SelectionBackground_th/en) 바이트 교체(meta 유지), 신규 UIs_Common/Logo/NSMLogo.png, Editor/LogoAuthoring.cs(배경 다음 형제 NSMLogo), ExhibitionUIBuilder.Background에서 호출, 씬 3개 추가만, Tools/UI/apply_logo_split.py.
- Figma: 복제본 5개 작업 후 모두 삭제, 원본 12프레임 확인. 복제 시 TH/En 섹션 폭이 4065로 자동 확장된 것을 2194로 복구. 실수로 섹션 전체 복제 1회→즉시 삭제. 재내보내기 비교 차이는 디자이너의 태국어 문구·이름 수정(영상 종료 문구 오탈자, 선택 화면 버튼 안내)로 판단, 미반영.
- 검증: Play12 PASS, 이전 스크린샷 대비 로고 상자 밖 RMSE≤0.08. 콘솔 오류0. Player/실장비 미수행.
- 다음: 디자이너가 고친 태국어 영상 종료 문구·선택 화면 버튼 안내 반영 여부 결정.

### [2026-11-09 / Asia/Seoul] TIMEOUT-HOLD-001 · Claude → 다음 작업자 · 완료(미커밋)

- 요청: 블록을 하나라도 올려 뒀다면 홈으로 돌아가지 않도록.
- 변경: BlockExperienceController.LateUpdate가 hub.HasPresentBlocks(그룹 우선으로 무시된 카드 포함) 또는 생성 중이면 InactivityTimer.Pause, 아니면 Resume(Resume이 카운트를 0부터 다시 시작). 매 프레임 적용해 전환 finally의 Resume에도 보류 유지. 생성 종료 시 블록이 있으면 재개하지 않음. OnDisable에서 해제. 타이머 이벤트 자체를 막으므로 move_idle_timeout 운영 로그도 보류 중에는 안 남음. 범위는 1_Game(그룹·선택)만.
- 검증(Claude, Editor Play, 블록은 허브 API): hold_runtime.json 12항목 PASS: 그룹 블록 45초·무시된 제품 35초·선택 4블록 40초 유지, 일부 제거 시 유지, 마지막 제거 후 마지막 활동 기준 30.0초 복귀, 결과 화면에서 해제. EditMode139/139. 첫 실행(hold_runtime_attempt1_failed.json)은 입력 이벤트를 세지 않아 마우스 입력으로 늦어진 97초를 실패로 판정한 스크립트 문제였고, 재실행에서 마우스 468건 확인. 콘솔 오류는 그 스크립트 예외 1건뿐.
- 미검증/위험: 실제 리더·Player. 보드에 블록을 둔 채 떠나면 그룹·선택 화면에 계속 머문다(요청대로). 실제 이미지 생성 후 보류 유지는 코드 경로로만 확인(AI 요청 안 함). 결과·광고 화면은 블록이 있어도 기존처럼 복귀.
- 종료: 소유권 해제.

### [2026-11-09 / Asia/Seoul] CHECK-RFID-TIMEOUT-2 · Claude → 다음 작업자 · 확인 완료(코드 변경 없음)

- 요청: UI-IDLEPOP-001 허브 변경 뒤 RFID 화면 30초 무입력 복귀 재확인.
- 결과(Editor Play): 그룹 화면 블록 없음 30.01초, 그룹 블록만 두고 방치 30.0초, 선택 화면 4블록(생성 안 함) 방치 30.0초에 홈 복귀(마지막 활동 기준). 블록을 남긴 채 복귀하면 타이틀 블록 제거 팝업 표시. docs/validation/rfid_timeout/runtime_20261109*.json.
- 관찰: 첫 측정 중 마우스 이벤트 3,073건이 타이머를 계속 초기화해 실제 대기가 길어졌고 전체 제한 시간 초과로 셋째 경우가 중단되어 단독 재실행함. 모든 입력 장치 이벤트가 초기화하는 기존 설계 그대로(사용자 결정 대기 유지). 실제 리더·Player 미시험. 소유권 없음.

### [2026-11-09 / Asia/Seoul] UI-IDLEPOP-001 · Claude → 다음 작업자 · 완료(미커밋)

- 요청: 웹 2번 타이틀 쪽 신규 팝업을 영상 종료 팝업과 같은 연출로 타이틀에 표시. 콘텐츠 시작 전 RFID 리더에 블록이 인식되면 띄우고 버튼 입력 차단. 언어 구분 없음.
- 변경: Group 149의 Rectangle 11은 VideoEndPopup과 픽셀 동일해 재사용, 문구 PNG 1개 추가. TitleBlockPopup(LateUpdate에서 HasPresentBlocks·타이틀·비전환 판단, 표시 요청~닫힘 완료까지 Presentation.LockInput). RfidReaderHub에 _present/ReportPresence/HasPresentBlocks/PresenceChanged, 규칙성 내부 비우기는 ClearReader, 홈 복귀는 ResetSession(존재 유지). RfidTcpReaders.ApplyPending이 규칙 판단 전에 존재 보고. 세션 인식 규칙은 그대로.
- 이유: TCP는 UID 변화 때만 보고하므로 홈 복귀 후 남은 카드와 타이틀의 그룹 우선 무시 카드는 기존 Readers로 감지 불가.
- 검증(Claude, Unity Editor): verify_title_block_popup Play25 PASS(양언어, 입력 차단/복구, 무시 카드, 홈 후 남은 카드, 미등록 카드, 빠른 반복), 웹 합성본 대비 RMSE 2.76, EditMode139/139. 첫 실행에서 최초 Play 시 GameManager 미생성으로 컴포넌트가 꺼지는 결함 발견→지연 조회로 수정 후 재실행. 콘솔 오류0.
- 미검증/위험: 실제 TCP 리더 UID 경로, 물리 버튼, Player 재빌드, verify_slots_flow 전체 흐름 미재실행. 다른 Unity가 TCP 10123 점유 중이었음.
- 종료: 소유권 해제. 새 치명적 장애 없음.

### [2026-11-06 / Asia/Seoul] UI-WEB-005 / AUDIO-002 · Claude → 다음 작업자 · 완료(미커밋)

- 요청: 웹 Figma 2번 프로젝트와 다른 리소스(특히 태국어 폰트) 감지·ZIP 일괄 적용, 기능 보존. Audios의 selected(RFID 감지 화면에서 카드 인식)·language(언어 변경)를 기존 AudioManager로 재생. 블록 없는 RFID 화면 30초 복귀 확인.
- 변경: Tools/UI/apply_web5_assets.py로 웹 원본 바이트 복사(meta/GUID 유지, before/ 백업). 태국어 UI14(좌표는 located.json 매칭값)·GroupSlot_th+GroupSlotLabel_th 재분리·ThaiBlocks16 교체, 연령4 Thai PNG 신규→ConfigureArtwork로 thaiSprite 연결·blocks_th.json 경로. 영어 그룹/선택 안내 896폭 x518. ExhibitionUIAssets 기본사본 갱신. AudioManager PlaySelected(프레임당 1회)/PlayLanguage, GameManager 토글 수락 시 language, BlockExperienceView.OnReaderChanged에서 새 카드(이전과 다름, 전환 중 아님) selected.
- 확인: 태국어 제품/색 칸·추천 상품 상자·배경은 웹과 동일해 미교체. 영어 영상 안내 크기 동일(영어 폰트 미변경). Figma에는 임시 내보내기 설정 추가/삭제만 함.
- 검증(Claude, Unity2022.3.62f3 Editor): verify_web5_runtime Play60 PASS(runtime.json), 무블록 화면 마지막 활동 뒤 30.0초 복귀·대기 중 입력 이벤트0·home 재생. 스크린샷 직접 확인. 콘솔 오류0, Play 중지/0_Idle. EditMode 테스트 미실행.
- 미검증/위험: Player 재빌드, 실제 리더·버튼, 스피커 청취. 자동 진행 시 selected+confirm 동시 재생은 미확정 기본값. CHECK-RFID-TIMEOUT의 입력 장치 신호 지연 위험·사용자 결정 대기는 유지.
- 종료: 소유권 해제. 새 치명적 장애 없음.

### [2026-11-05 / Asia/Seoul] DEPLOY-GUIDE-02 · Claude → 사용자 · 안내만(코드·파일 변경 없음)

- 요청: 대상 PC의 기존 Thai_02(Python·모델·배치파일)를 재사용해 Desktop/NSM_DNA_02 빌드로 업데이트하는 방법. 사용자가 StreamingAssets를 빌드에서 일부러 삭제함(대상 PC 것 사용).
- 확인: 이전 Player는 Unity-Startup-Template.exe. portable_boot는 package_manifest.json의 player로 Data 폴더를 찾으므로 이 값을 NSM_DNA_02.exe로 바꿔야 함(BOM 없이 저장). 대상 PC 사본이 없어 가장 가까운 Builds/RfidStable(11-02)과 비교: 174파일 중 Lang_En/Lang_Th(이미지14→27)·Settings.json(targetFrameRate 60→0)·build_info만 다름. UnityPlayer SHA 동일.
- 미검증: 대상 PC 실제 파일, 안내 절차 실행, 이 빌드의 Player 구동. 소유권 없음.

### [2026-11-05 / Asia/Seoul] CHECK-RFID-TIMEOUT · Claude → 다음 작업자 · 확인 완료(코드 변경 없음)

- 요청: RFID 인식 창에서 30초간 인식이 없으면 최초 화면으로 복귀하는지 확인.
- 결과: 구현됨. 전용 RFID 타이머가 아니라 템플릿 InactivityTimer(useInactivityTimer true/resetTime 30) + GameManager 구독. 블록 변화, 버튼, 모든 Input System 이벤트(마우스·키보드 등)가 초기화. 같은 블록 반복 인식은 초기화하지 않고, 생성 중·영상 중에는 멈춤.
- 검증(Claude, Editor Play): docs/validation/rfid_timeout/runtime.json. 마지막 입력 이벤트 뒤 복귀까지 30.01/29.48/30.00초, 3상황 모두 리더 초기화. 첫 측정은 이벤트 954건으로 104초, 셋째는 224건으로 54.6초 지연되어 AUDIO-001의 56.9초 추정을 뒷받침(이벤트 발생 장치는 미확인). 블록은 Hub에 직접 입력, 실제 TCP 리더·Player 미시험.
- 미해결/결정 대기: 설치 PC에서 입력 장치 신호가 계속 들어오면 복귀가 지연·차단될 수 있음. RFID·버튼만 세도록 바꿀지 사용자 결정 대기. 소유권 없음.

### [2026-11-05 / Asia/Seoul] UI-ADHINT-001 · Claude → 다음 작업자 · 완료(미커밋)

- 요청: 광고 화면에서 버튼 입력이 가능한 시점에 버튼 안내를 페이드 표시. 후속: 폭죽 재생 중에도 3초 후 입력 가능. 이 기록이 AD-FX-002의 "컨페티 완료까지 입력 잠금" 규칙을 대체한다.
- 변경: AdProductHighlight에 InputLockSeconds=3, PlayEntranceAsync는 입력 해제까지만 대기하고 RunFiniteEffectsAsync가 자체 CTS로 효과를 끝까지 진행(StopEffects/OnDisable에서 취소). ResultVideoController.advertisementHint + LateUpdate 판정. 2_Result 참조 1개(직전 사본 validation/ad_hint/before), Builder 동기화.
- 검증(Claude, Unity2022.3.62f3/1920×1080): Play34 PASS(양언어), 입력 해제 3.006/3.000초, 최종 EditMode139/139 PASS. 교차검증 없음. Play 중지/0_Idle dirty false.
- 미검증/위험: Player 재빌드, 실제 버튼, 장시간. 구 verify_ad_highlight*·verify_ad_fx*는 이전 규칙 전제로 미재실행. verify_slots_flow 미재실행. 3초 시점에 누르고 있던 버튼은 기존 규칙대로 한 번 떼야 인식.
- 종료: 소유권 해제. 새 치명적 장애 없음. docs/validation/ad_hint/README.md.

### [2026-11-05 / Asia/Seoul] AUDIO-001 · Claude → 다음 작업자 · 완료(미커밋)

- 요청: 전역 오디오 매니저, 버튼으로 화면 전환 시 confirm, 최초 화면 회귀 시 home 재생.
- 변경: 신규 Core/AudioManager(Resources/Audios 로드, AudioSource 1개). 템플릿 SoundManager는 StreamingAssets+Settings 항목을 요구하고 씬에 없어 쓰지 않음. GameManager.BeginTransition에서 대상 씬으로 분기, StopSessionMedia에서 매니저 소스 제외. BlockExperienceView 그룹→선택, ResultVideoController.PlayAsync 광고→영상. 사용자 WAV 2개와 meta는 미추적 상태 그대로.
- 사용자 미확정 기본값: 자동 진행도 confirm, 시간 초과·블록 제거도 home, 생성 시작/언어 전환은 무음.
- 검증(Claude): Play32 PASS, 7개 시점 믹서 peak 0.325~0.330, 매니저 1개 유지. 스피커 청취 안 함. UI-ADHINT-001 이전 실행이며 재실행 안 함. 교차검증 없음.
- 미검증/위험: Player 재빌드, 실제 버튼, 현장 음량. 시간 초과가 30초 설정에도 56.9초에 발생, InactivityTimer가 모든 입력 이벤트에 초기화되므로 검증 중 PC 입력 때문으로 추정(미확인, 기존 동작). 설치 PC에서 마우스·키보드 신호 노이즈가 있으면 복귀가 늦어질 수 있어 현장 확인 권장.
- 종료: 소유권 해제. 새 치명적 장애 없음. docs/validation/audio/README.md.

### [2026-11-05 / Asia/Seoul] UI-ATTN-001 · Claude → 다음 작업자 · 완료(미커밋)

- 요청: 블록 인식 화면에서 3초간 블록을 올리지 않으면 놓을 곳의 텍스트가 살짝 커졌다 돌아오는 DOTween 연출, 요구보다 많은 블록이 인식되면 기존 블록이 좌우로 진동하고 유지 시 3초에 1회 반복. 후속 지시: 모든 인식 화면 동일 적용, 텍스트 연출은 필요한 블록 중 랜덤, 텍스트도 3초에 1회 반복.
- 기준/담당: main/3852ee6 + 기존 미커밋 보존, Claude 단독. 커밋/push/배포 빌드 없음.
- 변경: RfidReaderSelection.IsSlotOverfilled, 신규 BlockAttentionSchedule(순수 타이밍), BlockExperienceView의 LateUpdate 판정·Tween·정리. 칸 PNG에 글자가 구워져 있어 Tools/UI/extract_slot_labels.py로 글자 PNG6·덮개 PNG1 파생(웹 ZIP 원본 아님, 원본 칸 PNG 무수정), author_slot_labels.cs로 1_Game의 5칸에 SlotLabelCover/SlotLabel 추가·바인딩5개. 언어 JSON/ExhibitionUIAssets, Builder/UIMotionAuthoring 동기화.
- 사용자 미확정 기본값: 색 초과는 표시된 색 두 칸이 함께 진동, 같은 ID 블록 2개도 초과로 처리, 초과 진동 시작은 조건 발생 즉시, 빈 칸 글자는 직전 칸을 다시 뽑을 수 있음. 튕김 18%/0.55초, 진동 10px/0.45초는 BlockExperienceView 상수.
- 검증(Claude, Unity2022.3.62f3/1920×1080): EditMode139/139, Play64(runtime.json), 칸 회귀152(slots_regression.json, 스크린샷 생략), 반복 간격 실측3.00~3.02초, 대기 칸 픽셀 RMSE 0.114/0.13. 스크립트 조건 오류로 3회 중단 후 스크립트만 수정(README 기록). 교차검증 없음. Play 중지.
- 미검증/위험: Player 재빌드·배포, 실제 RFID(이번 Play도 10123 AddressAlreadyInUse로 소켓 미시험, 점유 프로세스 미확인)·물리 버튼, 장시간. verify_slots_flow는 이번에 재실행하지 않음. 그룹 화면 초과 진동은 계속 안내 페이드 뒤 약0.28초에 시작. 실제 리더의 인식 지연은 별도.
- 종료: 소유권 해제. 다음 Windows 빌드 필요. 새 치명적 장애 없음. docs/validation/ui_attention/README.md.

### [2026-11-05 / Asia/Seoul] UI-SLOTS-002 · Claude → 다음 작업자 · 완료(미커밋)

- 요청: 옆의 Figma 웹과 프로그램을 비교해 남은 변경을 모두 적용(MCP 금지, 일괄 ZIP). 칸 용도가 정해졌으므로 리더 위치가 아닌 용도별 배치로 변경, 그룹→선택 전환은 그룹 칸이 줄어들며 이동. 선택 화면의 그룹 그림은 예시이며, 그룹이 빠지면 그룹 화면 칸 리소스를 재사용.
- 기준/담당: main/3852ee6 + 기존 미커밋 보존, Claude 단독. 커밋/push/배포 빌드 없음. 이 기록이 UI-WEB-004의 '기존4칸 유지'와 GROUP/리더 순서 색상 규칙(리더 번호 오름차순)을 대체한다.
- 변경: RfidReaderSelection에 용도 칸 투영(DisplayBlock, 도착 순·유지·재충전), Color1/2가 칸 순서를 따름. BlockExperienceView는 groupSlot 1개+selectionSlots 4개, 칸이 차면 프레임 Image 숨김. UIBlockSlotTransfer는 그룹 칸 1개만 이동. LocalizedImageSet Binding.keepObjectName, 피벗 반영 배치, 스트레치 이미지는 스프라이트만 변경. 1_Game은 GroupFrame1~3 삭제, GroupFrame0을 GroupSelection 직속으로 이동. BoardImage/SelectGroupPanel_bg/SelectAllPanel_bg 바이트 교체(meta 유지), 칸 PNG6·언어별 선택 배경2 신규. 언어 JSON 항목 추가, 그룹 제목/부제 웹 위치. Builder/UIMotionAuthoring/RfidPlayerSmoke 갱신. Tools/UI/apply_slots_assets.py·author_slots.cs·verify_slots_runtime.cs·verify_slots_flow.cs.
- Figma: 일시적으로 10개 노드에 내보내기 설정을 넣고 제거, 배경용 복제 프레임 8개를 만들어 내보낸 뒤 삭제(원본 프레임 무수정). 다른 공동 작업자에게 잠시 보였을 수 있음.
- 검증(Claude, Unity2022.3.62f3/1920×1080): EditMode135/135, Play131(언어별 배경 연결 전)·전체흐름48(최종 리소스), 전환 0.484/0.493초·424→252 단조 감소. 웹 대비 칸 RMSE1.62, 보드 3.27(웹의 배경 흐림은 PNG로 재현 불가). 교차검증 없음. Play중지/0_Idle dirty false.
- 미검증/위험: Player 재빌드·배포, 실제 RFID(이번 Play는 다른 Unity PID7412가 10123 점유로 소켓 미시험)·물리 버튼, 영상 전체 길이, 장시간. 구 도구 verify_motion/runtime/refresh_runtime/web4_runtime은 네 칸 구조 전제로 실패함. 선택 화면 그룹 빈 칸은 424 리소스를 252로 축소한 것(사용자 지시). 칸보다 많은 같은 종류 블록은 먼저 놓인 것만 표시(사용자 미확정 기본값).
- 종료: 소유권 해제. 다음 Windows 빌드 필요. 새 치명적 장애 없음. Downloads에 임시 ZIP(.tmp) 1개 남음, 사본은 validation/ui_slots에 보존.

### [2026-11-05 / Asia/Seoul] UI-WEB-004 · Codex → 다음 작업자 · 완료(미커밋)

- 요청: 옆의 Figma 웹 2번 콘텐츠 UI를 현재 프로그램과 비교하고 변경점을 일괄 ZIP 다운로드·적용. Figma MCP 금지, 동작/연출 보존. 후속으로 영상 재생 좌상단 안내 확인 요청 포함.
- 기준/담당: main/3852ee6 + 기존 미커밋 전체 보존, Codex 단독 구현/검증. 타 작업 소유권 해제 확인 후 순차 작업. 커밋/push/배포 빌드 없음.
- 변경: 브라우저64PNG ZIP과 영상안내2PNG ZIP 보관. 기존 UI15PNG를 원본바이트로 교체(meta/GUID보존), 영상 홈 안내2PNG/meta 신규. 언어JSON/라이브러리 기본사본, ResultVideoController의 안내 오브젝트 표시/숨김6줄, Result씬의 안내/바인딩, Builder4줄 및 전용도구/문서.
- 경계: 웹의 연령1칸은 현재4리더와 칸 이동 연출에 맞지 않아 기존4칸/위치·헤더y 유지. 영상 안내는 기존5초홈 동작을 보여주는 정적 Image이며 입력/Skip/재생 로직/타이밍은 바꾸지 않음. 기존107씬객체 중3개에 안내 참조/자식/바인딩만 추가·삭제0. 기존688파일 중666SHA동일; 의도21파일 외 Reporter자동생성build_info1개 갱신. 기존 작업/원본영상/효과/입력·RFID·생성/설정·패키지 보존.
- 검증: Unity2022.3.62f3/1920×1080, 컴파일오류0·바인딩30. 실제양언어6화면/입력/미디어98항목 PASS, 실패주입/재시도/재생중5초홈/반복/준비중취소·RT해제10 PASS. 정적요소26픽셀 RMSE최대0.1003, 영상좌상단2개 원본불투명픽셀 RMSE0. 실제12화면 직접확인. 실패주입의 영문오류2건은 의도한 결과이며 그 외 오류없음. 교차검증 미수행.
- 증거/미검증: [ZIP·매핑·실제화면·검증](../validation/ui_web4/README.md). 영상은 끝부분seek, 전체길이시청/Windows Player재빌드·배포/실물버튼·RFID/장시간 미수행. 기존배포본은 그대로, 다음빌드 필요.
- 종료: Play중지/0_Idle dirtyfalse, 소유권 해제. 새 치명적 장애 없음. 기존 현장검수 TODO 유지.


### [2026-11-04 / Asia/Seoul] RENAME-001 · Codex → 다음 작업자 · 완료(미커밋)

- 요청/기준: 프로젝트 이름과 빌드 실행 파일명을 NSM_DNA_02로 변경, 빌드 실행 금지. main/3852ee6 및 기존 사용자/에이전트 미커밋 변경 보존. Codex 단독 구현·검증, 교차 검증 없음.
- 변경: ProjectSettings/ProjectSettings.asset의 productName만 NSM_DNA_02로 변경. Tools/ComfyUI/build_player.cs 출력은 Builds/ComfyAutostart/NSM_DNA_02.exe. 현재 Unity의 Windows64 저장 경로는 기존 Desktop/Thai_02 디렉터리에서 파일명만 NSM_DNA_02.exe로 변경(로컬 에디터 상태). CHANGELOG 갱신. 프로젝트 폴더명/원격 저장소명·Company MyCompany·Standalone 식별자 com.Unity-Technologies.com.unity.template.urp-blank·빌드 3씬·패키지는 보존.
- 실제 검증: 실행 중 Unity 2022.3.62f3에서 Product Name/Windows64 출력 경로를 다시 조회해 일치 확인, Settings diff는 productName 한 줄만 변경, 빌드 스크립트 출력명 및 git diff --check 확인. Play/build 상태 false, 활성 씬 dirty false, 씬/build_info 추가 변경 없음.
- 미수행/위험: 사용자 요청에 따라 Play/컴파일/빌드·새 실행 파일 검증 미수행. 기존 배포 실행 파일은 변경하지 않았고 다음 빌드부터 적용. Product Name에 따른 기본 로그/영구 데이터 경로 변경 가능, 기존 데이터 자동 이관 미수행. 기존 RFID 패치/과거 검증 도구의 구 배포 이름은 과거 배포본용으로 보존.
- 종료/다음: 소유권 해제, 커밋/push 없음. 다음 Windows 빌드 시 NSM_DNA_02.exe 및 NSM_DNA_02_Data 확인. 신규 치명적 장애 없음.

### [2026-11-02 / Asia/Seoul] RFID-STABLE-001 · Codex → 다음 작업자 · 수정/자동검증 완료(미커밋)

- 요청/확정: 같은카드값순환 원인분석/수정. 사용자 LAN-754(KA118)·실제7바이트값2회반복·고정길이원치않음 확인. main/3852ee6+기존변경보존, Claude RFID소유권해제 후 단독구현/검증, 교차검증 없음.
- 원인: 기존 FixedLength16이7바이트보고를 이어잘라 시작점2바이트씩이동/7종가짜UID로순환. 가상동형UID로 재현해 before_seven.json 저장. 실제장비는35초허용IP수신에서접속0, 현장실측통과로보고안함. 제조사 UID필드/패킷계약확보못해 헤더/CRC 임의제거안함.
- 변경: RfidConfiguration의기본UidBurst·수신공백까지누적/정확반복단일값·새값2회확인/재접속초기화, TcpReaders연결 및 F10바이트수, RFID JSONframing만변경. 기존TCP서버/IP/1초Presence/그룹우선/생성freeze·매핑목록보존. 테스트/Tools/RFID 및 운영문서/CHANGELOG/TODO/검증/HANDOFF 추가갱신, 씬/프리팹/패키지/Comfy미변경.
- 검증: 전체Edit130/130(14.1346326초/job efedabf7bafa4fbdb83925096c579d96), 새실소켓4클라이언트16검사(4/7/10/16bytes·중복/분할/선행0·5.5초유지/교체/제거/매핑/재접속/생성잠금) 및 기존67회귀 PASS. 스크린샷직접확인, 임시JSON만저장/설정·입력복원. Windows3씬 None빌드 오류0/기존경고5·9.766초, Builds/RfidStable. Play중지/Idle dirtyfalse, 최종콘솔오류0. 빌드시읽은preloaded목록복원/기존ProjectSettings미커밋보존.
- 배포: Desktop/Thai_02 DLL2와framing적용, DLL/JSON백업 RFID_Backup_20261102_202135_210. 보호7파일SHA동일·매핑/IP/그외JSON보존. WindowsPowerShell5.1업데이트실행/추가2매핑·별도port/추가필드보존fixture PASS. Builds/Desktop RFID_Update.zip 98252바이트. 적용은앱완전종료→실행파일폴더에압축풀기→Apply_RFID_Update.cmd→재시작. 잘린16바이트로등록한값은F10의새UID로재등록필요. 배포도구중간실패는사전거부/원본복원후수정·최종재적용검증(README).
- 한계/다음: UidBurst는조각지연보다길고보고묶음간공백보다짧은100ms기본조건 필요. 무공백스트림/묶음내다른카드혼합/내부반복패턴UID는프로토콜경계명시없이는보장불가, Delimiter등실계약필요. 실제KA118공백/보고간격·정확한UID필드·Player직접화면·장시간/설치PC미검증, 고유값의필드구조를확정한것은아님. 운영로그에UID전송없음, 새치명장애발견없음. 커밋/push없음·소유권해제. [상세](../validation/rfid_stable/README.md).

### [2026-11-02 / Asia/Seoul] ALPHA-OPAQUE-001 · Codex → 다음 작업자 · 수정/검증 완료(미커밋)

- 요청/기준: 흰색 물체 내부 투명화와 다른색 동일결함만 수정. main/3852ee6+기존변경 보존, 단독구현/검증·교차검증 없음. Claude RFID-LISTEN/PRESENCE 최신변경 및 에디터 조작을 건드리지 않았다.
- 원인/구현: 흰배경+흰물체의 BiRefNet 오분리를 재현. Catalog의 배경문구1개만 중간회색/opaque로 변경하고 원본RGB의 흰색·유색·어두운 재질을 기본노드로 보완한다. white RGB곱0.65/chroma0.12/dark0.015, 신뢰도0.5 내부1px침식만 기존알파에 가산해 RGB/원래외곽알파를 보존. 자전거는 원래 마스크로 분기해 바퀴/프레임 틈 유지. 모델/추론횟수/샘플러/4단계/스타일prefix/색HEX/연령 불변, 최종480RGBA.
- 파일: Tools/ComfyUI/build_workflow.py·alpha_mask_nodes.py·test_workflow.py·API/UI/inputs, Assets/StreamingAssets/BlockGeneration의 Catalog/API, 신규 inspect_alpha/probe_alpha 계열·verify_alpha_workflow/verify_alpha_native·deploy_alpha_workflow, 운영문서/CHANGELOG/TODO/HANDOFF·validation/alpha_opaque. C#/씬/프리팹/패키지/원본그림 미수정.
- 실제검증: 10물건·6색·4연령 포함100개 실생성물에 최종마스크 재실행(verified100), 최종전체파이프라인 새4개(end_to_end). RGB동일/내부255/배경0/외곽알파 정확보존 등8조건 PASS, 잘못된 흰색/유색내부 후보0. 자전거3자동후보는 실제빈공간으로 직접확인해 유지. 실제기본노드6색 손상주입12케이스·단위9·새도구 py_compile PASS. 대비색체커 시트4장/전후사진 직접검수. 최종기준 sourceSHA/summary.json·README에 기록, 중간실패실험은 진단으로 보존.
- 배포: Desktop/Thai_02 기존JSON2개 백업 후 좁은필드/노드 패치, 기존EXE/DLL·Settings/RFID5SHA동일. Builds 및 Desktop/Thai_02에 동일 Alpha_Workflow_Update.zip(7647바이트, JSON2개+README). USB복사본에 압축풀어덮어쓰기 후 앱완전재시작; 두JSON 함께적용 필수. 모델재설치/Player재빌드 불필요.
- 한계/다음: 현5070TiLaptop API검증이며 실제3060/새PC·Unity/Player화면/장시간은 미수행. 향후 모든랜덤시드 무결점 보장으로 확대하지 않는다. 기존8188서버 유지/테스트완료, 재시작·전역interrupt·모델교체 없음. 커밋/push 없음·소유권해제, 새 치명장애발견없음. 상세 [검증](../validation/alpha_opaque/README.md).

### [2026-11-02 / Asia/Seoul] COMFY-PORTABLE-001 · Codex → 다음 작업자 · 패키징/현PC검증 완료(미커밋)

- 지시/기준: Desktop/Thai_02 기존빌드에 새PC Comfy준비물 전부동봉. Win10/11x64·RTX3060 12GB 확인, main/3852ee6+기존변경보존·단독구현검증/교차검증없음. 사용자최신2:04빌드보존, 이번Unity빌드/C#·씬·패키지변경없음.
- 전달: 약20.3GB/54,463파일, 독립Python3.13.12+정확한venvsitepackages/_pth격리/Torch2.11cu128/Comfy0.42·frontend1.57.1/기본노드·4모델/스타일·라이선스. 공식Studio617.42Desktop(RTX3060/Win10/11지원)/VC++14.51x64 서명Valid. Run_Thai02.cmd/Check_Runtime.cmd/한국어README. 배포Comfy설정만원자갱신/원본백업, EXE/DLL·Settings/RFID5SHA동일. 운영자는필수드라이버/VC++설치·재부팅후Run실행, Python/Desktop/Git/CUDA Toolkit별도설치불필요.
- 검증: 모델4고정SHA/바이트·설치서명2·GPU연산·한글공백다른경로prefix/설정재작성·다른cwd PowerShell준비·py_compile PASS. 별도8189동봉서버3새생성8.146/2.429/4.561초(현5070TiLaptop/32GB,3060아님),3개480RGBA/alpha0..255/모서리0. package/diagnostics/relocation/installers/generation JSON·PNG증거.
- 실패/수정: [TS-20261102-01](../TroubleShooting.md#TS-20261102-01) /XD models basename으로내부ldm누락, 루트절대경로제외로수정/재복사후3실생성재검증해해결. 백그라운드기동자동검토차단은좁은foreground기동으로검증. 임시트리삭제자동검토차단은삭제하지않고절대경로확인후Temp/PortableRelocationValidation_20261102로파일보존이동, 배포본에는없음.
- 정리/한계:8189 Ctrl+C/Stopped server·동봉Python프로세스0. 기존8188 PID17356유지, queue유휴확인후/free로캐시만해제(다음사용coldreload가능). 호스트드라이버설치/방화벽/권한/전역interrupt/Windows종료없음. 실제빈Windows/3060·PlayerGUI입력/RFID/장시간미검증. 기존Player직접실행없어운영API추가전송안함. 커밋/push없음·소유권해제.
- 파일: 신규Tools/ComfyUI/package_portable.py·portable_boot.py·portable_server.py·portable_start.ps1·Run_Thai02.cmd·Check_Runtime.cmd·PORTABLE_README_KO.txt·verify_portable_relocation.py, 대상폴더추가파일/Comfy설정백업·COMFYUI_SETUP·validation/comfy_portable·TroubleShooting/CHANGELOG/TODO/HANDOFF. 재패키징도구는로컬환경복사를담당하며Installers/추가라이선스다운로드는이번별도절차였다.

### [2026-11-02 / Asia/Seoul] RFID-TCP-MAP-001 · Codex → 다음 작업자 · 구현/자동검증 완료(미커밋)

- 요청/확정: 네 칸IP121/122/123/124·TCP10123·HEX. 후속16바이트 예시는 의미 해석 없이 전체카드값으로 매핑하고 예시자체는 등록하지 않음. main/3852ee6+기존변경보존, 단독구현/검증·교차검증 없음.
- 구현범위: 신규 Data/RfidConfiguration(HEX검증·고정/구분자/공백프레임)·RfidTcpReaders(4개독립클라이언트/자동재접속/메인스레드제한큐)·RfidMappingPanel(F10실시간/카드선택/20블록/저장교체삭제/재로드) 및meta, StreamingAssets/RFID/RfidSettings.json/meta. Hub.Start가1개씩연결, Simulator는F10중F9차단. 씬/프리팹수정없음. 정상설정만교체·원자저장/백업·오류노출, F10전시입력잠금/생성중조회만. 카드ID운영로그전송안함.
- 기본값: FixedLength16/asciiHexfalse/UID전체, 실IP순서와10123, cards빈배열. 제거코드/보고주기미제공으로removedValues빈배열/absentTimeout0. 제거신호없이연결만유지되면마지막UID유지하므로 현장확인후설정필요. 선택의생성freeze/그룹우선/색순서기존규칙보존.
- 실제검증: Edit112/112(job441c6db156134733ae37bed5f7892f28,12.3784146초), 실제Loopback TCP4서버 Play44 PASS(16바이트예시분할수신전체보존·저장/교체/삭제/실패/재로드/단절재접속/F10/F9/버튼잠금/생성lease). 임시경로저장만사용하고원본JSON카드0개유지. 최종창캡처직접확인. 중간Input이름충돌컴파일오류수정후전부재검증, 실패주입6진단보존. 최종실제Windows3씬 BuildOptions.None 오류0·경고0,8.9479544초·동봉JSON SHA동일. 최초미컴파일경고는refresh후재빌드해해소. 상세README참조.
- 보존/최종상태: 기존미커밋입력/Comfy/광고/언어/자원유지, 씬/vendor/효과/패키지diff0. 빌드preloaded추가1개만원복, 기존VContainer유지. Play중지/0_Idle dirtyfalse. Tools/RFID/verify_tcp_mapping.cs·docs/RFID_SETUP.md·BLOCK_SIMULATOR/BUTTON_INPUT/CHANGELOG/TODO/HANDOFF·검증JSON/PNG추가갱신. 커밋/push없음/소유권해제.
- 미검증/다음: 실제장비네대·현장망·실제제거HEX/보고주기·WindowsPlayer마우스직접조작·장시간. 장비가조회명령을요구하면제조사계약추가필요. 이번작업은새Comfy생성미수행(동일freezeleaseTCP검증), 기존이미지생성코드/워크플로미변경. 치명장애새발견없음.

### [2026-11-02 / Asia/Seoul] AD-FX-005 · Codex → 다음 작업자 · 완료(미커밋)

- 요청/기준: 내부 이펙트가 작고 흐릿한 경우 보강, 박스 밖에 보이지 않게 마스킹, 폭죽은 모든 광고요소보다 전면. main/3852ee6+기존 변경 보존, Codex 단독 구현/검증·교차검증 없음.
- 구현: AdProductHighlight만 제품소스 수정. 빛살740~860·링620~780·별54~94/알파/빈도 증가, 순백색 음영으로 구분. AdBoard Sprite를 복사한 showMaskGraphic=false Image+Mask를 board12px 안쪽에 한 번 만들고 내부 effectRoot를 이관, 모든 내부 graphic.maskable=true. 원본효과/씬 미저장. 폭죽 Canvas overrideSorting/32766을 활성화 후 적용하고 광고이미지·제목·안내 전면에 표시한다. 전환검정32767은 유지. Clear에서마스크/전면Canvas숨김, Resume은내부마스크만, 별Stop(false)로남은수명유지.
- 검증: 실제 Editor렌더47 PASS(빨강파랑·검정흰색·흰색흰색). 거대한2600×1800 probe가 박스밖샘플변화0, mask해제대조군>500변화·박스안보임·둥근코너가림. 불투명전체UI lastSibling 위 폭죽샘플13843, 검정전환의밝은샘플0. 물건Rect밖 가시샘플2143/2621/1583. 취소·비활성/재개·중복/영상암전·첫프레임0/홈8 PASS. 실제 Comfy 새가방 #EC6868/#5FB0E3 버튼흐름/최종PNG직접확인·홈 PASS. 앞47은이전고정PNG fixture로생성품질과분리했으며실제새출력은별도preview.json. UI변경이므로Data NUnit 재실행은하지않음.
- 중간수정/진단: 추가Canvas의 Unity가짜null을 명시적bool검사로 처리하고 구성중IsPlaying노출/비활성초기화정렬을보정했다. refresh연결실패후 이전DLL을검수한실패는제외하고DLL시각>소스확인후재실행. 해당최종47시콘솔오류0. 실제생성미리보기도구의 NonReadableTexture.EncodeToPNG 오류는도구에서제거하고제품텍스처설정불변으로정상재실행, 중간로그보존. 표준UI/Default인것을확인해셰이더변경없음.
- 빌드/보존: Windows3씬 BuildOptions.None 성공/오류0·기존경고5,9.868초. Builds/ComfyAutostart/Thai02.exe 갱신. 씬/효과프리팹/vendor/패키지 diff0, 기존 생성팔레트/입력/로그 설정/사용자변경 보존. InputSystem이추가한 preloadedAssets1개만원복, Play중지/0_Idle·dirty false. 소유권해제/미커밋·커밋push없음. Player광고육안·설치PC/실물/장시간은미검수. 치명장애발견없음. docs/AD_PRODUCT_HIGHLIGHT.md·validation/ad_fx5의README/JSON/PNG참조.

### [2026-11-02 / Asia/Seoul] AD-FX-004 / LOG-003 · Codex → 다음 작업자 · 완료(미커밋)

- 요청/기준: 광고 연출 규칙성 완화·박스 내부 효과를 이미지 생성에 사용한 두 색상코드로 다양하게. 이어 지정 HULIAC content_log API의 start/end/end_kill/move_idle/move_idle_timeout/Call API/Return OK/Return fail 사용, Comfy 제외. main/3852ee6+기존 사용자/이전기능 변경 보존, Codex 단독·순차 수행/검증, 교차검증 없음.
- 광고: Controller의 유효 revision에서 카탈로그HEX2개를 결과Texture와 함께 Manager에 넘겨 이후RFID/언어에 흔들리지않고 홈/결과해제에서 함께 비운다. 런타임AdProductHighlight가 중첩 링 포함3emitter를 캐시하고 두색+옅은변형의 입자혼합, Random별 위치/크기/수명/지연/느린 이동, 링의 가변 rate/크기/수명/작은중심분산, 빛살 회전/주기변화를 적용한다. 좌우컨페티 지연·상단시점/양도 분산. 원본효과프리팹·씬/vendor 미수정. 중간시각검수에서 speed×100/fitRect를 확인해 보정하고 별 정점 검사 추가.
- 광고검증: 실제새3이미지/빨강파랑·초록노랑·검정흰색 Play39 PASS, RGB전달/후속RFID 무영향/별·파동16px내부/TimeScale0/간격·색변화/입력복구/홈 확인. 기존PNG fixture로 취소/비활성/재개/중복/영상암전·첫프레임0 등8 PASS. 최종전체컨페티 분산후 새가방1개 추가생성14 PASS·최종실화면확인. Edit91/91(job f4d32d1cdc65480f8771599125318926,10.4465917초). 신규14검증은이후컨페티 변경을포함하고 이전수명8은동일정리코드회귀다.
- 로그: APIManager는 기존Base의Singleton/DI/구독/종료보류 보존. 정확한 start/end/Return fail 문구를 적용하고 restart/종료이유/실패사유 접미사 제거. 기존수동/Timeout이벤트사용·Idle자체timeout무시. ComfyClient는계측하지않고 헬퍼/이벤트에서loopback·설정된Comfy origin·Comfy호스트·logAPI 제외, URL민감정보가림. 별도 WindowsPowerShell감시가PID+시작시각의앱종료를기다리고정상표시없으면end_kill전송. PC종료/방화벽/UAC 변경없음.
- 로그검증: 로컬실수신15 PASS(실제Scene수동/timeout/외부성공·실패/원본예외/URL인코딩·민감값/Comfy로컬·원격IP·실제system_stats제외/HTTP503제한). WindowsDevelopment 실제정상start,end/강제start,end_kill 등5 PASS, 최종Release도강제종료start,end_kill확인. 실제운영서버 직접연결start/end HTTP200, 본문확인용추가end1건에서logged/ok:true. 대량/실패/종료시험은로컬만사용, 운영관리화면 저장목록은미검수.
- 빌드/종료: 실제3씬 Windows 배포BuildOptions.None 성공/오류0·기존CS4014 4개+Editor필드CS0414 1개유지. Builds/ComfyAutostart/Thai02.exe 및동봉폴더에최종운영설정·watcher포함/SHA일치. 검증사본임시apiUrl는원본으로복원. InputSystem빌드디스크추가preloadedAssets1개만원복, 기존InputManager변경보존. 로컬fixture51692/소유테스트Player/watcher정리, 기존Comfy8188보존. Play중지/Idle·dirty false, 소유권해제·미커밋/커밋push없음.
- 참고/남은항목: 다른프로젝트기동으로MCP포트6400/6401이변경돼도구응답대기를중단하고exact Name@hash로재고정후마무리. Console의의도한503실패진단은보존, 새컴파일오류없음. 씬/효과prefab/vendor/패키지git diff0. 물리버튼/RFID·Player광고육안·설치PC/장시간·전원/망단절전달보장은미검증. 치명장애발견없음. docs/AD_PRODUCT_HIGHLIGHT.md·CONTENT_LOG.md·두validationREADME/JSON/PNG참조.

### [2026-11-02 / Asia/Seoul] COMFY-AUTOSTART-002 · Codex → 다음 작업자 · 적용/자동검증 완료(미커밋)

- 요청/기준: Desktop GUI를 따로 켜지 않고 빌드가 Python 서버를 직접 시작하며 기존서버 재사용. 후속으로 서버 준비 전 RedButton/숫자1 차단·타이틀 버튼 안내 숨김, 네트워크 알림 질문(사용자가 직접허용). main/3852ee6+기존변경 보존, Codex 단독 구현/검증·교차검증 없음.
- 구현: 기존 .venv Python/main.py/모델·입출력 인자는 실제 유효해 유지했다. standalone-env 자식만 보고 Python 경로를 바꾸면 torch누락이 발생함을 읽기시험으로 확인. launchServerInEditor를 C#/JSON에서 true. Scope가 Launcher를 Manager에 연결, IsInputReady는 설정+활성 Launcher.Ready를 요구하며 차단 중 제스처 Reset/타이머 무갱신. UI idle.help Image.enabled를 OnEnable부터 차단하고 LateUpdate에서 준비상태와 동기화해 언어페이드/카드연출로 노출되지 않게 했다. 첫 Play시험에서 초기 UI가 매니저보다 빨리 준비되어 캐시null인 문제를 확인해 Manager OnEnable/SceneChanged의 명시적 BindInputManager로 고친 뒤21 PASS.
- 검증: Unity2022.3.62f3 Edit91/91(job c4633c284c3749bca0c88330aa33ae8f,14.4441981초). Play21은 주입한3서버상태/실제 키보드어댑터/직접 버튼API·준비 전 유지/해제 무효·새누름/언어왕복/비활성·홈검사. 실제Windows3씬 Mono 배포빌드 오류0·경고0(BuildOptions.None), 실제Player11항목 서버 cold/reuse/실행파일누락 실패·로컬바인딩·부모프로세스·종료후서버생존·Desktop신규0 검증. 준비27.229초는 현PC서버시작 시간. 별도18192 Python에서 실제Klein480이미지1개11.936초 완료. 세부 JSON/스크린샷/도구와한계는 docs/validation/comfy_autostart/README.md.
- 알림/GUI: 개발빌드 네트워크알림을 사용자가 직접허용했으며 방화벽변경/UAC/보안프롬프트 자동조작 없음. 배포빌드 headless/실제GUI 모두 Player TCP listen0·UDP0, 서버127.0.0.1만 바인딩. 실제GUI 준비후타이틀안내 직접확인. Computer Use 숫자1의 Game진입은 확인되지 않았고 추가 숫자패드 검수에서 사용자가물리Escape로중단해 더이상 UI조작하지않았다. 전체Player입력/실물버튼/RFID·다른PC·장시간/3060은 미검수 유지.
- 종료/보존: 검증용서버18192(21500/48568)와18194·테스트Player정리, 기존Desktop/8188 PID17356 유지. 최종GUI 종료는 사용자Escape시 Player로그종료흔적/후속조회로기록, 추가조작 없음. InputSystem빌드가추가한 preloadedAssets1개만 텍스트로원복, 기존InputManager사용자변경 유지. 패키지/씬/모델/원본UI 미수정. Play중지/Idle, 소유권 해제. 도구 공유로그읽기/이전로그오인 수정후최종PASS. 이번치명장애 발견없음, 커밋/push 없음.

### [2026-11-02 / Asia/Seoul] UI-NEW-003 · Codex → 다음 작업자 · 완료(미커밋)

- 요청/기준: Assets/UI/NEW의 인트로 배경·각 버튼 안내를 정확히 대응해 기존 파일명으로 덮어쓰고 NEW삭제. main/3852ee6+기존 사용자/기능 변경 보존, Codex 단독 수행/검증·교차 검증 없음. NEW9PNG를 전부 직접 시각확인했고 기존target9개와 해상도 동일함을 확인했다.
- 대응: E02-S01→Backgrounds/Intro_bg.png. 안내2→ENIdle,2-1→THIdle.3/3-1/3-2→ENGroup/SelectAll/RecommendedAd,3-3/3-4/3-5→THGroup/SelectAll/RecommendedAd. 기존 폴더/파일명에 PNG바이트만 복사했고 meta9개 전체SHA/GUID 및 라이브러리 참조동일. 영어3/3-1은 제공본의 동일 그림, 태국어Group의 Home표기도 제공본 그대로 유지.
- 검증: 대상9PNG sourceSHA일치·meta전체보존/라이브러리9연결, sourceNEW GUID10개(폴더포함) 참조0. 실제Unity2022.3.62f3 양언어 Idle/Group/Selection/Result8화면 직접비교,18항목 PASS(언어전환/안내바인딩·정상결과전환/홈). 결과화면은 과거 생성PNG를 명시적 테스트로 인계했으며 새 GPU생성/영상재생은 하지 않았다. 콘솔 오류0. 모든이미지크기가같으므로 씬/JSON/코드/연출 재작성이나 별도 NUnit재실행 없음.
- 정리: 일반PowerShell 재귀삭제 명령은 자동정책에서 차단됐고 실행되지 않았다. 정확한절대경로/일반디렉터리/9PNG+9meta/대상복사완료를 다시 검증한 뒤 Unity manage_asset.delete의 명시적 Assets/UI/NEW경로로 안전하게 삭제성공. 폴더/폴더meta 없음 및9target/meta 해시 재검증. NEW에알수없는파일/참조가있었다면삭제하지않도록 확인했다.
- 보존/범위:9기존PNG, Tools/UI/verify_ui_new3.cs·검증자료, IMAGE_UI/CHANGELOG/HANDOFF. Scripts/StreamingAssets/3씬/라이브러리/패키지 보호452파일SHA동일. 최초포괄검사에서 ignored StreamingAssets/build_info1개가변경돼 ReporterEditor.cs의자동타임스탬프작성임을확인하고별도기록,해당파일은그대로두었다. PackageCache읽기만/미수정. 기존사용자변경임의커밋/push/리셋없음.
- 종료/남은항목: Play중지/0_Idle·dirty false,소유권해제. Player/실장비·영상전체검수등기존후속유지. docs/validation/ui_new3/README.md·mapping/verification/runtime JSON·8PNG/preview 참조.

### [2026-11-02 / Asia/Seoul] RFID-SNAPSHOT-001 · Codex → 다음 작업자 · 완료(미커밋)

- 요청/기준: 생성 중 RFID 인식 끊김을 프로그램에 반영하지 않고 생성 시작 시 블록 그대로 표시. 해제/재인식/교체/이동/일반 전체비우기까지 같은 고정 규칙을 적용하며 무시 입력은 큐에 저장하지 않는다. main/3852ee6+기존 변경 보존, Codex 단독 구현/검증·교차 검증 없음.
- 구현: RfidReaderSelection.FreezeInput()의 idempotent IDisposable/refcount lease가 Set/CanRecognize/Move/Clear를 차단한다. Hub Recognize는 검증/이벤트 전에 거부하고 일반 Clear는 할당 전 반환. Controller는 첫 await 이전 생성 try 진입에서 lease를 얻고 기존 요청 소유권의 ReleaseGenerationInput에서 해제한다. 이전 finally/중복Dispose가 새 generation 잠금을 풀 수 없다. Hub Idle 전환만 Clear(true)로 강제초기화한다. 무시 신호는 revision·선택·Changed/ReaderChanged/블록 이미지/진행 요청을 바꾸지 않는다. 종료 시 밀린 입력을 재생하지 않으며 이후 새 신호는 정상 처리한다.
- 검증: Unity2022.3.62f3 컴파일 오류0. 신규3사례 포함 Edit91/91(job0a9528588f994fd5b73bd14054e90a33,13.8610984초). 실제 앱45항목 PASS: 실제 Comfy 생성 중4리더 해제·교체/직접Data.Set·이동·clear, revision/선택/event0·양패널8이미지 동일·로딩/타이머잠금/성공 결과 유지. 완료 직후 무시신호 재생 없음 및 새 신호 수용. 취소·이전finally/재요청·타임아웃·HTTP400실패·Controller비활성/재진입·홈·다음 세션 무누수 검증. generating_snapshot.png/success_snapshot.png 직접 확인. HTTP400 fixture의영문 오류/원래예외2로그는 의도한 진단이며 보존, 기타 오류 미발견.
- 범위/보존: Data/RfidReaderSelection.cs·RfidReaderHub.cs·BlockExperienceController.cs, RfidReaderSelectionTests, 신규 Tools/UI/verify_rfid_snapshot.cs/검증 자료. 기존 verify_block_art_loading의 블록제거 시 취소 기대값을 snapshot유지→명시취소→새입력으로 갱신했으며 도구 전체 재실행은 미수행. BUTTON_INPUT/BLOCK_RESOURCES/BLOCK_SIMULATOR/CHANGELOG/HANDOFF 갱신. 원본아트/meta/3씬/패키지/입력/생성 설정82SHA동일(preservation.json). 씬/모델/외부리소스/색/로딩디자인 미수정.
- 종료/남은 항목: Play중지/0_Idle·dirty false, 소유한fixture PID33924만 확인 후 종료, 소유권 해제. 커밋/push 없음. 실물RFID/Windows Player·장시간 검수는 미수행이며 기존장비 후속 유지. 이번 실제 생성1건은 전체 모델 품질 재승인이 아니다. docs/validation/rfid_snapshot/README.md·runtime.json 참조.

### [2026-11-02 / Asia/Seoul] BLOCK-ART-LOADING-001 · Codex → 다음 작업자 · 완료(미커밋)

- 요청/기준: 블록 리소스를 전용 JSON/StreamingAssets 이미지로 교체·언어별 설정, 완전한 블록 뒤 버튼으로 생성하며 우측 박스에 UI에 맞는 원형 무한회전 로딩. 후속 지시로 흰 원판/고정 장식 제거, 꼬리는 부드러운 투명 그라데이션, 선두와 꼬리를 같은 붉은 계열로 통일. main/3852ee6+기존 사용자/RED/Thai/suffix/group 변경 보존, Codex 단독 구현/검증·교차 검증 없음.
- 외부 리소스: BlockArtwork/blocks_en.json·blocks_th.json 각20개 kind/id/image. Images/en20개·th16개는 원PNG 바이트 복사. 태국어 연령4개는 JSON에서 영어 경로 공유. 새 순수 BlockArtworkDocument는 schema/언어/중복/로컬 PNG/JPG 경로 검증. StreamingBlockArtwork는 두 언어를 async 로드·같은 경로 캐시·자산복제 카탈로그에 원자적으로 적용, JSON 오류는 해당 언어/이미지 오류는 해당블록 기본값, 원래 예외 보존. 재로드 후 이전 런타임 Sprite/Texture 해제. Hub가 시작/Idle에서 재로드하고 준비 전 인식/초기화를 기다림. View는 ArtworkChanged만 별도 갱신, F9도 같은 변형 Sprite 사용. 원본 SO/PNG/GUID는 불변.
- 생성/로딩: Controller의 인식 완료 자동 생성 제거, 기존 Continue의 GenerateImage 버튼 경로로 시작. 블록 변경은 취소/비우고 새 버튼을 기다림. GenerationLoadingIndicator/LoadingRingGraphic을 결과 이미지와 같은 Rect의 형제에 생성해 배경 없이 둥근 붉은 분홍 링만 표시. RGB(.94,.48,.64)는 선두/꼬리 동일, 꼬리 알파는 SmoothStep0→1. CanvasRenderer 명시,96분할 메시 고정·transform unscaled회전. 요청 소유권이 있는 성공/실패/취소/타임아웃/블록변경/홈/비활성에서 로딩·기존 입력잠금/타이머를 정리. 씬 저장/원본UI/모델·생성 팔레트 변경 없음.
- 검증: Unity2022.3.62f3 컴파일 정상. Edit88/88(job b5c2c2cf799248c2a57e052f48ace2d2,11.9554524초;신규12 포함). 실제 외부 PNG 바이트 교체·언어독립/JSON 경로변경/중복재로드·손상JSON/이미지누락 fallback·반복Texture36개 유지·실제 버튼/Comfy성공·0배속회전·취소/블록변경/타임아웃/HTTP400/이전요청 finally·홈47 PASS(runtime.json). 최종 붉은 RGB/투명도0·중간·255/실제renderer/배경없음/회전·취소/홈9 PASS(visual.json), 최종 loading.png 직접 확인. 외부이미지 Thai→English→Thai 전체20종/두패널480슬롯/F9 등432 PASS(localized/runtime.json), 실제English/Thai 화면 직접 확인. 기존 Thai 검증 기록은 덮어쓰지 않고 별도 폴더에 출력.
- 검증 중 수정/경계: 초기 UniTask.Preserve는 진행중 동시 대기를 지원하지 않아 시작/scene callback 중복 예외가 났으며 Task bridge로 coalesce 후 실제동시재로드 PASS. 초기 커스텀 Graphic의 CanvasRenderer 누락은 캡처에서 미표시를 확인해 명시 생성/요구속성으로 해결하고 실제renderer 검사 추가. 최종 색상 변경은9항목 집중 재검증,47항목 lifecycle 결과는 직전 동일알파/배경없는 링 구성. malformedJSON/누락PNG/HTTP400의4로그는 의도한 실패 진단이며 숨기지 않았고 최종 정상 실행 콘솔0. fixture결과를 새 모델 품질 검수로 계산하지 않음.
- 범위/보존: 신규 Data/BlockArtworkDocument(+tests/meta), StreamingBlockArtwork, GenerationLoadingIndicator/LoadingRingGraphic, StreamingAssets/BlockArtwork JSON·36PNG/meta·운영README, Hub/View/Simulator/Controller, Tools/UI fixture/검증 및 기존 group/Thai 검증 절차 갱신, BLOCK_RESOURCES/BLOCK_SIMULATOR/BUTTON_INPUT/CHANGELOG/HANDOFF. 원본블록PNG/meta/3씬/패키지/입력/생성 설정82SHA동일,36외부PNG 원본일치·JSON40경로/meta누락0. runtime 테스트로 임시교체한 JSON/PNG 복원 확인. 임의커밋/push 없음.
- 종료/남은 항목: Play중지/0_Idle·dirty false, 최종정상콘솔0, 소유한 fixture PID46804만 명령행/실행파일 확인 후 종료. 실제Windows Player·설치PC/RFID/RedButton/장시간 검수는 미수행이며 기존후속 유지. 운영교체 방법 docs/BLOCK_RESOURCES.md 및 StreamingAssets/BlockArtwork/README.md, 검증 docs/validation/block_art_loading/README.md.

### [2026-11-02 / Asia/Seoul] GROUP-FIRST-001 · Codex → 다음 작업자 · 완료(미커밋)

- 요청/기준: GroupSelection은 아동/학생/직장인/시니어 그룹이 먼저다. 이전 물건/색은 무시하고, 정확히1그룹 뒤 새 비그룹 입력은 버튼 없이 같은 연출로 진행. 그룹2개일 때는 진행하지 않는다. main/3852ee6+기존 RED/Thai/suffix·사용자 변경 보존, Codex 단독 구현/검증·교차 검증 없음.
- 구현: RfidReaderSelection.RequireGroupFirst/CanRecognize로 인식 순서와 교체/이동의 도착 그룹 수를 검사. 최초 무시 입력은 저장/재생하지 않고, 유일 그룹을 물건으로 덮으면 기존 그룹을 지우고 새 물건은 거부. Hub는 장비/F9/기존컨트롤러 공통 진입점에 적용하며 Idle에서 gating 복구. View는 ReaderChanged의 새 비그룹에만 자동 요청을 만들고 LateUpdate에서 준비/정확히1그룹/비그룹 존재/연출·생성 잠금 종료를 확인해 기존 Continue를 호출한다. 그룹 제거/중복·비그룹 모두제거·홈/비활성은 대기를 취소한다. 배치 진입 시 gate를 해제하고 재활성화 시 기존 배치 상태는 보존. 기존 그룹 제거 후 배치화면 유지·5초홈/생성중 입력 버림/세션 취소는 유지했다.
- 검증: Unity2022.3.62f3 컴파일/최종 콘솔 오류0. 신규10사례 포함 Edit76/76(job90a6e396ba2044309b3092f2217346fa,11.4751019초). 실제 앱101항목 PASS: 선입력16종 무시/무저장·4그룹/4리더·기존 버튼·물건/색 자동진행·정상 슬롯 인계/블랙아웃0/전환레이어 정리·애니메이션 잠금대기·2그룹 수동/자동차단·대기중 중복/그룹제거/동일프레임 복원/교체/이동/홈 및 다음세션 누수 없음. 빠른4블록 입력에서 실제 Comfy 생성 잠금 중 버튼/자동 진행 차단, Image ready 후 배치 화면까지만 진행하고 홈 정리 확인. after_auto.png 직접 확인.
- 범위/보존: Data/RfidReaderSelection.cs, RfidReaderHub.cs, BlockExperienceView.cs, RfidReaderSelectionTests.cs, 신규 Tools/UI/verify_group_first.cs·검증 자료, 기존 verify_thai_blocks.cs는 그룹+버튼으로 배치에 먼저 들어간 뒤 전체아트 검수하도록 절차 갱신. BUTTON_INPUT/BLOCK_SIMULATOR/CHANGELOG/HANDOFF 갱신. 검증 시작~종료 사이 블록 PNG/meta·3씬/패키지/이전입력/생성 설정83SHA동일(preservation.json). 씬/아트/패키지/생성 워크플로 미수정.
- 종료/남은 항목: Play중지/0_Idle·dirty false, 임의 커밋/push 없음·소유권 해제. 실물 RFID/RedButton·Windows Player/장시간은 이번 검증에 포함하지 않는다. 기존 배포/3060 후속 유지. 이번 실제 생성1건을 전체 모델 품질 재승인으로 확장하지 않는다. docs/validation/group_first/README.md·runtime.json 참조.

### [2026-11-02 / Asia/Seoul] BLOCK-SUFFIX-001 · Codex → 다음 작업자 · 완료(미커밋)

- 사용자 요청으로 영문20개에 _Eng, 태국어16개에 _Thai 접미사를 추가. 예: EnglishBlocks/BagBlock_Eng.png, ThaiBlocks/BagBlock_Thai.png. main/3852ee6+기존 변경 보존, Codex 단독 구현/검증이며 교차 검증 없음. AssetDatabase.MoveAsset로 meta/GUID를 함께 유지했다.
- RfidBlockAuthoring, sync_block_palette.py/sync_block_references.py, verify_thai_blocks.cs의 파일명 경로 갱신. 카탈로그의 GUID 기반 참조36개와 양언어 조회/4연령 fallback 확인, 원본 PNG 해시/GUID36/36 동일 및 이전 이름 파일 없음. Unity2022.3.62f3 컴파일·콘솔 오류0, 팔레트 --check PASS. 씬/입력/생성·언어 선택 로직은 변경하지 않았다.
- docs/BLOCK_SIMULATOR·CHANGELOG·thai_blocks/README/HANDOFF 갱신, suffix_rename.json에 변경 전후 경로·해시·GUID 검증 보존. 앞선 Thai/English/Thai 실행 검증은 THAI-BLOCKS-001 시점이며 이번 접미사 변경에서 Play/Player 재실행은 미수행. Play중지/Idle·dirty false. 커밋/push 없음, 소유권 해제.

### [2026-11-02 / Asia/Seoul] THAI-BLOCKS-001 · Codex → 다음 작업자 · 완료(미커밋)

- 요청/기준: 사용자가 추가한 ThaiBlock의 파일명을 정리하고 태국어 모드에서 사용. 실제 폴더명은 ThaiBlocks이며 물건10종/색6종16개, 연령 이미지는 없다. main/3852ee6 + 기존 RED-BUTTON 및 사용자 InputManager·EnglishBlocks 이동 보존. 담당/검증 Codex 단독, 교차 검증 없음.
- 구현: AssetDatabase.MoveAsset로 한국어 파일명을 EnglishBlocks의 대응 basename(예 BagBlock.png)으로 변경해 GUID 유지. 태국어16개를 원래252×252 Sprite/NPOT 없음/미압축/밉맵 없음으로 가져와 RfidBlockArtwork 각 항목의 thaiSprite에 연결. 기존 sprite/인식 ID20개 유지. GetSprite/Find 언어별 선택과4연령 공용 fallback, BlockExperienceView 두 패널·RfidSimulator 목록/리더/드래그에 현재 언어 전달. RfidBlockAuthoring.ConfigureArtwork()로 씬 저장 없이 재연결하며 필수 리소스 누락은 영문 오류로 실패. 사용자 이동에 맞춰 팔레트/과거 참조 동기화 도구 경로도 EnglishBlocks로 갱신. 활성 생성 그래프/색상값은 불변.
- 검증: Unity2022.3.62f3 최종 컴파일 오류0. 실제 Play Thai→English→Thai 왕복에서20종을 각 리더에 순환 배치하고 두 패널 이미지/빈칸/활성 상태480개 확인, 인식 ID/영문·태국어 참조·미등록 빈칸/홈/언어/이미지생성 없음 포함426항목 PASS. F9 양언어20개 목록/4리더와 실제1920×1080 화면 직접 비교, 태국어 전체16장 contact sheet 검토. Edit19/19(job83cc06970cd94c67a9db6bcb4778369c,0.6070731초), Python workflow8/8(0.728초), sync_block_palette --check 및 양언어 원6색 RGB 일치 PASS.
- 검증 수정 이력: 초기 검증용 NUnit 파일을 기존 Input 전용 asmdef 아래 둬 Assembly-CSharp 타입을 참조하지 못한 컴파일 오류가 발생했다. 해당 신규 파일/meta만 제거하고 Tools/UI/verify_thai_blocks.cs의 실제 앱 검증으로 대체. 삭제 후 force refresh로 재컴파일/오류0 확인. 존재하지 않는 새 NUnit 테스트를19개 결과에 포함하지 않는다.
- 보존/범위: 원본 PNG36개 SHA/GUID 전부 동일(preservation.json), 기존 영문20개 전체 PNG/meta가 HEAD 원본과 동일(english_preservation.json). 태국어는 PNG/GUID 유지하고 importer만 변경. 3씬/패키지2개/생성Catalog/API git diff 없음(untouched.json). 이전 입력 변경 보존. 문서 BLOCK_SIMULATOR/CHANGELOG/HANDOFF 및 검증 자료 추가. 커밋/push 없음, 소유권 해제.
- 종료/미검증: Play중지/0_Idle·dirty false, 최종 콘솔 오류0. 연령4종 태국어 아트 미제공으로 공용 이미지 사용. 실제 장비/Windows Player는 기존 후속 유지하며 이번 작업에서 이미지 모델을 다시 검수/생성했다고 보고하지 않는다. 자료 docs/validation/thai_blocks/README.md·runtime.json·실화면/시뮬레이터 PNG.

### [2026-11-02 / Asia/Seoul] RED-BUTTON-001 · Codex → 다음 작업자 · 완료(미커밋)

- 요청/기준: 사용자의 Input Manager RedButton으로 진행하고 숫자1도 대체 입력으로 유지. main/3852ee6, Codex 단독 구현/검증, 교차 검증 없음. 시작 시 유일한 사용자 변경인 InputManager의 Fire1→RedButton/physical keys 설정은 읽기만 하고 보존했다.
- 변경: 기존 KeyboardButtonSimulator의 숫자1/숫자패드1 유지 상태에 ENABLE_LEGACY_INPUT_MANAGER 조건의 Input.GetButton("RedButton")을 OR 병합. 조이스틱 번호를 코드에 고정하지 않는다. 같은 SetButtonPressed/gesture 경로이므로 입력 잠금/타이머 갱신·2초 언어/5초 홈·포커스/해제 대기·중복 어댑터 방지 유지. 추가 런타임 컴포넌트/씬 변경 없음. BUTTON_INPUT/CHANGELOG와 재현용 Tools/Input/verify_red_button.cs·검증 자료 추가.
- 검증: Unity2022.3.62f3 컴파일 오류0, 기존 ExhibitionButtonGestureTests13/13 PASS(job c4438b274396448a929e45a3c26ef6f9,0.4864376초). 실제 어댑터+InputSystem 가상 키보드로 상단1/숫자패드1 시작, 겹침 중 한 키 해제 시 진행 없음/모두 해제 시 한 번, 포커스 취소,5초 홈,2초 언어 전환/원래 언어 복원,Idle 복귀 포함11 PASS. RedButton의 실제 legacy 읽기는 휴지 상태에서 예외 없이 false 확인. 최종 콘솔 오류0·매니저1개. Play중지/0_Idle·dirty false 확인.
- 초기 실행 관찰: 첫 Play 중 비주입 매니저/루트 중복과 준비 대기가 관찰되어 첫 검증을 중지했다(완료 보고서 없음). 컴파일/도메인 재로드도 관찰했으나 중복 원인은 미확정. Play 종료 후 새 실행에서 단일 매니저로 전체 검증 통과했고 오류가 재현되지 않았다. 진단을 숨기거나 DI/씬/패키지를 변경하지 않았다. 재발 시 초기화 수명 후속 조사 필요.
- 보존/한계: InputManager SHA256 F2A9E886ADB17E81298A722445C1A0E388DCE003B44E07C7C81D610D89366D2A 시작/끝 동일. 씬/패키지/생성 설정·공통 매니저 미수정. 실제 joystick 장비의 누름·연결 해제/Windows Player는 미수행이며 가상키보드를 장비 검증으로 보고하지 않는다. 남은 설치PC 검수에 포함. 커밋/push 없음, 소유권 해제.

### [2026-11-02 / Asia/Seoul] COMMIT-004 · Codex → 다음 작업자 · 완료

- 요청/기준: 지금까지의 모든 사항 커밋. main/37723c7 및 origin 개발 저장소 확인. 사용자 명시 요청에 따라 기존 전체 변경을 포함하며 원격 push는 수행하지 않는다. 이전 작업 소유권 해제 및 진행 중 편집 충돌 없음 확인. 담당/검증 Codex 단독.
- 범위: 총3404경로. TH/EN 이미지 UI·언어/영상·연출/파티클·RFID 시뮬레이터/블록 아트·ComfyUI 복원/Klein 스타일·연령/프레이밍/보존·테스트/도구·검증 PNG/JSON·사용자 씬/프리팹/URP/ProjectSettings 변경 및 문서. 이번 작업에서 구현/에셋은 변경하지 않고 HANDOFF만 갱신. 변경3403파일의 시작/완료 SHA256 동일 확인.
- 새 검증: ComfyUI Python 환경으로 unittest discover 워크플로8/8 PASS(0.891초). 기본 Python312에서는 Pillow 미설치로 처음7성공/1오류였고 기존 Pillow 포함 환경으로 재실행하여 모두 통과(설치/소스 수정 없음). 변경 텍스트415개 비밀키 주요패턴 검사 일치 없음 및 JSON 파싱 오류0. 신규 Assets/meta·부모meta 누락0, orphan meta0, 전체 asset GUID 중복0.
- 저장 검증: 신규3369파일 약1400.67MiB 포함. 영상2개(각 약166MiB)와 이미지 등은 기존 Git LFS 규칙으로 저장. staged 고유LFS1597객체의 존재/크기/SHA256 일치, 일반Git blob100MiB초과0. staged diff 공백 검사에서 기존 Unity 작성 YAML/meta 및 검증 스냅샷/도구의 trailing whitespace1121건 확인, 내용 보존을 위해 일괄 정리하지 않음.
- 프로젝트 확인: Unity2022.3.62f3, 빌드 씬0_Idle/1_Game/2_Result. Company MyCompany / Product Unity-Startup-Template / Standalone ID com.Unity-Technologies.com.unity.template.urp-blank 유지. 패키지 갱신·종료 설정 변경·실제 PC 종료 명령 없음. ignored 캐시/모델/로컬 결과는 Git 대상에서 제외하고 삭제하지 않음.
- 미검증/다음 행동: 이번 커밋 준비에서는 Unity 컴파일·Edit/Play/Windows Player·GPU 실생성·장시간을 재실행하지 않음. 이전 검증은 각 원작업 기록 참조. RFID 직접 마우스/F9 후속, Windows Player/RTX3060·실물입력·영상 전체길이/장시간/Profiler 검수는 여전히 미완료. 커밋이 배포 검수 완료를 의미하지 않음.
- 종료: 이 기록 포함 로컬 커밋으로 저장, 해시는 git log 확인. 파일 소유권 해제, 새 치명적 장애 발견 없음. 원격 push 없음.

### [2026-11-01 / Asia/Seoul] STYLE-QUALITY-003 · Codex → 사용자 검토 · 완료(적용/표본검수, 미커밋)

- 요청/기준: 원본 작은 신발 일러스트처럼 귀엽고 부드러운 완성2D, 실사/반실사·거친선·잘림/기형 탈락, 여러색/연령/물건 반복 확인. Codex 단독 구현/직접 검수이며 외부 교차검증 아님. main/37723c7+기존 미커밋을 before/*/before_hashes로 보존했다.
- 선택/수정: 범용Turbo의 실사혼입/거친선과 Anime/Hyper 후보의 포스터/중복 결과를 불채택. 기존 설치 Klein4B FP8와Qwen3 4B의 해시/출처 확인, Apache2 출처Klein BF16 VAE168MB만 추가. 최종640/4단계·단일 생성·원본 사용자 스타일 PNG 해시 참조를 활성화. 회색 전처리로 참고 이미지의 색 복제 영향 억제.10종4연령·6색 원HEX/RGB/색부위 유지. 로퍼 끈/장화/이어폰 혼동을 보강하고 깨진 디지털숫자는 바늘형 스포츠 시계로 단순화. 블록윤곽 강제/가이드색칠/외부 유료 API/새 커스텀노드/패키지 업데이트 없음.
- 실제 파일: Assets/Scripts/Blocks/BlockExperienceController.cs/ComfyImageClient.cs/Data/BlockSelection.cs/GeneratedImageFraming.cs, BlockCatalog.json/StreamingAssets API/신규StyleReferences(+meta), Blocks Selection/FramingTests, Tools/ComfyUI model/text_encoder/vae/presets/builder/API/UI/input/benchmark/test_workflow/prepare_style_reference/검증·gallery도구/원본참조가드/해시·licenses, docs/COMFYUI_SETUP/CHANGELOG/TODO/HANDOFF, docs/validation/comfy_style3. 다른 사용자 변경은 되돌리지 않았다.
- 런타임: SamplerCustomAdvanced는 RandomNoise36의 실제 세션시드와ReferenceLatent/BasicGuider를 유지하며 KSampler식positive/negative/seed로 덮어쓰지 않는다. 포함된 스타일PNG와서버같은이름의해시불일치는 덮어쓰지않고실패. 원본알파64가3px경계에닿음/불투명영역0.5%미만/주전경8%초과의두번째분리부품은 표시 전 거부, 기존새버튼 재시도/입력·타이머복구. 정상은480중앙/24px투명여백. 구조 검사는 완전한 의미/스타일 분류기가 아니다.
- 최종 품질/자료: 미리 정한80계획(10×4×기본색40/새시드·다색40)의 전체 결과를 직접 상하반부/확대 비교. final_accepted/report.json은 현재전체API/PNG해시·요청ID/선택/시간을 보존. 직전 회색참조 배치에서 그래프가같은76개만 재사용, 변경4개는같은계획시드로재생성.80개에서 실사/반실사·잘림·큰형태파손·포스터중복 미발견, 실제Unity strict검사/알파/중앙/여백80/80 PASS. 전체자료는 age_comparison.png/color_comparison.png, 표본quality_preview.png, 원본과이전비교reference_comparison.png. 셀별 유리한시드선택/내용수정/재그림 없음.
- 새 실행 증거: Unity2022.3.62f3 실제Editor 컴파일오류0·Edit66/66(jobebd56d9f91be4944872e8693c788d2f1,11.2190014초). Python8/8. 실제JsonUtility/CreatePrompt1440조합이Python과일치. 실제앱4새이미지로 두패널/완료를가로지르는5초유지무시·새탭/같은Result텍스처/홈/알파/중앙 등27 PASS. 원본잘림 PNG/HTTP400/지연fixture를 실제Client로처리해표시거부/재시도·실패/취소/기한초과/복귀10 PASS(예외/진단보존). 합성fixture는그림체검수로계산하지않음. evidence verification.json/visual_acceptance.json/strict_unity_framing.json/runtime_*.json.
- 성능/경계: RTX5070Ti Laptop12GB·Comfy0.42/PyTorch2.11+cu128, 최종80 API 중앙3.3805/최대3.936초, 실제앱3.806/3.808/4.039/3.783초. 실제3060/Windows Player·실물입력/장시간은 미수행. 과거Turbo3060 5~12초 추정을 이모델의실측처럼재사용하지않음.6색 전체픽셀RGB·모든랜덤시드/의미구조무결점 보장 아님. 검수한 범위의 합격과 자동검사범위를 구분했다.
- 보존/종료:3씬/manifest/packages-lock/ProjectSettings는시작SHA동일. 기존블록아트/GUID/기타사용자변경보존. fixture서버종료, Comfy서버는warm상태유지. 검증용Unity는Play중지/0_Idle·dirtyfalse이며Exit안전패턴차단으로종료실행하지않고우회하지않음. 원시Editor로그는gitignore 대상, 보고자료는검증JSON/PNG. 미커밋·커밋/push없음, 소유권해제. 다음은 사용자스타일검토/3060·Player장비검수이며 새불합격표본은원본보존후수정. 기존RFID인계대기 등 독립작업상태유지.

### [2026-10-30 / Asia/Seoul] AGE-DESIGN-002 · Codex → 사용자 검토 · 완료(구현/실생성, 미커밋)

- 요청: 연령별 일반 디자인 수식어만으로는 차이가 약하므로 모자처럼 물건 종류/형태를 다르게 지정. 기준 main/37723c7+기존 미커밋, 담당/검증 Codex 단독. 병행 편집/소유권 충돌 없음.
- 적용: BlockAgeVariant와 제품별 ageVariants 40개. 주제와 두 색 부위를 연령별로 교체하며 누락/중복/미등록/빈 부위 검증. 예: 곰 유치원배낭/학교배낭/서류가방/크로스백, 유치원모자/비니/볼캡/중절모, 티셔츠/후드티/셔츠/카디건. 비니에 챙을 색칠하는 충돌 제거. 연령은 대표 디자인 예시이며 개인 취향 규칙 아님. 카탈로그 재로드 후 적용.
- 변경 파일: Assets/Scripts/Blocks/Data/BlockSelection.cs, Assets/StreamingAssets/BlockGeneration/BlockCatalog.json 및 product_480.api.json, Assets/Tests/Blocks/BlockSelectionTests.cs, Tools/ComfyUI/build_workflow.py/test_workflow.py/export_age_gallery.py/workflows/product_480.{api,ui,inputs}.json, docs/COMFYUI_SETUP.md/CHANGELOG/HANDOFF, 신규 docs/validation/comfy_age_design/*. 모델/4steps/512·색 원 HEX/RGB·알파/중앙보정·입력/취소·씬/패키지/기존 변경 보존.
- 실제 검증: Python8/8 PASS. 실제 생산 C#을 Add-Type으로 컴파일하고 1440조합의 C#/Python 최종 프롬프트/형태/색 부위/HEX/RGB 완전 일치 및 거부6경로 PASS. ColorUtility는 #RRGGBB 전용 shim. 최종설정 2시드80 실제 출력의 요청 그래프/PNG 해시·480RGBA·80/80 중앙±1px/24px여백·40셀 매핑/PNG 무결성 확인. 두 시드 상하반부 직접 육안 검토. 동일 전체 API 해시 출력만 재사용, 요청ID/출처 보존.
- 결과: 대표 전체40개는 docs/validation/comfy_age_design/preview.png(2248×5330, 동일 시드1206502), 다른 시드는 second_seed_final/all_products_by_age.png(1206501). 대표40 API 시간 중앙1.8795초/최대2.715초, 다른40 중앙1.733초/최대2.4초. 현PC5070Ti Laptop의 원본 요청→PNG 시간으로 앱 전체/3060 측정이 아님. 셀별 시드 선별·런타임 시드 고정 없음. README/verification.json/보고서에 매핑·출처·실험 보존.
- 한계: Unity MCP Transport closed로 새 Editor 컴파일/JsonUtility/Play/Player 미수행. 원본 GeneratedImageFraming.cs 순수 배열 코드를 별도 프로세스에서 실행했고 중복 shim 타입 오류는 새 프로세스로 해결. 일부 시드의 여러 물건/잘린 의류·밴드·바퀴 및 색/세부구조 오차는 남아 있으며 전시드 품질 완벽 해결로 보고하지 않음. 접힌 우산이 계속 펼쳐지는 문제는 학생용을 접이식 열린 구조/시니어를 전통 양산으로 조정.
- 종료/다음: 검증 자료와 원시 RGBA 보존, 범위 diff 확인. 커밋/push 없음, 소유권 해제. 사용자 디자인 검토 및 새 Editor/Player·3060/실물입력·잔여 시드 품질 후속. 기존 RFID 인계 대기 등 독립 작업 상태 유지.

### [2026-10-30 / Asia/Seoul] AGE-GALLERY-001 · Codex → 사용자 검토 · 완료(미커밋)

- 기준 main/37723c7 및 기존 변경 보존. 담당/검증 Codex 단독. 수정 범위는 신규 `Tools/ComfyUI/export_age_gallery.py`, `docs/validation/comfy_age_all/*`, HANDOFF. 활성 모델/설정/앱/씬은 변경하지 않았다.
- 전체 가방/신발/모자/선글라스/헤드폰/상의/자전거/텀블러/손목시계/우산 × 아동/학생/직장인/시니어 40개를 현재 Turbo4로 준비. 빨강/파랑·시드1206501 고정. 동일 API 해시의 기존16개 재사용, 추가24개 실제 순차생성 성공. 결과를 선별하거나 보기 좋은 시드로 교체하지 않았다.
- 최종 `docs/validation/comfy_age_all/all_products_by_age.png`는 2248×5330, 한국어 4열×10행. 40/40 조합·색/시드·PNG 무결성 확인 및 상/하반부 직접 육안 검토. 원본/보정 PNG·report/layout/verification JSON에 입력·출처·해시·배치 보존.
- Unity 연결은 Transport closed로 사용할 수 없었다. 에디터 조작 없이 원본 GeneratedImageFraming.cs의 순수 배열 코드를 동일 Color32 필드 shim으로 실행하고 Unity 행 순서를 맞췄다. 기존 Unity 보정16개와 픽셀 완전 일치 확인. framing_method.json/framing_host.cs에 방법 보존. 새 Editor 컴파일/실행 검증은 수행하지 않았으며 런타임 변경 없는 산출물 작업이다.
- 한계/다음 행동: 일부 물건의 연령 차이는 작고 원본 생성물의 잔여 부분/잘린 표현도 그대로 노출. 사용자 전체 품질 검토 후 필요 항목만 후속 수정. 원시 RGBA 중간자료 보존. 기존 Player/3060/실물입력 후속은 유지. 커밋/push 없음, 소유권 해제.

### [2026-10-30 / Asia/Seoul] COMFY-FAST-002 · Codex → 다음 작업자 · 완료(적용/현PC검증, 미커밋)
- 요청/목표: 직전34초 복귀/미완료 종료를 사용자 지적. 최신 기준은 엄청난 디테일/외곽선 강조 불필요,간단한 물건 일러스트에 인식 두 색 표현·3060 10~15초 목표. 현재PC 실측과3060 예상은 구분.
- 기준/소유권: main/37723c7+사용자/기존 미커밋 보존. Codex 단독 구현/시각검토/측정,외부 교차검증 아님. 소유권 충돌 없음.
- 현재 적용: 공식 SDXL-Turbo fp16(고정revision/SHA검증),내부512/4steps/CFG1/SDTurboScheduler+EulerA/단일 생성. 기존ControlNet/Canny/가이드 업로드·LoRA·IPAdapter·두 번째 생성 없음. 짧은 물건/연령/색 프롬프트,6색 원 HEX/RGB 메타데이터 유지. API/UI/StreamingAssets 동기화,SamplerCustom의noise_seed를 실제 세션 시드에 연결. negative는CFG1에서미사용이므로중복 인코드생략. 기존선택패널 모든버튼/5초유지 잠금·실패/취소/홈의입력/타이머복구 보존.
- 추가 전경 처리: 작업스레드에서8방향 연결 알파의 가장 큰 영역 선택으로 별도 물건/장식 제거. 원본 배열/서버PNG 불변·취소·프레임여백/비율/480중앙 유지,가는대각선연결보존2테스트추가. 원본부터 잘린 부분이나 서로 붙은 다른 물건은 완전히 해결하는 의미가 아님.
- 변경: Assets/Scripts/Blocks/BlockExperienceController.cs,Data/GeneratedImageFraming.cs,BlockCatalog.json/활성API,BlockSelection/FramingTests,Tools/ComfyUI 모델/presets/builder/UI/API/test_workflow/benchmark/sync팔레트·참조가드/licenses/README,관련 문서/CHANGELOG/TODO/HANDOFF,docs/validation/comfy_fast2. 원본20블록 PNG/3씬/매니저·입력/View·패키지/ProjectSettings 미변경.
- 검증: RTX5070Ti Laptop12GB/Comfy0.42/PyTorch2.11.0+cu128/Unity2022.3.62f3 Editor. 최종API20회(10물건/4연령/4색쌍·새시드)중앙1.750/P952.443/최대2.541초. 실제앱 캐시해제첫6.242초·child/green/white반복2.647초,각8항목총16 PASS(입력잠금/480RGBA/중앙/24px여백/코너알파0/Result같은텍스처/홈해제·타이머재개). Edit61/61(job8dc8cc0140e44aa7a86aefd712adf779,12.4734841초),Python7/7.10종2시드+대표2종4연령28논리/26PNG·모든실제프레이밍중앙/여백PASS,같은시드4연령서로다른결과. 원본보존31목록중명시적Framing1개외30SHA동일,범위diff검사PASS. API로그서버apiUrl없음의기존exit-log건은설정미지정안내이며감추거나설정변경하지않음.
- 실측 경계/남은 검수: 3060은현재장비아님. 예열5~12초는여유를둔계획추정이며target_gpu_acceptance_verified=false유지. 실제3060/Windows Player·실물RFID/버튼·장시간/전색·전시드품질후속. 모델라이선스고정사본/NOTICE/출처보존,배포/상업조건은문서참조. 모든시드의완벽한형태/정확RGB/외곽선부재를보장하지않음.
- 종료: Play중지/0_Idle·dirty false,임의커밋/push/리셋 없음,소유권 해제. 후속은빠른현재기본값을유지하고장비검수. 이전느린34초모드복구기록은과거시도임.
- 장애: 이번 작업에서치명적장애발견없음. 실제성능/이미지/리스크는검증폴더README·JSON·PNG로확인.


### [2026-10-30 / Asia/Seoul] COMFY-LIGHT-001 · Codex → 다음 작업자 · 인계 대기(속도/전체품질 미완료, 미커밋)
- 요청/목표: RTX3060 12GB10~15초/장과 첨부 파스텔2D일러스트 수준 유지. 최신 지시로 실제 레퍼런스 유사성 재검토.
- 기준: main/37723c7+기존 미커밋, 단독 구현/시각검토·교차 검증 아님. 기존 유저/RFID/UI 변경 보존.
- 판단/현재값: Lightning/LCM/Hyper-SD·SD1.5/XL·두 단계 마무리·IPAdapter 직접 참조를 실제 비교. 빠른 후보는 옅은 색/입체감/잡선/구도·일부 물건 인물화 때문에 불채택. 원본 레퍼런스를 생성한 AnimagineXL1024/36steps·CFG5.5·Euler ancestral/normal로 기본값 복구. 10~15초/품질 동시 달성은 완료로 보고하지 않는다.
- 수정: BlockCatalog의 실제10블록/제품별구도·임계값/원 RGB 설명, BlockSelection의 참조/임계값 검증·전달, Controller의 활성 그래프 컨트롤모델/임계값 연결, 모델/presets/builder/API/UI/설치/benchmark·비교도구/검증 및 COMFYUI_SETUP/BLOCK_SIMULATOR/ToolsREADME/CHANGELOG/TODO/HANDOFF. 사용하지 않는 새 adapter/refiner manifest는 검증 폴더로 보관. 기존 세션·타이머·입력잠금·프레이밍 동작 보존.
- 실제 검증: RTX5070Ti Laptop12GB/Comfy0.42/PyTorch2.11.0+cu128. 최종 Edit59/59(job79879aa359ae404e9ffc714c00c1b11f,11.8315898초), Python7/7, 복구 Play8/8·33.767초/480RGBA/중심(240,240)/24px여백/코너알파0·Result동일텍스처·홈 해제/타이머재개. 복구 신발2시드33.235/25.299초·프레이밍2/2. 보존31파일 SHA동일, 범위 diff체크 PASS(기존 씬/ProjectSettings 전역 공백 경고는 수정하지 않음). 전체10종2시드 및 대표2종4연령은 여러 후보에서 실행했으나 최종 품질 복구 모드 전체 승인으로 재사용하지 않음.
- 실험/미검증: 불채택 두 단계 후보20회 중앙4.588/P954.992/최대5.197초와 Play6.141초를 현재 구성/3060 결과로 혼동하지 않는다. 직접 스타일 참조 신규 설치 IPAdapterPlus@a0f451a는 .disabled로 변경·빈큐 Manager재시작, 기존 Comfy코어/Unity패키지 미수정. 원본첨부는 docs/validation/comfy_light/reference_requested.png 보존, 원본 블록 PNG/GUID 미변경. 모든 생성/실험 API·원본 PNG/시간/판단은 해당 검증 폴더.
- 남은 항목/다음: 3060/Windows Player·실물장비·전 물건/색/시드 품질. 복구996531에 작은 장식이 남아있으며 전시드 완전무결을 보장하지 않는다. 레퍼런스 스타일을 유지하는 경량/학습/참조 방식 추가 검토 필요. Play종료/0_Idle·dirty false, 파일 소유권 해제.
- 장애: 치명적 장애 발견 없음. 이번 결과는 성능/미술 품질 목표 미달이며, TroubleShooting의 크래시/데이터손실 유형으로 등록하지 않음.


### [2026-10-30 / Asia/Seoul] COMFY-FRAMING-001 · Codex → 다음 작업자 · 완료(미커밋)

- 요청/기준:480×480 이미지 내부 제품 정중앙·측면만 강조할 필요 없음. main/37723c7 + COMFY-RESTORE-001 및 이전 사용자 미커밋 보존. AGENTS/개발기준/상태·diff/관련 코드·씬·패키지/식별자 확인 후 소유권 해제된 후속 범위만 담당. Codex 단독 구현/검증, 교차검증 없음.
- 변경: 신규 Data/GeneratedImageFraming(+meta)·Tests9개(+meta), ComfyImageClient.Download에 읽기 가능한 수신→취소 가능한 worker 알파경계 보정→Apply(false,true)→결과 반환 연결. 알파8이상 실제 가시영역을480캔버스 중앙에 배치, 최소24px여백·비율보존·필요할때만 축소/작은그림 확대없음, premultiplied bilinear로투명RGB혼입방지. 빈/잘못된이미지 실패·취소/예외에서Texture해제. 서버 원본보존, 앱 최종Texture에 적용. BlockCatalog의신발side view/자전거side 조건제거·자연스러운사선/가로세로중앙/균등여백 적용, Tools API/UI/inputs와배포API 동기화. 프레임/색/세션/버튼잠금·영상/씬 미변경.
- 실제 검증: Unity2022.3.62f3 컴파일·EditMode58/58 PASS(기존49+신규9:가로/세로/홀수경계/색·비율축소/여백·희미한노이즈·투명RGB가장자리·빈/잘못된입력·취소·반복), Python workflow7/7 PASS. 실제Comfy/Play8/8 PASS·17.8371초,480RGBA가시경계[50,121,430,358](상한exclusive)·중심(240,239.5)/24px여백·모서리알파0. 같은Texture Result 인계·잠금·홈/타이머복구, 실제PNG/화면시각확인. 기존실생성PNG2개도C#보정해 중심(240,240)/(240,240) 확인. docs/validation/comfy_framing/README.md·editmode/runtime/existing_images/preservation.json·PNG.
- 보존/종료: View/BlockExperienceController/GameManager·씬3개·패키지2개 총8파일SHA256동일. 관련문서/CHANGELOG/TODO갱신·범위diff-check PASS. 신규meta GUID생성/Unity임포트 완료. Play종료/0_Idle·dirty false, PC종료/패키지갱신/임의커밋/push 없음. 소유권해제.
- 미검증/남은위험: WindowsPlayer/실물장비·전체품목/색/시드 스타일/단일제품/시점/색/알파경계·장시간/Profiler 미수행. 생성 품질은 확률적이며 이번 실제 샘플의 복수신발형태는 전조합품질 후속에 포함; 이번 검증 통과는 중앙배치/투명여백·연결/측면강제제거 범위. 기존 RFID인계대기는 유지. 새 치명적장애 발견없음.

### [2026-10-30 / Asia/Seoul] COMFY-RESTORE-001 · Codex → 다음 작업자 · 완료(미커밋)

- 요청/기준: 이전 ComfyUI 생성 복원·모든 선택 패널 생성 중 버튼 차단, 현재 물건 블록 반영·색상 블록 원의 실제 RGB 사용. 후속 첨부 운동화처럼 부드러운 파스텔/윤곽/입체음영의 새 일러스트와 실제 투명배경 요청. 기준 main/37723c7 + 기존 미커밋. AGENTS/개발기준/HANDOFF/TODO/문제기록·실제 diff/씬·패키지 확인 후 단독 인수, 다른 작업자 교차검증 없음. RFID-BLOCKS-001은 인계 대기/소유권 해제를 확인해 관련 연결만 인수했다.
- 변경: 보관 ComfyImageClient/ComfyServerLauncher/GeneratedImageRetention/순수보관정책·테스트·Comfy설정/API/가이드의 필요한 원본을 GUID 유지해 Assets에 복사(보관본 미변경). GameLifetimeScope base.Configure 유지·영속 서비스 하나씩 등록. BlockExperienceController는 세션/화면 CTS·generation 제한시간·생성 lease/Pause·즉시 Cancel과 source별 finally 소유권, 다운로드→TryDisplayResult→Result 소유권·예외/저널을 연결. View는 생성 중 두 안내와 직접 Continue 차단, 실패 후 새 버튼 재시도. queue-delete 성공 빈본문을 허용해 취소 정리의 허위 JSON 경고 수정.
- 최종 그림/색: 강한 가이드 때문에 색칠처럼 보이던 경로와 ControlNet/업로드를 제거. SDXL Base1.0(기설치)/1024·28steps/CFG6.5/DPM++2M/Karras·BiRefNet→InvertMask→JoinImageWithAlpha→480RGBA. 모델 manifest는092fb12의 SDXL 고정정보를 복원하고1024 지정, 새 다운로드/패키지 갱신 없음. 기존10물건/아트20개·PhoneCases/Equipment 제외 유지. Black #171619, White #FFFFFF, Blue #5FB0E3, Red #EC6868, Green #B7DBA9, Yellow #F6EEA2를 원 내부에서 추출. sync_block_palette.py로 현재/보관팔레트 동기화. HEX/RGB는 요청 메타/저널에, 모델에는 대응 색 설명을 전달(숫자를 CLIP 본문에 넣은 중간 후보는 도표/분해도를 유도해 제외). AI의 명암/재질 때문에 전 픽셀 RGB 일치는 보장하지 않는다.
- 검증: Unity2022.3.62f3 최종 컴파일 완료, EditMode49/49·Python7/7 PASS. 실제 ComfyUI0.42.0/RTX5070Ti Laptop에서 최종31항목 PASS/17.5797초: 두 패널 입력·5초 유지 버림/새 입력·실제480RGBA 수신·같은 텍스처 Result/광고입장 대기·홈·블록제거/취소/늦은결과거부·접속실패/재시도·비활성 정리. 최종 종료9항목 PASS: local delayed fixture1초 timeout·입력/타이머복구·완료를걸친유지입력버림·취소후대체요청을이전finally가해제하지않음·실제Comfy빈삭제응답·홈. 실제 수신PNG 내보내기/화면 시각확인·480RGBA alpha0/255·투명154591/모서리4곳0 PASS. docs/validation/comfy_restore/README.md·runtime.json/exits.json/editmode.json/alpha.json/PNG.
- 보존/정리: 씬3개·원본UI/Resources/영상·효과/vendor·매니저/입력·패키지/ProjectSettings 포함397파일 중 View(승인범위)1개 외396개 SHA256동일. 로컬 색칠 코드/샘플·과거가이드/보관본 유지. 중간 실패/스타일 후보는 최종 증거와 분리 보존. 도구 재전송으로 검증이 중첩된 정황의 harness 실패는 root marker 중복 가드 후 순차 재실행 PASS. 의도적 접속실패의 영문 오류/원본 예외는 유지, 콘솔 삭제 없음. 자신의 delayed fixture PID8252만 명령행 확인 후 종료, 실제 ComfyUI/다른 프로세스는 유지. Play종료/Idle·dirty false. 이번 소스/문서 diff-check PASS, 기존 씬/설정 YAML 공백은 건드리지 않음.
- 미검증/다음: Windows Player/설치PC·실물버튼/RFID·전체물건×색×시드 스타일/단일물체/색상·알파경계/장시간·Profiler 미수행. 이전 RFID-BLOCKS-001의 직접마우스/F9·Player 후보 확인은 별도 인계대기 유지. 운영 식별자는 기존 MyCompany/Unity-Startup-Template/기존 Standalone ID이며 미변경. PC자동종료·임의커밋/push 없음. 새 치명적 장애 발견 없음. 문서·CHANGELOG/TODO갱신, 소유권 해제.

### [2026-10-29 / Asia/Seoul] UI-REFRESH-002 · Codex → 다음 작업자 · 완료(미커밋)

- 요청/기준: 2_new의 언어가이드/새요소 적용·기존이미지 이름으로 교체·이전이미지 및 가이드 삭제·크기 정렬, 기능/연출 점검. 영상 이후는 변경 없음. main/37723c7+기존미커밋·시작부터 존재한vendor변경 보존, Codex 단독 구현·검증(교차검증 없음). Figma MCP 재호출 없이 사용자 로컬 자료를 기준으로 진행.
- 변경: TH/EN4화면의PNG24개를 기존경로/이름으로 바이트 복사, 원래meta24개/GUID 모두 동일. 새요소 불투명픽셀을1920×1080가이드에서 매칭해 Lang_Th/En 좌표/래스터크기 갱신. 두언어타이틀안내307/879/1315×173, 그룹보드y274·네칸동반7px상향. 다른공통보드/프레임 유지. LocalUIRefreshAuthoring으로 기존UI의Rect/기본JSON만 변경, 전체UIBuilder 미실행(Builder 그룹y값만274동기화). 영상.end JSON·영상/종료PNG/컴포넌트/연출·프로그램소스는 미변경.
- 검증: Unity2022.3.62f3 컴파일오류0, 실제양언어52항목 PASS. 짧은 시작/2초언어페이드/5초홈·불완전 조합/그룹 미인식 차단·배치진입·인식해제후 유지/재인식·로컬색칠·프레임 원색/크기·그룹→배치 암전없음·광고입장잠금/반복아우라·영상암전/두언어선택/실제디코더0프레임·종료팝업·영상자원 해제.8개실제화면 직접확인, 가이드24정적슬롯 불투명영역 대조 PASS(RMSE .036~10.141/255, 소수점 래스터/AA차이 포함; 투명그림자/동적블록·물건 제외). docs/validation/ui_refresh2/runtime.json/pixel_comparison.json/PNG/editor_apply.json.
- 정리/보존: 가이드8장과원본요소24개 중복수입본의 live dependency0/절대경로 Assets/UI/2_new 확인후 사용자명시승인 범위만Unity DeleteAsset으로 삭제, 폴더/meta없음. 기본실행도구의삭제패턴검사 거부 후 도구가 문서화한 의도적삭제 옵션으로 처리. 정리후3씬 missing script/Sprite0. 원래런타임소스·효과프리팹/영상/종료이미지/블록설정/패키지·시작부터변경된vendor 포함291파일SHA256 동일. 자료cleanup.json/replacement_audit.json/preserved_hashes.json/final_references.json. 이전 UI-REFRESH-001 대기는 이 작업으로 대체완료.
- 미검증/남은항목: Windows Player/설치PC·실물버튼/RFID·장시간/Profiler 미수행. 영상은 끝부분seek로종료를검사했으며 전체길이 시청은 아님. RFID-BLOCKS-001 실제F9/마우스·Player/장비 인계대기는 미완료 유지. Play종료/0_Idle, 패키지/운영설정·PC종료·커밋/push 없음. 문서/CHANGELOG/TODO갱신·소유권해제, 치명적장애발견없음.

### [2026-10-29 / Asia/Seoul] UI-REFRESH-001 · Codex → 다음 작업자 · 인계 대기(미완료)

- 요청/기준: 최신 Figma의 이미지 교체·크기만 수정, 기존 연출/프로그램 기능 유지. main/37723c7+기존미커밋 보존, Codex 단독 조회/검토. figma-design-to-code/use 지침 적용, 원본 Figma 읽기만 수행. session cc36e87f-a142-4503-a916-0f5665a896ee.
- 확보/차이: Page0:1 메타데이터에서 새27계열10화면 확인, TH타이틀27:525/광고27:579/종료27:587 및EN종료27:727 디자인 컨텍스트/스크린샷 확보. TH타이틀 안내는 최신 English/Thai 병기·짧은 시작/2초토글로 변경되었고 기존851×173 Thai PNG와 다름. TH광고5초홈 행 가운데정렬도 기존766×153 PNG와 다름. 기존보드/팝업 크기는 확보된 화면에서는 동일.
- 차단: 나머지6화면 get_design_context, read-only use_figma, 안내노드27:537 download_assets 모두 `You've reached the Figma MCP tool call limit on the Starter plan.` 반환. 권한 문제와 구별. 확인하지 못한 화면/이미지/렌더경계를 추정하여 덮어쓰지 않고 인계 대기. 사용자에게 한도 복구 또는 최신 내보낸PNG·배치정보가 필요함을 보고.
- 변경/검증: docs/validation/ui_refresh의4개컨텍스트/프레임목록/REVIEW/app_preserved_hashes 및 TODO/HANDOFF만 작성. 앱 소스/씬/파티클/언어JSON/PNG·meta 미수정. 이미지 크기/파일현황 조회와 기존 TH 타이틀·광고 안내 PNG 직접 확인. 문서 링크/diff 검토, Unity컴파일/Play/빌드 미수행(앱 변경 없음). 시작부터 다시 생긴 vendor ConfettiFullscreen 직렬화변경은 임의 복원하지 않음. 패키지/운영설정·PC종료·커밋/push 없음. 소유권 해제, 치명적 장애 발견 없음.

### [2026-10-29 / Asia/Seoul] AD-FX-003 · Codex → 다음 작업자 · 완료(미커밋)

- 요청/기준: 원형 파동의 시작 크기를 줄이고 광고 박스를 벗어나지 않도록. main/37723c7 + 기존미커밋 보존, Codex 단독 구현·검증(교차검증 없음). 미저장0_Idle 유지.
- 변경: AdItemEmphasis.prefab의 ProductRing만 startSize700→480, sizeOverLifetime 선형.65→1.15에서.25→1로 변경하여 실제 지름455~805→120~480. 작은 중심에서 확산하고 최대 크기 자체를 물건480 영역 안으로 제한하여 광고 박스에 여유 확보.2.4초 반복·페이드·색/속도 유지. Editor작성도구 동일 설정 동기화. 씬/실행C#·빛살/별/컨페티 설정 미변경.
- 검증: 기존 verify_ad_highlight_loop.cs를 현재 크기/실제 정점 경계 검사로 갱신, 증거경로ad_fx3 분리. 최종Play28/28 PASS, timeScale0에서2.6초 동안 파동 정점620개가 광고 박스16px 안쪽에 유지, 관측 최대477.46875/설정480. 물건4곳 픽셀 불변·느린 반복·컨페티2초 이상 자연재생·입력해제·영상암전/실제첫프레임0·중복/재진입/비활성/취소/홈 회귀 포함. 컴파일오류0, 실제 화면 직접 확인. docs/validation/ad_fx3/runtime.json/PNG.
- 보존/한계: 씬/컨트롤러/컨페티·vendor 포함199파일 SHA256 동일. Play중 기존 vendor ConfettiFullscreen의 Unity 직렬화 마이그레이션이 다시 저장되어, 원래 시작 해시와 일치하는 HEAD/CRLF 바이트를 확인 후 이번 에디터 자동 변경만 복원함. 기능 설정 변경 없음. 미저장 Idle의 루트 상태 동일·Play종료. 자료preserved_hashes.json/source_preservation.json. 패키지/운영설정·PC종료·커밋/push 없음, Player/실장비/장시간은 미수행. 문서/CHANGELOG/TODO 갱신·소유권 해제. 치명적 장애 발견 없음.

### [2026-10-29 / Asia/Seoul] AD-FX-002 · Codex → 다음 작업자 · 완료(미커밋)

- 요청/기준: 모든 입자보다 물건을 앞에, 아우라는 화면 이동 전까지 반복·느리게, 터지는 효과 확대, ConfettiFullscreen 추가·2초 제한 해제. main/37723c7 + 기존 미커밋 보존, Codex 단독 구현·검증(교차검증 없음). 현재 씬은 이전 종료의 Scroll과 달리 미저장0_Idle이었으며 저장/재작성하지 않음.
- 변경: AdProductHighlight가 반복 아우라와 유한 입장을 구분. 빛살660/6초·회전60도(10도/초), 링700/2.4초 반복. 별32→64·초기7→14개,1.8초 후 별만 정리. ConfettiFullscreen의3방출기/트레일을 프로젝트 전용 AdConfettiFullscreen 복사본으로 전체1920×1080 배치(크기2배·회전20%·좌우 하단 시작). 자연 종료까지 입력 잠금, 아우라는 완료 대기에서 제외.12초 비정상 종료 안전 복구. ResultVideoController는 영상 암전 완료 때 자기 효과를 정리하고 광고 복원 때 아우라만 재개. 기존 영상 디코딩/프레임0·언어·블록/색칠/세션 알고리즘 미수정.
- 층순서: 2_Result/RecommendedAdvertisement/AdvertisementBoard에 효과 두 개를 ResultImage 앞의 형제들로 이동, ResultImage를 마지막으로 배치. 입자와 트레일 모두 물건 뒤, 기존 검정 전환 오버레이는 정상적으로 물건 위를 덮음. 추가 Canvas sorting override 없음. Editor의 UpgradeAdvertisementEffects로 작성, 현재 미저장 씬은 유지하면서 Result만 additive 저장.
- 실제 검증: 최종26/26 PASS, 컨페티 관측5.8549556초(timeScale0).2초 이후 컨페티 존재·자연종료·입력해제·일시정지에서 아우라 다회 반복·물건 불투명4지점 픽셀 불변·입장/컨페티/아우라 캡처 직접 확인. 단발 종료 후에도 아우라 활성·다음 입력 허용, 영상암전 및 실제 디코더첫프레임0, 중복/재진입/비활성/취소/홈 정리 확인. Tools/UI/verify_ad_highlight_loop.cs 및 docs/validation/ad_fx2/runtime.json/PNG. 이전 finite-aura 검증 스크립트는 과거요구 전용임을 주석으로 표시. 컴파일오류0; 기존 WindowsMediaFoundation 색원색 메타 경고는 원본 영상에서 발생하며 숨기지 않음.
- 보존: 최초 nested 컨페티 저장 중 Unity가 vendor 원본을 신버전 필드로 자동직렬화한 것을 발견. 프로젝트 복사본을 unpack하고 향후 작성도구도 분리 복사를 사용하도록 수정. 원본은 작업시작 SHA256과 일치하는 HEAD의 CRLF 바이트를 확인한 뒤 이번 자동직렬화만 복원(초기 검증에서 LF 바이트 불일치 시 쓰기 거부). 그 후 최종26항목 재실행·vendor195파일 해시 모두 동일. 원래 미저장 Idle 루트 활성/이름/Transform 상태도 동일, Play 종료. source_preservation.json/vendor_hashes.json 근거.
- 문서/남은 항목: AD_PRODUCT_HIGHLIGHT·UI_MOTION·CHANGELOG·TODO·HANDOFF 최신화. Windows Player/실물 버튼·RFID·장시간/Profiler·두 영상 전체길이는 미검증, RFID-BLOCKS-001 전체 인계대기를 완료로 바꾸지 않음. 외부 패키지/운영 설정 변경·PC종료·커밋/push 없음. 소유권 해제. 이번 작업의 치명적 장애 발견 없음.

### [2026-10-29 / Asia/Seoul] AD-FX-001 · Codex → 다음 작업자 · 완료(미커밋)

- 요청/기준: 현재 열린 Item의 ParticleImage 두 묶음을 광고 물건 강조에 적용, 데모 프리팹 전부 검토,2초 이내 종료 및 종료 전 광고 버튼 차단. main/37723c7 + 기존 미커밋/사용자 미저장 Scroll 보존, Codex 단독 구현·검증(교차검증 없음).
- 변경: 새 AdProductHighlight/Editor복사도구·Assets/UI/Effects/AdItemEmphasis.prefab(+meta), Result 광고 ResultImage 하위에 빛살·자식링·별 총3컴포넌트 설치. 금빛 단발/별 주변 제한/전체 실시간1.8초 및 입장.12초·퇴장.25초 페이드. UIScenePresentation 입장 훅과 ResultVideoController 직접진행 가드만 연결. 입력은 공통 lease, 중복 요청 무시, 취소·비활성화·씬퇴장 시 자기 효과/잠금 정리. 기존 색칠·센서·언어·영상 디코딩 알고리즘/설정·패키지 미변경.
- 전체 검토: Demo/Prefabs19개(하위_Primitives7개 포함)의 직렬화 시간·반복·입자/스프라이트·부가스크립트 확인. 현재 Item의 GlowRays/Ring2/4Star 채택, Highlight/EnergySpread의 짧은 강조 방향 참고. 컨페티·코인·슬라이더 등 제외 이유와 데이터는 AD_PRODUCT_HIGHLIGHT.md 및 validation/ad_fx/prefab_review.json.
- 실제 검증: 최종 코드/프리팹 Play21/21 PASS, 실측1.8135867초(Time.timeScale=0). 물리API 누름/해제와 직접 Continue 무시·대기입력 재실행 없음·별 범위·입자 종료·중복/재진입/취소/비활성/홈 정리, 광고→영상 완전암전과 실제 첫프레임0 확인. 컴파일오류0, 최종 전후 캡처 직접 확인. Tools/UI/verify_ad_highlight.cs와 docs/validation/ad_fx/runtime.json/PNG. 최초 도구는 그룹 입장 연출 중 탭을 보내 시간초과(initial_fixture_failure.json); 잠금 해제 후 탭하도록 도구 수정. 시각 확인 중 별 속도가 라이브러리 내부에서100배 적용됨을 찾아 .25(약25UI단위/초)로 제한한 최종 후보로21항목 재실행.
- 보존/최종상태: vendor195파일 SHA256 동일, 미저장 Item 디자인 직렬화 동일(Play 전환이 바꾸는 숨겨진 burst.used 실행 플래그만 제외), 원본 씬 저장/재작성 없음. PreviewScene 복사·Result만 additive 저장, Play 종료 후 임시 Idle을 닫고 원래 dirty Scroll 복원. docs/validation/ad_fx/source_preservation.json/source_after_play_diff.json/vendor_hashes.json.
- 미검증/남은 항목: Windows Player 빌드/설치PC·실물버튼/RFID·두 언어 영상 전체길이/장시간·Profiler 미수행. RFID-BLOCKS-001 전체는 계속 인계대기. 전체 git diff --check는 기존/Unity YAML의 빈 필드 공백을 보고하므로 임의 포맷하지 않음. 이번 신규 C#/문서 범위 확인, 외부 패키지 수정/PC종료/커밋/push 없음. 소유권 해제, 치명적 장애 발견 없음.

### [2026-10-29 / Asia/Seoul] VIDEO-FIRST-001 · Codex → 다음 작업자 · 완료(미커밋)

- 요청/기준: 영상 진입에서 첫 프레임 누락·Skybox/흰·검정 빈 화면·정지프레임 방지. main/37723c7 + 기존 미커밋 보존, Codex 단독 구현·검증(교차검증 없음). 기존 코드는 광고를0.5초 퇴장시킨 후 isPrepared만 확인해 재생하고 영상을0.5초 페이드인해, 빈 배경과 첫 부분 누락이 가능한 순서였음. 설치 Unity2022.3 VideoPlayer API/XML·실제 디코더 확인 후 수정.
- 변경: Assets/Scripts/UI/ResultVideoController.cs만 제품 코드 수정. 광고는 준비 내내 alpha1 유지, 미초기화 RawImage 비활성. frameReady를 준비 구간만 켜고 skipOnDrop=false·임시 음소거로 실제0번 프레임을 숨겨서 확보, EOF 후 RT 쓰기 완료를 기다려 원본 첫 프레임을 불투명하게 인계. 첫 렌더 한 프레임 후 즉시 Play/오디오·드롭설정 복원, 매프레임 이벤트 해제. 실패 시 광고 복원 후 RawImage를 끈 상태에서 기존 팝업 표시, 비활성화/자원 해제도 광고를 먼저 복원. 최종 프레임 팝업·센서/5초홈·타이머/세션 책임 보존. 씬·authoring·영상파일·이미지/언어/운영JSON·매니저·선택/색칠 코드는 이번 작업에서 수정하지 않음.
- 검증 도구/문서: 신규 Tools/UI/verify_video_first_frame.cs(실제 Native frameReady/Canvas 표시/RT 색·alpha/준비 화면 덮임/재생진행·종료/실패/재시도/복귀/비활성화·재활성화), verify_runtime.cs 보고 경로 분리. IMAGE_UI·UI_MOTION·CHANGELOG·TODO·이 문서 갱신.
- 실제 검증: 컴파일오류0. 최종 코드로 [첫 프레임41항목](../validation/ui/video_first_frame.json) PASS. Thai/en_retry/en_repeat3회 모두 firstPresentedFrame0, progressFrame8, uncoveredRenderCallbacks0. 첫 프레임이 실제 그림·alpha1임을 픽셀 검사했고 첫 화면 PNG 직접 확인. 준비 중 홈 요청/미디어 취소, 없는 영상 실패 시 불투명 광고+팝업 유지/타이머Resume, 올바른 영상 재시도, 끝부분seek 종료·최종 프레임/블록제거홈, 종료 컨트롤러 disable/enable 후 광고 복구·RT해제·반복 자원 정리 확인.
- 회귀: [기존 양언어 UI/영상58항목](../validation/ui/video_first_ui_regression.json) PASS.5초홈·영상준비중복귀·영어/태국어 올바른 clip·종료/세션·타이머·렌더텍스처 정리 포함. UI58 실행 후 OnDisable의 광고 복원1줄 보강 및 해당 경로 포함 첫 프레임41항목을 최종 코드로 재실행. 예상 진단은 첫프레임검증의 없는영상2개, UI회귀의 잘못된언어JSON1/없는영상2개뿐이며 주입상태 원복. 소스/도구/문서 diff 공백검사 통과, [원본58파일 SHA256 동일](../validation/ui/video_first_preservation.json).
- 증거/한계: docs/validation/ui/video_first_th/en_retry/en_repeat_screen.png는 최초 Canvas 표시의 실제 화면, *_target.png는0번 RT 픽셀 샘플. Windows Unity Editor의 실제 디코더 결과이며 Windows Player/다른 GPU·설치PC/영상 전체길이·실제 음량 청취/장시간·Profiler는 미검증. 순수입력/색칠 EditMode와 이전 색칠18은 이번에 재실행하지 않음. 기존 사용자 URP/ProjectSettings·원본/GUID 보존, 패키지 변경·PC종료·커밋/push 없음. Play 종료/Idle, 소유권 해제. 이번 작업에서 발견한 치명적 장애 없음.

### [2026-10-29 / Asia/Seoul] UI-SLOTS-001 · Codex → 다음 작업자 · 완료(미커밋)

- 요청/기준: 그룹 인식 후 계속 입력에서 암전 없이 원래 네 칸을 배치 화면의 숨긴 네 칸 위치로 이동. 후속 지시로 이동 중/도착 후 인식 해제돼도 이전 패널로 돌아가지 않고 현재 슬롯만 빈칸으로 유지. main/37723c7 + 기존 UI/사용자 미커밋 보존, 담당·검증 Codex 단독(교차 검증 없음).
- 변경 파일: BlockExperienceView(그룹 미인식 시 자동복귀 분기 제거·동시 패널전환/이동 연결), 신규 UIBlockSlotTransfer(+meta, 실제 네 원본과 BlockContent 임시 reparent/렌더 모서리 보간/도착 인계/원래 계층·배치 복구), UIScenePresentation(패널 교차페이드 중 이미 숨김 요청된 안내 보존). Tools/UI/verify_motion·verify_runtime의 최신 판정/별도 보고 경로, UI_MOTION·BUTTON_INPUT·CHANGELOG·TODO·이 문서. 씬 YAML·언어/원본/콘텐츠/운영설정·선택모델/색칠/영상/매니저는 이번 작업에서 수정하지 않음.
- 동작: 기존 네 칸은0.5초 InOutSine으로 함께 위치·크기 보간, 패널 내용은 동시 교차페이드. 목적지 칸은 이동 완료까지 비활성, 원본은 이동 내내 alpha1·추가뾰잉 없음. 도착 순간 목적지 네 칸 동시 활성화, 원본의 부모/순서/앵커/피벗/위치/크기/회전/디자인스케일 복구. 그룹 해제 시 이동은 계속되고 배치 화면을 유지하며 필수블록 완료 전 안내/짧은 진행 차단.5초 홈/다른 씬 암전/언어·색칠·영상 흐름 보존.
- 실제 검증: Unity2022.3.62f3 Windows Editor 컴파일오류0. [이동/연출/입력53항목](../validation/ui/slot_transfer_motion.json)·[기존 UI/영상58항목](../validation/ui/slot_transfer_ui_regression.json) PASS. Time.timeScale=0에서 동시이동/목적지 숨김/원본자식보존/정확한 도착 모서리·재진입 복원/이동중 및 이후 그룹 제거·재인식·진행차단/홈요청 후 임시레이어·잠금 해제/TH·EN 반복진입 확인. 언어JSON오류1·없는영상2의 의도한 진단 외 오류 없음. 원본58파일 해시동일([보존](../validation/ui/slot_transfer_preservation.json)), 새 스크립트 meta 존재, 소스·도구·문서 diff 공백검사 통과.
- 시각 확인: docs/validation/ui/slot_transfer_start·middle·end·removed.png 캡처. 중간 프레임에 원본 네 칸만 보이고 암전/목적지 중복 없음, 도착 후 정확한 빈칸4개와 조건에 맞게 숨긴 안내 확인. 원본 블록 그림 연결은 기존 후속 TODO이며 새 색칠/텍스트 표시를 추가하지 않음.
- 미검증/다음: 이번 작업에서 EditMode·Windows Player·실물버튼/RFID·Profiler·장시간 재실행은 하지 않음. 순수입력/색칠의 이전 EditMode27/27·색칠18 회귀는 이전 기록으로 보존하며 현재 새 통과로 계산하지 않음. 영상 종료는 기존 검증대로 재생 후 끝부분 seek로 확인, 전체길이 시청은 후속. 기존 URP/ProjectSettings·사용자 자산 변경 보존. 패키지 변경·PC종료·커밋/push 없음. Play 종료/Idle·씬dirty없음, 소유권 해제. 이번 작업에서 발견한 치명적 장애 없음.

### [2026-10-29 / Asia/Seoul] UI-MOTION-001 / UI-GATE-001 · Codex → 다음 작업자 · 완료(미커밋)

- 요청/기준: 사용자 지정 연출만 적용 후, 후속 명시 요청으로 필수 블록 완료 전 안내 숨김·짧은 물리1 진행 차단 추가. main/37723c7 + 기존 UI-LANG/POS/사용자 미커밋 보존. 담당·검증 Codex 단독, 교차 검증 없음.
- 변경: 신규 UI/UIMotion·UIAnimatedElement·UIPresentationTransitions·UIScenePresentation·UIPopupPresentation, 기존 GameManager/LocalizedImageSet/BlockExperienceView/ResultVideoController 시각 훅, BlockExperienceController의 잠금·안내 표시 위임. Editor/UIMotionAuthoring·ExhibitionUIBuilder와3씬 연결. 네 칸은 동시페이드/신규 배치만OutBack, 우측·광고이미지 페이드, 팝업 배경0.25초→원래크기OutBack0.3초, 안내0.25초/패널·씬각0.5초. UI_Gate는 연령1개/배치4개 인식 시 안내·진행 허용, 제거 시 되돌림. 입력 조건 외 색칠/세션/언어/영상/타이머 정책 보존.
- 실제 검증: Unity2022.3.62f3 Editor 컴파일오류0. EditMode27/27(job3df12518e0714277a0b8e99480a3115d), Tools/UI/verify_motion.cs39항목, 기존 UI회귀58·색칠회귀18 PASS. 원래 비단위 크기/OutBack·역전·동일요청·취소·일시정지·입력무시·씬/패널 준비순서·동시페이드·인식부족/제거/재인식·양언어영상/실패재시도/준비중취소·각단계5초홈 확인. 증거 [연출/입력](../validation/ui/motion.json), [UI회귀](../validation/ui/motion_ui_regression.json), [색칠회귀](../validation/ui/motion_color_regression.json), [EditMode](../validation/ui/motion_editmode.json).
- 보존/정적검사: 원본PNG/MP4·언어/콘텐츠/운영JSON·선택/색칠/입력58파일 SHA256 동일([증거](../validation/ui/motion_preservation.json)).3씬 누락스크립트0/관람객Text0·프레임8개원본색/페이드·빈BlockContent·Game계속안내 초기비활성([증거](../validation/ui/motion_scenes.json)), 신규스크립트메타누락0. 소스/도구/문서 diff 공백검사 통과. 전체 diff검사에는 Unity가 직렬화한 빈필드 뒤 공백과 기존 ProjectSettings 공백이 남아 있어 일괄포맷하지 않음.
- 검증 중 예상진단: 의도적 잘못된언어JSON1·없는영상2·없는색칠PNG2의 원본예외/영문메시지만 발생, 주입데이터 원복·해시 확인. 기존 UI검증기의 배경 상위 CanvasGroup 부재 가정은 새 공통 root와 맞지 않아 실제 배경불변/상위alpha1 검증으로 교체 후 통과.
- 문서: UI_MOTION·IMAGE_UI·BUTTON_INPUT·BLOCK_SIMULATOR·CHANGELOG·TODO·이문서 갱신. 검증 도구는 연출 완료 대기를 추가했으며 기존 기능의 판정은 유지.
- 미검증/다음: Windows Player/실물버튼·RFID, 영상 전체길이·음량, 설치PC 포커스/장시간·Profiler 후속. 기존 사용자 URP/ProjectSettings 변경 보존. 패키지/외부설치/PC종료 미변경, 커밋/push 없음. Play 종료·Idle 씬, 소유권 해제. 이번 작업에서 발견한 치명적 장애 없음.

### [2026-10-29 / Asia/Seoul] UI-POS-001 · Codex → 다음 작업자 · 완료(미커밋)

- 요청: 영어 타이틀 버튼 안내의 Figma 위치가 부정확하므로 태국어와 동일한 위치로 교정. 기준 main/37723c7 + UI-LANG-001 미커밋, Codex 단독 구현/검증. 시작/종료 HANDOFF와 Git 상태 확인, 별도 진행 소유권 없음.
- 변경: Lang_En.json의 idle.help x=692.31836→514.81787, y=840.08252→840.08301. Thai 원본 기준이며 width875.68164/height172.91748 유지. Unity 도구로 ExhibitionUIAssets.defaultEnglish만 동기화·저장하여 fallback에도 적용. 다른 JSON 항목·태국어·씬·스크립트·원본PNG/메타·기존 사용자 변경 보존. IMAGE_UI/CHANGELOG 및 검증/HANDOFF 갱신. Figma 원본은 조회 증거로 유지하고 수정하지 않음.
- 검증: JSON x/y 정확히 일치, 영어 원본크기 및 나머지13항목의 원본좌표 보존 확인. Unity2022.3.62f3 Play에서 실제2초 버튼 입력 TH→EN→TH 페이드 왕복 후 hint의 anchoredPosition이 두 언어 (514.8179,-840.083)로 일치, 영어Sprite 및 크기 유지 PASS. 기본사본=외부JSON 일치, sceneDirty=false. docs/validation/ui/title_hint_position.json·title_en.png(실제GameView) 기록/시각확인. 소스·씬 변경 없는 배치조정이라 전체 테스트/빌드 재실행 없음. Play 종료의 기존 apiUrl 미설정 exit-log 진단1건 외 새 오류 없음.
- 종료: Play 종료/Idle, 미커밋/push 없음, 파일 소유권 해제. Windows Player·실장비 검증은 기존 미완료 유지. 새 치명적장애 발견 없음.

### [2026-10-29 / Asia/Seoul] UI-LANG-001 · Codex → 다음 작업자 · 완료(미커밋)

- 요청: Figma/로컬 이미지로 텍스트 없는 UI, 일반 프레임, 기존 블록/색칠/결과 기능 유지. 타이틀 짧은 입력 현재 언어 시작·2초 유지 언어토글, 배경 제외 페이드교체, Lang_En/Lang_Th JSON, 모든 단계5초 홈 및 두 언어 영상. 후속 사용자 지시로 블록 칸 색칠/틴트/색상 견본을 전부 제거하고 실제 블록 이미지는 추후 연결하도록 빈 BlockContent만 남김.
- 기준/담당: main/37723c7, origin 현재 프로젝트 일치. Codex 단독 구현/검증, 교차검증 없음. 시작/종료 HANDOFF·git 상태 재확인, 기존 URP3개·ProjectSettings 변경 보존, 사용자 Assets/UI와 Resources/Videos 원본 PNG/MP4·GUID 보존. UI PNG41개는 필요한 Sprite/NPOT None/mipmap off/alpha/Clamp/uncompressed importer만 적용. 패키지·빌드등록·Player 식별자 변경 없음. Company MyCompany/Product Unity-Startup-Template/Standalone com.Unity-Technologies.com.unity.template.urp-blank 유지. 자동종료 설정/명령 미수정·미실행.
- Figma: 처음 로그인/편집권한 오류 후 사용자가 플러그인 연결 및 권한을 부여해 원본 읽음. TH20:223/236/257/279/287, EN20:350/363/384/406/436의 high-fidelity context/스크린샷 및 absoluteBoundingBox/absoluteRenderBounds 확인. 페이지/원본은 수정하지 않음. 모든1920×1080, 28안내 이미지와8블록칸 좌표를 확보. 렌더경계는 PNG의 글자/그림자 외곽과 대응하며 언어별 x/y/width/height를 JSON에 적용. Figma 원본 Student 예시 대신 사용자가 요구한 동일 일반 FrameImage를 배치.
- 변경: GameManager/ExhibitionButtonGesture(+경계/누름해제 테스트), BlockExperienceController의 TMP 선택필드 선택화·PresentationChanged·개발진단 기본숨김, 신규 Scripts/UI4개 및 Editor/ExhibitionUIBuilder, Lang JSON2개·UIAssets 라이브러리/기본JSON 사본, 기존3씬의 Canvas 이미지 UI. ResultImagePresenter 소스는 그대로 재사용/참조만 연결. Tools/UI 검증기 및 기존 Tools/Coloring 검증기를 새 버튼/화면 흐름에 맞춤. BUTTON_INPUT/BLOCK_SIMULATOR/IMAGE_UI/CHANGELOG/TODO/HANDOFF와 검증 증거 갱신.
- 동작: 타이틀 짧은 입력은 놓을 때 판정,2초에1회 토글하고 동일누름의 시작 억제. 배경 제외0.25초 fade-out→28매핑 중 현재 씬 이미지/좌표 일괄교체→0.25초 fade-in. 화면 취소/재활성 복원 및 잘못된 언어JSON의 마지막 작성본 fallback. 홈에서 세션/선택/결과는 초기화하되 모드는 유지. Game 연령 인식 후 짧은 입력으로 물품단계, 색칠완료 후 Result. 진행 안내는 Figma 위치에 유지하고 불완전 선택의 다음 이동은 차단. 블록 칸은 항상 흰 Image.color(원본 FrameImage색), 추가 색/그림 없음.
- 영상: JSON Videos/Video_en·Video_th 선택. 짧은 입력 재생/실패 재시도, 준비15초·재생길이+20초 상한, 화면+세션 취소, 영상 중 타이머 Pause. 종료/실패/취소/홈에서 Resume. 정상 종료는 마지막 프레임을 팝업 배경으로 보유했다가 홈/비활성화에서 클립·RenderTexture 해제. 완료안내는4블록 제거(SetBlockPresent) 또는 짧은/5초 입력 홈. 실장비 드라이버 추가 아님.
- 검증(이번 작업에서 Codex 실제 수행): Unity2022.3.62f3 컴파일오류0. EditMode27/27 PASS(job85ae6795a3eb4b7186d47563a72cbec6, ui/editmode.json). UI runtime-v2 58항목 PASS(ui/runtime.json): 두 언어·페이드 배경 제외/취소/복원·JSON 고의문법오류 fallback/정상복원·모든 바인딩 좌표/이미지·텍스트없음·미선택 진행차단·선택8칸 무색칠·자동색칠/같은Texture Result·영상 누락 실패/재시도·양언어 실제디코더 재생·seek후 종료·4블록 제거·그룹/물품/광고/재생/완료의5초홈·준비중 취소·타이머/단일매니저/어댑터·홈후소유RT0. 기존 COLOR runtime-v3 18항목도 재실행 PASS(연속변경 최신결과, 명시취소/재시도, 고의누락원본 실패복구, controller Off/On, Equipment, 세션취소 포함).
- 시각/정적 검증: 실제 TH/EN 타이틀·그룹·완성선택·광고·영상·종료 PNG 확인. EN 계속안내는 캡처프레임 안정화 후 실제 activeSelf/activeHierarchy=true, alpha1, 올바른 Sprite 확인하고 selection_en.png 갱신(continue_hint.json). 28로컬PNG의 실제크기와 Figma렌더크기 반올림1px이내 일치(asset_geometry.json). 세씬 missingScript0/Text0/8일반칸·빈비활성BlockContent, 저장dirty=false(scenes.json). 새 에셋/메타짝·JSON·문서링크·소스/문서diff --check 통과. 런타임 예상진단은 고의JSON문법·누락영상(2진단)·누락색칠원본(2진단)으로 원본예외 보존. 제품 치명적장애 발견 없음.
- 한계/남은일: 영상은 전체길이를 처음부터 시청하지 않고 재생 후 끝부분seek로 종료를 확인. Windows Player 빌드·설치PC, 실제 물리1버튼/RFID, 포커스와 실제키 조합, 오디오 볼륨/최종아트, 장시간/Profiler 미검증. Figma/제공PNG의 고정PressOnce→Thai/Hold→English 및 TH화면 영문부제는 그대로이므로 현 규칙에 맞는 안내 이미지 갱신 검토. 블록 아트는 후속 추가 예정으로 현재 비표시. 소유권 전부 해제, Play 종료/Idle씬, 커밋/push 없음.

### [2026-10-28 / Asia/Seoul] COMMIT-003 · Codex → 다음 작업자 · 완료

- 요청: 모든 변경사항 커밋. 기준 main/092fb12, origin 현재 프로젝트 확인. 담당/검증 Codex 단독. 이 기록을 포함한 로컬 커밋이며 해시는 git log -1로 확인, 원격 push 없음.
- 범위: COLOR-001 로컬색칠/48쌍원본·마스크/테스트/도구·문서·검증, REMOVE-COMFY-001 제거/legacy_runtime 이동 및 QUALITY-001 Tools·검증 보관 변경, 기존 사용자 AssetKits(ParticleImage)·메타, Idle변경, .vscode3파일·slnx를 포함한508경로. 사용자의 명시적 '모든 변경사항' 요청에 따라 이전에 제외했던 IDE설정/자동생성slnx도 이번에 포함. ignored Library/Temp/모델·도구결과/빌드 등은 강제추가하지 않음.
- 검증: stage507개 추가/수정·이동 대상의 에셋/메타 누락0, 341개 텍스트의 자격증명 패턴 검사 일치0, 신규50MiB초과파일0, staged LFS166개가 로컬객체의 크기·SHA256과 일치. 충돌파일/별도미등록변경 없음. staged diff/JSON 및 기존 runtime PASS증거 확인.
- 형식: git diff --cached --check에서 기존 Unity생성 YAML/메타·서드파티 에셋의 공백 경고가 있음. 사용자/서드파티 원문 보존을 위해 일괄수정하지 않음.
- 실행 검증: 직전 COLOR-001에서 수행한 Unity컴파일error0/EditMode26/26/Play18항목 PASS를 그대로 보존. 이번 커밋 준비에서 Unity·테스트를 다시 실행하지 않음. WindowsPlayer/실장비/장시간·Profiler/최종아트 검수 미완료 유지.
- 종료: 소유권 해제. 새 치명적장애 발견 없음. 다음 작업은 최종원본·마스크 교체 및 배포검수.

### [2026-10-28 / Asia/Seoul] COLOR-001 · Codex → 다음 작업자 · 완료

- 요청: ComfyUI 대신 나잇대에 맞는 서로 다른 물품 외형의 흰 원본+검정 외곽선에 설정한 색상으로 색칠. 작업 중 기술 설명 요청에는 마스크 기반 픽셀 곱셈 합성으로 답변.
- 기준: main/092fb12 + REMOVE-COMFY-001·QUALITY-001 미커밋 및 사용자 AssetKits/.vscode/slnx 보존. 시작 전/종료 전 HANDOFF와 git 상태 재확인, 다른 진행 소유권 없음. 기존 삭제·아카이브 변경을 되돌리지 않음. 커밋/push 없음. 담당/검증 Codex 단독, 교차 검증 없음.
- 실제 설정: Unity2022.3.62f3, 빌드0_Idle/1_Game/2_Result, Company MyCompany / Product Unity-Startup-Template / Standalone com.Unity-Technologies.com.unity.template.urp-blank 유지. 패키지·ProjectSettings·씬 편집 없음. Shutdown 모든 요일 비활성 유지, 명령 실행 없음.
- 코드: BlockColoringCatalog로 모든 연령×물품·RGB·중복/누락/상대경로 검증. BlockColorizer가 마스크 R/G 가중치로 원본RGB에 선택색 곱셈(검정선/회색명암/알파 보존, 원본배열 불변, 경계합256 반올림 허용/그 이상 중첩 거부). LocalBlockColoring은 로컬PNG 비동기로드/크기상한2048·16MiB/서로같은크기 확인, 배열계산만 worker, 취소 시에도 main으로 돌아와 Unity 텍스처 해제. 최종 텍스처 CPU사본은 업로드 후 해제.
- 흐름: 네 블록 완성/변경 시 자동색칠, 반복인식 무시. 선택·화면·세션 취소 연결, revision/작업토큰으로 늦은결과 거부. GenerateImage/CancelGeneration 호환 진입점·Editor 재시도/취소 버튼. 화면 완성Texture는 기존 GameManager에 전달, Result에 유지/Idle·선택변경에서 해제. 타이머 Pause하지 않음. 원본예외 스택/inner exception 기록. 저널 coloring_completed에 선택/확정물품/두RGB/파일경로/크기, 이미지 디스크 자동저장 없음.
- 에셋/문서: BlockColoring.json·Coloring/Samples 48쌍480RGBA+메타. Python/Pillow 자체도형 제작 도구 Tools/Coloring/build_samples.py 및 실행검증 verify_runtime.cs. Equipment는 연령별 고정매핑(Child robot/Student laptop/Adult keyboard/Senior laptop), 색 변경 시 유지. 샘플은 전시최종아트가 아니며 별도 Artwork 원본/마스크로 교체 가능. COLORING/BLOCK_SIMULATOR/CHANGELOG 갱신.
- 검증(Codex): EditMode26/26 PASS(job0233b7d2f228425c98c97a043904209a), source/마스크의 두영역·선·알파·명암·흰/검정/같은색·경계·취소·오류설정 및48쌍 검증. 처음25/26에서 보간 경계R+G256을 발견해1byte 정규화 수정 후 전부통과. 최종컴파일error0. Play18항목 PASS를 최종 오류문맥 개선 후 재실행 확인: Thai/English 진입, 자동색칠, 반복인식,3회 연속변경 최신결과, 취소/재시도, 누락파일 실패/원본복구 재시도, 비활성중 취소·선택초기화/재활성, Equipment, 동일Texture Result→Idle 파괴, 타이머 unpaused, 생성중 세션복귀/늦은결과 없음.
- 진단 구분: 실행검증 초기 버튼API에 키보드 어댑터가 간섭해 Game진입 timeout, 검증기에서 어댑터를 잠시 끄고 release프레임을 기다려 해소. 제품오류 아님. 고의누락파일 실패는 영문 메시지+원본예외2개가 정상 진단이며 복구 PASS. Play종료의 기존 apiUrl 미설정 메시지는 공통 서비스 기존 동작. 최종 실제 Game의 가방 빨강본체/파랑포켓·끈·검정선·PressButtonText 시각확인.
- 증거: docs/validation/coloring_runtime.json, coloring_bags.png(상단원본/하단빨강·파랑, 좌→우4연령), coloring_game.png. 파일/메타짝·JSON·변경diff/문서링크 확인. 최종 Play 종료·Idle씬. 새 치명적장애 발견 없음, TroubleShooting 추가 없음.
- 미검증/위험: WindowsPlayer 빌드/설치PC, 실제물리키/RFID, 전체48종 최종미술승인/모든1728색상조합 시각검수, 장시간메모리/Profiler·최대2048px 성능, 손상PNG·디스크권한 등의 실행오류 주입은 미수행. 누락파일실패/단위데이터오류를 손상PNG 배포검증으로 간주하지 않음. 최종 삽화와마스크 제작/배포검수 후속. 모든파일 소유권 해제, 미커밋.

### [2026-10-22 / Asia/Seoul] REMOVE-COMFY-001 · Codex → 다음 작업자 · 완료

- 요청: ComfyUI를 이용하던 기능을 교체할 예정이므로 프로젝트에서 사용하지 않도록 수정. 신규 서비스는 미지정.
- 기준/보존: main/092fb12 + QUALITY-001 미커밋(이전 Codex 작업으로 확인). 원격 origin 현재 프로젝트 일치. 사용자 AssetKits/.vscode/slnx와 다른 씬 레이아웃 보존. 커밋/push 없음. Unity2022.3.62f3, 빌드0_Idle/1_Game/2_Result, Company/Product/Identifier 기존 일반값 유지.
- 변경: GameLifetimeScope build callback의 Launcher/Retention 등록 제거(base.Configure·GameManager DI 유지). BlockExperienceController에서 Comfy 설정/클라이언트/그래프/네트워크·생성대기·Pause/Resume/이미지삭제 연결 제거. BlockSettings(Data+JSON+meta, 저널30개/4MiB)와 취소 가능한 로컬 파일 비동기 로더로 선택 초기화 분리. Generate/Cancel 임시 버튼 제거, GenerateImage는 기존 UnityEvent 호환 미설정 안내만 수행. 인체/제품 프롬프트 카탈로그·CreatePrompt/선택기록은 보존하고 Comfy 가이드 경로 필드만 제거.
- 결과 연결: TryDisplayResult(Texture2D,int revision)에서 실제 크기 Sprite 할당 후 표시. 미완성/늦은/비활성·씬전환 결과 false(호출자 소유), 성공 true(컨트롤러/매니저 소유). 향후 생성기는 화면/세션 취소 책임을 별도 구현해야 함. 기존 Result 전달·수동/타임아웃 Idle 및 자원 해제 코드 유지.
- 아카이브: ComfyImageClient/Launcher/StatusPresenter/GeneratedImageRetention/Policy와 메타, 전용 보관테스트15건, ComfyUISettings/API/ControlGuides14개+메타를 Tools/ComfyUI/legacy_runtime/Assets로 이동. 이전 미커밋 QUALITY-001 내용 보존 확인. Unity 컴파일/Player 배포 대상 밖. Idle Presenter를 Unity로 제거하고 상태텍스트는 비활성 ImageGenerationStatusText/빈 문구로 보존. Tools README/COMFYUI_SETUP에 보관 경고, BLOCK_SIMULATOR/CHANGELOG 갱신. 외부 설치본·모델·기존 파일·실행 중 서버를 호출/삭제/종료하지 않음. 이전 파일도 앱이 더 이상 자동 정리하지 않음.
- 검증(Codex): Unity 컴파일error0, EditMode19/19 PASS(job4375718327e848b3b2b74a7dddede1e1). 활성 Scripts/Tests/StreamingAssets/Scenes/Prefabs에서 Comfy/retention/8188URL/workflow/controlImage 참조0, 컴파일된 전용타입 부재, 루트/Idle에서 서버컴포넌트/문구 없음. Play 두 세션에서 Thai/English 버튼 API 입력, 숫자키와 같은 ToggleSlot 팝업/재입력 제거·네 OptionText, 미설정 GenerateImage(이미지/진행상태 없음), 합성32×16/16×16 Texture로 현재 결과 수용·늦은/비활성 결과 거부, Game→Result 같은Texture→Idle 파괴, 선택변경3회 자원해제 및 Off/On 선택초기화 확인. Play중 Consoleerror0, 미설정 진입의 의도된 영문 경고1회. Play종료 후 기존 공통 ApiManagerBase의 apiUrl 미설정/exit로그 생략 메시지1(서버 요청 없음, 이번 변경 범위 밖). 실제 새 선택 JSONL에서 started/ended/recognized/removed 및 generation_* 없음 확인.
- 증거: docs/validation/comfy_removal.json. 합성 텍스처 시험이며 AI 생성 성공을 뜻하지 않음. Play 종료·Idle 씬. 파일/메타 짝·diff 확인. 미검증: WindowsPlayer, 실제물리키/버튼/RFID, 새 생성기/네트워크·보관 정책, 장시간/Profiler. 기존 결과UI를 쓰는 새 생성 서비스 구현은 후속.
- 종료: 파일 소유권 해제, 미커밋. 현재 동작은 블록 선택/데이터 기록까지만이며 새 이미지를 자동 생성하지 않음. 치명적 장애 발견 없음, TroubleShooting 추가 없음.

### [2026-10-19 / Asia/Seoul] QUALITY-001 · Codex → 다음 작업자 · 완료

- 요청: 세부 묘사/단일 물품·인체 배제 개선, 배경 실제 알파 투명 처리, 시간보다 품질 우선.
- 기준/보존: main/092fb12. 작업 전 tracked 변경 없음. 사용자 AssetKits·.vscode·slnx 보존, 씬/ProjectSettings/패키지·Claude 자동실행/Result/보관 기능 미수정. 커밋/push 없음.
- 변경: Animagine XL4.0 Opt 체크포인트(약6.94GB)·BiRefNet(기설치)·SDXL Canny Control-LoRA rank256(약.77GB)의 고정 revision/SHA 검증. 내부1024/28steps/CFG5/Euler ancestral normal→BiRefNet 전경→InvertMask→JoinImageWithAlpha→480 RGBA. Control1.4로 단일 제품 구조14개 제약. 그룹을 인물 이름 대신 디자인 특성으로 표현, 제품별 재질/구조·부정 프롬프트. Shoes는 신발1개. Tools와 런타임 카탈로그 공유, API/UI 재생성. 모델 weights는 저장소 밖 Comfy Shared/models에만 설치. 기존 모델/워크플로 보존, 커스텀노드/서버 재시작 없음.
- Unity: BlockPrompt.controlImage/negativePrompt, BlockExperienceController가 선택 가이드 노드24에 주입하고 생성 전 로컬 StreamingAssets 가이드를 SHA 확인/404 시 업로드(타임아웃/취소/서로 다른 기존 내용 거부, POST 자동재시도 없음). Download는480 RGBA 헤더 검사. 기존 노드7 SaveImage 계약·결과 소유권·세션 취소·Pause/Resume·JSONL/보관 제한 유지. 생성완료 기록에 RGBA 포맷 추가. 새 가이드/메타14개, 설치/재생성 도구·모델 manifest·운영 문서/CHANGELOG/검증 자료 추가.
- 검증(Codex): Python5개(1728입력 조합 포함) PASS. Unity2022.3.62f3 컴파일error0, 최종 EditMode33/33(job fc191e645ad1411cba05e0cd807f3948). RTX5070 Ti Laptop12GB / Comfy0.37.0 torch2.12.1+cu130, 최종 구조 제어14샘플31.392~44.854초(중앙38.059). 모두480RGBA·alpha0~255·네 모서리0. 각 샘플에서 단일 제품 구도/사람 신체 없음 관찰. 텀블러 캔 무늬는 프롬프트 수정 후 Unity 실생성43.169초 재검사. 실제 GPU readback 모서리0/중앙약.996, 격리fixture 업로드/기존확인/내용불일치·RGB거부/오프라인/사전·진행중 취소 통과. Game 표시→버튼API짧은입력→Result 동일texture→수동Idle 복귀 시texture파괴·언어해제 확인. Consoleerror0. Comfy 사용자 워크플로 Thai_02_product_480_quality_alpha.json 새 이름 저장·readback 일치.
- 증거: Tools/ComfyUI/validation/quality의 gallery/원본RGBA/report.json/unity_client.json/unity_flow.json/runtime_prompt.json. 약한 Control.85·텍스트만 적용한 실패도 report 요약에 보존. 개별 원본 중간 실패는 ignored results에 보존. API quality_probe 결과는 유한한 개발 샘플로 운영 보관 prefix 대상 아님.
- 미검증/위험: RTX3060 20초·WindowsPlayer·실제키/버튼/RFID·장시간/메모리Profiler·전색/전시드 품질. 일부 추가색/문양·자전거 페달/키보드키/로봇관절·휴대폰/노트북 그림자 유사 색면·얇은 선/렌즈 알파 편차. 완전한 인체/복수 생성 금지는 보장하지 않음. 고품질 대신 속도 절충을 되돌리지 않음. Control-LoRA는 별도 Community License(100만MAU 초과 추가 상업조건)이며 manifest/설치문서 링크 참조.
- 종료: Play 종료·Idle 씬, 검증 fixture18188 종료, 실제8188 유지. 임시 런타임 타이머/시뮬레이터 상태는 Play 종료로 폐기. 파일 소유권 해제. 다음은 대상PC/배포 검수와 미술 승인. 새 치명적 장애 없음(TroubleShooting 추가 없음).

### [2026-10-19 / Asia/Seoul] COMFYLAUNCH-001 · Claude → 다음 작업자 · 완료

- 요청: 빌드본 실행 시 ComfyUI 실행 여부 검사, 꺼져 있으면 실행. Idle `ComfyUIStatusText`에 상태 표시. 사용자 결정: Python 서버 직접 실행, 앱 종료 시 서버 유지, 실행 후 작동까지 로딩 중에만 표시·완료 시 비활성화.
- 기준/보존: main/ceef831. 작업 전 사용자 미커밋 0_Idle 변경(ComfyUIStatusText 추가 등) 보존, IdlePanel에 컴포넌트 1개만 추가. AssetKits·.vscode·slnx 미수정. 미커밋.
- 변경: 신규 `Blocks/ComfyServerLauncher`(루트 GameManager에 GameLifetimeScope build callback으로 추가, 앱 실행당 1회 /system_stats 3초 probe → 없으면 Process.Start UseShellExecute=false/CreateNoWindow → 2초 간격 probe, 프로세스 조기 종료/시간 초과 시 Unavailable, 이후 10초 probe만, 재실행 없음, 종료 시 핸들만 Dispose). 응답 JSON에 `system` 키가 있어야 준비로 판단. 신규 `Blocks/ComfyStatusPresenter`(IdlePanel, Starting/Unavailable에서만 텍스트 표시, 문구는 직렬화 필드). `ComfyBlockSettings`에 autoLaunchServer/launchServerInEditor(false)/serverExecutable/serverArguments/serverWorkingDirectory/serverStartupTimeoutSeconds 및 별도 ValidateLaunch(loopback 필수). ComfyUISettings.json에 이 PC Comfy Desktop 설치 기준 `%LOCALAPPDATA%` 값(models/input/output은 ComfyUI-Shared 직접 지정, Desktop 진단 프로필 yaml·manager/telemetry 플래그 미사용). COMFYUI_SETUP 자동 실행 절, CHANGELOG.
- 검증(Claude/Unity2022.3.62f3 Editor): 컴파일 오류0, EditMode 33/33(job 5da1413be6614c339ccf0ce3938621e8). Play① 기존 8188 실행 중: Ready, 텍스트 비활성, launcher 1개. ② 기존 서버를 끄지 않도록 임시로 serverUrl/--port 8189·launchServerInEditor=true: Checking(텍스트 숨김)→프로세스 시작·"Starting..." 표시→약12초 후 Ready·텍스트 비활성. Play 종료 후 해당 서버 200 응답 유지, MainWindowHandle 0(창 없음). 확인 후 이 테스트 8189 프로세스 2개만 종료. ③ 8189·에디터 기본값: Unavailable·"not available" 표시·경고 1회. 설정 파일은 백업본으로 원복(8189 잔존0), 기존 8188 서버 계속 정상.
- 미검증/위험: Windows Player 빌드에서의 실제 자동 실행·권한·다른 계정/PC 경로, 첫 생성 시 모델 로딩 시간(서버 준비는 모델 로딩 전 시점), 실행 중 서버 종료 감시(요구 범위 밖, 1회 검사), Desktop 앱과 동시 실행 시 포트 충돌(응답하는 서버가 있으면 Ready 처리). 직접 실행 서버의 user/custom_nodes는 설치본 기본 폴더 사용.
- 종료: 파일 소유권 해제, Play 종료, Idle 씬 열림. 사용자 요청으로 사용자 Idle 변경(ComfyUIStatusText)과 함께 이 기록을 포함해 로컬 커밋(해시는 git log -1, push 없음). AssetKits·.vscode·slnx 제외. 장애: 발견 없음.

### [2026-10-19 / Asia/Seoul] RESULTSCENE-001(씬 연결·검증) · Claude → 다음 작업자 · 완료

- 기준: main/9d45652 + 아래 기록의 미커밋 코드. Unity MCP 재연결 후 컴파일 오류0 확인.
- 씬(Unity SerializedObject로 저장): 1_Game `BlockExperienceController.pressButtonText`←`Canvas/ProductChoicePanel/PressButtonText`, 해당 오브젝트 초기 비활성(diff 2줄). 2_Result `Canvas/ResultPanel`에 ResultImagePresenter 추가, `resultImage`←`ResultPanel/AdImageBox/ResultImage`(실제 계층; 문서 경로도 정정). EditorBuildSettings에 2_Result(index 2) 추가. 사용자 레이아웃·텍스트·AssetKits 미수정.
- 검증(Claude/Unity2022.3.62f3 Editor): EditMode 33/33 PASS(job efe2f53a1dd64013a5fa9c090103c92c, ShortPress 3건 포함). Play: Idle에서 GameManager.SetButtonPressed로 단일 입력→Game(Thai) 진입, 4블록(student/bags/blue/white) 인식 시 PressButtonText off, GenerateImage→실제 localhost ComfyUI 480×480 수신 후 PressButtonText on·ResultTexture 이전. 약 3초 짧은 누름→2_Result, 세션/Thai 유지, ResultImage sprite가 같은 텍스처 사용·활성, GameManager 1개. 5초 유지→Idle, 세션 해제, ResultTexture null, 480 텍스처 잔존 0개. Console error0(MCP 중복명령 억제 경고만). Play 종료, Game 씬 열림.
- 검증 방법 주의: 키보드 시뮬레이터가 매 프레임/포커스 이벤트로 버튼 상태를 덮어써 테스트 중 Play 런타임에서만 비활성화(씬 변경 없음). 실제 숫자1 물리 키 입력은 미수행.
- 미검증: Result에서 30초 무입력 복귀(같은 Idle 진입 해제 경로), 생성 중 짧은 누름, Windows Player, 실제 RFID/버튼 하드웨어.
- 종료: 파일 소유권 해제. 사용자 요청으로 이 기록을 포함해 로컬 커밋(해시는 git log -1, push 없음). 미추적 AssetKits(ParticleImage)·.vscode·slnx는 2_Result가 참조하지 않아 제외·보존. 장애: 발견 없음.

### [2026-10-19 / Asia/Seoul] RESULTSCENE-001 · Claude → Claude(MCP 재시작 후) · 인계 대기

- 요청: Game에서 이미지 생성 전 PressButtonText 비활성화, 표시 후 버튼(숫자1) 입력 시 2_Result로 이동해 ResultPanel/ResultImage에 그 이미지 출력.
- 기준/보존: main/9d45652. 사용자 미추적 `Assets/Scenes/2_Result.unity`(+meta), `Assets/AssetKits/` 보존·미수정. 미커밋.
- 결정: 5초 유지 복귀와 충돌하지 않도록 비Idle 씬 입력에 `ButtonGesture.ShortPress`(5초 전 놓을 때 1회) 추가. GameManager가 생성 Texture2D를 소유(SetResultTexture/ClearResultTexture), Game→Result는 세션·언어·토큰 유지(EndSession 생략), Idle 복귀·새 Game 세션·OnDestroy에서 해제. BlockExperienceController는 이미지 표시 후 소유권 이전 성공 시에만 PressButtonText 활성화, 선택변경/재생성 시 숨김+해제, 씬 전환 중 비활성화에서는 이전 이미지 유지. 신규 `Assets/Scripts/Result/ResultImagePresenter.cs`(Sprite만 생성·해제, 텍스처는 매니저 소유). 언어 선택 없이 Game 직접 Play 시에는 이전하지 않음.
- 검증(Claude): 스크래치 csproj(dotnet 10)로 Assembly-CSharp+Blocks+Result 컴파일 오류0, Exhibition.Input 오류0. ShortPress EditMode 테스트 3건 추가했으나 Unity Test Runner 미실행. Play/씬 검증 미수행.
- 남은 작업(씬, Unity MCP 필요): 1) 1_Game `BlockExperienceController.pressButtonText`←PressButtonText 연결 및 PressButtonText 초기 비활성. 2) 2_Result ResultPanel에 ResultImagePresenter 추가, resultImage←ResultPanel/ResultImage. 3) EditorBuildSettings에 2_Result 추가. 이후 EditMode 테스트·Idle→Game→생성→짧은누름→Result→5초/무입력 복귀 Play 검증.
- 장애: 발견 없음.

### [2026-10-19 / Asia/Seoul] LOG-002 · Claude → 다음 작업자 · 완료

- 요청: 프로그램 주요 분기별 Debug.Log 추가. 영문, 기존 `"[Tag] Sentence.", this` 형식 준수.
- 기준/범위: main/6e1a923, 시작 시 미커밋 소스 변경 없음(.vscode/.slnx 로컬 파일만). 로그 문장 추가만 수행, 동작·씬·설정·패키지 변경 없음. 사용자 요청으로 이 기록을 포함해 로컬 커밋, push 없음.
- 추가 위치: GameManager(버튼 입력 준비/간격, 무입력 복귀, 수동 복귀 요청, 씬 전환 시작·취소, Idle 복귀 완료; 기존 Game 진입 언어 로그는 1회 유지), KeyboardButtonSimulator(등록/중복 파괴), GameLifetimeScope(Retention 컴포넌트 추가 시), BlockExperienceController(설정 로드, 블록 인식/제거, 생성 시작/대기열/선택 변경으로 폐기/취소·타임아웃, 세션 종료), ComfyImageClient(history 재시도 경고), GeneratedImageRetention(시작 한도, 출력 폴더 해석 — 같은 경로 반복 기록 방지). 모두 이벤트성 1회 로그이며 매 프레임 로그 없음. API 키·개인정보 미기록.
- 검증(Claude): Unity MCP 도구가 이 세션에 로드되지 않아 에디터 컴파일은 미수행. 대신 스크래치 폴더에 Assembly-CSharp.csproj 복사본(기존 생성본에 Blocks/ScriptableObjects 스크립트 누락 → 추가)을 만들어 dotnet 10 build: 오류 0, 프로젝트 스크립트 경고 0. Play 실행·실제 로그 출력 확인은 미수행.
- 위험: GameManager 간격 로그의 double 표기는 시스템 문화권 서식을 따름. 루트 Assembly-CSharp.csproj가 오래되어 IDE에서 Blocks가 보이지 않을 수 있음(Unity가 재생성).
- 다음 행동: Unity 에디터 컴파일 확인 후 Idle→Game→생성→복귀 Play로 콘솔 로그 확인. 파일 소유권 해제.
- 장애: 이번 작업에서 발견 없음.

### [2026-10-19 / Asia/Seoul] COMMIT-002 · Codex → 다음 작업자 · 완료

- 요청: 작업 내용 커밋. 기준 main/50cbe9b, 실제 origin이 현재 프로젝트 저장소임을 확인. BLOCK-001/RETENTION-001/RESULT-001과 관련 Game 씬·폰트·ScriptableObject 변경을 함께 저장. 원격 push 요청 없음.
- 포함: Assets의 블록/ComfyUI/JSONL/이미지 보관 정책·루트 등록·결과표시와 테스트/메타, StreamingAssets 카탈로그·API 그래프·설정, 사용자 Game UI·폰트·GameSceneTitleTextSO, CHANGELOG/운영 문서/HANDOFF, Tools/ComfyUI/validation의 block_runtime/result_ui/retention JSON.
- 제외: .vscode 및 자동생성 slnx, ignored 생성물·모델·캐시·임시fixture/junction. 파일을 삭제하지 않고 로컬 보존. 이전 자동검토 차단된 테스트junction도 이 범위에 남아 있으므로 무분별한 재귀 정리를 하지 말 것.
- 검증: 커밋 직전 관련 Asset 메타 짝과 설정/검증 JSON 파싱 확인. Unity30개 테스트 및 실제 생성/정리/결과UI 검증은 각 작업의 실제 기록이며 이번 커밋 준비에서 재실행하지 않음. WindowsPlayer/실제RFID/RTX3060/장시간 미검증 유지. 사용자 변경을 일괄 포맷하지 않음.
- 종료: 이 기록을 포함한 커밋 해시는 git log -1로 확인. 커밋 후 상태 확인, 파일 소유권 해제. 새 치명적 장애 없음.

### [2026-10-19 / Asia/Seoul] RESULT-001 · Codex → 다음 작업자 · 완료

- 요청: RecommendedProductImageBox 내부 ResultImage에 생성물을 표시. 생성 전 TextGroup, 완료 후 먼저 이미지 로드/할당한 다음 ResultImage 활성화.
- 기준/보존: main/50cbe9b + 기존 미커밋. 새 사용자 Game UI 구조(TextGroup/ResultImage 형제·ResultImage 비활성)와 폰트 변경 확인·보존. 원본/패키지/이미지 저장방식 변경 없음. 커밋/push 없음.
- 변경: BlockExperienceController.resultImage를 부모 박스 Image에서 자식 ResultImage Image로 재연결하고 resultTextGroup 직렬화 참조 추가. OnEnable/ClearImage에서 ResultImage Off/TextGroup On, 완료 경로는 다운로드·Sprite생성·Image할당·preserveAspect 후 ResultImage On/TextGroup Off. 기존 취소/선택 변경/비활성화 자원 해제 보존. Game 씬은 Unity SerializedObject로 참조 및 초기 상태만 저장, 사용자 레이아웃/텍스트는 수정하지 않음. BLOCK_SIMULATOR/CHANGELOG 갱신.
- 검증(Codex/Unity2022.3.62f3): 컴파일error0. 초기/생성중 image=false/text=true. 실제ComfyUI 480이미지 수신 후 child ResultImage sprite할당/image=true/text=false, 부모박스 Image에는 sprite없음 유지. Canvas.willRenderCanvases 관찰에서 첫 활성 렌더의 sprite누락/안내중복0회. 컴포넌트Off/On 시 sprite해제·안내복구. localhost 닫힌포트의 의도적연결실패 및 즉시취소에서 image=false/text=true, 취소 후타이머재개 확인. 저장설정 서버주소는 변경하지 않았고 Play 종료 시 런타임 변경 소멸.
- 증거: Tools/ComfyUI/validation/result_ui.json. 이번 단순 UI 변경용 테스트를 새로 작성하지 않았으며 이전30개 테스트 통과는 RETENTION-001의 별도 기록. 실제물리클릭/WindowsPlayer/장시간은 미검증. 의도적연결실패 로그1회 외 새제품오류 없음. 새 치명적 장애 없음.
- 종료: Game 씬 저장 및 Play 종료. RESULT-001 파일 소유권 해제, 미커밋. 향후 생성결과 표시참조는 부모박스가 아니라 자식ResultImage를 사용.

### [2026-10-19 / Asia/Seoul] RETENTION-001 · Codex → 다음 작업자 · 완료

- 요청: 생성 이미지 누적 방지를 위한 프로젝트별 개수/총용량 제한·오래된 파일 자동 삭제. 작업 중 사용자가 이미지 저장 이유를 질문하여 표준 SaveImage→/view 전달 방식 때문이며 AI 생성에 영구저장이 필수는 아니라고 설명. 현재 보관 한도 방식을 유지하며 표시 후 즉시 삭제 방식은 별도 후속 변경.
- 기준/범위: main/50cbe9b + BLOCK-001 등 기존 미커밋 보존. GeneratedImageRetentionPolicy(Data)·GeneratedImageRetention(root)·검증 테스트 및 메타 신규, GameLifetimeScope build callback, BlockExperienceController 제출 전 검사, ComfyUISettings, 운영 문서/CHANGELOG/검증 JSON/HANDOFF. 패키지/원본/서버 설치 변경·커밋·push 없음.
- 기본: 최대1000장 및512MiB, 시작/30초 주기/새 생성 전 정리. LastWriteTimeUtc 오래된 순서. 최근60초 및 활성 요청 보호, 보호/권한 문제로 한도 복구 불가하면 새 제출 거부. 앱 종료 뒤 서버에서 생성된 파일은 다음 앱 시작 때 처리. 강제 디스크 quota/서버 상주 프로세스가 아님.
- 경계: ComfyUI output/Thai02 바로 아래 blocks_<32자리소문자GUID>_<숫자>_.png만 삭제. 하위/다른 이름/확장자/모델/JSONL/기존product_*.png 제외. 절대 경로/정확한 하위 경로 검사, junction/symlink 경로 및 파일 거부. loopback system_stats.argv --output-directory 자동탐색; 원격/인자 누락은 comfyOutputDirectory 공유 경로 명시 필요. 설정 변경 후 재시작.
- 검증(Codex): Unity2022.3.62f3 최종 EditMode30/30 PASS(job f399ba50cea041de9da5fadb2c7a3183). 개수/용량/양한도, 오래된 순, 최근/활성 보호, 잠긴 파일 실패 후 다른 파일 삭제, 범위 밖/하위 보존, 취소, 잘못된 경로·한도, 파일명 검사. 수동 경로 보호 시험에서 / 구분자 절대경로의 오거부를 발견해 정규화 수정 및 테스트 추가 후 전체 재통과.
- 실제 실행: root 서비스1개, 실제 Comfy output 자동탐색. 격리fixture3개×500000bytes에서1개 삭제/2개1000000bytes 보관, foreign 보존. 실제 Unity→ComfyUI 이미지480 생성 성공. Idle 상태에서 수동Sweep 없이30초 자연 주기로 격리fixture3개→최신1개, foreign 보존. fixture 설정은 유지보수 gate를 잡고 변경/복구했으며 마지막에 실제output·1000장/512MiB 복구 확인. 실제 서버의 기존 결과는 한도 미만이므로 정리 테스트는 모두 격리fixture로 수행.
- junction 검증: ignored results/retention_fixture/out/Thai02를 같은 테스트영역 outside에 연결한 junction으로 만들고 정책의 삭제 거부·대상 sentinel 보존 확인. 테스트 후 Remove-Item으로 해당 junction만 정리하려던 도구 호출은 자동 승인 검토가 blocked by policy로 거부(추가 이유 제공 없음). 다른 삭제 방법을 시도하지 않고 ignored 결과에 그대로 남김. 재귀 도구로 무작정 정리하지 말 것.
- 증거/종료: Tools/ComfyUI/validation/retention.json. 최종 Console error0(MCP 중복명령 억제 경고만), 메타/설정/diff 검사. Play 종료·Game 씬 열림. 미검증: Windows Player/원격공유/배포 권한·디스크가득참/장시간. 새 치명적 장애 없음. 파일 소유권 해제, 미커밋.

### [2026-10-19 / Asia/Seoul] BLOCK-001 · Codex → 다음 작업자 · 완료

- 요청: RFID 준비 전 숫자2/3/4/5로 Group/Product/Color1/Color2를 인식/해제하고 기존 OptionText에 표시. 4그룹·12제품·6색, Equipment는 화면에는 이름만, 내부 robots/keyboards/laptops 중 하나로 이미지 생성. 선택·프롬프트를 분석용 데이터로 저장하고 이미지 생성 연결.
- 기준: main / 50cbe9b. 작업 전부터 Game 씬 수정(140줄), 신규 ScriptableObjects 및 GameSceneTitleTextSO 등이 있었으며 그대로 보존. 씬은 열린 에디터에서 기존 구조를 읽고 BlockExperience 오브젝트 및 필드 연결만 추가. 추가 패키지 업데이트 없음. 커밋/push 없음.
- 변경: Assets/Scripts/Blocks의 순수 선택/카탈로그 데이터(별도 asmdef), BlockExperienceController, BlockJournal, ComfyImageClient 및 메타. StreamingAssets/BlockGeneration의 카탈로그·별도 서버 설정·API 그래프, Tests/Blocks 6개, Game 연결, docs/BLOCK_SIMULATOR.md, COMFYUI_SETUP의 후속 구현 링크, CHANGELOG, validation/block_runtime.json.
- 동작: Editor 전용 키/임시 목록·Generate/Cancel. 네 슬롯 완성 시 명시적으로 Generate. Equipment는 요청마다 내부 분기를 선택해 원본 선택과 함께 기록. API는 공개 RecognizeBlock/RemoveBlock/GenerateImage로 미래 RFID 연결 가능. UI 표시는 지정된 영문명; 태국어 번역은 별도. 그룹 전체 변경 시 옵션을 자동 삭제하지 않으며 각 슬롯 독립.
- 데이터: persistentDataPath/BlockData/blocks_*.jsonl에 익명 세션·선택/해제·정확한 양/음 프롬프트·장비분기·seed·전체 모델/샘플링 그래프를 저장한 후 서버 제출. 서버 오프라인에도 생성 요청 조건 저장. 쓰기 실패면 제출 중단. 파일4MiB/최대30개 순환, 쓰기대기128개 제한, 작업 스레드 직렬 기록. 서버 PNG 보존은 ComfyUI에서 별도 관리. 대규모 데이터 플랫폼을 구축한 것으로 해석하지 말 것.
- 실패/수명: 요청15초·전체180초(실제 시간), history GET만 상한2회 재시도, POST 자동재시도 없음. 바쁜 큐는 제출 전 안내. 생성 중 타이머Pause 및 모든 종료Resume. 선택 변경·세션/씬/컴포넌트 종료에서 수신 취소/늦은 결과 차단, Sprite/Texture 해제, 재진입 초기화. 자신의 pending만 삭제하고 전역interrupt를 쓰지 않으므로 실행 중 서버 생성은 남을 수 있음.
- 실제 검증(Codex/Unity2022.3.62f3): EditMode 신규6+기존9=15/15 PASS, job fd002d92a2a44c018330d2660d82d238. 1728선택 조합의 프롬프트 색상 부위·장비3분기·해제·리셋·잘못된ID/중복 인식 검증. 직접 Game 실행 및 실제 Thai 세션 진입에서 컴포넌트 준비 확인. Input System 가상 Digit2~5 전달과 Update 경로로 해제 확인, 슬롯 팝업 재열기/기존 OptionText 연결·Equipment 라벨 확인. popup 및 이미지 UI 스크린샷 시각 확인. 실제 물리 마우스 클릭 자동화는 미수행이며 선택 API/클릭 핸들러 공통 경로로 검증.
- 실제 서버: localhost8188 ComfyUI, RTX5070Ti Laptop12GB에서 Unity로 가방/Equipment(robots) 두 요청 완료 및480PNG 표시. 요청 기록→완료 기록12.144초/6.544초(모델 온도·환경이 달라 성능 보증 아님). 색 변경 중 취소→오래된 이미지 없음/타이머재개, localhost 닫힌포트로 연결 실패→오류1회/복구 후 재시도, 컴포넌트Off/On 초기화, Idle복귀/재진입, 실제Thai세션 요청중 Idle복귀에서 controller0/타이머정상 확인. 초기Cursor가 루트Start에서 숨겨져 준비 완료 후 재표시하도록 수정하고 재실행에서 true 확인. 최종 컴파일/Console error0; MCP 재연결 중복명령 억제 경고만 있음.
- 증거: Tools/ComfyUI/validation/block_runtime.json. 전체 로컬 이벤트는 persistentDataPath/BlockData, 캡처는 ignored Tools/ComfyUI/results/block_runtime. 원본 사용자 데이터/모델 가중치를 저장소에 넣지 않음.
- 미검증/위험: Windows Player, 실제RFID, 전체12제품·모든색의 시각 품질, RTX3060 20초, 프로파일러/GC·장시간, 파일권한/디스크부족/보존한도 도달 실험, 전체180초 타임아웃 대기는 미수행. 정지 이미지까지만 연결; 영상·생성 중 이미지 스트리밍·연속latent 전환 미구현. 기존 SDXL의 인물·복수 물품·화풍편차 한계 유지. 강제종료 시 JSONL 마지막줄 손상/종료이벤트 누락 가능하므로 완전한 줄 단위 수집 필요.
- 종료: 에디터 Play 종료, Game 씬 열림. 신규 메타/참조·설정 JSON·영문 로그·문서 링크 확인. 사용자 기존 SO/씬 변경 및 로컬 IDE 파일 보존. 치명적 장애 신규 발견 없음, 파일 소유권 해제. 실제RFID 어댑터·배포검수는 후속.

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
