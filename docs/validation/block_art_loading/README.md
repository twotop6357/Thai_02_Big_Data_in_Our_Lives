# BLOCK-ART-LOADING-001

2026-11-02 Asia/Seoul · Codex 단독 · main/3852ee6+기존 변경 보존 · 미커밋.

언어별 블록 전용 JSON/외부 이미지와 생성 버튼/로딩을 연결했다. 현재 외부 파일은 [운영 안내](../../../Assets/StreamingAssets/BlockArtwork/README.md), 전체 동작은 [BLOCK_RESOURCES](../../BLOCK_RESOURCES.md)를 따른다. 최종 로딩은 배경 없이 붉은 분홍 링만 회전하며, 머리와 꼬리는 같은 RGB에 투명도만 다르다.

## 실제 검증

- Unity2022.3.62f3 컴파일 정상, EditMode88/88 PASS. job b5c2c2cf799248c2a57e052f48ace2d2,11.9554524초. 신규12사례는 양언어20경로/중복·언어·kind 검증/상대 이미지 경로 경계를 검사한다.
- [runtime.json](runtime.json):47항목 PASS. 외부 PNG 바이트 교체/언어 독립·태국어 JSON 경로 변경·중복 대기 재로드·잘못된 JSON/없는 이미지의 범위별 복구, 반복 교체 후 외부Texture36개 유지, 버튼 전 생성 없음·실제 ComfyUI 성공·입력/타이머 잠금·회전/실제renderer/꼬리 알파·배경 없음·취소/시간 초과/HTTP400/블록 변경/이전 finally/홈 정리.
- [visual.json](visual.json): 최종 붉은 링9항목 PASS. 선두/꼬리 모든 메시 RGB 동일, alpha0·중간·255, 배경/고정장식 없음, CanvasRenderer 표시, timeScale0에서도 회전·취소/홈 정리. [최종 실제 로딩 화면](loading.png)을 직접 확인했다. [성공 화면](success.png)은 실제 ComfyUI 생성 결과다.
- [localized/runtime.json](localized/runtime.json): 외부 JSON을 사용한 Thai→English→Thai20종·두패널480슬롯/F9 등432항목 PASS. [영어](localized/runtime_English.png), [태국어](localized/runtime_Thai.png), [영어F9](localized/simulator_English.png), [태국어F9](localized/simulator_Thai.png). 화면 직접 검토, 과거 검증 파일은 덮어쓰지 않았다.
- [preservation.json](preservation.json): 원본 블록 이미지/meta·씬·패키지·입력·생성 설정82개 SHA 동일. 외부 PNG36개는 원본과 바이트 일치, JSON40개 참조 경로 존재, 신규 자산/부모meta 누락0. 임시교체 JSON/PNG를 모두 원복했다. 범위 diff-check 정상.

## 재현과 경계

실행 전 Idle에서 Play한다. [주 검증](../../../Tools/UI/verify_block_art_loading.cs)은 [로컬 응답 fixture](../../../Tools/UI/block_loading_fixture.py)를127.0.0.1:18191로 실행한 뒤 Unity execute_code로 명시 실행한다. 외부 파일 임시 교체는 finally에서 복원하며,47항목의 생성1회는 실제 GPU 요청이고 타임아웃/400/지연은 로컬 응답 검사다. [최종 비주얼 검증](../../../Tools/UI/verify_loading_visual.cs)은 지연 응답 중 화면을 캡처한다. fixture 성공 이미지는 과거 실제 PNG를 반환하며 새 생성 품질 표본으로 취급하지 않는다.

개발 중 동시 UniTask 대기·CanvasRenderer 누락·검증용 Reflection 오버로드 문제를 수정했다. 정상 완료 후 실제renderer/메시/캡처를 다시 검사했다. 손상JSON/누락PNG/HTTP400의4오류 로그는 의도한 실패 진단을 보존한 것이며, 최종 정상 실행 콘솔은 오류0이었다.

최종 Play중지/0_Idle·dirty false. 소유한 fixture만 종료했다. Windows Player 빌드/실장비·장시간 검수는 미수행이다. 이번 생성1건을 기존 모델 전체 품질 재승인/3060 속도 실측으로 확장하지 않는다.
