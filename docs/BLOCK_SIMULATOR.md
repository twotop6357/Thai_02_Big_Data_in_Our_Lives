# 블록 시뮬레이터와 ComfyUI 이미지 생성

2026-11-02 RFID-TCP-MAP-001: 실제 TCP 리더 연결과 F10 카드 매핑은 [RFID_SETUP](RFID_SETUP.md)을 따른다. RFID-LISTEN-001부터 앱이 포트10123을 열고 리더가 접속한다. 네 위치는 리더 IP121/122/123/124의 좌상/우상/좌하/우하 순이며 JSON 설정/저장·실시간 HEX 조회를 지원한다. 리더가 접속하지 않은 칸의 시뮬레이터 블록은 유지된다. 생성 중 고정과 그룹 우선 규칙을 유지한다.

## 생성 중 RFID 고정 (2026-11-02 / RFID-SNAPSHOT-001)

생성 버튼을 누른 시점의 리더4개를 생성/다운로드/최종 배치가 끝날 때까지 고정한다. 이때 인식 해제·새 블록·이동·일반 전체 비우기 신호는 공통 리더 진입점과 데이터 모델에서 모두 무시한다. 변경 이벤트/선택 revision을 올리지 않으므로 블록 표시나 생성 요청이 취소되지 않는다. 종료 후 이전 신호를 적용하지 않고, 그 뒤 새로 들어오는 신호부터 정상 반영한다. 명시적 취소·실패·시간 초과·비활성·홈은 잠금을 해제하며 홈만 강제로 리더를 초기화할 수 있다. 아래의 생성 중 블록 변경에 의한 취소 설명은 과거 동작이다.

## 외부 리소스와 생성 버튼 (2026-11-02 / BLOCK-ART-LOADING-001)

블록 표시 이미지는 이제 `StreamingAssets/BlockArtwork/blocks_en.json`, `blocks_th.json`과 `Images/en`, `Images/th`에서 언어별로 읽는다. 기존 `Assets/UI/Blocks`와 Sprite 카탈로그는 오류 복구용 기본값이다. 이미지/JSON은 시작·홈 복귀에서 다시 읽으며 다음 체험에 반영한다. [교체 방법과 현재 동작](BLOCK_RESOURCES.md)을 따른다.

배치가 완성돼도 자동 생성하지 않는다. 버튼을 눌러 생성하고, 우측 박스에는 배경 없이 꼬리가 투명해지는 원형 회전 링을 표시한다. 생성 중 입력을 차단하고 모든 종료 경로에서 로딩을 정리한다. 블록 변경은 생성/기존 이미지를 취소·비우며 새 조합에 대해서는 새 버튼을 기다린다. 아래의 자동 생성/임베디드 블록 경로 설명은 이전 이력이며 이 절이 우선한다.

## 블록 표시 언어 (2026-11-02 / THAI-BLOCKS-001)

영문 블록은 `Assets/UI/Blocks/EnglishBlocks`의 `BagBlock_Eng.png`, `RedBlock_Eng.png`처럼 `_Eng` 접미사를 사용한다. 태국어 블록은 `Assets/UI/Blocks/ThaiBlocks`의 `BagBlock_Thai.png`, `RedBlock_Thai.png`처럼 `_Thai` 접미사로 구분한다. 태국어 모드에서는 제공된 물건10종·색상6종의 태국어 Sprite를 두 선택 화면과 F9 테스트창의 목록/리더/드래그 이미지에 사용한다. 연령4종은 태국어 파일이 없어 기존 이미지를 공용으로 사용한다. 언어 전환은 타이틀에서만 가능하며 다음 화면 진입부터 해당 모드를 사용한다.

`RfidBlockArtwork`의 기존 `sprite`는 영문, `thaiSprite`는 태국어 참조다. 인식 ID·리더 점유·생성 프롬프트는 표시 이미지와 독립적이다. 영문/태국어6색 원 내부 RGB는 모두 같아 생성 팔레트를 변경하지 않았다. 리소스 재연결은 `RfidBlockAuthoring.ConfigureArtwork()`만 실행하면 씬을 수정하지 않는다. 아래 생성 모델/가이드 설명은 과거 단계가 섞인 이력이며 최신 활성 생성 설정은 [COMFYUI_SETUP](COMFYUI_SETUP.md)와 [STYLE-QUALITY-003](validation/comfy_style3/README.md)를 따른다.

현재 Comfy 생성 설정(2026-10-30): SDXL-Turbo512/4단계 단일 생성. 실제 두 색과 연령 디자인을 짧게 전달하며 가이드/ControlNet/추가 생성은 사용하지 않는다. 가장 큰 전경 영역을 골라480RGBA 정중앙에 배치한다. 전체 생성·다운로드·배치가 끝날 때까지 모든 선택패널 버튼/5초 유지 입력을 잠그고 취소/실패/홈에서 복구한다. 앱 실측 첫6.24초/반복2.65초(5070Ti Laptop),3060 예상5~12초는 실측 전 추정이다. [현재 설정](COMFYUI_SETUP.md)·[검증](validation/comfy_fast2/README.md)을 따른다.



## 현재 동작 (2026-10-30 / COMFY-RESTORE-001)

COMFY-FRAMING-001: 앱의 최종480×480 투명 이미지에서는 제품의 실제 가시 알파 경계를 중앙(240,240)에 맞춘다. 투명 여백 최소24px·가로세로 비율을 유지하고 필요할 때만 축소한다. 작은 이미지는 확대하지 않는다. 모델 시점은 옆모습으로 고정하지 않고 자연스러운 사선 시점도 사용한다. 서버 원본은 보존하고 다운로드 후 프레이밍을 적용한다.

네 리더에 그룹1개·물건1개·색2개가 완성되면 ComfyUI로 새 제품 일러스트를 생성한다. 가이드에 색만 입히던 로컬 색칠과 강한 형태 제어는 사용하지 않는다. 최신 첨부처럼 단순한 둥근 형태·파스텔 색면·흰 악센트·부드러운 명암을 가진 완성된2D그림을 기준으로 프롬프트를 구성하고, 텍스트→새 이미지→BiRefNet 배경 제거→실제 알파 결합→480×480 RGBA PNG 순으로 처리한다. 최신 후속에서는 모든10종에 실제 Assets/UI/Blocks 물건 그림의 윤곽을 약하게 참조한다(기본0.5/초기60%, 헤드폰/상의/모자0.6/초기65%). UI테두리와 글자는 제외하고 분위기/구성을 이어가되 표면·채색·명암은 새로 그린다. 예전 기하학 가이드 색칠은 사용하지 않는다. 생성물의 디자인·배경 분리 품질은 시드에 따라 달라져 실물 검수가 필요하다.

그룹 패널은 아동/학생/직장인/시니어 중 그룹 블록부터 인식한다. 그룹보다 먼저 들어온 물건/색은 표시·저장하지 않으며 자동으로 다시 채우지 않는다. 그룹이 정확히1개일 때 버튼을 누르거나, 새 물건/색 블록을 올리면 같은 기존 슬롯 이동 연출로 배치 패널에 진행한다. 그룹2개는 진행과 새 비그룹 입력을 막는다. 자동 진행은 배치/화면 연출과 이미지 생성 잠금이 끝날 때까지 대기하며 대기 중 그룹 제거/중복·홈/화면 해제는 요청을 취소한다. 이미 배치 화면에 들어간 후에는 그룹 제거 시 이전 화면으로 돌아가지 않는 기존 규칙을 유지한다. 어느 선택 패널에서든 완전한 조합이 들어오면 생성하며, 생성 중에는 계속 안내를 숨기고 **짧은 버튼·5초 홈을 포함한 모든 버튼 입력을 버린다.** 생성이 끝난 뒤 버튼을 놓고 새로 눌러야 버튼 진행된다. 실패/취소/시간 초과 후 완전한 조합에서 새 버튼 입력은 재시도한다. 블록 미완성일 때는 짧은 진행만 차단하고5초 홈은 유지한다. 생성 중 무입력 타이머는 Pause, 모든 종료 경로에서 Resume/Reset한다.

실제 하드웨어의 블록 변경·제거도 생성 중에는 RFID-SNAPSHOT-001에 따라 선택에 반영하지 않는다. 늦은 이미지 응답은 현재 화면·선택 revision·세션 토큰으로 거부한다. 공유 ComfyUI의 전역 interrupt는 호출하지 않는다.

## 현재 블록과 색상

| 종류 | 값 |
|---|---|
| Group | Child, Student, Working Adult, Senior |
| Product | Bags, Shoes, Hats, Sunglasses, Headphones, Tops, Bicycles, Tumblers, Wristwatches, Umbrellas |
| Color | Black, White, Blue, Red, Green, Yellow |

Phone Cases/Equipment는 실제 그림이 없으므로 계속 제외한다. 실제20개 블록 이미지/GUID·RFID 매핑을 보존한다. 네 물리 리더에는 종류 제한 없이 놓을 수 있지만, 화면에서는 용도별 칸(좌상 그룹, 우상 제품, 좌하·우하 색)에 표시한다. F9 창은 리더 기준 그대로다. 색은 먼저 놓인 순서로 칸을 채우고 그 칸 순서로 주색/보조색을 정한다. 미인식 칸은 빈 칸 이미지로 표시하며 그룹이 제거돼도 배치 패널에 남는다.

| 색상 원 | HEX | RGB |
|---|---|---|
| Black | #171619 | 23,22,25 |
| White | #FFFFFF | 255,255,255 |
| Blue | #5FB0E3 | 95,176,227 |
| Red | #EC6868 | 236,104,104 |
| Green | #B7DBA9 | 183,219,169 |
| Yellow | #F6EEA2 | 246,238,162 |

색상은 `Assets/UI/Blocks/EnglishBlocks/*Block_Eng.png`의 테두리/글자가 아닌 원 내부에서 추출한다. `python Tools/ComfyUI/sync_block_palette.py`(Pillow 필요)로 동기화하며 `--check`로 검증한다. BlockCatalog의 hex/RGB는 주색·보조색 요청 메타데이터와 저널에 전달한다. 현재 Klein 경로의 시각 프롬프트에는 코드·색 이름·부위를 함께 전달한다. AI의 명암·재질 때문에 결과물의 모든 픽셀이 지정 RGB와 같지는 않다. 보관용 BlockColoring 팔레트도 같은 값으로 맞췄다.

## 입력·설정·보관

- F9는 Player 포함 RFID 테스트창 표시/숨김이며 점유 상태는 유지한다. 생성/연출 중 마우스 조작은 공통 잠금을 따르고, 실제 센서도 생성 중 선택 고정을 따른다. F10 운영창과 F9는 동시에 열리지 않는다.
- 에디터 숫자2/3/4/5는 Group/Product/Color1/Color2 선택/제거다. showEditorDiagnostics 개발 패널의 Generate image / Retry는 같은 생성 진입점을 사용한다.
- `BlockCatalog.json`: 현재 블록 목록·제품/스타일 프롬프트·원에서 추출한 hex. `ComfyUISettings.json`: 서버 URL·실행 경로·요청/생성 제한시간·저널/생성물 보관 한도. `product_480.api.json`: 텍스트 생성과 투명 배경 워크플로.
- GameLifetimeScope는 기존 base.Configure를 보존하며 영속 GameManager에 서버 검사/Player 자동 실행과 보관 서비스를 하나씩 연결한다. Editor 자동 실행은 기존처럼 기본 false다. 기존 이미지 UI를 유지하고 서버 상태는 영문 로그로 확인한다.
- 생성물 보관은 기존 규칙대로 ComfyUI output/Thai02의 blocks_GUID_counter_.png만 대상, 기본1000개/512MiB와60초 쓰기 유예를 적용한다. 다른 파일·다른 서버 사용자의 작업은 삭제/중단하지 않는다.
- BlockData JSONL은 최대30개/각4MiB. 선택·리더번호·revision·언어와 generation_requested/queued/completed/cancelled/failed를 기록한다. requested에는 정확한 그래프·시드·제품·두 색 HEX/RGB 프롬프트가 포함된다. 기존 저널/보관본은 유지한다.

`TryDisplayResult(texture, revision)` 성공 시 컨트롤러/매니저로 소유권을 이전한다. 거부 시 호출자가 텍스처를 해제한다. Game→Result는 같은 생성 텍스처/언어/세션을 유지하고 홈은 선택·RFID·작업·자원을 정리한다. 로컬 색칠 코드/샘플과 과거 기하학 ControlGuides는 보관용이며 실제 물건 블록에서 추출한 새 참조만 사용한다.

최종 모델은 기설치 Animagine XL4.0 Opt, 내부1024/36steps/CFG5.5/Euler ancestral/normal이다. 모든10종에 기설치 Canny Control-LoRA rank256도 사용한다. 외곽선은 강제하지 않는다. 모델·패키지 다운로드/갱신은 하지 않았다.

현재 검증은 [COMFY-RESTORE-001](validation/comfy_restore/README.md), 설치는 [COMFYUI_SETUP](COMFYUI_SETUP.md), 버튼/언어·영상은 [BUTTON_INPUT](BUTTON_INPUT.md)·[IMAGE_UI](IMAGE_UI.md)를 따른다.
