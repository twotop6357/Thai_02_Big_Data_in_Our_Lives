# 씬 연결과 언어 선택 버튼

## 운영·테스트 방법

1. `Assets/Scenes/0_Idle.unity`에서 Play를 시작한다.
2. 키보드 위쪽 `1` 또는 숫자패드 `1`을 한 번 누르면 이중 입력 대기 시간이 지난 뒤 태국어로 `1_Game`에 진입한다.
3. 같은 키를 놓았다가 설정 시간 안에 다시 누르면 영어로 진입한다. 두 번 누르기는 **누름 시작 사이 간격**으로 판정한다. 누른 채 유지하는 것은 이중 입력이 아니다.
4. Idle 외 모든 활성 씬에서 버튼을 **연속 5초** 누르면 `0_Idle`로 복귀한다. 중간에 놓으면 유지 시간은 초기화된다. Time.timeScale과 무관하게 동작한다.
5. Game에서 생성 이미지가 표시되고 `PressButtonText`가 보이면 버튼을 5초 전에 놓는 **짧은 누름**으로 `2_Result`에 이동한다. 짧은 누름은 놓는 순간 판정하므로 5초 유지 복귀와 겹치지 않는다. 다른 씬·이미지 없는 상태의 짧은 누름은 무시한다.
6. 전환 직후에는 버튼을 한 번 놓아야 다음 입력을 받는다. 포커스 상실·시뮬레이터 비활성화 시 대기 중 입력을 취소한다.

## 설정

`Assets/StreamingAssets/Settings.json`:

```json
"buttonDoublePressIntervalSeconds": 0.4
```

- 단위 초, 허용 범위 0.1~2초. 누락·잘못된 값은 0.4초를 사용한다. 변경 후 앱/Play를 재시작한다.
- 단일 입력은 이 시간만큼 판정을 기다린다. 크게 설정하면 태국어 진입도 그만큼 늦어진다.
- 5초 복귀 시간은 현재 요구사항에 맞춰 코드 상수로 고정했다.
- 공통 설정은 기존 AppSettingsProvider가 로드한다. 패키지 Settings 타입에 프로젝트 필드를 확장할 수 없어, GameManager.OnSettingsLoaded 이후 프로젝트 필드만 같은 JSON에서 한 번 더 비동기로 읽는다. 패키지 캐시는 수정하지 않는다. 설정 실패 시 공통 90초 무입력 대체 정책은 그대로 유지된다.
- 기존 사용자 설정 `resetTime: 30`, `useInactivityTimer: false`를 보존했다. 이 버튼 기능은 무입력 타이머 활성화 여부와 별개다.

## 숫자 1 시뮬레이터 On/Off

- IdleScene 루트의 **ButtonInputSimulator** 오브젝트에 **KeyboardButtonSimulator**를 배치했다.
- 이 컴포넌트 체크 또는 오브젝트 활성 상태로 켜고 끈다. Play 전 꺼두면 숫자 1 입력은 사용하지 않는다.
- 활성 상태로 시작하면 오브젝트는 DontDestroyOnLoad로 이동하여 Game 및 이후 씬에서도 동작한다. Play 중에는 해당 영역의 원본 컴포넌트를 끄면 된다.
- Idle 복귀 시 새 씬의 중복 오브젝트는 제거한다. 원본의 런타임 On/Off 상태는 유지된다.
- Game 씬에서 바로 Play하면 Idle의 시뮬레이터는 생성되지 않는다. 버튼 테스트는 Idle에서 시작한다.

## 코드 연결점

- `Core.GameManager.CurrentLanguage`: `SessionLanguage.Thai` 또는 `English`.
- `LanguageCode`: `th` 또는 `en`. `HasLanguageSelection`으로 선택 완료 여부를 구분한다. Idle 복귀 시 선택을 초기화한다.
- 이번 구현은 **언어 상태 선택·전달**이다. 기존 GameScene의 영문 문구를 자동 번역하지 않는다. 태국어 문구·폰트와 실제 UI 지역화는 후속 콘텐츠 작업에서 이 언어값에 연결한다.
- Game 씬 로드 성공 시 `[GameManager] Game started. Language: Thai (th).` 또는 `[GameManager] Game started. Language: English (en).`을 한 번 출력한다. 프로그램 로그는 선택 언어와 무관하게 영문으로 작성한다.
- `SetButtonPressed(bool)`: 향후 Arduino 어댑터가 메인 스레드에서 눌림/놓임을 전달할 공통 진입점. 실제 장비 연동은 아직 없다. 연결이 끊기면 `ResetButtonInput()`을 호출하고, 자동 반복 신호를 별도 누름으로 보내지 않는다. 키보드와 하드웨어를 함께 사용할 경우 입력 소스 병합 정책을 먼저 구현한다.
- `ReturnToIdleManually()`: 수동 복귀와 MoveIdleEvent 1회 발행. 무입력 이벤트 경로는 수동 이벤트를 발행하지 않는다.
- `SessionCancellationToken`: 콘텐츠 API·영상 작업은 화면 수명 토큰과 함께 사용한다. 복귀 시 취소된다. 이전 세션 토큰을 다음 세션에 재사용하지 않는다. 씬에 종속된 작업은 별도 파괴/비활성화 취소도 필요하다.
- 새 콘텐츠 씬은 Build Settings에 추가한다. 씬별 매니저/복귀 감지기 중복 배치는 필요 없다. Idle 경로를 제외한 활성 씬에 공통 유지 판정이 적용된다.

## 검증 범위

순수 입력 판정 테스트는 `Assets/Tests/Editor/ExhibitionButtonGestureTests.cs`에 있다. 실제 수행 결과와 남은 Windows/장비 검증은 [HANDOFF](agents/HANDOFF.md)의 FLOW-001을 확인한다.
