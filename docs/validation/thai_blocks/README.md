# THAI-BLOCKS-001

2026-11-02 Asia/Seoul · Codex 단독 · main/3852ee6 + 기존 사용자/RED-BUTTON 변경 보존 · 미커밋.

태국어16개 이미지는 ThaiBlocks의 `BagBlock_Thai.png` 등으로, 영문20개는 EnglishBlocks의 `BagBlock_Eng.png` 등으로 구분한다. 후속 BLOCK-SUFFIX-001에서 두 언어의 접미사를 추가했으며 [suffix_rename.json](suffix_rename.json)에36개 변경 전후 경로/PNG 해시/GUID 보존 확인을 기록했다. authoring/팔레트/참조 동기화·검증 코드도 새 경로로 변경하고 Unity 컴파일 오류0·카탈로그36참조/양언어 조회·팔레트 검사를 통과했다. 접미사 변경에서 Play/Player는 다시 실행하지 않았다. 아래 THAI-BLOCKS-001의 실행 검증과 이전 경로를 기록한 JSON은 당시 이력이다.

태국어 모드의 물건10종·색상6종을 교체하며 태국어 파일이 없는 연령4종은 기존 이미지를 사용한다. 두 선택 패널 및 F9 목록/리더/드래그가 같은 언어별 조회를 사용한다.

## 검증 결과

- [runtime.json](runtime.json): 실제 Thai→English→Thai 왕복, 각 모드 전체20종 인식·2패널8슬롯의 이미지/빈칸/활성480개 비교, 참조/미등록/홈/언어·F9/생성 미실행 포함426항목 PASS. [재현 코드](../../../Tools/UI/verify_thai_blocks.cs)는 명시적으로 실행할 때만 동작한다. Play를 Idle에서 시작한 후 Unity execute_code로 실행하고 runtime.json의 passed를 확인한다.
- [태국어 실제 화면](runtime_Thai.png), [영어 실제 화면](runtime_English.png), [태국어 F9](simulator_Thai.png), [영어 F9](simulator_English.png)를 직접 육안 검토했다. 연령4종은 F9에서도 기존 공용 이미지로 표시된다. [정리한 태국어16종](thai_blocks.png)도 모두 대응 물건/색 확인.
- 최종 Unity2022.3.62f3 컴파일·콘솔 오류0, EditMode 기존 RFID/버튼19/19 PASS(job83cc06970cd94c67a9db6bcb4778369c,0.6070731초). Python 워크플로8/8 PASS(0.728초), sync_block_palette.py --check PASS. 초기 NUnit 파일의 asmdef 참조 오류는 신규 파일/meta 제거·Tools 검증 전환 후 force refresh로 해결했다. 새 NUnit 테스트는 결과에 포함하지 않는다.
- [preservation.json](preservation.json): 최초36개 PNG의 바이트 SHA와 GUID 보존. [english_preservation.json](english_preservation.json): 기존 영문20개 PNG/meta 전체가 HEAD 원본과 동일. 태국어16개는 Sprite importer 설정만 변경했고 PNG 내용·GUID는 유지했다.
- 영문/태국어 원 내부6색 RGB 동일: black171619,whiteFFFFFF,blue5FB0E3,redEC6868,greenB7DBA9,yellowF6EEA2. 표시 리소스와 인식 ID·생성 프롬프트를 분리해 팔레트/모델/워크플로 변경 없음. [untouched.json](untouched.json): 씬3개·패키지2개·생성Catalog/API 변경 없음.

종료: Play중지/0_Idle·dirty false. 실제 RFID/버튼 장비와 Windows Player는 미수행이며 기존 장비 검수 후속을 유지한다. 이번 작업에서 이미지 생성 모델의 품질/성능을 재검증한 것은 아니다.
