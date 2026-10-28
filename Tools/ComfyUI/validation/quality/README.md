# QUALITY-001 검증 (2026-10-19 / Codex)

현재 PC: RTX5070 Ti Laptop12GB / ComfyUI0.37.0 / PyTorch2.12.1+cu130 / Unity2022.3.62f3 Editor. RTX3060·Windows Player·장시간/전 시드 품질 검수는 미수행이다.

![품목별 실제 결과, 알파 확인용 체크무늬 합성](gallery.png)

체크무늬는 검증용 합성 배경이다. 개별 PNG는 실제 RGBA 원본이며 체크무늬를 포함하지 않는다. 원본 예: [가방](bags.png), [선글라스](sunglasses.png), [헤드폰](headphones.png), [시계](wristwatches.png), [텀블러 재시험](tumblers_runtime.png).

- [report.json](report.json): 최종 구조 제어1.4의14개 세부품목(제품11+Equipment3), 정확한 그래프/시드/서버ID·관찰·알파와 과거 실패 실험 요약. 직접 API 요청→PNG31.392~44.854초, 중앙38.059초. 워밍업20회 성능 승인 시험이 아니다.
- 14개 원본 모두480×480 RGBA, 알파0~255 및 모서리4개0 확인. 각각 단일 제품 구도이며 사람 신체는 관찰되지 않았다. 이 샘플에 대한 관찰이며 모든 시드에서의 보장은 아니다.
- 텍스트만 적용하거나 약한 제어(.85)를 사용한 중간 시험에서는 헤드폰/로봇 오인식·시계/신발/키보드 복수·잘림이 남아 최종 설정에서 제외했다. 원본 실패 파일은 로컬 ignored results에 보존하고 재현용 서버ID/시드/시간은 report에 남겼다.
- 최초 텀블러는 캔처럼 나와 탈락. travel mug와 lid 묘사 및 캔 금지 조건으로 재시험한 tumblers_runtime.png는 캔 무늬가 제거된 단일 금속 원통이다. 뚜껑의 세부 표현은 단순하여 추가 미술 검수가 필요하다.
- 색 지정은 두 부분에 적용하지만 추가 장식색이 남는다. 자전거 페달/스포크·로봇 관절·키보드/노트북 키·작은 문양은 단순화/오류가 남는다. 휴대폰 케이스/노트북 일부에는 그림자처럼 보이는 색면이 전경에 남는다. 반투명 렌즈·얇은 끈/프레임의 알파는 전 품목/색/시드 검수가 필요하다.
- [unity_client.json](unity_client.json): 실제 서버 가이드 SHA확인 및 RGBA 다운로드, GPU readback 모서리alpha0/중앙약.996. 격리localhost18188 fixture에서404→업로드→재확인, 다른 내용 거부, RGB거부, 시작 전/진행 중 취소, 닫힌 포트 실패 확인. fixture 서버 종료, 실제 ComfyUI 유지.
- [unity_flow.json](unity_flow.json): 학생/텀블러/파랑·흰색 실제 생성 약43.169초. Game 이미지 할당/표시, 매니저 소유권, 짧은 버튼 API 입력→2_Result 동일 텍스처, 수동 복귀→0_Idle 텍스처 파괴/언어 초기화 확인. 물리 키보드/아두이노 입력은 이번 시험에서 사용하지 않았다.
- [runtime_prompt.json](runtime_prompt.json): Unity 재시험의 정확한 그래프와 서버 파일. 기존 제한 보관 prefix 사용. API 품질 시험의 quality_probe 파일은 유한한 개발 샘플이며 런타임 정리 대상이 아니다.
- Python offline5개(전체1728선택 조합 포함) 통과. Unity EditMode33/33, 컴파일/Play Console error0 확인. 기존 자동실행/이미지 보관/Result 씬 기능 보존. 테스트 중 임시로 무입력 타이머를 멈췄고 Play 종료로 테스트 상태를 폐기했다.

ComfyUI 사용자 워크플로에 Thai_02_product_480_quality_alpha.json을 새 이름으로 저장했다. 이전 워크플로는 보존했다. 운영 설정·모델 설치/라이선스는 [COMFYUI_SETUP](../../../../docs/COMFYUI_SETUP.md)을 따른다.
