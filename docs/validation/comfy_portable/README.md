# COMFY-PORTABLE-001 / 2026-11-02

Codex 단독 구현·검증, main/3852ee6+기존미커밋 보존. 대상Win10/11x64·RTX3060 12GB 사용자확인. 전달위치 `C:/Users/김성훈/Desktop/Thai_02`,20,309,480,765바이트(약20.3GB)/54,463파일. 기존EXE/DLL·Settings/RFID5파일 SHA동일(`package.json`), 기존빌드를 다시빌드/덮어쓰지 않았다.

## 구성과 실행

- ComfyRuntime: 독립Python3.13.12에 검증된venv의 정확한site-packages를 포함, `_pth`로 설치Python/레지스트리/사용자패키지에서 격리. venv의 절대home경로는 사용하지 않는다. Torch2.11cu128/Comfy0.42/frontend1.57.1·소스/라이선스를 포함한다.
- 현재4모델(FLUX2Klein4B FP8/Qwen3 4B/KleinVAE/BiRefNet)을 복사후 고정manifest4개의bytes/SHA256와 대조했다. 사용자스타일 포함. 과거모델/사용자input/output/user·토큰/custom_nodes/.git/.venv/개발캐시는 가져오지 않았다.
- Installers: [NVIDIA Studio617.42 Desktop](https://www.nvidia.com/en-us/drivers/details/280101/)·[Microsoft VC++ x64](https://learn.microsoft.com/en-us/cpp/windows/latest-supported-vc-redist/) 공식파일. NVIDIA 공식getDownloadDetails의RTX3060·Win10/11지원 확인, 두파일 Authenticode Valid/제조사서명/SHA256/바이트 확인(`installers.json`, 배포Installers/verified_installers.json). 호스트에 설치하거나 권한/드라이버/방화벽을 변경하지 않았다.
- Run_Thai02.cmd→PowerShell→동봉Python이 현재위치로 배포Comfy설정만 원자갱신하고 기존Player를 실행한다. 원본백업은 ComfyRuntime/ComfyUISettings.original.json. 일반/RFID설정은 보존한다. 기존앱이 서버를 숨김실행하고 기존8188이 있으면 재사용한다. 필수드라이버/VC++ 설치·재부팅은 운영자가 수행하며 Python/Desktop/Git/CUDA Toolkit 별도설치는 필요없다.
- Check_Runtime.cmd·README_먼저읽기.txt·로그/모델출처/라이선스 동봉. 서버는127.0.0.1에 바인딩, offline/기본노드 한정 및HF/Torch캐시를 동봉폴더로 지정했다. 기존운영로그 API는 인터넷을 사용한다.

## 실제 검증

- `diagnostics.json`: 동봉exe/prefix·CUDA12.8·현5070TiLaptop CUDA연산[2,4] PASS, Torch sm_86포함. 실제3060시간 측정은 아니다.
- 별도8189동봉서버 system_stats의Comfy0.42/Python3.13/Torchcu128/동봉경로 확인. 새3PNG 생성(`generation/benchmark.json`): 첫8.146초, 다음2.429/4.561초, target_gpu_acceptance_verified=false. 현5070TiLaptop/32GB 수치다.
- `generation/png_checks.json`:3개480×480/8bitRGBA·alpha0..255·모서리0 PASS. 기존스타일/워크플로 사용, 인터넷모델다운로드 없음.
- `relocation.json`: 한글/공백 별도디렉터리에서 동봉Python/CUDA연산·prefix/설정경로재작성 PASS, cwd=C:/Windows. 검증트리의 파일내용만 hardlink해 저장공간을 절약했고 junction/호스트Python참조는 없다. 배포본은 최초 실제파일복사로 구성했다.
- PowerShell -PrepareOnly도 C:/Windows/Temp에서 성공. 기존5파일SHA보존·4모델SHA·서명2·py_compile 확인. Unity C#/씬/UPM 미수정으로 이번작업 재빌드/EditMode검사는 수행하지 않았다. 기존Player도 직접 시작하지 않아 운영API start/end를 추가전송하지 않았다.

## 중간 실패/정리

[TS-20261102-01](../../TroubleShooting.md#TS-20261102-01): /XD models basename이 내부ldm코드를 제외한 오류를 루트절대경로제외로 수정/재복사 후3생성을 재검증했다. 원본진단/로그 보존. cu130최적화/triton/ascend 부재 경고는 동봉cu128 eager경로의3이미지 성공과 별개이며 임의업그레이드하지 않았다.

8189테스트서버는Ctrl+C/Stopped server로중단, 해당동봉Python프로세스0. 기존8188 PID17356 유지. queue유휴확인후 /free로 기존모델캐시만해제해VRAM충돌을 피했다. 기존설정/파일/프로세스종료/전역interrupt는 없었고 다음사용시 모델재로딩이 가능하다.

자동검토가 백그라운드테스트명령을 차단해 더좁은 foreground기동으로 검증했다. 검증임시트리 삭제도 차단돼 삭제하지 않고 절대경로확인후 workspace Temp/PortableRelocationValidation_20261102로 이동/보존했다. 배포폴더에는 임시트리가 없다. 다른프로세스 종료/Windows종료/보안설정 변경없음.

## 남은 확인

실제빈Windows·3060·최초드라이버/VC++설치·Player GUI직접입력·RFID/장시간은 현장미검증이다. 이번작업은 동봉서버실생성과 기존Player자동실행 계약에 필요한경로/설정을 확인했다. 새PC는 전체복사→Installers설치/재부팅→Run_Thai02.cmd. 8GB모델 이동매체는exFAT/NTFS를 사용한다.
