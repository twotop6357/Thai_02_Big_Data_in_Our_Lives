# COMFY-RESTORE-001 — 2026-10-30 Asia/Seoul

Codex 단독 구현/검증. 기준 main/37723c7 + 세션 시작부터 존재한 미커밋 보존. 최종 구성은 SDXL Base1.0 / 내부1024 / 28steps·CFG6.5·DPM++2M·Karras, 텍스트 기반 새 일러스트 → BiRefNet → 실제 알파 → 480 RGBA. 가이드/ControlNet/로컬 선화 색칠을 사용하지 않는다. 모델/패키지 다운로드·갱신·커밋/push 없음.

## 최종 증거

- [runtime.json](runtime.json): 실제 Unity2022.3.62f3 Play, 로컬 ComfyUI0.42.0 / RTX5070Ti Laptop에서 생성·480RGBA 수신·그룹/배치 패널 잠금·5초 유지 버리기·새 입력만 수락·동일 텍스처 Result 전달·취소/제거/늦은 응답 거부·실패/버튼 재시도·비활성/홈 정리. 마지막 실행값/체크 개수는 JSON을 따른다.
- [generated_product.png](generated_product.png): 최종 Unity 수신 텍스처를 GPU에서 알파 포함 그대로 내보낸 이미지. [selection_success.png](selection_success.png)는 실제 배치 화면, group_success.png는 실제 그룹 화면. 직접 시각 확인.
- [alpha.json](alpha.json): 실제480×480 PNG의 알파0/255 존재·네 모서리0 확인. 투명 픽셀154591개·250이상73379개. 유색 배경 위의 실제 표시와 별도로 원본 알파 검사.
- [exits.json](exits.json): loopback delayed-queue fixture로1초 생성 시간 초과·Pause/Resume·제한시간을 걸친 유지 입력 버리기·새 입력 재시도·취소 직후 대체 요청을 이전 finally가 풀지 않음·홈 정리. 실제 ComfyUI의 빈 queue-delete 응답도 확인. GPU 작업을 전역 중단하지 않음.
- [editmode.json](editmode.json): 최종 C# 변경 후49/49 PASS. 블록/색상/저널·보관정책·RFID 선택·버튼 경계값 포함. Python workflow7/7 PASS: 4×10×6×6 입력·6색 원 픽셀 대조·모든 색쌍·현재 API와 Builder 일치·실제 알파 연결·ControlNet/LoadImage 부재.
- [preservation.json](preservation.json): 기존 씬3개·UI/Resources 원본·영상·효과/vendor·GameManager/입력·패키지/ProjectSettings 포함397파일 중396개 SHA256 동일. 변경1개는 승인 범위의 BlockExperienceView(생성 중 Continue/안내 차단·실패 후 재시도)다.

## 중간 후보와 제한

runtime_initial_guide.json / runtime_style_first.json / runtime_style_second.json / runtime_animagine_final.json 및 style*.png·selection_style*.png는 중간 비교 자료다. 강한 가이드 제어, 과한 장식/복수 신발/형태 손상 후보는 채택하지 않았다. runtime_harness_overlap.json의 실패는 도구 재전송으로 두 검증 실행이 겹친 정황(중복 명령 진단과 두 예외)이 있어 검증 루트 중복 가드를 추가하고 순차 재실행했다. 최종 runtime.json과 구분한다.

색상 HEX/RGB는 블록 PNG 원의 평평한 내부에서 추출하여 요청 메타데이터/저널에 보존한다. 모델에는 대응 색 설명을 넣는다(숫자 RGB를 CLIP 본문에 넣던 중간 후보는 도표/분해도를 유도하여 제거). 명암과 모델 특성 때문에 모든 결과 픽셀이 블록 RGB와 같지는 않다. 스타일·단일 물체·색 준수·알파 가장자리의 전 조합/시드 품질은 보장하지 않는다.

Windows Player/설치PC·실제 물리버튼/RFID·전 물품/색/시드 품질·장시간/Profiler는 미수행. 시작 시부터 인계 대기인 RFID-BLOCKS-001의 직접 마우스/F9·Player 후속은 별도 유지. 의도적으로 서버 접속 실패를 만든 테스트의 영문 오류/원본 예외는 정상 증거이며 콘솔을 지우지 않았다. 기존 씬/운영 식별자/MyCompany/Unity-Startup-Template·종료 설정은 변경하지 않았다. 저장소 전체 diff-check의 기존 씬/ProjectSettings YAML 공백은 보존하고 이번 소스/문서 범위의 diff-check만 통과 확인.
