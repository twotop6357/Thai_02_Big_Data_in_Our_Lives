# LOG-003 — 2026-11-02 / Codex 단독

사용자가 지정한 content_log.cfm URL을 Settings.json.apiUrl에 연결하고 프로젝트 APIManager에서 정확한 메시지 규칙을 적용했다. 원본 ApiManagerBase의 DI/영속 중복 방지/OnEnable/OnDisable/정상 종료 장벽을 보존하며 PackageCache를 수정하지 않았다. Comfy 통신은 계측하지 않고 헬퍼/기존 이벤트에서도 로컬·설정 origin·Comfy 호스트/로그 API 자체를 제외한다. 민감한 URL 인증 값은 가린다.

- [contract.json](contract.json): 실제 Editor Play 로컬 수신15 PASS. start/end 접미사 제거, 실제 Game→Idle 수동·타임아웃 각각1회/중복없음, 이미 Idle인 타임아웃 무시, 외부 콘텐츠 API의 성공/실패·원본 예외 보존, URL 파라미터 인코딩, 인증 값 실제 wire 가림, 기존 이벤트, 로컬/원격IP Comfy와 실제 system_stats 통신 제외, HTTP503의 제한된 재시도/콘텐츠 계속 동작.
- [player.json](player.json): Windows Development Player 실제실행5 PASS. 정상 Application.Quit은 정확히 start,end 후 종료하고 end_kill 없음. 테스트 Player만 강제 종료하면 독립 Windows 감시 프로세스가 start,end_kill을 기록. Comfy 시작/probe의 Call/Return 없음. 정상 종료 전용 CLI 검증 플래그는 Development+loopback일 때만 작동하며 배포 빌드에서 제거된다.
- [release_player.json](release_player.json): 최종 Release Player에서도 로컬 수신 start,end_kill 실제 확인. 빌드 사본의 apiUrl만 임시 로컬 주소로 바꿨고 시험 후 실제 운영 주소로 복원했다. 감시 스크립트가 빌드에 포함되고 source와 SHA가 같음을 확인했다.
- [production_transport.json](production_transport.json): 사용자가 지정한 실제 서버에 연결 시험으로 start/end를 GET 전송해 HTTP200 확인. 응답 본문 확인용 추가 end1건에서 [production_acknowledgement.json](production_acknowledgement.json)의 message=logged,ok=true,message_text=end를 확인했다. 이는 직접 전송 시험이며 Player lifecycle을 운영 서버에서 검증한 기록은 아니다. 대량/실패/강제 종료 시험은 로컬에서만 수행했다.
- [build_development.json](build_development.json), [build_release.json](build_release.json): Unity2022.3.62f3/Windows64, 실제 등록3씬 빌드 성공. 최종 BuildOptions.None, 오류0·기존 소스 경고5개. 출력 Builds/ComfyAutostart/Thai02.exe와 동봉 폴더를 함께 사용한다.

Source APIManager·Settings.json·StreamingAssets/ContentLog/WatchProcess.ps1(+meta), Launcher의 읽기 전용 ServerUrl, Tools/Logging 로컬 수신기/계약/Player 검증 도구를 추가/수정했다. 감시는 해당 PID+시작 시각의 앱이 끝날 때까지만 기다리고 정상 표시가 없으면 end_kill을 전송한다. 앱/PC를 끄는 명령이나 방화벽/UAC 설정 변경은 없다. 운영 기능과 제한은 [CONTENT_LOG](../../CONTENT_LOG.md)에 있다.

로컬 수신기는127.0.0.1:18196, PID51692로 실행했고 검증 후 소유한 프로세스만 정리했다. 원시 요청은 Temp/ContentLog/requests.jsonl, 원시 *.log는 ignored다. 상태 전환 중 Unity 연결 포트가 다른 프로젝트 실행에 따라6400/6401로 재배정되어 일부 도구 응답 대기를 중단하고 현재 프로젝트 exact Name@hash로 다시 고정해 마무리했다. 직전 포트에 보낸 Stop Play 요청 하나는 응답이 없어 실행 여부를 확인하지 못했다. 그 뒤의 빌드 코드는 해당 중단된 호출에서 실행하지 않았고, 다른 프로젝트의 파일을 수정하지 않았다. 최종 빌드는 현재 프로젝트 이름/해시를 명시해 실행한 결과다.

미검증: 운영 서버 관리 화면에서의 저장 목록, 설치PC·실물 입력·장시간, 네트워크 단절/PC 전원 상실에서의 전달 보장. 네트워크 실패 시 일반2회·종료5초로 제한하며 전원/OS가 사라지거나 watcher도 함께 종료되면 즉시 강제종료 로그를 보장할 수 없다. 미커밋, 교차 검증/커밋/push 없음.
