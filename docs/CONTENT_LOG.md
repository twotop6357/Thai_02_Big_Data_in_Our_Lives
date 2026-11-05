# 콘텐츠 운영 로그

LOG-003 /2026-11-02. `Assets/StreamingAssets/Settings.json`의 `apiUrl`에 사용자가 지정한 HULIAC content_log 주소를 적용했다. 주소의 기존 장비/UID 쿼리를 유지하고 `message=` 뒤에 메시지 전체를 URL 인코딩하여 GET으로 보낸다. 응답을 받아 HTTP 성공 여부를 확인한다.

| 상황 | 메시지 |
|---|---|
| 프로그램 시작 | `start` |
| 정상 종료 | `end` |
| 실행 중인 Windows Player 강제 종료 | `end_kill` |
| 수동 홈 복귀 | `move_idle` |
| 무입력으로 홈 복귀 | `move_idle_timeout` |
| 외부 콘텐츠 API 호출 | `Call API ` + 주소와 파라미터 |
| 외부 콘텐츠 API 성공 | `Return OK` |
| 외부 콘텐츠 API 실패 | `Return fail` |

같은 날 재실행에도 `start (restart)`를 쓰지 않고, 종료 이유/실패 사유 접미사를 붙이지 않는다. 프로그램 시작 자체는 start이며 이미 Idle인 상태의 타임아웃은 화면 이동으로 기록하지 않는다. GameManager의 기존 수동 MoveIdleEvent와 공통 InactivityTimeoutEvent를 사용해 타임아웃에 move_idle을 중복 발행하지 않는다.

`Network.APIManager`는 기존 ApiManagerBase의 영속 매니저 중복 방지·DI·구독 및 wantsToQuit 종료 장벽을 보존한 프로젝트 파생 구현이다. 종료 시 최대5초 안에서 end 전송을 시도하고 성공/실패와 관계없이 종료한다. 일반 전송도 한 시도3초·최대2회/.5초 간격으로 제한하고 순차 처리한다. 기록 서버 자체의 실패로 콘텐츠 API의 원래 성공/예외를 바꾸지 않는다. 망 장애/응답 손실에서 전송이나 서버의 정확히 한 번 저장을 보장하지 않으며 별도 오프라인 저장 큐는 이번 범위가 아니다.

Windows Player는 창 없는 PowerShell 감시 프로세스1개를 시작한다. 지정한 PID와 시작시각을 확인해 앱 프로세스가 끝날 때까지만 기다린다. 정상 종료 표시가 있으면 지우고 종료하며, 표시 없이 프로세스가 사라지면 end_kill을 전송하고 종료한다. 이 감시 스크립트에는 앱/PC를 종료시키는 명령이 없다. PC 종료 예약과 모든 요일 비활성 설정도 바꾸지 않았다. OS/전원 자체가 사라지거나 감시 프로세스까지 함께 죽으면 즉시 전송할 수 없다.

ComfyImageClient/서버 probe/업로드/다운로드에는 외부 콘텐츠 API 로깅을 연결하지 않는다. 로그 헬퍼도 localhost, 현재 설정된 Comfy 서버의 origin(원격 IP 포함), Comfy 호스트명, content_log 자체를 제외해 Call/Return 둘 다 남기지 않는다. 나중에 외부 콘텐츠 API를 추가할 때는 구체 타입 `Network.APIManager.ExecuteWithExternalApiLoggingAsync`로 요청 전체를 감싼다. 일반 URL/파라미터는 보존하고 인증정보와 api_key/token/password/secret 등의 값은 로그에서 가린다. 기존 MessagePipe Call/Return 이벤트는 순서가 짝을 이루는 호출을 지원하며, 동시 요청에는 요청별 헬퍼를 사용한다.

에디터/Development Player는 운영 서버 전송을 생략해 반복 테스트가 운영 로그를 채우지 않게 한다. 로컬 수신기 주소는 검증용으로 실제 전송한다. 배포 빌드는 실제 지정 서버로 전송한다. 실제 서버 연결 확인용 start/end는 명시적으로 전송해 HTTP200 및 `ok:true` 응답을 확인했다. [검증 기록](validation/content_log/README.md)에 실제 전송 시험과 로컬 계약/Player 시험을 구분한다.
