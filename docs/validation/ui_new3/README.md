# UI-NEW-003 — 인트로 배경과 버튼 안내 교체

2026-11-02 Asia/Seoul · Codex 단독 · main/3852ee6+기존 변경 보존 · 미커밋.

NEW의9개 PNG를 아래 기존 경로에 바이트 그대로 덮어썼다. 모든 크기가 같아 배치·JSON·씬·기능을 수정하지 않았다. 원래9개 meta/GUID와 라이브러리 참조도 보존했다. 소스 NEW는 교체/검증 후 삭제했으며 표의 소스명은 기록용이다.

| NEW 원본 | 기존 대상(Assets/UI 기준) |
|---|---|
| E02-S01. 인트로 및 체험 시작.png | Backgrounds/Intro_bg.png |
| 버튼 안내문구 2.png | UIs_En/Idle/ButtonHelpBox_en.png |
| 버튼 안내문구 2-1.png | UIs_Th/Idle/ButtonHelpBox_th.png |
| 버튼 안내문구 3.png | UIs_En/GroupSelect/ButtonHelpBox_groupselectPanel_en.png |
| 버튼 안내문구 3-1.png | UIs_En/SelectAll/ButtonHelpBox_selectallPanel_en.png |
| 버튼 안내문구 3-2.png | UIs_En/RecommendedAd/ButtonHelpBox_RecommendedAd_en.png |
| 버튼 안내문구 3-3.png | UIs_Th/GroupSelect/ButtonHelpBox_groupselect_th.png |
| 버튼 안내문구 3-4.png | UIs_Th/SelectAll/ButtonHelpBox_selectall_th.png |
| 버튼 안내문구 3-5.png | UIs_Th/RecommendedAd/ButtonHelpBox_recommendedAd_th.png |

## 확인 결과

- [mapping.json](mapping.json): 원본/이전대상/기존meta SHA256·GUID·크기. 최종9개는 원본과 바이트 동일, meta 전체 불변. 영어3/3-1의 그림은 제공된 원본부터 동일하며 태국어Group의 Home영문 표기도 원본 그대로다.
- [asset_connections.json](asset_connections.json): 기존 GUID/라이브러리9연결 정상. [source_reference_check.json](source_reference_check.json): NEW의 sourceGUID/폴더GUID10개에 대한 Assets 내 참조0.
- [runtime.json](runtime.json): 실제 Unity2022.3.62f3 양언어8화면의 안내/배경·언어전환/정상결과전환/홈18항목 PASS. [8화면 미리보기](preview.png)와 개별 idle/group/selection/result_th/en.png를 직접 확인했다. 결과화면에는 이전 생성PNG를 테스트 인계했으며 새 AI생성이나 영상재생은 하지 않았다.
- [verification.json](verification.json): NEW폴더/meta 삭제, 최종9개해시/meta 확인. 보호파일452개 SHA 동일. ignored StreamingAssets/build_info의 ReporterEditor 자동작성 타임스탬프1개만 별도 제외/기록했다. 해당 자동파일을 되돌리거나 PackageCache를 수정하지 않았다.
- PowerShell 재귀삭제는 자동정책에서 차단되어 실행되지 않았다. 정확한 절대경로/파일18개/참조0·대상해시를 확인한 뒤 Unity의 명시적 Assets/UI/NEW 에셋 삭제로 폴더/meta를 함께 정리했다.

재현 도구는 [verify_ui_new3.cs](../../../Tools/UI/verify_ui_new3.cs)이며 Idle에서 Play한 뒤 Unity execute_code로 명시 실행한다. 최종 콘솔 오류0,Play중지/0_Idle·dirty false. 코드 변경 없는 자산 교체라 NUnit/Player/실장비/영상 전체 검수는 이번 작업에서 수행하지 않았다.
