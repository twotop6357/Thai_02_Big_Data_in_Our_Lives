# AD-FX-004 — 2026-11-02 / Codex 단독

main/3852ee6+기존 미커밋 변경 보존. 광고 효과는 원본 프리팹을 수정하지 않고 인스턴스에서 설정한다. Controller의 유효한 결과 revision에서 생성에 사용한 카탈로그 HEX2개를 Texture와 함께 GameManager에 넘긴다. 별을 균일 원 배치에서 무작위 위치/크기/수명으로 바꾸고 파동 방출 간격·크기·색 및 빛살의 느린 회전을 분산했다. 전체 화면 컨페티도 양쪽 지연과 상단 시작/방출량을 분산했다.

- [runtime.json](runtime.json): 실제 새 이미지3개(학생 가방 빨강/파랑·모자 초록/노랑·신발 검정/흰색),39항목 PASS. 생성 HEX 전달·두 색 혼합·이후 RFID 무영향·물건 최전면·TimeScale0·별 정점/파동 정점의 광고 박스16px 내부 유지·가변 간격/입자 색상·컨페티 완료/입력 복구·홈 색상 초기화.
- [lifecycle.json](lifecycle.json):8 PASS. 이전 PNG를 결과 bridge fixture로 명시적으로 사용해 비활성·취소·중복 진입·아우라만 재개·영상 암전/효과 종료·디코딩 첫 프레임0·홈 정리 검증. 새 생성 품질 검사가 아니다.
- [confetti_variation.json](confetti_variation.json): 마지막 전체 화면 컨페티 분산 후 새 가방1개 생성으로14 PASS. 세 emitter 지연 독립성과 정상 종료/입력·내부 효과 회귀 포함.
- bags/hats/shoes의 entrance/ambient PNG6개 직접 시각 확인. bags의 최종2장은 컨페티 분산 이후 새 생성 결과이며 이전3색 시험을 덮어 쓴 runtime JSON은 아니다.
- 초기 테스트에서 루트 emitter2개 외에 중첩된 링이 있어 전체 자식3개를 캐시하도록 수정했다. 시각 확인 중 ParticleImage의 speed×100 환산/fitRect 자동 반지름 덮어쓰기를 확인해 낮은 이동 속도·fitRect 해제·반지름184로 수정하고 정점 검증을 추가해 재실행했다. 이 중간 실패는 최종 통과로 대체했으며 원본 vendor는 수정하지 않았다.

기존 EditMode91/91 PASS(job f4d32d1cdc65480f8771599125318926,10.4461981초). Windows 최종 배포 빌드는 [로그 작업 build_release](../content_log/build_release.json)와 함께 생성했다. 오류0, 기존 소스 경고5개(CS4014 4개/Editor 키보드 필드 CS0414 1개)는 그대로 남았다. 광고 화면의 Player 직접 육안·실물입력·장시간/모든 랜덤 결과 검수는 미수행이다. 실제 Play 검수와 Player 로그 종료 시험을 구분한다.

변경: AdProductHighlight·BlockExperienceController·GameManager, Tools/UI/verify_ad_fx4.cs·verify_ad_fx4_lifecycle.cs 및 문서. 3씬/프로젝트 효과 프리팹/vendor/패키지에는 Git diff가 없음을 확인했다. 기존 사용자 블록/입력/인트로 변경은 보존, 미커밋·커밋/push 없음.
