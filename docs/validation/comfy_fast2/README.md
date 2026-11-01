# COMFY-FAST-002 — 단순 일러스트/속도 우선 적용

2026-10-30 Asia/Seoul, main/37723c7 + 기존 미커밋 보존. Codex 단독 구현/실측/시각 검토. 최신 사용자 기준은 복잡한 디테일이나 외곽선 강조보다 **간단한 물건 일러스트, 인식한 두 색 표현,3060 10~15초 목표**다. 이전34초 품질 복귀는 이 기준의 완료 결과로 사용하지 않는다.

## 실제 활성 결과

**SDXL-Turbo fp16,512×512,4steps,CFG1,SDTurboScheduler+Euler ancestral,단일 생성**을 앱 기본값에 적용했다. ControlNet/Canny/가이드 업로드·LoRA·IPAdapter·두 번째 생성/스타일 마무리는 없다. BiRefNet의 실제 투명 마스크와480RGBA 출력은 유지한다. 선택한 물건/색/연령을 짧은 문장으로 전달하며, 미세 재질·복잡한 표면·특정 외곽선 묘사 지시는 제거했다. 모델이 경계선을 그리는 경우는 남는다.

![실제 앱과 다른 물건 생성 예시](preview.png)

왼쪽 두 장은 실제 앱에서 빨강/파랑 및 초록/흰색을 인식시킨 결과다. 원본 PNG를 흰 배경에 배치한 검토용 이미지이며 내용을 재그리거나 색칠하지 않았다. 서버 원본은 각 결과 폴더에 보존한다.

## 현재 PC 실측

RTX5070 Ti Laptop12GB / ComfyUI0.42.0 / PyTorch2.11.0+cu128 / Unity2022.3.62f3 Editor.

| 측정 | 실제 시간 |
|---|---:|
| 최종API20회 중앙값 | 1.750초 |
| API P95 / 최대 | 2.443 / 2.541초 |
| 실제 앱 첫 생성(서버 모델 캐시 해제 후) | 6.242초 |
| 실제 앱 반복 생성(다른 연령/색) | 2.647초 |
| 이전 앱 품질 기본값(이전 작업 증거) | 33.767초 |

API 시간은 요청→샘플링→VAE→BiRefNet→PNG 저장→다운로드 전체다. 앱 시간은 실제 블록4선택을 완성한 뒤 최종 텍스처 수신/프레이밍까지다. 첫 생성과 반복 생성을 분리했다. 최종20회는10물건,4연령,4가지 색 쌍,각기 다른 시드로 실행했다. `verified_benchmark/benchmark.json`과 `runtime`/`runtime_warm`이 실제 증거다. 이전 중간 benchmark 폴더는 후보/프롬프트 변경 전의 기록이다.

3060 12GB에서는 **예열 후 약5~12초를 계획 추정치**로 본다. 이는현재PC 측정에 여유를 둔 추정이며 하드웨어별 정밀 환산/실측 보장이 아니다. 실제3060은 사용하지 않았고 `target_gpu_acceptance_verified=false`를 유지한다. 첫 모델 로딩·SSD·다른 GPU 프로그램 부하에 따라 시간이 늘 수 있다. 런타임 중 GPU 메모리 스냅샷은8,137MiB/12,227MiB였으며 피크 측정값은 아니다.

## 기능/반복 검증

- 최종 Python 워크플로7/7 PASS. 모든4연령×10물건×6색×6색 입력,원 RGB,API/런타임 사본,UI 링크,Turbo 스케줄과noise_seed 확인.
- EditMode61/61 PASS(job8dc8cc0140e44aa7a86aefd712adf779,12.4734841초). 기존59개에 별도 물건/장식 제거 및 얇은 대각선 연결 보존2개 추가.
- 최종 물건10종×2시드 + 가방/신발4연령,28 논리 경우/26개 실제 이미지. 같은API 해시의 기존 결과만 재사용했고 변경한 품목은 재생성했다. `final/report.json`에 요청ID/선택/시간/해시를 남겼다.
- 실제 앱 첫/반복 각각8항목,합계16항목 PASS. 생성 중 입력잠금,480RGBA,중심(240,239.5)/(240,240),24px 여백,모서리알파0,Result 같은 텍스처,홈 이미지 해제/타이머 재개 확인. 반복은child/green/white로 다른 입력을 사용했다.
- 실제 프레이밍26/26 중앙/여백 검사. 여러 물건/잔여 장식은 가장 큰8방향 연결 알파 영역만 선택한다. 서버PNG/원본 배열은 보존한다. 일부 물건이 서로 붙어 있으면 함께 남을 수 있고 생성 단계에서 잘린 부분을 복구하지는 않는다.
- 시작 보존 목록31파일 중30개 동일. 변경1개는 이번에 명시적으로 확장한GeneratedImageFraming.cs다. 원본20블록 PNG·3씬·입력/View/매니저·패키지/설정은 동일. 미커밋 기존 변경을 삭제/덮어쓰기/커밋하지 않았다.

자료: [첫 시드10종](final/products_first.png),[다른 시드10종](final/products_second.png),[연령 비교](final/age_comparison.png),[요약JSON](summary.json),[보존 검사](preservation_check.json). 모든 시드의 미술 품질/색 부위가 완벽하다는 승인은 아니다. 색 블록 원의 HEX/RGB는 요청에 보존되지만 생성 픽셀 RGB를 강제 고정하지 않는다.

## 설치/출처

[현재 설정/설치](../../COMFYUI_SETUP.md),[공식Turbo모델](https://huggingface.co/stabilityai/sdxl-turbo),[공식ComfyUI스케줄 예제](https://comfyanonymous.github.io/ComfyUI_examples/sdturbo/). 새 체크포인트의 고정revision/크기/SHA256 검증 완료, 기본 노드만 사용하므로 Comfy코어/Unity패키지/커스텀노드 업데이트 없음. 이전 IPAdapter 실험은disabled 유지.

Powered by Stability AI. [고정 라이선스](../../../Tools/ComfyUI/licenses/sdxl-turbo-LICENSE.md)와[고지](../../../Tools/ComfyUI/licenses/NOTICE.txt)를 보존했다. 배포/상업 사용에는 해당 조건이 적용된다.3060 실측20회·Windows Player/실물입력·장시간 검수는 후속 장비 검증이다.
