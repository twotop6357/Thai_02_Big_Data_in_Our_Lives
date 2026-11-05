# AD-FX-005 — 2026-11-02 / Codex 단독

main/3852ee6+기존 변경 보존. AdProductHighlight 인스턴스의 빛살/링/별 크기·알파·노출 빈도를 올리고 순백색을 음영으로 구분했다. 원래 박스 Sprite를 복사한 숨김 Image+Mask를 사방12px 안쪽에 생성하고 내부 입자를 모두 마스킹한다. 전체 화면 폭죽에는 별도 전면 Canvas(32766)를 사용해 광고 이미지·안내·제목보다 앞에 그린다. 화면 전환용 검정 덮개32767과 취소/입력 잠금 정리는 유지한다. 원본 씬·프리팹·패키지/아트·생성·로그·입력 설정을 바꾸지 않았다.

- [runtime.json](runtime.json): 실제 Editor 렌더47 PASS. 빨강/파랑·검정/흰색·흰색/흰색3조합. 이전 PNG는 결과 bridge fixture로 고정해 사용했으며 이 시험은 생성 품질 평가가 아니다. 마스크Sprite/roundcorner/자식 maskable/전면Canvas·확대값, 효과 자연 종료·재개·홈 해제를 확인했다.
- 큰2600×1800 자홍색 테스트 사각형을 내부에 넣었을 때 박스 밖 샘플 픽셀 변화0. 마스크를 끈 양성 대조군에서는500개 이상 샘플이 달라져, 단순히 효과가 없어서 통과한 경우를 배제했다. 박스 안 유효 콘텐츠는 보이고 둥근 모서리는 가려졌다. [mask_probe_clipped.png](mask_probe_clipped.png)는 시험용 임시 사각형으로 제품에 포함되지 않는다.
- 화면 전체를 덮는 불투명 last-sibling 임시 UI 위에서도 폭죽이 실제 렌더되었고 샘플13843개가 변했다. [confetti_over_all_ui.png](confetti_over_all_ui.png). 전환용 검정 덮개가 켜지면 밝은 샘플0으로 완전히 가려졌다.
- 물건Rect 밖의 내부 효과를 끄고 켜서 비교한 가시성 샘플: 빨강/파랑2143·검정/흰색2621·흰색/흰색1583개(3px 간격/RGB합 차이>.04). 단순 설정값만 확인하지 않고 렌더 결과를 비교했다. 세 조합 entrance/ambient PNG와 마스크 시험 그림을 직접 검토했다.
- [lifecycle.json](lifecycle.json):8 PASS. 비활성/취소/재개/중복 진입·영상 암전/아우라 해제·디코딩 첫 프레임0·홈 초기화. 이전 verify_ad_fx4_lifecycle를 같은 코드로 실행하되 결과 경로/작업 ID만 이 폴더로 분리했다.
- [preview.json](preview.json), final_entrance.png/final_ambient.png: 최종 확인용 실제 Comfy 생성1건(학생/가방/빨강·파랑)의 정상 버튼 흐름. 위 고정 PNG 픽셀 검증과 구분한다.

중간 검증에서 Canvas의 Unity 가짜 null과 활성화 전 설정 순서 문제를 발견해 명시적 객체 검사와 활성화 후 정렬 설정으로 수정했다. IsPlaying은 구성 완료 후에 켜서 준비 중 상태를 외부가 완료로 오인하지 않게 했다. Unity refresh 연결 실패 후 수정 전 DLL이 실행된 검증은 버렸고, DLL 시각이 소스보다 최신임을 확인한 뒤 최종47항목을 재실행했다. 해당 실행의 콘솔 오류0을 확인했다. 이후 미리보기 도구가 읽기 불가인 완성 Texture에 EncodeToPNG를 호출한 오류는 도구에서 제거했으며, 제품 Texture 설정은 바꾸지 않고 다시 정상 흐름/미리보기 PASS를 확인했다. 중간 진단 로그는 숨기지 않았다. 표준 UI 셰이더임을 확인해 머티리얼 변경은 하지 않았다.

최종 실제 생성 미리보기는 #EC6868/#5FB0E3 두 색을 전달한 학생 가방이며 final_entrance/final_ambient PNG를 직접 확인했다. [build.json](build.json): Unity2022.3.62f3 Windows 등록3씬, BuildOptions.None, 성공/오류0·기존 경고5개(CS4014 4/Editor 필드 CS0414 1),9.868초. Builds/ComfyAutostart/Thai02.exe 및 동봉 폴더가 갱신되었다.

변경 파일: AdProductHighlight.cs, Tools/UI/verify_ad_fx5.cs·preview_ad_fx5.cs, 운영/검증 문서. Source3씬/효과프리팹/vendor/패키지 diff0. Input System 빌드가 디스크에 추가한 preloadedAssets1개만 원복했고 사용자 InputManager 변경은 보존했다. Play중지/Idle·dirty false로 종료. 실제 설치PC·Player 광고 육안·장시간·실물 장비 검수는 미수행이며, 이전 작업의 장비 후속을 유지한다. 미커밋, 커밋/push/교차검증 없음.
