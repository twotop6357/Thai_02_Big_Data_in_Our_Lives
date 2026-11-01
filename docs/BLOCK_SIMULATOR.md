# 블록 시뮬레이터와 ComfyUI 이미지 생성

현재 Comfy 생성 설정(2026-10-30): SDXL-Turbo512/4단계 단일 생성. 실제 두 색과 연령 디자인을 짧게 전달하며 가이드/ControlNet/추가 생성은 사용하지 않는다. 가장 큰 전경 영역을 골라480RGBA 정중앙에 배치한다. 전체 생성·다운로드·배치가 끝날 때까지 모든 선택패널 버튼/5초 유지 입력을 잠그고 취소/실패/홈에서 복구한다. 앱 실측 첫6.24초/반복2.65초(5070Ti Laptop),3060 예상5~12초는 실측 전 추정이다. [현재 설정](COMFYUI_SETUP.md)·[검증](validation/comfy_fast2/README.md)을 따른다.



## 현재 동작 (2026-10-30 / COMFY-RESTORE-001)

COMFY-FRAMING-001: 앱의 최종480×480 투명 이미지에서는 제품의 실제 가시 알파 경계를 중앙(240,240)에 맞춘다. 투명 여백 최소24px·가로세로 비율을 유지하고 필요할 때만 축소한다. 작은 이미지는 확대하지 않는다. 모델 시점은 옆모습으로 고정하지 않고 자연스러운 사선 시점도 사용한다. 서버 원본은 보존하고 다운로드 후 프레이밍을 적용한다.

네 리더에 그룹1개·물건1개·색2개가 완성되면 ComfyUI로 새 제품 일러스트를 생성한다. 가이드에 색만 입히던 로컬 색칠과 강한 형태 제어는 사용하지 않는다. 최신 첨부처럼 단순한 둥근 형태·파스텔 색면·흰 악센트·부드러운 명암을 가진 완성된2D그림을 기준으로 프롬프트를 구성하고, 텍스트→새 이미지→BiRefNet 배경 제거→실제 알파 결합→480×480 RGBA PNG 순으로 처리한다. 최신 후속에서는 모든10종에 실제 Assets/UI/Blocks 물건 그림의 윤곽을 약하게 참조한다(기본0.5/초기60%, 헤드폰/상의/모자0.6/초기65%). UI테두리와 글자는 제외하고 분위기/구성을 이어가되 표면·채색·명암은 새로 그린다. 예전 기하학 가이드 색칠은 사용하지 않는다. 생성물의 디자인·배경 분리 품질은 시드에 따라 달라져 실물 검수가 필요하다.

그룹 패널은 그룹1개 인식 후 배치 패널로 진행한다. 어느 선택 패널에서든 완전한 조합이 들어오면 생성하며, 생성 중에는 계속 안내를 숨기고 **짧은 버튼·5초 홈을 포함한 모든 버튼 입력을 버린다.** 생성이 끝난 뒤 버튼을 놓고 새로 눌러야 진행된다. 실패/취소/시간 초과 후 완전한 조합에서 새 버튼 입력은 재시도한다. 블록 미완성일 때는 짧은 진행만 차단하고5초 홈은 유지한다. 생성 중 무입력 타이머는 Pause, 모든 종료 경로에서 Resume/Reset한다.

실제 하드웨어의 블록 변경·제거는 생성 중에도 받는다. 기존 작업을 취소하고 바뀐 조합이 완전하면 새 요청을 시작한다. 공유 ComfyUI에서 이미 실행 중인 작업은 계속될 수 있으며 전역 interrupt는 호출하지 않는다. 서버가 바쁘면 실패 후 재시도한다. 늦은 응답은 현재 화면·선택 revision·세션 토큰으로 거부한다.

## 현재 블록과 색상

| 종류 | 값 |
|---|---|
| Group | Child, Student, Working Adult, Senior |
| Product | Bags, Shoes, Hats, Sunglasses, Headphones, Tops, Bicycles, Tumblers, Wristwatches, Umbrellas |
| Color | Black, White, Blue, Red, Green, Yellow |

Phone Cases/Equipment는 실제 그림이 없으므로 계속 제외한다. 실제20개 블록 이미지/GUID·RFID 매핑을 보존한다. 네 물리칸은 종류 제한 없이 자유 배치하고 색은 리더 번호 오름차순으로 주색/보조색을 정한다. 미인식 슬롯은 빈칸으로 표시하며 그룹이 제거돼도 배치 패널에 남는다.

| 색상 원 | HEX | RGB |
|---|---|---|
| Black | #171619 | 23,22,25 |
| White | #FFFFFF | 255,255,255 |
| Blue | #5FB0E3 | 95,176,227 |
| Red | #EC6868 | 236,104,104 |
| Green | #B7DBA9 | 183,219,169 |
| Yellow | #F6EEA2 | 246,238,162 |

색상은 `Assets/UI/Blocks/*Block.png`의 테두리/글자가 아닌 원 내부에서 추출한다. `python Tools/ComfyUI/sync_block_palette.py`(Pillow 필요)로 동기화하며 `--check`로 검증한다. BlockCatalog의 hex/RGB는 주색·보조색 요청 메타데이터와 저널에 전달하고, 모델의 시각 프롬프트에는 코드에 대응하는 색 설명을 사용한다. 숫자 코드가 도표/문자를 유도하지 않도록 CLIP 본문에서는 제외한다. AI의 명암·재질 때문에 결과물의 모든 픽셀이 지정 RGB와 같지는 않다. 보관용 BlockColoring 팔레트도 같은 값으로 맞췄다.

## 입력·설정·보관

- F9는 Player 포함 RFID 테스트창 표시/숨김이며 점유 상태는 유지한다. 생성/연출 중 마우스 테스트 조작도 공통 잠금을 따른다. 실제 센서 API는 잠금과 관계없이 상태를 갱신할 수 있다.
- 에디터 숫자2/3/4/5는 Group/Product/Color1/Color2 선택/제거다. showEditorDiagnostics 개발 패널의 Generate image / Retry는 같은 생성 진입점을 사용한다.
- `BlockCatalog.json`: 현재 블록 목록·제품/스타일 프롬프트·원에서 추출한 hex. `ComfyUISettings.json`: 서버 URL·실행 경로·요청/생성 제한시간·저널/생성물 보관 한도. `product_480.api.json`: 텍스트 생성과 투명 배경 워크플로.
- GameLifetimeScope는 기존 base.Configure를 보존하며 영속 GameManager에 서버 검사/Player 자동 실행과 보관 서비스를 하나씩 연결한다. Editor 자동 실행은 기존처럼 기본 false다. 기존 이미지 UI를 유지하고 서버 상태는 영문 로그로 확인한다.
- 생성물 보관은 기존 규칙대로 ComfyUI output/Thai02의 blocks_GUID_counter_.png만 대상, 기본1000개/512MiB와60초 쓰기 유예를 적용한다. 다른 파일·다른 서버 사용자의 작업은 삭제/중단하지 않는다.
- BlockData JSONL은 최대30개/각4MiB. 선택·리더번호·revision·언어와 generation_requested/queued/completed/cancelled/failed를 기록한다. requested에는 정확한 그래프·시드·제품·두 색 HEX/RGB 프롬프트가 포함된다. 기존 저널/보관본은 유지한다.

`TryDisplayResult(texture, revision)` 성공 시 컨트롤러/매니저로 소유권을 이전한다. 거부 시 호출자가 텍스처를 해제한다. Game→Result는 같은 생성 텍스처/언어/세션을 유지하고 홈은 선택·RFID·작업·자원을 정리한다. 로컬 색칠 코드/샘플과 과거 기하학 ControlGuides는 보관용이며 실제 물건 블록에서 추출한 새 참조만 사용한다.

최종 모델은 기설치 Animagine XL4.0 Opt, 내부1024/36steps/CFG5.5/Euler ancestral/normal이다. 모든10종에 기설치 Canny Control-LoRA rank256도 사용한다. 외곽선은 강제하지 않는다. 모델·패키지 다운로드/갱신은 하지 않았다.

현재 검증은 [COMFY-RESTORE-001](validation/comfy_restore/README.md), 설치는 [COMFYUI_SETUP](COMFYUI_SETUP.md), 버튼/언어·영상은 [BUTTON_INPUT](BUTTON_INPUT.md)·[IMAGE_UI](IMAGE_UI.md)를 따른다.
