# 광고 물건 강조 효과

광고 화면을 공개한 뒤 물건 뒤에서 금빛 빛살·링 아우라가 천천히 반복되고, 큰 별과 전체 화면 컨페티가 입장 때 한 번 재생된다. Scroll 씬의 Item에서 복사한 두 ParticleImage 묶음과 ConfettiFullscreen의 프로젝트 전용 복사본을 사용한다. 원본 데모 씬과 패키지 프리팹은 보존한다. 이 설정은 이전 AD-FX-001의 전체1.8초 종료 규칙을 사용자 후속 요청(AD-FX-002)에 따라 대체한다.

- 전용 프리팹: `Assets/UI/Effects/AdItemEmphasis.prefab`, `Assets/UI/Effects/AdConfettiFullscreen.prefab`.
- 배치: `2_Result`의 `RecommendedAdvertisement/AdvertisementBoard` 내부에서 아우라·컨페티 다음에 `ResultImage`를 그린다. 물건의 불투명 영역은 모든 입자와 트레일보다 앞에 보이며, 언어와 관계없이 같은 물건 중심을 따른다. 별도 Canvas 정렬 순서를 추가하지 않아 기존 화면 암전이 물건도 함께 덮는다.
- 반복 아우라: 빛살660크기·6초 주기·회전60도/주기(초당10도). 원형 파동은 시작 지름120에서 최대480까지2.4초 주기로 부드럽게 확산한다. 파동의 최대 크기를 물건 영역480에 맞춰 광고 박스 안에 여유를 두고 유지한다. 화면을 넘길 때까지 반복한다.
- 입장 별: 크기32→64, 초기7→14개, 방출12→16개/초로 확대.0.65초 방출·0.9초 수명,1.8초에 남은 단발 입자만 정리한다.
- 전체 화면 컨페티: 지정한 `Demo/Prefabs/ConfettiFullscreen.prefab`의 상단 방출·좌우 버스트·트레일을 그대로 복사하여1920×1080 영역에 배치. 입자 크기2배, 회전량은 원본의20%, 좌우 버스트 시작점은 화면 아래 양쪽으로 맞췄다.2초에 자르지 않고 약6초 동안 자연 종료한다. 원본의 자동 직렬화 변경을 피하기 위해 연결을 분리한 프로젝트 소유 복사본이다.
- 입력: 단발 별과 컨페티가 끝날 때까지 공통 입력 잠금을 유지한다. 반복 아우라는 완료 대기에 포함하지 않으므로 컨페티 종료 후 아우라가 보여도 물리 버튼·숫자1로 진행할 수 있다. 무시한 반복 입력을 보관하지 않는다. 컨페티가 비정상적으로 끝나지 않으면12초에 정리/영문 진단 후 입력을 복구한다.
- 화면 퇴장·컴포넌트 비활성화·취소 시 해당 효과만 정리하고 잠금을 반환한다. 같은 화면의 중복 등장 요청은 효과를 재시작하지 않는다. 화면 재진입 시 새로 재생하며 게임 일시정지 중에도 진행한다.
- 연결: `UIScenePresentation`이 암전 해제 후 유한한 광고 입장을 기다린다. 영상 진입은 암전 완료 시 아우라까지 정리하며, 실패 후 광고 복원 시 아우라만 재개한다. 기존 광고 이미지 페이드와 영상 첫 프레임 준비는 유지한다.

## 데모 프리팹 검토

`Demo/Prefabs` 하위 전체 19개 프리팹의 입자·반복·시간·스프라이트·부가 컴포넌트를 조회했다. `_Primitives`와 메타 파일도 목록/보존 해시에 포함했다. 상세 값은 [검토 데이터](validation/ad_fx/prefab_review.json)에 남긴다.

| 프리팹 | 광고에 대한 판단 |
|---|---|
| Scroll | Item의 GlowRays·Ring2·4Star 두 묶음 채택. 목록·마스크·배경·아이콘을 제외하고 느린 반복 아우라와 큰 단발 별로 재설정 |
| Highlight | 짧은 수명과 주변 강조 배치 참고. 별도 사방 효과는 중복되므로 추가하지 않음 |
| EnergySpread | 빛살·링·별의 조합 참고. 원본의 긴 반복·큰 확산은 사용하지 않음 |
| Puff | 구름이 물건을 덮는 연출이므로 제외 |
| ConfettiBurst | 축하용 분산·트레일이 물건 강조와 겹쳐 제외 |
| ConfettiFullscreen | AD-FX-002 사용자 명시 요청으로 추가. 물건 뒤에서 전체 화면 방출·좌우 버스트/트레일,2초 제한 없이 자연 종료 |
| CoinAttraction | 코인·숫자·흡인 대상이 있는 보상 UI여서 제외 |
| WorldSpaceConstraintAttractor | 월드 대상 흡인과 코인 표현이 필요 없어 제외 |
| AlignToDirection | 방향 화살표 안내여서 제외 |
| Masking | 마스크 내부 장식용이며 별도 강조가 아니므로 제외 |
| Slider | 슬라이더/불꽃/마스크가 광고와 맞지 않아 제외 |
| UniformEmitter | 균일 별 방출 참고. 현재 Item의 별 묶음으로 충분하여 제외 |
| _Primitives/AttractorBurst | 흡인 대상 없는 광고에 불필요하여 제외 |
| _Primitives/AttractorContinuous | 지속 방출/흡인 때문에 제외 |
| _Primitives/AttractorLine | 방향성 흡인 때문에 제외 |
| _Primitives/BackgroundRays | 배경 빛살 참고. Item 빛살과 중복되어 추가하지 않음 |
| _Primitives/BackgroundScale | 배경 크기 변화 참고. 물건 자체 확대 없이 기존 링 사용 |
| _Primitives/BurstDirectionalWithNoise | 방향성·노이즈 이동보다 물건 중심의 짧은 반짝임이 적절하여 제외 |
| _Primitives/BurstWithNoise | 노이즈 확산이 불필요하여 제외 |

원본 Item을 다시 가져오는 작성 도구는 `AdProductHighlightAuthoring.ImportOpenItem()`이다. 미저장 원본은 그대로 둔 채 PreviewScene에서 복사본을 만들고 Result만 추가 로드하여 저장한다. 중복 설치는 거부한다.

현재 반복/컨페티 구성은 같은 도구의 `UpgradeAdvertisementEffects()`로 작성했다. 기존 프로젝트 효과만 수정하고 현재 열린 미저장 씬은 저장하지 않는다.

## 검증

AD-FX-003의 최종28항목을 통과했다. 파동을 줄인 뒤 실제 입자 정점620개를 반복 한 주기 이상 검사해 광고 박스의16px 안쪽 경계 안에 머무는 것을 확인했다(관측 최대 지름477.469, 설정 최대480). 컨페티/아우라·입력·물건 최전면·영상첫프레임0·취소/재진입 회귀도 포함한다. [최신 실행 기록](validation/ad_fx3/runtime.json), [화면](validation/ad_fx3/confetti_after_two_seconds.png). 씬·실행 컨트롤러·컨페티 및 vendor 포함199파일의 시작/종료 SHA256 동일, 미저장 Idle 루트 상태 동일. Player/실장비 미검증.

현재 AD-FX-002는 `Tools/UI/verify_ad_highlight_loop.cs`의 최종26항목을 통과했다. [실행 기록](validation/ad_fx2/runtime.json), [입장](validation/ad_fx2/entrance.png), [2초 이후 컨페티](validation/ad_fx2/confetti_after_two_seconds.png), [반복 아우라](validation/ad_fx2/looping_aura.png). Time.timeScale=0에서 컨페티 자연 종료(최종 관측5.855초)·아우라의 다음 반복·입력 복구, 물건 불투명 지점4곳의 픽셀 불변, 취소/비활성/재진입 및 영상 암전·첫 프레임0을 확인했다. 패키지 원본195파일 해시 동일·현재 미저장 Idle의 루트 상태 동일도 확인했다. Windows Player·실장비·장시간은 미검증이다.

아래는 대체된 AD-FX-001의 과거 검증 기록이다.

Unity2022.3.62f3의 실제 Play 실행에서 최종21항목을 통과했다. Time.timeScale=0에서도1.814초 안에 종료했고, 효과 중 버튼 무시·종료 후 입력 복구·재진입·중복 요청·취소·비활성화·화면 퇴장 정리를 확인했다. 광고→영상 암전과 실제 첫 프레임0도 유지됐다. [실행 기록](validation/ad_fx/runtime.json), [효과 화면](validation/ad_fx/advertisement_emphasis.png), [종료 화면](validation/ad_fx/advertisement_after.png).

데모 원본195파일 SHA256 동일, 원본 미저장 씬의 디자인 직렬화 값 동일을 확인했다. Unity Play 전환이 초기화하는 내부 burst.used 실행 플래그만 비교에서 제외했다. 최초 검증 도구는 그룹 배치 연출 완료 전에 버튼을 보내 대기시간이 만료됐으며, 입력 잠금이 해제된 후 보내도록 도구를 수정한 뒤 재검증했다. Windows Player 빌드·설치 장비·장시간 검증은 이번 범위에서 수행하지 않았다.
