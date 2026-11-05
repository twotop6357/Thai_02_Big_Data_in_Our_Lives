Thai_02 새 PC 실행 안내
준비 대상: Windows 10/11 64비트, NVIDIA RTX 3060 12GB

1. Thai_02 폴더 전체를 새 PC의 쓰기 가능한 폴더에 복사합니다.
   모델 하나가 8GB이므로 USB는 exFAT 또는 NTFS를 사용하세요.
   ComfyRuntime, Installers, *_Data, MonoBleedingEdge를 함께 옮겨야 합니다.

2. 아무것도 설치되지 않은 PC에서는 Installers의 두 설치 파일을 실행합니다.
   - VC_redist.x64.exe: Microsoft Visual C++ 실행 라이브러리
   - NVIDIA_Studio_617.42_RTX3060_Win10_Win11_x64.exe: RTX 3060 그래픽 드라이버
   설치 화면의 라이선스/관리자 확인을 직접 완료한 뒤 PC를 재부팅합니다.
   이미 호환되는 최신 NVIDIA 드라이버가 설치돼 있으면 드라이버 재설치는 필요 없습니다.
   이 패키지는 설치 파일만 제공하며 PC 설정을 자동 변경하거나 자동 재부팅하지 않습니다.

3. Run_Thai02.cmd를 실행합니다.
   현재 폴더 위치로 Comfy 설정 경로를 맞춘 다음 기존 전시 프로그램을 실행합니다.
   Comfy Desktop, Python, Git, CUDA Toolkit을 별도로 설치할 필요 없습니다.
   프로그램이 동봉 Comfy 서버를 자동 실행하며, 8188에 기존 서버가 있으면 사용합니다.
   서버 준비 전에는 원래처럼 시작 버튼 안내가 숨겨지고 버튼 입력이 막힙니다.
   처음 실행/첫 이미지 생성은 모델 로딩 때문에 더 오래 걸릴 수 있습니다.
   폴더를 다시 이동한 경우에도 Run_Thai02.cmd로 실행하세요.

점검 방법
- Check_Runtime.cmd: 동봉 Python·GPU 연산 검사. 결과는 ComfyRuntime/logs/diagnostics.json.
- 서버 시작 오류: ComfyRuntime/logs/comfy_server.log.
- 실행 준비 오류: ComfyRuntime/logs/startup_error.txt.
- 카드/블록 연결은 기존처럼 F10에서 설정합니다. RFID 설정/일반 Settings는 보존했습니다.
- 현재 PC에서 새 동봉 서버로 480x480 투명 PNG 세 장 생성까지 확인했습니다.
  실제 RTX3060 장비에서의 생성 시간과 현장 연결은 해당 PC에서 확인해야 합니다.
- 이미지 생성은 동봉 모델로 로컬 처리합니다. 기존 운영 로그 전송 API는 인터넷이 필요합니다.

동봉 내용
- 격리된 Python 3.13.12 및 검증한 라이브러리, PyTorch 2.11.0+CUDA12.8
- ComfyUI 0.42.0 소스/프런트엔드와 필수 기본 노드
- FLUX.2 Klein4B FP8, Qwen3 4B, Klein VAE, BiRefNet
- 기존 스타일 참조 이미지, 모델 원본 manifest/라이선스
- 공식 Microsoft/NVIDIA 설치 파일(이 PC에 자동 설치하지 않음)

설정/원본 보존
- 기존 EXE/DLL, Settings.json, RFID JSON은 바꾸지 않았습니다.
- 배포본의 BlockGeneration/ComfyUISettings.json만 동봉 경로로 맞춥니다.
- 최초 원본은 ComfyRuntime/ComfyUISettings.original.json에 보관합니다.
- 사용 중인 기존 Comfy 설치와 모델은 수정/삭제하지 않았습니다.

공식 출처
NVIDIA Studio 617.42 (Desktop): https://www.nvidia.com/en-us/drivers/details/280101/
Microsoft VC++: https://learn.microsoft.com/en-us/cpp/windows/latest-supported-vc-redist/
CUDA12.8 드라이버 기준: https://docs.nvidia.com/cuda/archive/12.8.0/cuda-toolkit-release-notes/
모델 출처/revision/SHA256: ComfyRuntime/licenses/*json
ComfyUI와 모델 라이선스: ComfyRuntime/licenses 및 소스/각 Python 패키지의 라이선스
