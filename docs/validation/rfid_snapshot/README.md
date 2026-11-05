# RFID-SNAPSHOT-001

2026-11-02 Asia/Seoul · Codex 단독 · main/3852ee6+기존 변경 보존 · 미커밋.

이미지 생성 시작부터 다운로드/최종 배치가 끝날 때까지 리더4개의 선택을 고정한다. 인식 해제·교체·이동·일반 전체비우기를 무시하고 버리므로 화면/선택/revision/변경 이벤트가 그대로 유지된다. 성공·실패·명시적 취소·타임아웃·화면 종료에서 잠금을 해제하고, 무시했던 신호를 뒤늦게 재생하지 않는다. 그 이후 새로 들어오는 RFID 신호는 정상 처리한다. 홈 복귀는 강제로 모든 리더를 초기화한다.

## 실제 검증

- Unity2022.3.62f3 컴파일 정상. 신규3사례 포함 EditMode91/91 PASS, job0a9528588f994fd5b73bd14054e90a33,13.8610984초. 고정 중 제거/교체/이동/clear·revision/구성 보존·재생 없음, 이전 lease 중복 해제와 새 요청 소유권, 홈 강제초기화 확인.
- [runtime.json](runtime.json): 실제 앱45항목 PASS. 실제 ComfyUI 생성 중4리더 해제/교체·직접 모델 제거/이동/clear를 보내도 이벤트0/revision 동일/양패널8이미지 동일/생성·로딩·타이머 잠금 유지. 실제 성공 후4블록/결과 유지, 새 신호만 정상 반영.
- 로컬 지연/400 응답으로 명시적 취소·시간 초과·실패·이전 finally/재요청·Controller 비활성/재진입·홈/다음 세션 초기화까지 확인했다. 의도한 HTTP400의영문 오류/원래예외2로그를 보존했으며 그 외 오류는 발견하지 못했다. 지연/400 검사는 새 AI 이미지 품질 검수가 아니다.
- [생성 중 실제 화면](generating_snapshot.png), [완료 후 실제 화면](success_snapshot.png)을 직접 비교했다. 네 블록은 생성 시작 때와 같다.
- [preservation.json](preservation.json): 원본 블록 PNG/meta·씬·패키지·입력·생성 설정82개 SHA 동일. 범위 diff-check 정상. Play중지/0_Idle·dirty false, 소유한 fixture만 종료.

재현은 Idle에서 Play한 후 [로컬 fixture](../../../Tools/UI/block_loading_fixture.py)를127.0.0.1:18191로 실행하고 [verify_rfid_snapshot.cs](../../../Tools/UI/verify_rfid_snapshot.cs)를 Unity execute_code로 명시 실행한다. 실제 장비의 인식 해제 대신 같은 공통 Hub API를 호출했다. 실물 RFID/Windows Player·장시간 검수는 미수행이다. 기존 로딩 검증 도구는 새 규칙으로 기대값만 갱신했고 이번에 전체 재실행하지 않았다.
