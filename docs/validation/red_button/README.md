# RED-BUTTON-001

2026-11-02 Asia/Seoul · Codex 단독 · main/3852ee6 · 미커밋.

Input Manager의 RedButton(사용자 설정: joystick button 0)과 숫자1/숫자패드1을 기존 KeyboardButtonSimulator에서 병합한다. 모두 놓였을 때만 짧은 입력을 판정한다. 사용자 InputManager 설정은 편집하지 않았다.

## 검증

- Unity2022.3.62f3 컴파일 정상. ExhibitionButtonGestureTests13/13 PASS, job c4438b274396448a929e45a3c26ef6f9,0.4864376초. 짧은/2초/5초·정확한 경계·reset/연속/씬 변경 판정 회귀.
- [runtime.json](runtime.json): 실제 Play 어댑터에 InputSystem 가상 키보드를 연결해11항목 PASS. 두1키로 각각 시작, 겹친 키의 부분 해제/최종 해제, 포커스 상실 시 취소,5초 홈,2초 언어 전환·복원,Idle 정리. 기존 Input.GetButton("RedButton")은 휴지 상태에서 정상 읽기를 확인했다. 물리 조이스틱 누름은 검증하지 않았다.
- 재현: Idle에서 Play한 후 [verify_red_button.cs](../../../Tools/Input/verify_red_button.cs)의 내용을 Unity execute_code로 실행한다. 약15초 후 runtime.json 확인. 기존 언어/InputSystem 설정·runInBackground를 복원한다. 정상 앱 시작 시 실행되지 않는다.
- 최종 콘솔 오류0, 매니저1개, Play중지/0_Idle·dirty false. 변경 범위 diff-check 정상. 사용자 InputManager SHA256 시작/끝: F2A9E886ADB17E81298A722445C1A0E388DCE003B44E07C7C81D610D89366D2A.

첫 Play에서는 비주입 매니저/루트 중복이 관찰되어 준비 대기 중 검증을 중지했다. 당시 컴파일/도메인 재로드도 관찰했으나 중복 원인은 확정하지 않았다. 별도 DI 변경 없이 새 Play에서 전체 검증을 통과했다. 실제 RedButton 장비 입력·연결 해제와 Windows Player는 설치PC 후속이다.
