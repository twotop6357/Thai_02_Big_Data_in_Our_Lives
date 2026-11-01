# COMFY-LIGHT-001 — 레퍼런스 품질 감사 / 속도 목표 미달

**현재 설정은 후속 [COMFY-FAST-002](../comfy_fast2/README.md)의 Turbo4/512로 대체됐다. 이 문서의34초 품질 복귀는 과거 기록이며 현재 기본값이 아니다.**

2026-10-30 Asia/Seoul, main/37723c7+기존 미커밋. Codex 단독 구현·실측·시각 검토이며 교차 검증 아님.

**경량/가속 후보는 첨부 레퍼런스와 충분히 유사하지 않아 불채택했다. RTX3060 12GB에서10~15초와 참고 이미지 수준 품질을 함께 만족한 상태가 아니다.** 현재 기본값은 첨부 이미지가 실제로 생성됐던 AnimagineXL4.0 Opt/1024/36steps/CFG5.5/Euler ancestral/normal 품질 기준으로 복구했다. 운영 설정은 [COMFYUI_SETUP](../../COMFYUI_SETUP.md)을 따른다.

![레퍼런스·불채택된 빠른 결과·복구한 품질 모드](reference_comparison.png)

## 비교 판단

| 항목 | 첨부 레퍼런스 | 빠른 후보 | 복구 품질 모드 |
|---|---|---|---|
| 색 | 분홍·파랑·흰색의 분명한 큰 색면 | 흰색 비중/옅은 색이 과함 | 큰 색면과 대비가 더 가까움, 세부 배치는 다름 |
| 명암/재질 | 부드럽고 단정한2D 셀 명암 | 입체 렌더/미세 무늬·스케치 같은 선이 섞임 | 파스텔2D 명암과 매끈한 면에 더 가까움 |
| 선 | 가는 분홍/라벤더 색 경계 | 잡선·거친 작은 디테일 | 참고에 가까운 가는 색 경계, 검정 굵은 선은 억제 |
| 형태/구도 | 온전한 제품과 자연스러운 사선 시점 | 일부 분리된 신발·장식·인물화 | 신발2시드 재생성에서 온전한 자세 확인, 전체 조합 승인 아님 |

유사도 백분율은 산출하지 않았다. 수치화된 미술 품질 점수를 만들지 않고 실제 출력의 색면·명암·선·형태를 직접 비교했다. 비교 PNG는 원본480RGBA를 흰색에 합성한 검토용 배치이며 생성 이미지의 내용을 재그리거나 보정하지 않았다. 복구 예시는996532 새 시드다.996531 동일 시드 재생성도 함께 보존했으며 여기에 작은 장식 다이아몬드가 남아있다. 단일 신발/한 켤레의 차이는 사용자가 수량을 강제하지 않았으므로 스타일 불합격 사유로 삼지 않았다.

## 실제 시험

- 현재 장비: RTX5070 Ti Laptop12GB, ComfyUI0.42.0, PyTorch2.11.0+cu128. RTX3060은 사용하지 않았다.
- Lightning/LCM/Hyper-SD와 SD1.5/XL 모델·해상도·참조 강도를 실생성 비교. `probes*`, `quality_recheck`, `quality_fast`, `quality_sampling`은 후보별 실제 API/요청ID/단계별 시간/원본 PNG를 보존한다. 출력/타이밍을 캐시로 위장하기 위한 재전송은 하지 않았다.
- 빠른 두 단계 후보: `applied/report.json`에서10종×2시드 및 가방/신발4연령,28 논리 경우/26 실제 생성. 기능/알파/중앙 검사와 미술 품질 승인은 별개다. `benchmark/benchmark.json`은 이 **불채택 후보**의 예열20회 중앙값4.588초/P954.992초/최대5.197초, 최초 예열7.147초다. `runtime/runtime.json` Play8항목 PASS/6.141초도 이 후보의 결과다.
- 단일 Counterfeit16 후보: `quality_applied`에서 전체 반복 확인. 신발 색면은 개선됐으나 선글라스 인물화·모자/시계/우산의 장식/형태 오류가 남아 불채택.
- 직접 스타일 참조: cubiq IPAdapter Plus(a0f451a5113cf9becb0847b92884cb10cbdec0ef), h94 SD15 Plus98MB SHA검증, 기존 MOVA CLIP ViT-H14를 재사용했다. `ip_style_probe`/`ip_style_quality`에서 신발/헤드폰/상의/선글라스 및 다른색 가방 확인. 일부 느낌은 가까워져도 선글라스 인물화·색상 영향과 과한 질감이 남아 불채택. 새 커스텀 노드 폴더는 `.disabled`로 변경하고 빈 큐에서 재시작해 비활성화했다. 기존 Comfy 소스/Unity·UPM 패키지는 변경하지 않았다.
- 복구 기본값: `restored_quality/report.json`의 두 신발 요청33.235초/25.299초, 실제 Unity 중앙보정2/2 확인. **이 속도가 현재 활성 품질 모드의 측정값이며10~15초 목표 미달이다.**
- 복구 후 실제 앱: `restored_runtime/runtime.json`,8항목 PASS/33.767초, 중심(240,240),480RGBA/24px여백/모서리알파0·입력잠금·Result 동일 텍스처·홈 자원 해제/타이머 재개 확인.
- 최종 EditMode59/59 PASS, job79879aa359ae404e9ffc714c00c1b11f,11.8315898초. 최종 Python 워크플로7/7 PASS. 최종 범위 diff 공백 검사 PASS. 전체 저장소의 기존 씬/설정 공백 경고는 수정하지 않았다.
- `preservation_check.json`: 시작 시 보존 대상으로 기록한 원본20블록 PNG·3씬·입력/View/매니저/프레이밍·패키지/설정 등31파일 SHA256 동일. 사용자 기존 미커밋을 삭제/리셋/커밋하지 않았다.

## 남은 작업

- 3060 실제20회/Windows Player·장비·장시간 검증, 전 물건×색×시드의 최종 품질 승인.
- 레퍼런스 스타일을 충분히 유지하는 경량 모델/스타일 학습 또는 참조 방식의 추가 검토. 현재 시험한 가속 후보를 완료 설정으로 재활성화하지 않는다.
- 가는 잔여 선/장식, 잘못된 형태/인물 혼입, 색상 구역 및 정확한 RGB 일치는 확률적인 생성의 남은 위험이다. 숫자 색상코드는 메타데이터에 보존되지만 픽셀 색 강제 조건은 아니다.

공식 연구/모델 출처: [SDXL-Lightning](https://huggingface.co/ByteDance/SDXL-Lightning), [LCM-LoRA](https://huggingface.co/latent-consistency/lcm-lora-sdv1-5), [Hyper-SD](https://huggingface.co/ByteDance/Hyper-SD), [DreamShaper](https://huggingface.co/Lykon/DreamShaper), [Counterfeit](https://huggingface.co/gsdf/Counterfeit-V3.0), [IP-Adapter](https://huggingface.co/h94/IP-Adapter), [IPAdapter Plus 구현](https://github.com/cubiq/ComfyUI_IPAdapter_plus). 각 모델의 pin/revision/SHA 및 실제 trial API는 본 폴더의 manifest/report에 남겼다.
