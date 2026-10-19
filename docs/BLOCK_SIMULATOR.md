# 블록 인식 시험과 이미지 생성

Unity 2022.3.62f3의 `1_Game` 씬에 `BlockExperience` 오브젝트를 연결했다. Idle에서 언어를 선택해 들어오거나 Game 씬에서 직접 Play할 수 있다. 직접 시작하면 데이터 언어 코드는 en이다. 새 컴포넌트는 주입 속성을 사용하지 않고 시작 시 매니저·타이머를 한 번 찾으며 씬과 함께 해제된다.

## 에디터 사용

| 키 (상단·숫자패드) | 슬롯 | 목록 |
|---|---|---|
| 2 | Group | Child, Student, Working Adult, Senior |
| 3 | Product | Bags, Shoes, Hats, Sunglasses, Headphones, Tops, Bicycles, Tumblers, Wristwatches, Phone Cases, Umbrellas, Equipment |
| 4 | Color1 | Black, White, Blue, Red, Green, Yellow |
| 5 | Color2 | 같은 여섯 색상 |

1. Game 창에 포커스를 둔 뒤 키를 누르고 임시 목록에서 클릭한다.
2. 선택값이 기존 `SelectedOptionsGroup/Option_*/OptionText`에 표시된다.
3. 인식된 슬롯의 키를 다시 누르면 인식이 해제되고 텍스트가 `—`로 바뀐다. 이후 한 번 더 누르면 목록이 열린다. 목록만 열린 상태에서 같은 키를 다시 누르면 닫힌다.
4. 네 슬롯을 모두 채우면 임시 UI의 **Generate image**를 누른다. **Cancel generation**으로 취소할 수 있다. 자동 연속 생성·선택 도중 latent 이어 그리기는 구현하지 않았다.
5. 생성 전·생성 중에는 `RecommendedProductImageBox/TextGroup`을 표시하고 `ResultImage`는 비활성화한다. 다운로드한 480×480 이미지를 `ResultImage`에 먼저 할당한 다음 활성화하고 TextGroup을 숨긴다. 선택 변경·재생성·취소·실패·컴포넌트 종료 시에는 안내 상태를 유지/복구하고 이전 이미지 자원을 해제한다.

임시 키·선택창·생성 버튼은 `UNITY_EDITOR`에서만 표시/처리한다. `BlockExperienceController`의 Enable Editor Keyboard로 끌 수 있다. 에디터 마우스 선택을 위해 커서를 표시하며 컴포넌트가 비활성화되면 이전 커서 상태로 되돌린다. 실제 RFID/배포용 생성 트리거는 추후 어댑터가 같은 컴포넌트의 `RecognizeBlock(slot,id)`, `RemoveBlock(slot)`, `GenerateImage()`를 Unity 메인 스레드에서 호출하도록 연결한다.

## 선택값·프롬프트

정의는 `Assets/StreamingAssets/BlockGeneration/BlockCatalog.json`에 있다. 사용자 표시는 영문 이름이며 현재 태국어 번역은 추가하지 않았다. 두 색상은 물품별 본체·트림/렌즈 등의 부위로 매핑한다. 같은 색 두 개도 허용한다. Equipment는 화면에는 **Equipment**만 표시하고 요청마다 robots/keyboards/laptops 중 하나를 균등 무작위 선택한다. 선택된 내부 분기와 실제 프롬프트는 기록되므로 추후 재현할 수 있다.

현재 범위는 **정지 이미지 생성**이다. 영상 인코딩/영상 생성 모델, 중간 프리뷰 스트리밍, 실시간 동일 latent 전환은 이 변경에 포함하지 않는다. 기존 SDXL 워크플로의 화풍·색상·인물 배제 한계는 [ComfyUI 구성](COMFYUI_SETUP.md)과 같다. 제품 목록 확장은 생성 조건 지원이며 모든 물품의 이미지 품질 승인과는 별개다.

## ComfyUI 설정·실패 처리

`Assets/StreamingAssets/BlockGeneration/ComfyUISettings.json`에서 별도 서버 주소와 시간 제한을 설정한다. 템플릿 로그 서버용 Settings.apiUrl을 사용하지 않는다. 기본 localhost:8188, 개별 요청 15초, 전체 생성 180초다. 전체 시간 제한은 timeScale과 무관한 실제 시간을 사용한다.

`product_480.api.json`은 서버에 제출하는 기본 그래프다. 기존 SDXL Base, 내부768→출력480, 16 steps 설정을 사용한다. 실행마다 별도 시드·요청 ID와 제품별 프롬프트를 주입한다. 생성 전 선택값/정확한 그래프를 먼저 디스크에 저장하므로 서버 오프라인이어도 요청한 조건을 기록한다. 기록 실패 시 서버에 제출하지 않는다.

- 생성 중 무입력 타이머 Pause, 완료·오류·취소·컴포넌트 비활성화에서 Resume한다.
- 서버 큐가 바쁘면 요청을 제출하지 않고 재시도를 안내한다. 큐 검사와 제출은 서버에서 원자적 예약이 아니므로 다른 클라이언트가 동시에 제출하면 대기할 수 있다.
- 네트워크 연결 실패·서버 오류·시간 초과를 표시하고 Generate로 다시 요청할 수 있다. POST 자동 재시도는 중복 생성 위험 때문에 하지 않는다. history GET의 연결 오류/5xx만 최대 2회 지연 재시도한다.
- 선택 변경·씬 종료·세션 종료는 로컬 수신을 취소하고 늦은 결과를 무시한다. 알려진 자신의 pending prompt만 삭제한다. **공용 `/interrupt`를 호출하지 않으므로 이미 서버에서 실행 중인 생성은 계속될 수 있다.** 이 경우 큐가 빌 때까지 다음 생성을 기다려야 한다. POST 응답을 받기 전에 연결이 끊기면 prompt_id를 몰라 서버 작업을 정리하지 못할 수 있으며 기록에 실패/취소로 남는다.
- 결과 Sprite/Texture는 교체·비활성화 때 해제한다. 영속 루트에 씬 UI를 저장하지 않는다.

## 데이터 기록 계약

‘빅데이터’는 파일 확장자 하나가 아니므로 분석용 이벤트 데이터셋의 시작점으로 **JSON Lines (.jsonl)**를 사용한다. 원격 데이터 플랫폼/통계 대시보드나 대규모 데이터가 이미 구축되었다는 뜻은 아니다.

위치: `Application.persistentDataPath/BlockData/blocks_*.jsonl`. 현재 식별자 기준 Windows에서는 `%USERPROFILE%/AppData/LocalLow/MyCompany/Unity-Startup-Template/BlockData`다. 씬/에디터 재실행 후에도 유지하며 저장소에 커밋하지 않는다.

공통 필드: `schema_version`, 무작위 `event_id`·`session_id`, 세션 내 `sequence`, ISO8601 UTC `timestamp_utc`, `event_type`, `data`. 장비 ID·실명·RFID UID는 수집하지 않는다.

| 이벤트 | 주요 data |
|---|---|
| session_started / session_ended | 언어·카탈로그 버전 / 종료 |
| block_recognized / block_removed | 슬롯, block_id, revision, 네 슬롯 스냅샷 |
| generation_requested | request_id, 언어, revision, 네 선택값과 resolvedProduct, 양/음 프롬프트, 색상 부위 조각, seed, 모델·샘플링을 포함한 전체 workflow |
| generation_queued | request_id, ComfyUI prompt_id |
| generation_completed | request_id, prompt_id, 서버 이미지 파일·subfolder, 최종 크기 |
| generation_cancelled / generation_failed | 요청 식별자, 서버 작업 지속 가능성 / 오류 종류·메시지 |

해당 request_id의 requested→queued→completed/failed/cancelled를 연결해 분석한다. 선택 중간 단계는 결측값(null)을 보존한다. Equipment의 관람객 선택 `productId=equipment`와 실제 이미지 대상 `resolvedProduct`를 구분한다. 생성 완료는 픽셀 파일 수신 성공이며 화풍·색상 품질 자동 합격을 뜻하지 않는다.

기본 파일당 4MiB·최대30개이며 한도를 넘으면 이 디렉터리의 가장 오래된 `blocks_*.jsonl`부터 삭제한다. 영구 연구 데이터로 사용할 때는 삭제 전에 별도 수집 서버/백업으로 내보내야 한다. 쓰기는 메인 스레드 밖의 단일 직렬 작업으로 처리하고 대기128개를 넘으면 오류를 표시한다. PNG는 ComfyUI output/Thai02에 저장되며 아래 이미지 보관 한도를 적용한다. 기록은 일반 JSONL append로, 강제 전원 차단 때 마지막 줄이 불완전할 수 있으므로 수집 시 완전한 줄만 파싱한다. 프로세스 강제 종료의 session_ended는 보장하지 않는다.

## 생성 이미지 자동 정리

`ComfyUISettings.json`의 설정:

| 키 | 기본값 | 역할 |
|---|---|---|
| maxGeneratedImages | 1000 | 보관 개수 상한 |
| maxGeneratedImageMegabytes | 512 | 보관 용량 상한(MiB) |
| imageCleanupIntervalSeconds | 30 | 실행 중 정리 간격(실제 시간) |
| imageWriteGraceSeconds | 60 | 수정 후 이 시간 이내의 파일은 보호 |
| comfyOutputDirectory | 빈 문자열 | ComfyUI output의 절대 경로. 아래 자동 탐색 지원 |

두 한도를 모두 만족하도록 **수정 시간이 오래된 파일부터 삭제**한다. 사용 중인 생성 요청의 결과 파일은 보호한다. 대상은 output 바로 아래 `Thai02` 폴더의 `blocks_<32자리 소문자 요청ID>_<숫자>_.png`뿐이다. 하위 폴더·다른 이름/확장자·모델·JSONL·기존 prototype `product_*.png`는 삭제하지 않는다. 이 경로/파일명은 이 프로젝트가 예약한 이름이다. 해당 이미지를 영구 보관하려면 먼저 다른 폴더로 내보낸다. JSONL의 이미지 참조는 보존 기간이 끝나면 실제 PNG가 없어질 수 있다.

정리 서비스는 GameLifetimeScope의 build callback에서 **영속 GameManager에 한 번 생성**한다. Game 화면을 나가도 유지되므로 Idle에서 취소 후 늦게 생성된 PNG도 다음 정리 주기에 처리한다. 앱 시작 후·주기마다·생성 제출 전에 검사하며 파일 작업은 작업 스레드에서 직렬 실행한다. 최근 파일 유예/실행 중 요청 보호 때문에 한도를 잠시 넘을 수 있다. 이때 한도를 복구할 수 없으면 추가 생성을 거부하고 재시도 안내를 표시한다. 이는 파일시스템의 강제 디스크 할당량 기능은 아니다.

현재 PC의 ComfyUI 실행 인자 `--output-directory`를 localhost의 `/system_stats`에서 읽어 output을 자동 탐색한다. **자동 탐색은 loopback 서버에만 허용**한다. 실행 인자에 경로가 없거나 다른 PC의 서버라면 `comfyOutputDirectory`에 실제 접근 가능한 output 폴더(원격이면 공유 폴더)를 지정해야 한다. 지정 경로가 없거나 상대 경로/드라이브 루트/심볼릭 링크·junction 경로면 삭제를 거부한다. 폴더를 추측하거나 임의 검색하지 않는다. 설정·서버 output 경로 변경 후에는 앱을 재시작한다.

삭제 실패·권한 문제·보호 파일 때문에 한도를 복구하지 못하면 경고를 남기고 새 생성을 막는다. 다른 오래된 파일을 정리해서 한도를 복구할 수 있으면 정상 진행한다. 앱 종료 중에는 정리 서비스도 멈추므로, 종료 뒤 ComfyUI에서 끝난 작업의 파일은 **다음 앱 실행 때** 정리된다. 앱이 꺼진 동안 ComfyUI를 별도로 무제한 실행하는 경우까지 관리하는 서버 상주 서비스는 아니다.

## 검증 범위

BLOCK-001의 실제 수행 결과와 미검증 항목은 [HANDOFF](agents/HANDOFF.md)의 최신 BLOCK-001 기록 및 [검증 요약](../Tools/ComfyUI/validation/block_runtime.json)을 따른다. Windows Player·실제 RFID·장시간/Profiler·RTX3060 성능·전체 제품의 시각 품질은 별도 검수 대상이다.
