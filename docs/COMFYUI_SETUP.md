# 현재: 레퍼런스 기반 귀여운 2D 일러스트 / STYLE-QUALITY-003 (2026-11-01)

2026-11-02 COMFY-PORTABLE-001: 사용자가 만든 `C:/Users/김성훈/Desktop/Thai_02`에 약20.3GB 독립 ComfyRuntime·현재4모델·NVIDIA/VC++ 설치 파일·Run_Thai02.cmd를 동봉했다. 새PC에서는 전체복사 → Installers설치/재부팅 → Run_Thai02.cmd로 현재경로를 맞춰 실행한다. Python/Comfy Desktop/CUDA Toolkit 별도 설치는 필요 없다. 기존 EXE/DLL·일반/RFID설정은 보존하며 배포본 Comfy설정만 백업 후 보정한다. [검증/제한](validation/comfy_portable/README.md), 현장안내는 배포폴더 README_먼저읽기.txt를 따른다. 아래 설치명령은 개발/수동설치 절차다.

현재 기본값은 **FLUX.2 Klein 4B FP8 · 640×640 · 4단계 · Euler/Flux2Scheduler · 단일 생성**이다. 이전 범용 SDXL-Turbo의 제품 사진/거친 선 혼입을 줄이기 위해 사용자가 제공한 작은 신발 일러스트의 그림체를 직접 참고한다. [실제 비교/검수](validation/comfy_style3/README.md)를 기준으로 한다. 모델을 교체했으므로 과거 Turbo의3060 5~12초 추정은 새 경로의 실측 근거가 아니다.

## 생성과 색·연령

- 2026-11-02 ALPHA-OPAQUE-001: 흰배경에서 흰 물체가 함께 지워지는 문제를 피하도록 제거용 배경만 중간회색으로 바꿨다. 최종은 계속480RGBA 투명배경이다. BiRefNet에 원본RGB의 흰색/유색/검은 재질 보호 마스크를 보완하고, 바퀴/프레임 빈 공간이 많은 자전거는 기존 분리를 유지한다. 신뢰도0.5 내부를1px 침식해 불투명화하고 그 밖의 원래 가장자리 알파는 그대로 둔다. RGB·모델·4단계·스타일/연령/HEX는 유지, 추가AI추론/커스텀노드 없음. [104개 검증/전후](validation/alpha_opaque/README.md). 배포에는 BlockCatalog.json과 product_480.api.json **둘 다** 적용하고 프로그램을 완전히 재시작한다. 바탕화면 Thai_02는 적용 완료, 이미USB로 옮긴 빌드는 Builds/Alpha_Workflow_Update.zip(7647바이트)을 덮어쓴다.

- 블록에서 인식한 4연령/10물건/6색을 사용한다. 제품별 ageVariants로 종류·형태와 색 부위를 선택한다. 유치원모자/비니/볼캡/중절모 등 기존 연령 구분은 유지하며 로퍼에 끈이 그려지는 혼동과 잘못된 이어폰 형태는 명시적인 설명으로 수정했다.
- 언어 인코더에 실제 색 블록 원의 HEX와 대응 색 이름·부위를 함께 전달한다. 원 RGB/두 선택은 저널에도 보존한다. 모델이 명암/악센트를 그리므로 모든 픽셀을 지정 RGB로 고정한다는 뜻은 아니다.
- 원본 스타일 PNG는 StyleReferences에 해시 이름으로 포함한다. 서버의 같은 이름 파일이 다르면 덮어쓰지 않고 실패한다. 원본을 회색 입력으로 전처리해 레퍼런스의 분홍/파랑을 선택 색으로 잘못 복제하는 영향을 줄인다. ReferenceLatent는 그림체를 전달하고 새 물건을 그린다. 물건 블록의 윤곽을 강제로 따라 그리거나 기존 가이드에 색만 입히는 경로가 아니다.
- Qwen3 4B 텍스트 → 원본 스타일 조건 → Klein4단계 → VAE → BiRefNet → 실제 알파480RGBA → 앱의 원본 검사/중앙보정. CFG1에는 negative prompt 효과가 없고 스타일은 양성 설명과 참조 조건으로 지정한다. 노드5는 SamplerCustomAdvanced, 실제 시드는 RandomNoise 노드36이다. Controller가 노드5에 잘못된seed/positive/negative를 추가하지 않는다.
- quality/balanced/fast 모두 검수한4단계를 사용한다. 불필요한 두 번째 생성/후처리 모델/ControlNet/가이드형태제어/외부 유료 API/커스텀노드 추가는 없다.

## 잘린 결과 처리와 수명

다운로드 원본에 알파64이상 물건이3px 가장자리까지 닿거나, 주 전경의8%를 넘는 큰 분리 부품이 있거나, 가시 영역이 너무 희미하면 표시 전에 거부한다. 이미 잘린 물건을 축소/중앙배치로 감추지 않는다. 실패 후에는 입력/타이머를 복구하고 새 버튼 입력으로 재시도한다. 이 검사는 경계/분리/빈 이미지 검사이며 모든 의미적 구조나 미술 스타일을 판별하는 AI 검수기가 아니다.

허용된 원본은480×480 중심(240,240), 최소24px 투명 여백, 종횡비 유지로 배치한다. 생성·다운로드·검사 완료까지 선택 패널의 모든 버튼과5초 유지 입력을 잠그며 실패/취소/복귀/기한 초과 시 복구한다. 원본 서버PNG와 사용자 블록 아트/씬은 보존한다.

## 설치·실행

ComfyUI0.42.0의 기존 기본 노드로 검증한다. 설치 폴더/패키지를 임의 업데이트하지 않는다. 모델은 model.json, text_encoder.json, vae.json, background_model.json의 고정revision/바이트/SHA로 확인한다. 새 PC에는 다음을 실행한다.

```powershell
$modelsRoot = "$env:LOCALAPPDATA\Comfy-Desktop\ComfyUI-Shared\models"
python Tools/ComfyUI/download_model.py --manifest Tools/ComfyUI/model.json --models-dir "$modelsRoot\diffusion_models"
python Tools/ComfyUI/download_model.py --manifest Tools/ComfyUI/text_encoder.json --models-dir "$modelsRoot\text_encoders"
python Tools/ComfyUI/download_model.py --manifest Tools/ComfyUI/vae.json --models-dir "$modelsRoot\vae"
python Tools/ComfyUI/download_model.py --manifest Tools/ComfyUI/background_model.json --models-dir "$modelsRoot\background_removal"
python Tools/ComfyUI/build_workflow.py --output-dir Tools/ComfyUI/workflows
Copy-Item Tools/ComfyUI/workflows/product_480.api.json Assets/StreamingAssets/BlockGeneration/product_480.api.json
```

앱과benchmark.py는 포함된 스타일 참조를 검사/자동 업로드한다. 수동 API 시험은 `python Tools/ComfyUI/prepare_style_reference.py`를 먼저 실행한다. 기본 그래프와카탈로그는 다음Game 진입에 다시 읽는다. 새 C#은 Editor 재컴파일/Player 재빌드가 필요하다. 기존 사용자 설정/자동 종료 옵션은 바꾸지 않는다.

## 검증과 제한

현재 검수의 원본/요청ID/해시·각 색/연령·기능 정상/실패/취소·시간 측정은 [STYLE-QUALITY-003 기록](validation/comfy_style3/README.md)에 보존한다. 최종 샘플의 수치 검증과 직접 시각 검토는 분리한다. 검수한 조합에서 기준 충족을 확인하되 무한히 가능한 모든 랜덤 시드의 무결점을 보장하지 않는다. 실제3060/Windows Player·물리입력·장시간 측정은 해당 장비에서 따로 수행한다.

공식 모델/구현 근거: [Klein FP8](https://huggingface.co/black-forest-labs/FLUX.2-klein-4b-fp8), [ComfyUI Klein 기본 워크플로](https://docs.comfy.org/tutorials/flux/flux-2-klein). Klein/VAE/Qwen 배포 출처와 Apache2.0 라이선스는 고정 manifest 및 Tools/ComfyUI/licenses/flux2-klein-LICENSE.md/NOTICE.txt에 보존한다. 과거 Turbo/Animagine 모델은 삭제하지 않았다.

---

# 과거 구성 (현재 기본값 아님): 4단계 SDXL-Turbo / COMFY-FAST-002

사용자 최신 기준은 **세부 묘사/외곽선 강조 없이 간단한 물건 일러스트에 인식한 두 색을 표현하고,3060 12GB에서10~15초를 목표**로 한다. 이전34초 품질 복귀 설정은 현재 기본값이 아니다. 활성 경로는 **SDXL-Turbo fp16,512×512,4steps,CFG1,SDTurboScheduler+Euler ancestral,단일 생성**이다. 실제 결과/시간은 [FAST-002 검증](validation/comfy_fast2/README.md)을 따른다.

## 활성 경로

연령별 형태는 `BlockCatalog.json`의 각 제품 `ageVariants`에서 선택한다(AGE-DESIGN-002). 모자는 아동 유치원 모자/학생 비니/직장인 볼캡/시니어 중절모, 상의는 티셔츠/후드티/셔츠/카디건처럼 10종 모두 4개의 다른 제품 설명을 지정한다. 일반 제품 설명을 연령별 설명으로 **교체**하고 색을 입힐 부위도 함께 교체한다. 비니에는 챙이 아닌 접힌 커프에 두 번째 색을 넣는다. 4연령 중 일부 누락/중복/잘못된 ID는 카탈로그 검증에서 거부한다. 연령별 설정이 없는 기존 카탈로그는 일반 제품 설명을 사용한다. 연령 디자인은 체험을 위한 대표 예시이며 실제 개인의 취향을 제한하는 규칙이 아니다. 변경은 카탈로그를 다시 로드한 다음 체험부터 적용된다. 전체 매핑/실제 결과/검증 한계는 [AGE-DESIGN-002](validation/comfy_age_design/README.md)를 따른다.

블록4선택 → 짧은 제품/두 색/연령 디자인 프롬프트 → Turbo4단계 → VAE → BiRefNet → 실제 알파480RGBA(노드7) → 앱에서 가장 큰 연결 전경 선택·중앙보정. ControlNet/Canny·물건 가이드 업로드·IPAdapter·LoRA·두 번째 생성/스타일 보정은 사용하지 않는다. 프롬프트에서도 세밀한 재질/기계/복잡한 표면/외곽선 강조 조건을 제거했다. 모델이 자연스럽게 경계선을 그리는 경우는 있다.

기본 quality4단계,balanced3,fast2다. 모두 내부512/batch1,출력480RGBA다. 공식 권장 Turbo 스케줄을 사용하며CFG1이라 negative prompt는 효과가 없다. 같은 positive conditioning을 negative 입력에도 연결해 불필요한 음성 인코딩을 생략한다. 실제 네 선택과색상 HEX/RGB는 요청/저널에 유지하고, 짧은 색 이름과 부위 설명을 모델에 전달한다. 색상은 실제6개 색 블록 원 내부에서 추출한 값이며 픽셀 RGB를 강제 고정하는 방식은 아니다.

최종 프레이밍은8방향으로 연결된 알파8이상 픽셀 중 가장 큰 영역을 선택해 별도의 작은 물건/장식을 제거한다. 같은 물건의 대각선·가는 연결은 유지한다. 원본 배열/서버PNG는 변경하지 않고 작업스레드에서 취소 가능하게 처리한다.480×480의중심(240,240)/최소24px여백/비율유지·필요할 때만 축소를 유지한다. 물건 일부가 원본 생성부터 잘린 경우는 복구할 수 없으며 큰 물건끼리 붙으면 함께 남을 수 있다.

생성 완료·다운로드·중앙보정까지 모든 선택패널 버튼/5초 유지 입력을 막는다. 실패·취소·홈/세션 종료의 이미지 해제·입력/타이머 복구와 Result에 같은 텍스처를 넘기는 동작을 보존한다. 현재10종/4연령/6색 원본PNG/GUID·씬/패키지는 변경하지 않는다.

## 설치·갱신

현재 필요한 모델은 `model.json`의SDXL-Turbo fp16 체크포인트와`background_model.json`의BiRefNet이다. Turbo는 가중치 파일은 약6.94GB지만 생성 평가 횟수/해상도를 크게 줄이는 가속 모델이다. 파일 크기를2GB 경량 모델처럼 설명하지 않는다. 이 PC에서는 새 Turbo 모델의 고정 revision/크기/SHA256을 검증해 설치했다. 기존 기본 ComfyUI 노드만 사용한다.

```powershell
$modelsRoot = "$env:LOCALAPPDATA\Comfy-Desktop\ComfyUI-Shared\models"
python Tools/ComfyUI/download_model.py --manifest Tools/ComfyUI/model.json --models-dir "$modelsRoot\checkpoints"
python Tools/ComfyUI/download_model.py --manifest Tools/ComfyUI/background_model.json --models-dir "$modelsRoot\background_removal"
python Tools/ComfyUI/build_workflow.py --output-dir Tools/ComfyUI/workflows
Copy-Item Tools/ComfyUI/workflows/product_480.api.json Assets/StreamingAssets/BlockGeneration/product_480.api.json
```

`SamplerCustom/SDTurboScheduler/KSamplerSelect`와 배경 제거 기본 노드가 필요하다.Unity패키지/Comfy코어 소스/커스텀노드의 업데이트는 하지 않았다. 이전IPAdapter실험폴더는disabled유지. 구도 가이드 파일들은 보관용이며현재사용하지않는다. `sync_block_references.py`는Turbo모드에서poseGuide를다시활성화하지않는다. 팔레트 갱신/검사는Pillow가있는Python의`sync_block_palette.py [--check]`를사용한다. API와UI는[워크플로](../Tools/ComfyUI/workflows/product_480.ui.json)와StreamingAssets사본을함께갱신한다.

## 성능/검수

실측 장비는 RTX5070 Ti Laptop12GB,ComfyUI0.42.0/PyTorch2.11.0+cu128이다. 각기 다른 시드의 최종20회는 중앙1.750초/최대2.541초였다. 실제 앱은 모델 캐시 해제 후 첫6.242초,다른 연령/색의 반복2.647초였다. 자세한 조건은 [검증 기록](validation/comfy_fast2/README.md)을 따른다.

3060 12GB는 예열 후 약5~12초를 계획 추정치로 본다. 실제3060 측정/보장이 아니며 첫 모델 로드·SSD·다른GPU 부하에 따라 추가 시간이 필요할 수 있다.

```powershell
python Tools/ComfyUI/benchmark.py --warmup 1 --runs 20 --target-seconds 15 --output-dir Tools/ComfyUI/results/rtx3060
```

`target_gpu_acceptance_verified`는 실제3060에서 예열 후20회 모두15초 이내일 때만true다. 현PC 통과를3060 통과로 표시하지 않는다. Windows Player/실장비·장시간·전색/시드 품질은 별도 검수한다.

공식 근거: [Turbo 모델 카드](https://huggingface.co/stabilityai/sdxl-turbo),[ComfyUI Turbo 스케줄 예제](https://comfyanonymous.github.io/ComfyUI_examples/sdturbo/). 고정 [LICENSE](../Tools/ComfyUI/licenses/sdxl-turbo-LICENSE.md)와[NOTICE](../Tools/ComfyUI/licenses/NOTICE.txt)를 보존했다. 배포/상업 사용은 그 조건을 따른다. Powered by Stability AI.

---

# 과거 ComfyUI 물품 이미지 생성 구성 — QUALITY-001 보관 기록

## 당시 품질 우선 구성 (QUALITY-001)

서브컬처 모바일게임의 아이템 일러스트를 목표로 **Animagine XL 4.0 Opt**를 사용한다. SDXL Base 내부768/16steps 시험 구성을 교체했다. 인물을 직접 연상시키는 그룹 이름 대신 디자인 특성을 프롬프트에 넣고, 제품의 구조·재질·봉제선·금속/고무 등의 묘사와 제품별 금지 조건을 추가했다. 신발은 복수 생성을 줄이기 위해 한 켤레 대신 **신발 한 개**를 생성한다. UI의 Shoes 이름은 유지한다.

| 항목 | 값 |
|---|---|
| 체크포인트 | animagine-xl-4.0-opt.safetensors |
| 내부 생성 / 최종 출력 | 1024×1024 / 480×480 **8-bit RGBA PNG** |
| 샘플러 / 스케줄러 | euler_ancestral / normal |
| 기본 quality | 28 steps, CFG5, denoise1, batch1 |
| balanced / fast | 24 / 20 steps (품질 재검수 필요) |
| 형태 제어 | SDXL Canny Control-LoRA rank256, strength1.4, start0/end1 |
| 배경 제거 | BiRefNet, 내부1024에서 분리 후 area480 축소 |

흐름: **네 선택 → 단일 제품 구조 가이드 + 재질/색 프롬프트 → 샘플링 → BiRefNet 전경 마스크 → InvertMask → JoinImageWithAlpha → RGBA 축소 → SaveImage → Unity ResultImage**. ComfyUI의 JoinImageWithAlpha는 입력 마스크를 반전해 알파로 사용하므로 전경 마스크를 먼저 InvertMask로 바꾼다. 체크무늬 배경을 그려 넣는 방식이 아니다. Unity는 RGBA PNG 헤더·480 크기를 확인하고 다운로드가 끝난 텍스처를 할당한 후 표시한다.

원본 기하학 가이드14개는 `Assets/StreamingAssets/BlockGeneration/ControlGuides`에 포함된다(11제품+Equipment 3종). 가방1개, 신발1개, 시계1개 등 기본 윤곽/구도만 제어하고 색과 재질은 모델이 생성한다. 정면에 가까운 구도로 다양성을 줄이는 선택이다. Unity가 요청마다 해당 가이드를 ComfyUI input/Thai02Quality에 확인·필요 시 자동 업로드한다. 같은 이름의 다른 파일은 덮어쓰지 않고 실패로 안내한다. 로컬 읽기/확인/업로드는 취소·제한 시간을 사용한다. 가이드는 작고 고정된14개이므로 생성 결과처럼 계속 누적되지 않는다.

## 실제 검증과 제한

검증 자료: [품질 검증](../Tools/ComfyUI/validation/quality/README.md). 실제 측정 GPU는 **RTX5070 Ti Laptop 12GB**, ComfyUI0.37.0 / PyTorch2.12.1+cu130다. RTX3060 12GB의 시간/VRAM·Windows Player·장시간 운영은 미검증이다. 이번 변경은 품질 우선이며 과거 약20초 목표를 보장하지 않는다. 모델 전환/최초 로딩/동시 Unity GPU 사용으로 더 느려질 수 있다.

프롬프트만 보강했을 때 일부 헤드폰·시계·우산·신발은 잘못된 물체/복수 물체가 생성되었다. 약한 형태 제어(.85)도 부족하여 최종1.4로 높였다. 결과물의 단일 피사체·인체 배제·색상 준수는 확률적인 모델 특성상 전 조합에서 보장되지 않는다. 형상을 더 엄격하게 고정할수록 세부 디자인의 자유도는 줄어든다. 얇은 테·끈·반투명 렌즈의 경계/알파와 작은 잔여 무늬는 실물 검수가 필요하다. 전체 그룹×품목×색×시드 품질 승인은 별도다.

현재 기능은 정지 이미지 완성본 수신이다. 중간 프리뷰 스트리밍·같은 latent의 실시간 제품 전환·영상 생성은 포함하지 않는다. API 노드7이 최종 SaveImage라는 Unity 계약을 유지했다. JSONL에는 정확한 그래프·제품·색·resolved equipment·가이드 경로·시드를 남긴다. 취소 후 서버에서 이미 실행 중인 자기 작업은 계속될 수 있으며 공유 서버 전역 interrupt는 호출하지 않는다. 기존 이미지 보관 한도1000장/512MiB는 유지한다. 자세한 화면/보관 동작은 [블록 시뮬레이터](BLOCK_SIMULATOR.md)를 따른다.

## 자동 실행(Unity 앱)

2026-11-18 DEPLOY-SELF-001: **실행 파일 옆에 `ComfyRuntime` 폴더가 있으면 앱이 그 런타임으로 서버를 직접 실행한다.** `Run_*.cmd`로 `ComfyUISettings.json`의 경로를 고쳐 쓰던 단계는 없어졌다. 이때 `serverExecutable`·`serverArguments`·`serverWorkingDirectory`·`comfyOutputDirectory`는 JSON 값 대신 동봉 경로를 쓰고, 포트는 `serverUrl`의 포트, 준비 대기는 최소 300초다. 실행 전에 `python/python.exe`·`portable_server.py`·`ComfyUI/main.py`·`package_manifest.json`의 모델 크기를 확인하며, 빠진 파일이 있으면 `Bundled ComfyRuntime ... missing` 로그를 남기고 실행하지 않는다. 폴더가 없으면(개발 PC 등) 아래 표의 JSON 값으로 실행한다. 배포 폴더는 빌드 후 `python Tools/ComfyUI/package_portable.py "<배포 폴더>"`로 만든다. [검증](validation/deploy_self/README.md).

Unity 앱은 시작 시 1회 `serverUrl`의 `/system_stats`를 검사한다. 응답이 없으면 `ComfyUISettings.json`의 실행 값으로 서버를 **창 없이** 실행하고, 응답할 때까지 2초 간격으로 확인한다. 앱이 직접 실행한 서버도 앱 종료 시 끄지 않는다(모델을 메모리에 유지). 이미 실행 중인 ComfyUI(Comfy Desktop 포함)는 건드리지 않는다.

2026-11-02 COMFY-AUTOSTART-002: **Comfy Desktop GUI를 실행할 필요가 없다.** 설치된 Python으로 `ComfyUI/main.py`만 직접 실행한다. Python 환경·ComfyUI·모델은 PC에 설치되어 있어야 하며 모델을 빌드에 포함하거나 자동 다운로드하지 않는다. 이 PC의 `.venv/Scripts/python.exe`는 유효한 가상환경 진입점이다. 내부적으로 standalone-env 자식 프로세스를 실행하므로 자식의 경로만 보고 실행 파일을 바꾸지 않는다(standalone-env를 직접 호출하면 torch를 찾지 못한다).

서버가 `Checking`·`Starting`·`Unavailable`이면 RedButton/숫자1/숫자패드1 및 직접 버튼 어댑터 입력을 버린다. 타이틀 `idle.help`의 Image를 실제 비활성화해 언어 페이드/등장 연출 중에도 보이지 않게 한다. 설정과 서버가 모두 준비되면 입력과 안내를 복구한다. 준비 전부터 누르고 있던 버튼은 놓은 뒤 새로 눌러야 하며, 준비 전 누름으로 언어 전환/체험 시작을 예약하지 않는다.

| 필드 | 기본값 | 의미 |
|---|---|---|
| `autoLaunchServer` | true | 꺼져 있을 때 자동 실행 |
| `launchServerInEditor` | true | 에디터 Play에서도 동일하게 서버 직접 실행. false로 바꾸면 Editor 자동 실행만 생략 |
| `serverExecutable` | `%LOCALAPPDATA%\Comfy-Desktop\ComfyUI-Installs\ComfyUI\ComfyUI\.venv\Scripts\python.exe` | 실행 파일 |
| `serverArguments` | `-s ComfyUI\main.py --listen 127.0.0.1 --port 8188 --disable-auto-launch --models-directory/--input-directory/--output-directory <ComfyUI-Shared>` | 인자. 포트는 `serverUrl`과 같아야 함 |
| `serverWorkingDirectory` | `%LOCALAPPDATA%\Comfy-Desktop\ComfyUI-Installs\ComfyUI` | 작업 폴더 |
| `serverStartupTimeoutSeconds` | 180 | 준비 대기 상한(10~900) |

- 경로·인자의 `%ENV%`는 실행 PC 계정 기준으로 확장한다. 다른 설치 방식(포터블 등)은 세 값을 그 PC에 맞게 바꾼다. 자동 실행은 loopback `serverUrl`에서만 동작한다.
- 모델·입출력 폴더는 Comfy Desktop 공유 폴더를 직접 지정하므로 Desktop의 진단 프로필 yaml에 의존하지 않는다. Desktop 관리자/텔레메트리 플래그는 넘기지 않는다.
- 실행 실패/시간 초과는 영문 로그에 원인을 기록하고 입력/타이틀 버튼 안내는 계속 차단한다. 이후10초마다 서버를 확인해 정상 응답하면 안내와 입력을 복구한다. 실패 프로세스를 무한 재실행하지 않는다. 과거 문서의 ComfyStatusPresenter/ComfyUIStatusText는 현재 Assets에 없으므로 상태 텍스트가 표시된다고 가정하지 않는다.
- 실행 중 서버가 나중에 꺼지는 경우는 감시하지 않는다(앱 시작 시 1회).
- 기본 바인딩은 `127.0.0.1:8188`이며 외부 PC 접속은 필요 없다. 배포 빌드는 Development/Autoconnect Profiler를 끈다. 개발 Player의 디버그 검색/연결은 Windows 네트워크 허용 알림을 유발할 수 있다. OS 알림을 앱이 무조건 허용하거나 방화벽 전체를 해제하지 않는다. 이 세션에서 뜬 알림은 사용자가 직접 허용했으며 방화벽 설정은 변경하지 않았다. 다른 PC·다른 실행 경로의 첫 실행에서 OS가 별도 승인을 요구할 수 있다.
- 실제 검증/재현 도구: [comfy_autostart](validation/comfy_autostart/README.md). Windows 빌드·서버 직접 실행/재사용/실패를 검증하며 모델 성능·실물 버튼 검수와 구분한다.

## 다른 PC 설치·재생성

모델은 저장소/StreamingAssets에 넣지 않는다. 다음 경로를 실제 ComfyUI 모델 폴더로 바꿔 실행한다. 다운로드 도구는 고정 revision·크기·SHA256을 확인하며 다른 기존 파일/partial을 덮어쓰지 않는다.

```powershell
python Tools/ComfyUI/download_model.py --models-dir 'C:\ComfyUI\models\checkpoints'
python Tools/ComfyUI/download_model.py --manifest Tools/ComfyUI/background_model.json --models-dir 'C:\ComfyUI\models\background_removal'
python Tools/ComfyUI/download_model.py --manifest Tools/ComfyUI/control_model.json --models-dir 'C:\ComfyUI\models\controlnet'
python Tools/ComfyUI/install_control_guides.py --server http://127.0.0.1:8188
python Tools/ComfyUI/build_workflow.py
python -m unittest discover -s Tools/ComfyUI -p test_workflow.py
```

현재 PC에서는 세 모델의 SHA256을 검증했고 기본 노드가 이미 존재하여 커스텀 노드 설치/서버 재시작은 없었다. 다른 ComfyUI에 LoadBackgroundRemovalModel/RemoveBackground/InvertMask/JoinImageWithAlpha/ControlNetApplyAdvanced 등이 없으면 먼저 버전과 노드 지원을 확인한다. Unity 패키지는 업데이트하지 않았다. 가이드 재생성 도구 create_control_guides.py는 Pillow가 필요하며 보통은 저장된 PNG를 그대로 사용한다.

GUI 가져오기: [product_480.ui.json](../Tools/ComfyUI/workflows/product_480.ui.json). `10~13`의 네 입력과 `24`의 품목 가이드를 함께 바꾼다. API/Unity는 이 선택을 자동으로 맞춘다. Equipment는 robots/keyboards/laptops 중 하나로만 해석한다. 원본 카탈로그는 `Assets/StreamingAssets/BlockGeneration/BlockCatalog.json` 하나이며 Tools의 presets가 이를 읽는다. Tools에서 생성한 API를 런타임 product_480.api.json에도 함께 반영한다.

모델 라이선스: Animagine CreativeML Open RAIL++-M, BiRefNet MIT. **Control-LoRA는 별도 Stability AI Control-LoRA Community License Agreement**이며 라이선스·고지를 보존해야 한다. 월간 활성 사용자100만 초과 사업자에 대한 추가 상업 조건은 [고정 LICENSE.MD](https://huggingface.co/stabilityai/control-lora/blob/75590eb0e7868d3d0f1581b5126d487b33c7fdb4/LICENSE.MD)를 확인한다. 세 모델의 출처·revision·해시는 Tools의 각 manifest에 기록했다.

## 대상 PC 성능 검수

모델/가이드 설치 후 빈 큐에서 다음을 실행한다. 서로 다른 시드로 워밍업과 실측을 분리한다. benchmark는 480 RGBA 헤더를 검사하지만 미술 품질을 자동 판정하지 않는다.

```powershell
python Tools/ComfyUI/benchmark.py --profile quality --warmup 1 --runs 20 --output-dir Tools/ComfyUI/results/rtx3060
```

3060에서 실제20회 전부20초 이내인 경우에만 기존 시간 승인 플래그가 true가 된다. 품질을 낮춰 시간만 맞추지 않는다. 실제 전시 Unity 화면과 함께 GPU 메모리·지연·연속 생성·복귀 후 텍스처 해제도 검수한다.

## 공식 근거

- [Animagine XL4.0 모델 카드](https://huggingface.co/cagliostrolab/animagine-xl-4.0): 모델/권장 샘플링 설정.
- [Control-LoRA](https://huggingface.co/stabilityai/control-lora): SDXL용 구조 제어.
- [BiRefNet 공식 모델](https://huggingface.co/ZhengPeng7/BiRefNet), [Comfy 변환본](https://huggingface.co/Comfy-Org/BiRefNet).
- [ComfyUI 배경 제거 구현](https://github.com/Comfy-Org/ComfyUI/blob/master/comfy_extras/nodes_bg_removal.py).
