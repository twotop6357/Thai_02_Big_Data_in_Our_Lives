NSM_DNA_02 대상 PC 설치 안내

1. 이 NSM_DNA_02 폴더 전체를 대상 PC에 복사합니다.

2. 이전 Thai_02 폴더의 ComfyRuntime 폴더를 이 폴더 안으로 옮깁니다.
   같은 드라이브에서 잘라내기/붙여넣기 하면 약 20GB도 바로 끝납니다.

3. 등록한 RFID 카드를 유지하려면 아래 파일을 복사해 덮어씁니다.
   보낼 파일: Thai_02\Unity-Startup-Template_Data\StreamingAssets\RFID\RfidSettings.json
   받을 위치: NSM_DNA_02\NSM_DNA_02_Data\StreamingAssets\RFID\RfidSettings.json

4. Run_NSM_DNA_02.cmd 를 실행합니다. 이후에도 항상 이 파일로 실행합니다.
   폴더 위치와 실행 파일 이름은 자동으로 맞춰집니다.

오류가 나면 Launcher\startup_error.txt 와 ComfyRuntime\logs\comfy_server.log 를 확인합니다.
바로가기나 자동 실행을 쓰고 있다면 Run_NSM_DNA_02.cmd 를 가리키도록 바꿉니다.
ComfyRuntime을 옮기면 이전 Thai_02 프로그램은 실행되지 않습니다. 되돌릴 때는 다시 옮기면 됩니다.
