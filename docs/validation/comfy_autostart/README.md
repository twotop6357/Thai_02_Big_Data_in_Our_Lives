# COMFY-AUTOSTART-002 — 2026-11-02 / Codex 단독

main/3852ee6 위의 기존 미커밋 변경을 보존했다. Comfy Desktop을 호출하는 대신 기존 Python/main.py 직접 실행 경로를 실제 확인했다. launchServerInEditor=true, 설정+서버 준비를 결합한 GameManager 입력 게이트, 타이틀 idle.help Image 비활성화를 적용했다. UI 초기화가 루트 매니저보다 빠른 경우를 실제 테스트에서 발견해 매니저가 UI에 명시적으로 연결되도록 수정했다. 패키지·모델·씬·입력 매핑·원본 UI를 변경하지 않았다.

- EditMode: 91/91 PASS, job c4633c284c3749bca0c88330aa33ae8f, 14.4441981초.
- [gate.json](gate.json): 실제 Editor Play 21 PASS. Checking/Starting/Unavailable에서 실제 키보드 어댑터의 숫자1/숫자패드1 및 직접 어댑터 API 차단, 2초 유지 무효, 준비 전 누름/해제 무효, 새 누름 진행, 언어 왕복, 비활성/재활성, 홈 재진입. 서버 상태는 명시적 테스트 주입이며 물리 RedButton 장비 검수는 아니다.
- [title_waiting.png](title_waiting.png), [title_ready.png](title_ready.png): 버튼 안내 숨김/표시를 직접 시각 검토했다.
- [build.json](build.json): 실제 3씬 Windows Mono 빌드 성공, BuildOptions.None, 오류0·경고0. 출력은 프로젝트의 Builds/ComfyAutostart/Thai02.exe. 생성물은 ignored이며 이 폴더 전체를 유지해 실행한다.
- [player.json](player.json): 배포 빌드를 실제 실행해11항목 PASS. 비어 있는18194 포트에서 Python 서버 직접 실행·Player→가상환경→백엔드 부모 관계·ComfyUI0.42 응답·127.0.0.1 바인딩·앱 종료 후 서버 생존·재실행의 동일 서버 재사용·없는 실행 파일의 실패 후 앱 생존·Desktop 신규 프로세스0을 확인했다. 테스트용 빌드 사본의 JSON만 포트를 바꾸었고 종료 후 원본8188 설정으로 복원했다. Player의 TCP listen/UDP 포트는 모두0. cold 준비27.229초는 이 PC의 서버 시작 시간이며 이미지 생성 시간/3060 측정이 아니다.
- 개발 빌드에서도 앞선10항목 PASS(player_development.json, cold17.600초). 개발 빌드 네트워크 알림은 사용자가 직접 허용했다. 방화벽 설정·관리자 권한·보안 프롬프트를 자동 조작하지 않았다. 배포 빌드에서는 개발 네트워크 연결을 제거했으며 다른 PC의 OS 알림을 무조건 허용한다고 보장하지 않는다.
- 별도 Python 서버18192에서 실제 Klein 이미지1개 생성/480PNG 완료11.936초. 임시 결과는 Temp/ComfyAutostart/generation이다. 이는 새 품질/성능 승인이 아닌 서버 기능 확인이다. 검증용 Python21500/48568 및18194 테스트 서버/Player는 정리했고 기존8188 PID17356은 보존했다.
- 마지막 배포 GUI에서도 실제 타이틀/준비 후 안내를 직접 확인하고 TCP/UDP listen0, 기존8188 재사용 로그를 확인했다. Computer Use 숫자1 입력으로 진입 성공은 확인하지 못했고, 추가 숫자패드 검수는 사용자의 물리 Escape로 중단했다. 이후 Computer Use 입력은 수행하지 않았다. 물리버튼/RFID·전체 Player 흐름·다른 PC/3060/장시간 검수는 후속이다.

재현 도구: Tools/ComfyUI/build_player.cs(Unity Edit 실행), verify_startup_gate.cs(Idle Play 실행), verify_player_startup.ps1(Windows 빌드 서버 시험). 테스트 원시 로그는 *.log로 ignored이며 JSON/그림만 추적한다. Input System 빌드가 디스크에 추가한 preloadedAssets action 참조1개만 종료 시 제거해 원래 목록으로 복구했다. 초기 테스트 도구의 공유 로그 읽기/오래된 로그 판독 문제는 도구에서 수정한 뒤 재실행해 최종 통과했다.
