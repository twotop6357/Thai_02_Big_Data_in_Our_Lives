# 0_Startup — Unity 프로젝트 템플릿

전시·키오스크형 Unity 앱을 빠르게 시작하기 위한 템플릿입니다.
자주 쓰는 패키지와 설정, 공통 매니저 연결이 미리 들어 있습니다.

## 사용 방법

**준비물:** Unity 2022.3.62f3, Git
(일부 패키지를 Git 주소로 받으므로 Git이 설치되어 있어야 합니다.)

템플릿은 **이 저장소만 추적하는 폴더를 하나 받아 두고, 새 프로젝트는 그 폴더에서 필요한 파일만 복사해 시작**하는 방식을 권장합니다.
새 프로젝트는 템플릿의 git 기록 없이 깨끗하게 시작하고, 템플릿 폴더는 `git pull` 로 언제든 최신으로 맞출 수 있습니다.

### 1. 템플릿 폴더 준비 (처음 한 번)

```bash
git clone https://github.com/wonjeong97/0_Startup.git
```

이 폴더에서는 작업하지 않습니다. 새 프로젝트를 시작하기 전에 `git pull` 로 최신 상태로 맞춥니다.

### 2. 새 프로젝트 시작

1. 템플릿 폴더에서 아래 6개만 새 프로젝트 폴더로 복사합니다. `Library`, `.git` 등 나머지는 복사하지 않습니다.

   | 복사할 것 | 내용 |
   |-----------|------|
   | `Assets/` | 스크립트, 프리팹, 씬, 런타임 설정 파일 |
   | `Packages/` | 패키지 목록과 고정 버전 |
   | `ProjectSettings/` | 프로젝트 설정 |
   | `.gitignore` | git에서 뺄 파일 |
   | `.gitattributes` | 큰 이미지·영상은 Git LFS로, DLL·서드파티 파일은 일반 파일로 저장하는 규칙 |
   | `CLAUDE.md` | 프로젝트 설명 틀(Claude Code용) |

2. Unity Hub에서 **Add > Add project from disk** 로 새 폴더를 엽니다. 처음 한 번은 패키지를 받느라 몇 분 걸립니다.
3. **Edit > Project Settings > Player** 에서 **Product Name** 을 새 프로젝트 이름으로 바꿉니다.
   앱 로그 폴더와 PlayerPrefs 위치가 Company Name·Product Name으로 정해지므로, 그대로 두면 같은 PC에 설치한 다른 앱과 섞입니다.

> 이미 시작한 프로젝트에는 템플릿 변경이 자동으로 들어가지 않습니다.
> 공통 코드(HuliacDev Template)와 스택 패키지는 `Tools > Update Stack Packages` 로 올리고, 그 밖의 변경은 필요한 파일만 골라 옮깁니다.

GitHub의 **Code > Download ZIP** 으로 받은 폴더를 템플릿 폴더로 써도 되지만, 그때는 `git pull` 대신 다시 받아야 최신이 됩니다.

## 들어 있는 것

| 구분 | 패키지 |
|------|--------|
| 구조 | [VContainer](https://github.com/hadashiA/VContainer)(DI), [UniTask](https://github.com/Cysharp/UniTask)(async), [MessagePipe](https://github.com/Cysharp/MessagePipe)(Pub/Sub), [R3](https://github.com/Cysharp/R3)(리액티브) |
| 유틸 | [ZLogger](https://github.com/Cysharp/ZLogger)(로깅), [ZString](https://github.com/Cysharp/ZString)(문자열), [DOTween](https://dotween.demigiant.com/)(트윈) |
| 공통 코드 | [HuliacDev Template](https://github.com/wonjeong97/Template) — GameManagerBase, 무입력 복귀, 자동 종료, UI·Fade·Sound·Video 매니저, API·Arduino 매니저, 로그 보관 |
| Unity | URP 14, Input System, Addressables, TextMeshPro, Timeline, Visual Scripting, Test Framework, Memory Profiler, Profile Analyzer |
| 개발 도구 | [NuGetForUnity](https://github.com/GlitchEnzo/NuGetForUnity), [MCP for Unity](https://github.com/CoplayDev/unity-mcp)(Claude Code 연동) |

## 기본 구성

- **DI 루트:** `Assets/Prefabs/GameLifetimeScope.prefab` 이 VContainerSettings의 Root Lifetime Scope라서 씬에 두지 않아도 자동으로 생깁니다.
  GameManager, APIManager, InactivityTimer, ShutdownScheduler, Reporter(로그 뷰어)가 들어 있습니다.
- **런타임 설정:** `Assets/StreamingAssets/Settings.json`(무입력 복귀, 프레임레이트, 창 포커스 복구·항상 위, API 주소, 숨은 종료 버튼)과
  `ShutdownSettings.json`(요일별 자동 종료)은 빌드한 뒤에도 고칠 수 있습니다.
- **자동 종료 설정 프로그램:** `ShutdownScheduleEditor.exe` 는 용량이 커서(약 150MB) 저장소에 없습니다.
  필요하면 `Tools > Build Shutdown Scheduler Exe` 로 만듭니다(.NET 8 SDK 필요).
- **렌더링:** UI 위주 성능 설정(URP-Performant, 카메라 후처리 꺼짐)이 기본입니다. 3D를 쓰면 High Fidelity와 후처리를 켭니다.
- **Roslyn:** `Assets/Plugins/Roslyn` 은 MCP for Unity 전용이라 Editor 전용으로 설정되어 있습니다.
  폴더를 지우고 다시 설치하면 Inspector에서 Editor만 다시 체크합니다.

## 패키지 업데이트

`Tools > Update Stack Packages` 를 실행하면 VContainer, UniTask, MessagePipe, R3, ZLogger, ZString, HuliacDev Template, NuGetForUnity, MCP for Unity만 최신으로 올립니다.
URP·Input System·Addressables처럼 버전을 고정한 패키지와 `Assets/Plugins` 에 직접 넣은 DOTween은 건드리지 않습니다.

## 폴더 구조

```
Assets/
├── Input/            # 입력 액션
├── Packages/         # NuGet 패키지 (NuGetForUnity)
├── Plugins/          # DOTween, Roslyn(Editor 전용)
├── Prefabs/          # GameLifetimeScope.prefab
├── Scenes/           # 0_Idle.unity (첫 씬)
├── Scripts/          # GameLifetimeScope, GameManager, APIManager
├── Settings/         # URP 에셋
├── StreamingAssets/  # Settings.json, ShutdownSettings.json
└── UI/
Packages/
├── manifest.json       # 패키지 목록
└── packages-lock.json  # 패키지 버전 고정
```
