# STYLE-QUALITY-003 — 레퍼런스 기반 귀여운 2D 물건

2026-11-01 Asia/Seoul · main/37723c7 + 기존 미커밋 보존 · Codex 단독 구현/직접 검수(교차 검증 아님).

[10종 다색 미리보기](quality_preview.png) · [사용자 원본/이전/수정 비교](reference_comparison.png) · [전체4연령40개](age_comparison.png) · [다색40개](color_comparison.png)

## 적용 결과

- 활성 경로를 범용 SDXL-Turbo에서 FLUX.2 Klein4B FP8/640/4steps/Euler/Flux2Scheduler 단일 생성으로 변경. Qwen3 4B 텍스트 인코더·Klein BF16 VAE·BiRefNet 실제 알파를 사용한다. 커스텀노드/외부 유료 API/패키지 업데이트 없음. 필요한 기본 노드는 기존 ComfyUI0.42.0에 있었다.
- 사용자의 원본 작은 신발 그림 `docs/validation/comfy_shoepose/reference.png`를 해시 이름의 StyleReferences PNG로 그대로 포함했다. 모델 입력에서 녹색 채널을 회색 이미지로 바꿔 그림체/명암은 참고하고 레퍼런스의 분홍·파랑 복제 영향을 줄였다. 블록 원본의 윤곽을 강제하거나 기존 그림에 색만 입히는 방식이 아니다.
- 4연령×10물건의 개별 형태와 실제6색 원 HEX/RGB를 유지. 언어 인코더에는 코드와 이름/부위를 함께 전달. 로퍼는 끈 없는 구조, 아동 장화는 단순한 통 구조, 시니어 헤드폰은 가벼운 온이어 형태로 정리했다. 디지털 숫자의 깨짐은 학생 시계를 단순한 바늘형 스포츠 시계로 바꿔 피했다.
- 원본 클리핑/희미함/큰 분리 부품을 표시 전에 거부하고 기존 재시도 흐름으로 복귀한다. 3px 경계의 알파64이상 전경, 0.5%미만의 불투명 전경, 주 전경8%초과의 두 번째 분리 영역이 기준이다. 이는 의미/스타일을 완전히 판별하는 검수 AI가 아니다. 정상 물건은480중앙/24px 투명여백/비율 유지. 입력 잠금/취소/타이머/Result 전달 유지.

## 기준과 실제 검수

- 기준: 원본 레퍼런스처럼 부드러운 큰 색면·둥근 형태·완성된 셀 명암·정돈된 경계. 제품 사진/반실사 광택/거친 스케치/잘림/큰 분리·기형/포스터식 중복 구성은 탈락. 선택한 두 색과 연령별 주된 형태가 드러나야 한다. 작은 장식/표시/재질은 간략화할 수 있다.
- 최종 매트릭스는 final_accepted/report.json. 미리 고정한 10종×4연령×2조건80개의 모든 결과를 검수했다. 기본 red/blue40개와 새 시드/다색40개이며 다색은6색을 양쪽 역할로 포함한다. 성공적인 시드만 따로 골라 각 셀을 조립하지 않았다. 같은 전체 그래프 해시·PNG 해시인76개만 직전 중립 참조 배치에서 재사용하고, 설정이 바뀐4개는 같은 계획 시드로 다시 생성했다.
- 최종80개에서 직접 시각 검수 기준 미달0개, 원본 품질/알파/480/중앙/여백 검사80/80 PASS. 실사·반실사, 잘림·주요 물건의 큰 부품 파손·배경 패널 중복을 발견하지 못했다. 모두 실제 Unity GeneratedImageFraming.Center(...requireComplete:true)에서 검사/프레이밍했다. 원본/프레이밍 PNG를 별도 보존하고 연락시트는 흰 배경에 배치만 했다. 미리보기는 제품·연령·색 다양성용 선택이며 두 전체40개 시트를 함께 제공한다.
- 직접 검수는 유한한 표본의 판정이며 모든 랜덤시드의 무결점 보장이나 완전한 의미/스타일 자동 분류기로 보고하지 않는다. 색은 코드로 조건을 주지만 모든 픽셀 RGB/정확한 부위 비율을 강제하지 않는다. 미세한 반사/색조·소품/바늘 위치·작은 묘사는 달라질 수 있다. 새로운 실제 불합격 사례가 생기면 보존해 해당 조건을 수정해야 한다.

## 기능 검증

- Unity2022.3.62f3 EditMode66/66 PASS, 최종job `ebd56d9f91be4944872e8693c788d2f1`, 11.2190014초. Python 워크플로8/8 PASS. 실제 Unity JsonUtility/CreatePrompt의1440조합과 Python builder의 완성 프롬프트/부위/HEX가 일치했다. 파이썬/오프라인shim만으로 Unity 검증을 대체하지 않았다.
- 실제 앱4회는 기본 활성 그래프와 랜덤 세션 시드로 실행. 두 패널 입력 차단, 생성 시작~완료를 가로지르는5초 유지 무시, 새 입력 진행,480RGBA/중앙/여백/투명 코너, Result에 같은 텍스처 전달, 홈 해제 등27항목 PASS. runtime_success.json과runtime_*.png 참조. 이4개도 직접 그림체/형태를 확인했다.
- 별도 로컬 합성 fixture는 GPU생성을 하지 않는다. 원본이 잘린 PNG를 실제 Download에 반환해 표시 거부/예외 원문·입력/타이머 복구를 확인했다. 새 탭 재시도/정상복귀·취소·기한 초과·서버400·홈 수명 등10항목 PASS. 의도한2개 실패의 로그/스택은 검증 때 보존했다. 실제 그림체 검수80개와 합성 fixture를 혼동하지 않는다.
- 3씬/manifest/packages-lock/ProjectSettings는 시작 시 SHA256과 동일. 기존 사용자 변경/원본 블록 아트·외부 패키지는 덮어쓰거나 되돌리지 않았다. 미커밋, 임의 커밋/push 없음. 테스트 fixture는 종료했다. 검증용 Editor는 Play중지/0_Idle/dirty false로 남겨 두었다(도구의 Exit 패턴 차단으로 종료 요청을 실행하지 않았고 우회하지 않음).

## 시간과 배포 경계

- 측정 장비는 RTX5070 Ti Laptop12GB, ComfyUI0.42.0/PyTorch2.11.0+cu128. 최종80 API 요청→PNG 중앙 3.3805초, 최대 3.936초. 실제 앱4개는 3.806초, 3.808초, 4.039초, 3.783초였다. 시간은 원본 생성/실제 앱 처리에서 기록했으며 캐시 재제출로 위장하지 않았다.
- RTX3060 실측/Windows Player 빌드·실물버튼/RFID/장시간 검수는 미수행. 이전 SDXL-Turbo의3060 5~12초 추정을 이 모델의 측정처럼 재사용하지 않는다. 현재도3060 예열 후10~15초 목표는 남으며 새 PC에서20회 실제 측정해야 한다. 모델manifest의target_gpu_timing_verified=false 유지.
- model.json/text_encoder.json/vae.json/background_model.json의 고정revision/SHA로 설치. Klein FP8/Qwen encoder는 기존 설치본을 실제 해시 대조했고, Apache2.0 출처의 Klein VAE168MB만 새 이름으로 추가/크기·해시 확인했다. 원래Flux2-Dev 묶음 VAE나 기존 모델은 삭제/교체하지 않았다. 앱이 해시 이름 스타일PNG를 자동 검사/업로드하며 서버충돌 시 덮어쓰지 않고 실패한다. 설치·운영은 [COMFYUI_SETUP](../../COMFYUI_SETUP.md).

## 자료

- verification.json/verify_final.py, visual_acceptance.json, final_accepted/strict_unity_framing.json, editmode_results.json, prompt_parity.json/unity_prompts.json, runtime_success.json/runtime_fixtures.json.
- final_accepted/report.json의 요청ID/전체API해시/PNG해시/선택/시간 및 원본·framed PNG. 이전 sweep1/coverage1/matrix_v2/final_matrix/accepted_matrix/neutral_final/gray_reference_probe 등은 불채택/수정 중 기록이며 현재 승인 표본으로 혼동하지 않는다. before/*는 이번 작업 시작 직전 미커밋 상태 복사본이다.
- 원본스타일PNG/GUID·원판제품블록·씬·패키지 불변. 구조 검사와 시각 판단은 서로 다른 검증이며 모든 모델 출력의 완전한 자동 검수라고 표현하지 않는다.

## 연령별 대표 형태

| 물건 | 아동 | 학생 | 직장인 | 시니어 |
|---|---|---|---|---|
| 가방 | 곰 모양 유치원 배낭 | 학생용 책가방 | 서류가방 | 크로스백 |
| 신발 | 아동용 장화 | 하이탑 운동화 | 로퍼 | 편안한 워킹화 |
| 모자 | 유치원 모자 | 비니 | 볼캡 | 중절모 |
| 선글라스 | 하트 모양 선글라스 | 스포츠 선글라스 | 사각 선글라스 | 원형 선글라스 |
| 헤드폰 | 고양이 귀 헤드폰 | 게이밍 헤드셋 | 노이즈 캔슬링 헤드폰 | 가벼운 온이어 헤드폰 |
| 상의 | 아동용 티셔츠 | 후드티 | 셔츠 | 카디건 |
| 자전거 | 보조바퀴 자전거 | 로드 자전거 | 접이식 자전거 | 바구니 생활 자전거 |
| 텀블러 | 양손잡이 유아 컵 | 빨대 텀블러 | 커피 여행 머그 | 컵뚜껑 보온병 |
| 손목시계 | 아동용 시계 | 스포츠 시계 | 스마트워치 | 아날로그 시계 |
| 우산 | 돔형 아동 우산 | 접이식 우산 | 장우산 | 전통 양산 |
