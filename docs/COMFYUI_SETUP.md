# 보관 문서 — ComfyUI 연동 종료 (2026-10-22)

**이 문서는 과거 구현/검증 기록이다. 현재 설치 지침으로 사용하지 않는다.** 앱의 ComfyUI 자동실행·서버 검사·이미지 생성·다운로드·가이드 업로드·출력 자동삭제를 제거했다. 신규 생성 방식은 아직 연결되지 않았다. 현재 사용법은 [BLOCK_SIMULATOR](BLOCK_SIMULATOR.md)를 따른다.

기존 런타임 전용 파일과 메타는 `Tools/ComfyUI/legacy_runtime/Assets`에 보관하며 Unity 실행/빌드 대상에서 제외했다. 외부 설치본·모델·기존 생성물은 변경하지 않았다. 아래 가이드/StreamingAssets 경로와 모델/성능 검증은 이전 구현 기준이다.

---

# ComfyUI 물품 이미지 생성 구성

## 현재 품질 우선 구성 (QUALITY-001)

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

Unity 앱은 시작 시 1회 `serverUrl`의 `/system_stats`를 검사한다. 응답이 없으면 `ComfyUISettings.json`의 실행 값으로 서버를 **창 없이** 실행하고, 응답할 때까지 2초 간격으로 확인한다. 앱이 직접 실행한 서버도 앱 종료 시 끄지 않는다(모델을 메모리에 유지). 이미 실행 중인 ComfyUI(Comfy Desktop 포함)는 건드리지 않는다.

| 필드 | 기본값 | 의미 |
|---|---|---|
| `autoLaunchServer` | true | 꺼져 있을 때 자동 실행 |
| `launchServerInEditor` | false | 에디터 Play에서도 실행할지 여부. 기본은 빌드본에서만 실행 |
| `serverExecutable` | `%LOCALAPPDATA%\Comfy-Desktop\ComfyUI-Installs\ComfyUI\ComfyUI\.venv\Scripts\python.exe` | 실행 파일 |
| `serverArguments` | `-s ComfyUI\main.py --listen 127.0.0.1 --port 8188 --disable-auto-launch --models-directory/--input-directory/--output-directory <ComfyUI-Shared>` | 인자. 포트는 `serverUrl`과 같아야 함 |
| `serverWorkingDirectory` | `%LOCALAPPDATA%\Comfy-Desktop\ComfyUI-Installs\ComfyUI` | 작업 폴더 |
| `serverStartupTimeoutSeconds` | 180 | 준비 대기 상한(10~900) |

- 경로·인자의 `%ENV%`는 실행 PC 계정 기준으로 확장한다. 다른 설치 방식(포터블 등)은 세 값을 그 PC에 맞게 바꾼다. 자동 실행은 loopback `serverUrl`에서만 동작한다.
- 모델·입출력 폴더는 Comfy Desktop 공유 폴더를 직접 지정하므로 Desktop의 진단 프로필 yaml에 의존하지 않는다. Desktop 관리자/텔레메트리 플래그는 넘기지 않는다.
- Idle의 `ComfyUIStatusText`(부모 IdlePanel의 `ComfyStatusPresenter`)는 실행 후 준비 전(Starting)과 실행 불가(설정 오류·프로세스 조기 종료·시간 초과·에디터에서 실행 생략) 때만 표시하고 준비되면 숨긴다. 실행 불가 후에도 10초마다 확인해 운영자가 수동으로 켜면 숨긴다. 재실행은 하지 않는다. 문구는 컴포넌트에서 변경한다.
- 실행 중 서버가 나중에 꺼지는 경우는 감시하지 않는다(앱 시작 시 1회).

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
