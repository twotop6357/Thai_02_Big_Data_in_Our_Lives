# RFID-TCP-MAP-001 검증 / 2026-11-02

Codex 단독 구현·검증, main/3852ee6+기존 미커밋 변경 보존. Unity2022.3.62f3 / Windows Editor. 다른 에이전트의 교차 검증 없음. 사용 방법: [RFID_SETUP](../../RFID_SETUP.md).

## 최종 결과

- `editmode.json`: 전체 EditMode112/112 PASS. IP/포트/중복 UID/제거값/범위 검증, HEX 선행0/불완전 값, TCP 분할·합침·구분자·공백·초과프레임·재접속, 실제16바이트 예시 전체 보존을 포함한다.
- `runtime.json`: 실제 루프백 TCP 서버4개를 사용한 Editor Play44항목 PASS. TCP 클라이언트로 독립 접속, 위치/색 순서·그룹 우선 및 재보고 무시, F10/F9/숫자1 입력 격리, 생성 FreezeInput 중 교체/제거/미등록/단절 및 저장 차단, 잠금 해제 후 재생 없음, 매핑 저장/교체/삭제·백업, JSON 실패 유지/재로드, 독립 재접속, FixedLength/IdleGap/Delimiter·ASCII 옵션, 명시적 타임아웃, 컴포넌트 비활성화/잠금 정리.
- 사용자 예시 `08 41 FF 54 33 01 4B 31 30 32 30 00 00 39 38 EF`는 분할 수신한16바이트 전체를 의미 해석 없이 식별값으로 매핑하는 테스트에서만 사용했다. 배포 JSON에는 예시 카드를 등록하지 않았고 `cards: []`다. 최종 기본값은 FixedLength16 / 바이너리 / UID전체.
- `f10_panel.png`: 최종 코드의 실시간 창 캡처 직접 확인. 그림의127.0.0.1과 테스트UID는 가상 장비다. 실제 배포는121/122/123/124:10123, 좌상/우상/좌하/우하 순이다.
- `build.json`: 실제3씬 Windows64 배포빌드 BuildOptions.None 성공. 최종 오류0·경고0,8.9479544초. `Builds/ComfyAutostart/Thai02.exe`와 동반폴더 사용. 동봉 RFID JSON과 원본SHA256 `8D41FCCAE4710EB8B8B76893BB82D9B8E816BCB2C9664DF22C290D5659E8031D` 일치.

## 실행과 보존

`Tools/RFID/verify_tcp_mapping.cs`를 Idle Play에서 Unity execute_code로 실행한다. 실제 TCP 소켓을 열되 Loopback만 사용하며, 저장 테스트는 OS Temp의 `Thai02-RFID-TCP-validation/RfidSettings.json`에만 쓴다. finally에서 원래 설정/경로·키보드·포커스·백그라운드·리더 선택을 복원하고 테스트 클라이언트/서버/러너를 정리한다. 실제 운영 카드나 로그 서버로 RFID 값을 보내지 않는다.

최종 검증 뒤 Play중지/0_Idle·dirty false. 씬·프리팹·원본아트·vendor·패키지 변경 없음. 빌드가 추가한 Input Actions preloaded1개만 원복하며 기존 VContainer preloaded/사용자 InputManager 변경은 보존한다.

## 실패 경로와 중간 수정

최종 Play의 의도한 실패 진단은 잘못된HEX, 미등록블록ID, 문법오류JSON 세 경우 각각 오류 메시지+원본 예외다. 콘솔의6개 오류는 이 실패 주입 결과이며, 실제 정상 수신/매핑 경로의 오류로 집계하지 않는다. 새 컴파일 후/빌드 직전 콘솔 컴파일오류0을 확인했다.

초기 F10 입력 코드에서 프로젝트 Exhibition.Input 네임스페이스와 Unity Input 이름이 겹쳐 컴파일오류가 발생했다. UnityEngine.Input 전체이름으로 수정한 뒤 최종112개 및 Play44개를 수행했다. 최초 빌드의 미컴파일변경 경고를 확인해 Editor refresh/도메인 재로드 후 다시 빌드했고, 최종 보고서는 오류0·경고0이다. 중간5개 기존CS4014/CS0414 경고가 프로젝트에 존재했으며 이 작업은 해당 코드를 변경하지 않았다.

## 남은 검증

실제 RFID 네 대·현장 망·Windows Player의 화면/마우스 직접조작·장시간 검수는 미수행. 생성 잠금은 실제 동일 FreezeInput lease를 TCP 입출력과 연결해 검증했으나 이 작업에서 새 Comfy 이미지 생성은 실행하지 않았다. 기존 생성 코드/워크플로는 변경하지 않았다.

특히 장비가 카드 제거를 어떤HEX로 보내는지/주기 보고인지 알려지지 않았다. 기본 removedValues빈배열·absentTimeout0에서는 제거보고 없이 연결이 유지되면 마지막 UID가 남는다. 실제 제거 값 또는 확인한 주기성에 맞는 시간 제한 설정이 필요하다. 예시와 실제 패킷 길이가 다르면 fixedFrameBytes도 조정한다. 연결 후 조회명령이 필요한 장비는 해당 제조사 명령 계약 추가 확인이 필요하다.
