NSM_DNA_02 설치 안내
대상: Windows 10/11 64비트, NVIDIA 그래픽 카드(RTX 3060 12GB 기준)

실행 방법
1. 이 NSM_DNA_02 폴더 전체를 대상 PC의 쓰기 가능한 위치에 복사합니다.
   모델 파일 하나가 8GB이므로 USB는 exFAT 또는 NTFS를 사용합니다.
2. NSM_DNA_02.exe 를 실행합니다. 배치 파일이나 설정 수정은 필요 없습니다.
   프로그램이 같은 폴더의 ComfyRuntime 을 찾아 ComfyUI 서버를 창 없이 직접 실행합니다.
   폴더를 다른 위치로 옮겨도 그대로 실행됩니다.
   서버가 준비될 때까지 타이틀의 버튼 안내가 숨겨지고 버튼 입력이 막힙니다(첫 실행은 수 분 걸릴 수 있음).

반드시 함께 있어야 하는 것
- NSM_DNA_02.exe, UnityPlayer.dll, NSM_DNA_02_Data, MonoBleedingEdge, ComfyRuntime
- EXE만 따로 옮기면 실행되지 않습니다.

처음 설치하는 PC에서 확인할 것
- NVIDIA 그래픽 드라이버와 Microsoft Visual C++ 재배포 패키지(x64)가 설치되어 있어야 합니다.
  이전 버전을 실행하던 PC라면 이미 설치되어 있습니다.
- 처음 실행하면 Windows 방화벽 허용 창이 뜰 수 있습니다. RFID 리더가 이 PC의 TCP 10123 포트로
  접속하므로 허용합니다. 폴더 위치를 바꾸면 다시 물어볼 수 있습니다.

RFID 설정 (NSM_DNA_02_Data/StreamingAssets/RFID)
- RfidSettings.json: 수신 포트, 리더 IP와 칸 번호, 카드 UID와 블록 매핑(cards).
- RfidPresenceSettings.json: 카드 유지/제거 확인 주기.
- 이 배포본의 cards 는 비어 있습니다. 이전 설치에서 등록한 카드를 계속 쓰려면 이전 폴더의
  StreamingAssets/RFID/RfidSettings.json 을 같은 위치에 덮어씁니다. 없으면 프로그램에서 F10으로 등록합니다.

이전 버전에서 교체할 때
- 이전 프로그램을 끈 뒤 PC를 한 번 재부팅하고 새 버전을 실행합니다. 이전 버전이 띄워 둔 ComfyUI 서버가
  RFID 포트를 붙잡고 있을 수 있기 때문입니다. 새 버전끼리는 껐다 켜도 재부팅이 필요 없습니다.

문제가 생기면
- Check_Runtime.cmd: 동봉 Python과 GPU 연산 점검. 결과는 ComfyRuntime/logs/diagnostics.json
- ComfyUI 서버 로그: ComfyRuntime/logs/comfy_server.log
- 프로그램 로그: %USERPROFILE%/AppData/LocalLow/MyCompany/NSM_DNA_02/Player.log
  "Bundled ComfyRuntime ... is missing or incomplete" 가 보이면 폴더 복사가 덜 된 것입니다.
- 8188 포트에 이미 ComfyUI가 떠 있으면 그 서버를 그대로 사용합니다.

동봉 내용
- 격리된 Python 3.13.12, PyTorch 2.11.0+CUDA 12.8, ComfyUI 0.42.0
- FLUX.2 Klein 4B FP8, Qwen3 4B, Klein VAE, BiRefNet (출처와 라이선스는 ComfyRuntime/licenses)
- 이미지 생성은 인터넷 없이 로컬에서 처리합니다. 운영 로그 전송에는 인터넷이 필요합니다.
