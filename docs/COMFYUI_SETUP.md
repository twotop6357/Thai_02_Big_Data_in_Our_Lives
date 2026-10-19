# ComfyUI 물품 이미지 생성 구성

## 기본 시험 구성

**Stable Diffusion XL Base 1.0, Refiner 없이 단독 사용**을 기본으로 선정했다. 기존 설치 모델을 사용하며 별도 LoRA·ControlNet·커스텀 노드는 필요 없다. 화풍 목표는 **서브컬처 모바일게임의 인벤토리 아이템 일러스트**다. 가는 선, 부드러운 셀 명암, 색상 그라데이션, 재질 하이라이트를 고정 프롬프트로 유도하고 굵은 검정 외곽선·평면 벡터 표현은 억제한다. 현재 구성은 실제 실행 가능한 시험용이며, 모든 선택에서 일관된 화풍·물품 단독 구도를 확보한 운영 승인본은 아니다.

처음에는 가벼운 DreamShaper 8 / SD 1.5를 검토하고 실제 생성했다. 현재 PC에서 약 2~3초로 빨랐으나 인물 등장·선글라스 형태·화풍 편차가 커 기본 후보에서 제외했다. 비교한 SDXL은 물품 표현이 더 적합하여 기본 시험값을 변경했다. 이것은 이번 소수 샘플의 관찰이며 모델 전체 성능 순위를 의미하지 않는다. 시험용으로 내려받은 DreamShaper_8_pruned.safetensors는 ComfyUI 모델 폴더에 남아 있고 최종 워크플로는 사용하지 않는다.

| 항목 | 기본값 |
|---|---|
| 목표 설치 GPU | RTX 3060 12GB |
| 최종 파일 | 480 × 480 PNG, RGB / 불투명 배경 |
| 내부 생성 | 768 × 768, batch 1 |
| 모델 | sd_xl_base_1.0.safetensors |
| 샘플러 / 스케줄러 | dpmpp_2m / karras |
| steps / CFG / denoise | 16 / 6.0 / 1.0 |
| 축소 | 기본 ImageScale, area, 480 × 480, crop disabled |
| Refiner·Hires fix·후처리 AI | 사용 안 함 |
| 목표 시간 | 모델 준비 후 한 요청의 최종 이미지 수신까지 약 20초 이내 |

480에서 직접 생성하는 것보다 물품 형태를 확보하기 위해 내부 768을 사용하고 축소한다. SDXL의 대표 학습 해상도인 1024보다 낮춘 속도/품질 절충안이다. 3060에서 20초를 넘으면 먼저 fast(12 steps)를 비교한다. 정확히 20초가 되도록 지연을 추가하지 않는다.

**현재 측정 GPU는 RTX 5070 Ti Laptop 12GB이며 RTX 3060이 아니다.** ComfyUI 0.37.0 / PyTorch 2.12.1+cu130에서 측정했다. 실제 최종 설정의 결과는 [측정 JSON](../Tools/ComfyUI/validation/current_pc.json)에 기록한다. 여기의 시간은 ComfyUI 요청→완료 PNG 다운로드까지이며 Unity 화면 반영·동시 GPU 사용·실시간 프리뷰의 추가 비용은 포함한 검증이 아니다. 목표 3060의 20초 충족은 아직 미검증이다.

## 이번 실제 샘플 확인

서브컬처 모바일게임용 채색 방향으로 변경한 설정에서 워밍업 1회 후 4회 생성했다. 현재 RTX 5070 Ti Laptop에서 요청부터 PNG 수신까지 **6.533~6.967초, 중앙값 6.785초**, 워밍업 7.193초였다. RTX 3060의 측정값은 아니다.

| 선택 | 실제 관찰 |
|---|---|
| 학생 / 파랑·흰색 가방 | 일러스트 방향의 단일 가방. 갈색 장식과 크림색이 추가되어 색상 완전 일치는 아님 |
| 어린이 / 빨강·노랑 선글라스 | 단일 선글라스. 렌즈에 어두운 반사가 섞임 |
| 직장인 / 검정·파랑 가방 | 단일 가방이지만 3D 제품 렌더에 가까워 화풍 기준 미달 |
| 노인 / 갈색·초록 선글라스 | 인물이 등장하여 물품 단독 기준 미달 |

네 샘플 전체와 개별 판정은 [validation](../Tools/ComfyUI/validation/current_pc.json)에 보존했다. 속도 통과를 이미지 품질 통과로 간주하지 않는다. 추가 비교에서 2D 가중치를 높여도 복수 물품·글자·인물이 발생했고, img2img 단순 참조 방식은 원래 색을 강하게 유지해 두 색상 입력용 기본 구성으로 채택하지 않았다. 참조 시험 파일은 로컬 ignored 경로에만 남긴다.

현재 프롬프트만으로 요청한 화풍을 항상 보장할 수 없다. 운영 품질을 확보하려면 승인된 아이템 화풍을 기준으로 SDXL 호환 스타일 LoRA/체크포인트와 물품별 구조 제약을 함께 비교 검증해야 한다. 이번 작업에서 이를 검증 완료하거나 특정 서브컬처 전용 모델을 설치했다고 보고하지 않는다.

## 바로 사용

1. ComfyUI의 워크플로 목록에서 `Thai_02_product_480_mobile_game.json`을 연다. 목록에 바로 안 보이면 새로고침한다. 또는 [product_480.ui.json](../Tools/ComfyUI/workflows/product_480.ui.json)을 화면에 끌어다 놓는다.
2. Load Checkpoint가 `sd_xl_base_1.0.safetensors`인지 확인한다.
3. 왼쪽 INPUT 1~4는 그룹·물품·주 색상 부위·보조 색상 부위를 표현하는 **영문 프롬프트 조각**이다. 예: 학생 선택은 `practical campus design`, 파랑 가방 본체는 `(blue fabric body:1.15)`이다.
4. Run을 실행한다. 마지막 SaveImage에서 480×480 결과를 확인한다. 서버의 output/Thai02 아래에 저장된다.

UI의 네 문자열 노드는 선택 목록 위젯이 아니다. 한국어/태국어 원문을 자동 해석하는 노드도 아니다. Unity에서 전달할 선택 코드는 아래 매핑으로 프롬프트 조각으로 변환한다. 수동으로 물품을 바꾸면 색상 부위도 함께 바꿔야 한다. 아래 생성 도구를 쓰면 이를 자동으로 맞춘 UI/API JSON을 얻을 수 있다.

## 네 입력값 매핑

| 입력 | 지원 코드 / 적용 |
|---|---|
| 그룹 | student / child / working_adult / senior |
| 물품 | bag / sunglasses / cap / sneakers / watch |
| 색상 1·2 | red, orange, yellow, green, blue, purple, pink, white, black, gray, brown, cyan |

그룹은 실제 나이를 판정하는 값이 아닌 전시의 디자인 테마 선택이다. 학생과 직장인은 연령 구간과 같지 않으므로 임의의 나이 범위를 만들지 않았다. 그룹에 따라 캠퍼스용·장난스러운 둥근 형태·절제된 일상 디자인·가볍고 단순한 디자인 등의 프롬프트 조각을 쓴다. 인물 없이 물품만 생성하도록 유도하지만 인물 배제는 보장하지 않는다. UI 표시 언어가 Thai/English여도 모델 프롬프트는 동일한 영문 매핑을 사용한다.

| 물품 | 색상 1 | 색상 2 |
|---|---|---|
| bag | 백팩 본체 | 끈·지퍼 트림 |
| sunglasses | 프레임 | 착색 렌즈 |
| cap | 모자 크라운 | 챙 |
| sneakers | 신발 갑피 | 밑창·끈 |
| watch | 시곗줄 | 케이스·다이얼 |

`bag`는 현재 백팩, `sneakers`는 한 짝을 뜻한다. 다른 가방 종류나 신발 한 켤레가 필요하면 presets에서 제품 정의를 바꾼다. 색상은 텍스트 의미를 유도하는 것이며 RGB/HEX 일치나 부위별 정확한 색 분리를 보장하지 않는다. 착색 렌즈는 명암·반사 때문에 지정색과 다르게 보일 수 있다.

## 선택값으로 워크플로 만들기

저장소 루트에서 Python 3.11 이상으로 실행한다. 외부 Python 패키지는 필요 없다.

```powershell
python Tools/ComfyUI/build_workflow.py --group child --product sunglasses --color1 red --color2 yellow
```

`Tools/ComfyUI/generated/`에 UI JSON, API JSON, 입력값 JSON을 생성한다. 기존 파일 덮어쓰기를 피하려면 `--output-dir`로 새 출력 폴더를 지정한다. `--seed`로 시드를 고정한다. 동일 시드는 조건 비교를 돕지만 조건 변경 시 동일 구도·형태를 보장하지 않는다.

프로필:

- `--profile balanced`: 16 steps / CFG 6.0. 기본 권장 출발점.
- `--profile fast`: 12 steps / CFG 5.5. 목표 PC에서 20초를 넘으면 품질과 함께 비교.
- `--profile quality`: 20 steps / CFG 6.0. 여유가 있을 때 비교하며 20초 목표 충족을 보장하지 않음.

## Unity API 연결 계약

[product_480.api.json](../Tools/ComfyUI/workflows/product_480.api.json)은 ComfyUI `/prompt`의 `prompt` 값으로 넣는 실행 그래프다. UI 그래프와 서로 다른 파일이다.

- 입력 조각: node 10 `value`=그룹 디자인, 11=물품, 12=색상 1 부위, 13=색상 2 부위.
- 실행마다 node 5 `seed`를 바꾸면 새 이미지가 생성된다. 동일 옵션·시드를 재요청하면 ComfyUI 캐시가 적용될 수 있다.
- node 5에 steps/CFG/sampler/scheduler, node 4에 내부 해상도, node 8에 최종 출력 크기가 있다.
- WebSocket 연결의 client_id와 `/prompt` 요청의 client_id를 맞추고 prompt_id로 현재 요청을 구분한다. 초기화 상태·무입력 타이머·복귀 취소는 기존 GameManager 세션 수명에 연결한다.
- API 주소는 템플릿 운영 로그용 Settings.apiUrl과 분리한다. AI-001은 워크플로/도구 구성까지만 수행했다. 후속 BLOCK-001에서 Unity 블록 선택→실제 생성 요청→최종 이미지 표시·JSONL 기록을 연결했다. 현재 런타임 목록·사용법은 [BLOCK_SIMULATOR](BLOCK_SIMULATOR.md)를 따른다. 중간 미리보기 수신은 아직 구현하지 않았다.

```json
{
  "client_id": "a-unique-session-id",
  "prompt": "실제로는 API JSON 객체 전체를 여기에 삽입"
}
```

ComfyUI가 제공하는 latent preview를 켜면 생성 중 이미지를 받을 수 있다. 선택지가 바뀔 때 현재 latent를 이어서 바꾸는 기능은 이 워크플로에 없다. 현재 구성은 선택값 한 묶음마다 새 생성을 수행한다. 프리뷰가 필요하면 ComfyUI의 Preview Method를 Latent2RGB로 시작하고 실제 생성 중 이미지 수신·부하를 별도 검증한다.

## 모델 설치·검증

현재 PC의 Comfy Desktop 공유 models/checkpoints에 SDXL Base가 이미 있었고, 공식 파일 SHA-256과 일치함을 확인했다. 다른 PC에는 `model.json`의 고정 revision·파일명·SHA-256을 사용한다.

```powershell
python Tools/ComfyUI/download_model.py --models-dir 'C:\ComfyUI\models\checkpoints'
```

실제 checkpoints 경로로 바꾼다. 모델은 약 6.94GB이며 저장소/StreamingAssets에 넣지 않는다. 다운로드는 `.partial` 파일로 받은 뒤 해시를 확인하고 최종 이름으로 이동한다. 기존 파일의 해시가 다르면 덮어쓰지 않는다. 실패한 partial은 자동 삭제하지 않으므로 확인 후 수동 정리하고 재시도한다. 기존 파일은 해시만 확인한다. 모델 라이선스는 CreativeML Open RAIL++-M이며 모델 배포 시 라이선스·고지를 보존한다.

## 3060에서 시간 검수

Unity 전시 화면까지 켠 실제 설치 환경에서 빈 큐·batch 1·동일 설정으로 측정한다. 시작 시 한 번 워밍업하고 모델을 메모리에 유지한다. CPU offload·다른 GPU 프로그램·VRAM 부족으로 지연이 커질 수 있다. 12GB는 모델을 실행할 수 있는 메모리 목표이지 속도 보증이 아니다.

```powershell
python Tools/ComfyUI/benchmark.py --warmup 1 --runs 20 --output-dir Tools/ComfyUI/results/rtx3060
```

도구는 시드를 매번 바꾸어 샘플러 캐시로 시간이 잘못 측정되는 일을 피한다. 초기 워밍업과 실제 측정을 분리하고 각 PNG가 480×480인지 검사한다. 서버 큐가 비어 있지 않으면 새 작업을 보내지 않는다. 시간 초과 시 다른 작업을 중단하는 전역 interrupt를 호출하지 않으며, 이미 실행 중인 자신의 작업을 ComfyUI에서 확인해야 한다.

median/p95/max를 확인하고, 별도로 품질을 검수한다. `target_gpu_acceptance_verified`는 실제 RTX 3060에서 20회 이상 측정하고 모든 측정이 20초 이내일 때만 true가 된다. 현재 PC의 기록은 이 값이 false다. 선택을 계속 바꾸는 상황·장시간 연속 운영·중간 프리뷰 갱신까지 포함한 검수는 별도다.

## 품질의 범위

모델에 프롬프트만 주는 구성이라 씨드에 따라 인물 등장, 복수 물품, 로고 같은 무늬, 배경 변화, 사진에 가까운 표현이 나올 수 있다. 그룹별 테마와 두 색상이 모든 결과에서 정확하게 적용된다는 보장은 없다. 선글라스/가방은 실제 샘플을 검수했고 모자·신발·시계 및 전체 색 조합의 이미지 품질은 미검증이다. 출력 픽셀의 정확한 색과 단일 실루엣을 보장해야 한다면 물품별 템플릿·마스크/ControlNet 등 추가 제약이 필요하며 별도 성능 예산으로 검증한다.

## 근거

- [SDXL Base 공식 모델 카드](https://huggingface.co/stabilityai/stable-diffusion-xl-base-1.0): Base 단독 사용과 모델 제한·라이선스.
- [ComfyUI 기본 API 예제](https://github.com/Comfy-Org/ComfyUI/blob/master/script_examples/basic_api_example.py): 실행 그래프 및 /prompt.
- [ComfyUI WebSocket 예제](https://github.com/Comfy-Org/ComfyUI/blob/master/script_examples/websockets_api_example.py): 생성 상태·프리뷰·완료 이미지 수신.
- [ComfyUI latent preview 구현](https://github.com/Comfy-Org/ComfyUI/blob/master/latent_preview.py): 실제 중간 이미지 디코딩.
- [DreamShaper 8 모델 카드](https://huggingface.co/Lykon/dreamshaper-8): 최초 비교 후보의 출처. 현재 기본 모델은 SDXL이다.
