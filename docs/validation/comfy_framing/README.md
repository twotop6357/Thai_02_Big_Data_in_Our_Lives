# COMFY-FRAMING-001 — 2026-10-30 Asia/Seoul

기준 main/37723c7 + 기존 미커밋 보존. Codex 단독 구현/검증, 교차검증 없음. 요청:480×480 이미지 안의 실제 제품을 정중앙에 배치하고 측면 고정을 해제.

## 변경

- ComfyImageClient.Download에서480 RGBA 검증 후 가시 알파8이상의 경계를 구해 캔버스 중앙(240,240)에 맞춘다. 소수 래스터는 최대0.5~1px 허용. 최소24px 투명 여백을 위해 필요한 경우만 비율을 유지해 축소하며 작은 그림은 확대하지 않는다. 투명 RGB를 섞지 않는 premultiplied bilinear 방식, 원본 배열 미수정.
- 순수 배열 처리는 취소 토큰과 작업 스레드에 연결한다. 빈/거의 투명한 결과는 실패, 예외/취소 시 다운로드 Texture 해제, 성공 Apply(false,true)로 읽기 메모리 해제. 서버 원본파일은 유지하며 Unity 수신 최종 이미지에 적용한다.
- 신발 side view를 natural three-quarter view로, 자전거 side three-quarter를 natural three-quarter로 변경하고 전체 스타일에 가로/세로 중앙·균등 여백·앞/옆이 보이는 자연스러운 시점을 지정. 제품/색 코드/투명배경·생성 입력잠금·Result 소유권은 유지.

## 실제 검증

- editmode.json: Unity2022.3.62f3,58/58 PASS. 새 중앙보정9개: 서로 다른 가로/세로/홀수 경계, 비율축소/여백, 알파1 노이즈, 투명 RGB 가장자리, 빈/잘못된 입력, 취소, 반복. 기존 블록/보관정책/버튼49개도 같은 실행에서 PASS.
- Python workflow7/7 PASS:1440선택 조합/현재Builder=배포API/팔레트/알파 연결 확인. 새 워크플로/API를 같은 설정으로 동기화했다.
- runtime.json: 실제 Unity Play/ComfyUI0.42.0,8/8 PASS. 생성17.8371초, 가시 경계 [50,121,430,358](상한exclusive)·중심(240,239.5), 최소24px 여백·모서리4곳 알파0. 생성/중앙보정 동안 버튼잠금, 같은 최종 Texture Result 전달 및 홈/타이머 복구.
- generated_product.png/selection.png는 실제 앱이 수신/보정한 그림과 GameView. 직접 시각 확인. 단일 제품·세부 스타일/시점/색 준수는 이전 작업과 마찬가지로 생성 모델 특성상 전 시드 보장 아님(이번 샘플에 복수 신발 형태 포함). 이번 통과 범위는 중앙배치/여백/투명도·연결과 측면 강제조건 제거다.
- existing_images.json: 이전 실생성 PNG2개를 실제 C# 보정으로 재처리, 각각 중심(240,240), 비율/안전여백 확인.
- preservation.json: 기존 View/BlockExperienceController/GameManager·씬3개·패키지2개 총8개 SHA256 모두 동일. 기존 미커밋/다른 업무 파일 보존.

최종 Play 종료/0_Idle·dirty false, 임의 커밋/push·패키지 변경·PC 종료 없음. Windows Player/실물버튼/RFID·전체품목/시드 품질·장시간/Profiler 미수행. raw server output은 중앙보정 이전이며 앱의 final image가 이번 요구사항 적용 대상이다.
