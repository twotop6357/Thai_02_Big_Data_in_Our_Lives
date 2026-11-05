# RFID-STABLE-001 / 2026-11-02

Codex 단독 구현/검증. main/3852ee6+이전 미커밋/Claude RFID-LISTEN·PRESENCE 보존, 교차 검증 없음. Unity2022.3.62f3 / Windows.

## 원인

사용자가 LAN-754(KA118)와 실제7바이트 반복을 확인했다. 기존 `FixedLength16`은 실제 보고의 경계와 무관하게16바이트마다 잘랐다. 같은7바이트를 연속 전송하면 매번 2바이트씩 시작 위치가 밀려7종류의 잘못된16바이트 UID가 순환한다. `before_seven.json`은 실제 사용자 UID 대신 같은 구조의 가상 값으로 실행한 재현이다. `before.json`은 그 전에 실시한16+CRLF 보고의 오정렬 가설 실험으로 최종 실제 길이 확인과 구분한다.

이 PC의10123 포트는 검사 시작 당시 열려 있지 않았다. 허용된 IP4개만 받는35초 임시 수신에도 실제장비 접속/데이터0개였다. 현장 실측 대신 사용자 제공의 반복 형태를 재현한 것이며, 실제 장비 통과로 보고하지 않는다. 제조사 공식 자료에서 해당 펌웨어의 UID 필드/패킷 계약을 확보하지 못했으므로 임의로 앞/뒤 바이트를 제거하지 않았다. 실제 UID는 문서/외부 운영 로그에 저장하지 않았다.

## 구현

- `RfidWireFormat`과 배포JSON 기본값 `UidBurst`. `fixedFrameBytes`는 이 모드에서 무시하므로7이나16에 고정하지 않는다.
- 기존 TCP 버퍼가 수신 조각을 모으고 마지막 바이트부터100ms 공백에서 완성 묶음. 그 묶음의 정확한 반복 단위를 단일 값으로 사용한다. 비반복값은 전체 보존, 선행0/바이트순서/RFID IP와 매핑 목록 보존.
- 새 단일 값은2회 일치해야 노출한다. 묶음 안에 같은값2회면 바로 확인된다. 재접속/제거는 확인 상태 초기화. 기존1초 Presence·그룹우선·F10저장잠금·생성snapshot은 유지.
- `Raw`는 중복 포함 묶음, `UID`는 확정 단일값. F10에 UID 바이트 수를 표시한다. 잘린16바이트의 기존 등록값은 임의 변환하지 않으며 현장에서 올바른UID로 다시 등록한다.

## 실제 검증

- `editmode.json`: 전체 EditMode130/130 PASS,14.1346326초, job efedabf7bafa4fbdb83925096c579d96. RFID39개 별도 실행도 PASS. 기본/유효성, 반복7바이트의1/2/3/8회, 모든TCP분할 위치13개,4/7/10/16바이트·선행0·교체·과대묶음폐기/회복·재접속확인을 포함한다.
- `burst/runtime.json`: 실제127.0.0.11~14 출발지 TCP클라이언트4개/Unity서버,16검사 PASS. 7/4/10/16바이트·1/2/3회 중복/분할을 섞어5.5초 이상 반복,5개 이상1초검사에서 값/선택 revision 동일. 바뀐7바이트 즉시구별, 미등록값, 저장, 생성고정/저장금지/지연재생없음, 제거, 재접속2회 확인. `burst/f10_panel.png` 직접 확인.
- `legacy/runtime.json`: 기존 서버/4IP/미등록IP·점유복구/F9/F10/매핑저장·교체·삭제/JSON오류/Presence/생성잠금/재접속 및 명시FixedLength·Delimiter·IdleGap 호환67검사 PASS. 이전 검증자료를 덮어쓰지 않고 새 폴더에 저장. 실패주입 경고/예외는 예상된 경로이며 MCP 중복명령 방지 경고도 별도 발생했다. 빌드후 최종콘솔 조회오류0.
- `build.json`: Windows64/3씬/BuildOptions.None Succeeded, 오류0·기존경고5,9.7660257초. `Builds/RfidStable/Unity-Startup-Template.exe`. InputSystem 임시 preloaded 추가는 기존 VContainerSettings1개로 복원. Play중지/Idle/dirtyfalse 확인.

테스트 도구는 `Tools/RFID/verify_uid_burst.cs`와 기존 `verify_tcp_mapping.cs`를 Play에서 execute_code로 실행한다. 파일 저장은OS Temp에만 수행하고 finally에서 원래 설정/경로/Presence/키보드/포커스/선택을 복원한다. 실제카드등록/리더설정/방화벽/PC종료/원본씬 수정 없음. Comfy 새이미지생성은 이 작업에서 하지 않았다.

## 배포

Desktop/Thai_02의 Mono DLL2개를 위 성공빌드의 파일로 교체하고 RFID JSON의 framing만 바꿨다. DLL/JSON 원본은 RFID_Backup_20261102_202135_210에 보존했다. `deployment.json`: EXE/UnityPlayer·일반설정·Comfy설정/카탈로그/워크플로·Presence7파일SHA 동일, 등록매핑/IP 등은 framing 외 동일. 실제모델/런타임은 건드리지 않았다. UnityPlayer SHA가 같은 기존2022.3.62f3 빌드에만 업데이트가 적용된다.

`Builds/RFID_Update.zip` 및 `Desktop/Thai_02/RFID_Update.zip`은98,252바이트. 기존빌드 실행파일 폴더에 압축풀기→완전종료 상태에서 Apply_RFID_Update.cmd 실행→재시작. Windows 기본PowerShell5.1로 실제적용/내용검사 PASS. DLL2개/도구/README만 포함, JSON은 현장기존값을 읽어 framing만 변경해 카드 목록을 보존한다. 이미 실행중이거나 잘못된경로/런타임/파일누락이면 적용 전 거부, 쓰기 실패 시 원본3파일 복원.

`update_fixture.json`: 별도 Temp 빌드에 등록카드2개·기본아닌포트·추가현장필드를 넣고 WindowsPowerShell5.1 업데이트 실행, framing 외 의미상 완전동일 확인. 실제 Desktop의빈cards목록만 보존한 검사와 구별한다.

중간 배포도구 검증: Tools폴더에서 잘못 실행한 것은 payload누락 사전검사로 거부했다. 첫 실제적용에서는 PowerShell의 null문자열 변환으로 File.Replace가 실패해 원본복원했다. 백업경로를 명시하도록 고쳐WindowsPowerShell5.1 재적용/7보호해시/매핑보존/ZIP2DLL SHA를 확인했다. 실패본 ZIP은 최종수정본으로 교체했다.

## 한계와 현장 조건

UidBurst는 **보고 묶음 사이 수신 공백**이 있어야 한다. `idleGapMilliseconds`는 TCP조각간 최대지연보다 길고 실제보고묶음간 공백보다 짧아야 한다. 테스트는 조각25~70ms/묶음간160ms 이상으로 실시했으며 이것을 실제장비간격으로 간주하지 않는다. 무공백 영구스트림·서로다른카드가 한 묶음에 섞임·UID자체가 더짧은패턴의 반복인 특수값은 원시스트림만으로 완벽하게 구별할 수 없다. 그 장비모드는 제조사 프로토콜의 구분자/헤더·길이 규칙을 명시해야 한다(`Delimiter` 등). 현재는 사용자 확인의7바이트 반복과 같은 입력형태를 검증한 것이다. 512바이트 자원상한/초과묶음폐기는 유지한다.

실제 LAN-754/KA118 네대·실제보고/공백간격·정확한UID필드·현장Windows Player직접화면·장시간·3060 새PC는 미검증. 향후모든장비/스트림에 무조건 정상이라고 보장하지 않는다. 사용자요청의TCP서버/클라이언트방향과최초16바이트오정렬은 이번수정으로 재현/해소했으나, 현장간격검수가 남아있다.
