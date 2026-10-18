# Unity Startup Template 사용 가이드

전시·키오스크용 Unity 프로젝트를 시작하는 개인 템플릿입니다.

## 새 프로젝트 시작

1. GitHub에서 이 저장소의 **Use this template → Create a new repository**를 선택하고 새 프로젝트 이름을 입력합니다.
2. 생성된 새 저장소를 짧은 경로에 clone합니다. 아래 YOUR_ACCOUNT/YOUR_PROJECT는 새 주소로 바꿉니다.

```powershell
git -c core.longpaths=true clone https://github.com/YOUR_ACCOUNT/YOUR_PROJECT.git C:\Projects\MyApp
cd C:\Projects\MyApp
git config core.longpaths true
git lfs install --local
git lfs pull
git remote -v
```

3. Unity Hub에 **Unity 2022.3.62f3**와 대상 플랫폼 빌드 모듈을 설치합니다. **Add project from disk**로 새 폴더를 엽니다.
4. 패키지 임포트가 끝나면 `Assets/Scenes/0_Idle.unity`를 열고 Play로 확인합니다.
5. **Edit → Project Settings → Player**에서 Company Name, Product Name과 플랫폼별 Application Identifier를 새 프로젝트에 맞게 수정합니다. 로그와 PlayerPrefs가 다른 앱과 섞이지 않도록 합니다.
6. **File → Build Settings**에서 대상 플랫폼과 `0_Idle` 씬을 확인하고 빌드합니다. 설치 PC에서 화면, 입력, 소리, API, 복귀, 종료를 검증합니다.

## 주요 수정 위치

| 위치 | 용도 |
|---|---|
| Assets/Scripts/Core/GameManager.cs | 전역 동작과 첫 화면 복귀 |
| Assets/Scripts/Network/APIManager.cs | API 연동 |
| Assets/Scripts/App/GameLifetimeScope.cs | VContainer 의존성 등록 |
| Assets/Prefabs/GameLifetimeScope.prefab | 자동 생성 DI 루트 |
| Assets/StreamingAssets/Settings.json | 무입력 복귀, fps, 창 포커스, API 주소, 숨은 종료 버튼 |
| Assets/StreamingAssets/ShutdownSettings.json | 요일별 자동 종료 |
| Assets/Settings | URP 렌더링 설정 |

Settings.json의 useInactivityTimer와 resetTime으로 무입력 복귀를 설정합니다. 기본은 비활성화이며 targetFrameRate는 60입니다. apiUrl은 기본적으로 비어 있습니다. 설정 파일에 비밀 키를 넣지 마세요. 자동 종료는 모든 요일이 기본 비활성화입니다. 빌드 후에도 StreamingAssets 설정을 수정할 수 있습니다.

## 템플릿 관리와 업데이트

템플릿 자체를 개선할 때는 이 저장소에 커밋합니다. 개별 프로젝트 작업은 **Use this template**로 만든 별도 저장소에서 진행합니다. 템플릿 변경은 기존 프로젝트에 자동 반영되지 않으므로 필요한 파일을 검토해 옮깁니다.

패키지를 올리기 전에 커밋하고 **Tools → Update Stack Packages**를 실행한 뒤 Play와 빌드를 다시 확인합니다. URP·Input System·Addressables와 직접 설치된 DOTween은 별도로 관리합니다.

공통 코드 com.huliacdev.template은 https://github.com/wonjeong97/Template.git 에서 다운로드하는 외부 의존성으로 유지했습니다. 이 다운로드는 해당 저장소를 변경하지 않습니다. 공통 코드도 직접 관리하려면 별도 사본을 만들고 Packages/manifest.json의 주소를 바꿔야 합니다.

## 독립성과 출처

원본: https://github.com/wonjeong97/0_Startup

기준 커밋: 901e41cff7171e923190cdf6233218586b65b127 (복사일 2026-10-18)

이 저장소는 fork가 아닌 새 이력의 독립 저장소입니다. 원본 remote와 기존 Unity Cloud 프로젝트 식별자를 제거했습니다. 원본에 push하거나 원본 변경을 자동으로 가져오는 연결은 없습니다. 외부 패키지는 공급처에서 내려받습니다.

원본 루트에 LICENSE가 없어 새 라이선스를 임의로 부여하지 않았습니다. 기존 서드파티 저작권·라이선스를 보존했습니다. 공개 재배포나 상용 배포 전 원저작물과 패키지의 이용 조건을 확인하세요.

기존 구성과 패키지 상세는 [원본 설명](docs/ORIGINAL_README.md)을 참고하세요. 원본 설명 속 clone 주소 대신 이 저장소 또는 새 프로젝트 주소를 사용합니다.

## 문제 해결

- Filename too long: C:\Projects처럼 짧은 경로와 위 clone 명령을 사용합니다.
- 패키지 임포트 실패: Git 설치, GitHub·OpenUPM 연결, Console 오류 및 Unity 버전을 확인합니다.
- LFS 포인터만 보임: git lfs install --local 및 git lfs pull을 실행합니다. ZIP보다 clone을 권장합니다.
- Roslyn 빌드 오류: Assets/Plugins/Roslyn DLL이 Editor 전용으로 설정되어 있는지 확인합니다.
- 종료 스케줄 편집 도구: .NET 8 SDK 설치 후 Tools → Build Shutdown Scheduler Exe를 실행합니다.

이 복사 작업에서는 Unity 에디터 실행 및 빌드를 검증하지 않았습니다.
