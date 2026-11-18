# DEPLOY-SELF-001 — 실행 파일만으로 ComfyUI 구동, 재실행 시 RFID 포트 점유 수정

2026-11-18 Asia/Seoul · Claude 단독 구현·검증 · 기준 main/3d1a892 · 커밋 e210f10(fix), b188d54(feat). 교차 검증 없음. 씬·프리팹·패키지 변경 없음.

## 변경

- `ComfyBundledRuntime`: 실행 파일 옆 `ComfyRuntime`을 찾아 실행 파일·인자·작업 폴더·출력 폴더를 만든다. 실행 전 필수 파일과 모델 크기를 확인한다. `ComfyBlockSettings.Parse`가 세 군데(서버 실행기·이미지 보관·체험 컨트롤러)의 설정 로드를 대신하며 런타임이 있으면 JSON의 실행 값을 덮는다.
- `Core.DetachedProcess`: `CreateProcessW(bInheritHandles=false)`로 창 없는 자식을 실행한다. ComfyUI 서버와 운영 로그 감시기가 사용한다. `RfidTcpReaders`는 수신·접속 소켓에 상속 금지 표시를 한다.
- `Tools/ComfyUI/package_portable.py`: 실행기(`Run_*.cmd`)를 넣지 않고 새 안내문을 넣는다. `portable_boot.py --diagnose`는 설정을 고쳐 쓰지 않는다. `Run_NSM_DNA_02.cmd`·`Launcher_prepare_player.py` 삭제.
- RFID 카드 매핑은 이미 `StreamingAssets/RFID/RfidSettings.json`의 `cards`에 있어 새 파일을 만들지 않았다.

## 원인 재현(에디터, 시험 포트 10987)

| 조건 | 자식이 살아 있는 동안 재수신 |
|---|---|
| 수신 중 `Process.Start`로 자식 실행 후 수신 종료(수정 전 방식) | 실패 `AddressAlreadyInUse`, 자식 종료 후 성공 |
| 수신 중 `DetachedProcess.Start`로 자식 실행 후 수신 종료 | 성공 |
| 수신 소켓에 상속 금지 표시 후 `Process.Start`로 자식 실행 | 성공 |

`DetachedProcess`의 실행 중/종료 상태, 종료 코드 0, 중복 Dispose, 없는 실행 파일(Win32 오류 2)도 같은 실행에서 확인했다.

## 자동 검증

- 컴파일 오류 0. EditMode 148/148 PASS(기존 139 + `ComfyBundledRuntimeTests` 9: 런타임 없음, 공백·한글 경로의 인자, 작업 폴더 생성과 반복 호출, 덜 복사된 모델, 빠진 파일 5종).
- Windows 3씬 빌드 `BuildOptions.None` 오류 0·경고 5(기존과 같은 수), [build.json](build.json).

## 배포 폴더와 Player 검증

`C:/Users/김성훈/Desktop/NSM_DNA_02`(약 19GB): 새 빌드 + `package_portable.py`로 복사한 ComfyRuntime(모델 4개 SHA256 일치) + `Check_Runtime.cmd` + `README_먼저읽기.txt`. 이전 빌드는 `Desktop/NSM_DNA_02_old_20261105`로 이름만 바꿔 보존했다.

이 PC(RTX 5070 Ti Laptop)에서 창 모드(960×540)로 실행했다. 사용 중인 Comfy Desktop(8188)과 운영 로그 서버를 건드리지 않으려고 시험 동안만 배포본의 `serverUrl`을 8189로, `apiUrl`을 빈 값으로, `alwaysOnTop`을 false로 바꿨고 시험 뒤 원본으로 되돌렸다(프로젝트 파일과 SHA 일치 확인).

- 1회차: `Using the bundled ComfyRuntime at ...Desktop\NSM_DNA_02\ComfyRuntime` → `Started server process` → `ComfyUI server is ready`. 8189는 동봉 Python, 10123은 Player가 수신. 서버 로그에 ComfyUI 0.42.0 / Python 3.13.12 / torch 2.11.0+cu128 / cuda:0. 이미지 보관 폴더도 `ComfyRuntime\output`으로 잡혔다.
- 종료 후: 동봉 서버는 남고 10123 수신은 사라짐.
- 2·3회차(서버가 남아 있는 상태에서 재실행): `ComfyUI server is already running`, 10123을 새 Player가 수신, `Listen failed` 없음.
- 정리: 시험 서버 종료, 배포본의 서버 로그와 `BurstDebugInformation_DoNotShip` 삭제.

## 미수행

- 대상 PC(RTX 3060)에서의 실행, 실제 RFID 리더 접속, 방화벽 허용 창, 장시간 구동.
- 새 배포본에서의 이미지 생성(서버 준비까지만 확인). 운영 로그 감시기는 `apiUrl`을 비워 시험해 Player에서 실행되지 않았다(`DetachedProcess` 자체는 에디터에서 확인).
- 이전 버전이 띄운 서버가 포트를 잡고 있는 PC에서의 교체. 그 서버는 새 버전이 끝낼 수 없으므로 교체 시 한 번 재부팅한다.
- 배포본의 `RfidSettings.json` `cards`는 비어 있다. 대상 PC에 등록해 둔 카드는 이전 설치의 같은 파일을 복사해야 유지된다.
