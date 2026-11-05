# DEPLOY-NSM-001 — 기존 ComfyRuntime을 옮겨 쓰는 빌드 구성

2026-11-05 Asia/Seoul · Claude 단독 구현·검증 · main/3852ee6 + 기존 미커밋 보존. 재빌드·커밋·push 없음. 교차 검증 없음. C#/씬 수정 없음.

## 배경

대상 PC에서 수동 업데이트(DEPLOY-GUIDE-02)가 실패했다. 사용자가 실패 내용을 알려 주지 않아 원인은 미확정이다. 안내의 `Copy-Item ... -Recurse`는 대상 폴더가 이미 있으면 `StreamingAssets\StreamingAssets`로 중첩되므로, 이 PC처럼 폴더가 남아 있었다면 그것이 원인일 수 있다(**추정**).

## 구성

- Desktop/NSM_DNA_02의 `StreamingAssets`: 있던 5개는 그대로 두고(프로젝트와 SHA 동일) 없는 169개를 추가. 174개 전체가 프로젝트와 동일. RFID 카드 목록은 비어 있다.
- `Run_NSM_DNA_02.cmd`, `Launcher/prepare_player.py`, `README_먼저읽기.txt` 추가. 원본은 `Tools/ComfyUI/Run_NSM_DNA_02.cmd`·`Launcher_prepare_player.py`·`NSM_README_KO.txt`.
- 실행기는 PowerShell을 쓰지 않는다. 옮겨 온 ComfyRuntime의 Python으로 `package_manifest.json`의 player를 `NSM_DNA_02.exe`로 고치고(BOM 제거 포함) 런타임의 `portable_boot.prepare`로 경로를 맞춘 뒤 Player를 실행한다.

대상 PC: 폴더 복사 → `ComfyRuntime` 이동 → `RfidSettings.json` 복사 → `Run_NSM_DNA_02.cmd`.

## 실제 검증

Windows 이 PC, 한글·공백 경로의 임시 복사본. 운영 로그 전송을 막으려고 시험 복사본만 `apiUrl` 비움·`alwaysOnTop` 해제. ComfyRuntime은 실제 Python을 junction으로 연결하고 실제 boot 스크립트와 이전 이름 manifest를 넣은 모형이다.

- 실행기로 Player 창 모드 시작, 약 5분 응답 유지. 경로 3개가 임시 위치로 갱신됨. `player.log` 문제 줄은 포트 경고 1건뿐이며 언어 파일 대체 경고 없음.
- 이전 이름+BOM manifest 복구, 재실행 시 설정 불변, 모델 불완전 시 종료 코드 1과 오류 파일 기록 후 복구 시 삭제, ComfyRuntime 없으면 안내 후 종료 코드 1.
- 바탕화면 구성 174파일 SHA 일치, 중첩 폴더 없음, ComfyRuntime 없음.

## 미수행

대상 PC 실제 절차, 실제 ComfyRuntime 이동과 동봉 서버 구동(이 PC는 8188 서버 재사용으로 "already running"), 이미지 생성, 관람 흐름 조작, 실제 리더(이 PC에서 10123 점유), RTX3060. 포트 점유 프로세스는 확인하지 않았다.
