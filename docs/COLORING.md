# 물품 선화 색칠

## 동작

ComfyUI·AI 모델·서버 없이 로컬 PNG의 흰 사물 영역에 선택한 두 색을 입힌다. 검정 외곽선, 회색 명암, 원본 알파는 유지한다. Group/Product/Color1/Color2 네 블록이 모두 인식되면 자동으로 색칠하고, 블록 변경 시 다시 처리한다. 같은 블록의 반복 인식은 재처리하지 않는다.

나잇대와 물품 조합으로 원본을 찾는다. 같은 가방이라도 Child/Student는 백팩 형태, Working Adult/Senior는 서로 다른 손잡이·포켓의 가방 형태를 사용한다. **현재 포함된 48종은 기능 검증용 기하학 선화 샘플이다. 전시용 최종 삽화는 교체해야 한다.** 이미지가 없거나 손상되면 다른 연령 이미지로 대체하지 않고 실패 상태를 표시한다.

Color1은 본체, Color2는 포켓·끈·밑창 등 마스크에서 지정한 세부 부위다. 같은 색 두 개, 흰색, 검정색도 허용한다. 검정색을 선택한 면에서는 검정 선과 면의 대비가 없어지는 것이 정상이다.

Game에 완성 이미지와 PressButtonText가 표시되면 숫자1을 짧게 눌러 Result로 이동한다. Result에는 같은 텍스처를 전달한다. 5초 유지·무입력 Idle 복귀와 언어 선택은 [BUTTON_INPUT](BUTTON_INPUT.md)을 따른다. 출력은 화면 표시이며 프린터 인쇄·PNG 자동 저장은 포함하지 않는다.

## 이미지 교체와 영역 지정

`Assets/StreamingAssets/BlockGeneration/BlockColoring.json`의 templates에 4개 나잇대 × 12개 물품 조합을 등록한다. 항목 예:

```json
{
  "groupId": "child",
  "productId": "bags",
  "resolvedProductId": "bags",
  "baseImage": "Coloring/Artwork/child_bags_base.png",
  "maskImage": "Coloring/Artwork/child_bags_mask.png"
}
```

1. 원본은 흰 면과 검정 선으로 만든 PNG. 투명 배경을 권장하며 회색 명암도 사용할 수 있다. 흰 배경을 사용해도 마스크 밖은 원본 그대로 남는다.
2. 같은 크기·같은 위치의 영역 PNG를 별도로 준비한다. Color1 영역은 빨강 `#FF0000`, Color2 영역은 초록 `#00FF00`, 색칠하지 않을 영역은 검정 `#000000`으로 채운다. **마스크의 빨강·초록은 영역 구분용이며 실제 출력색은 선택 블록의 색이다.**
3. 외곽선까지 포함해 물품 면을 마스크로 채워도 된다. 원본 RGB에 색상을 곱하므로 검정 선은 검정으로 유지되고 회색 선·명암에는 같은 비율로 색이 반영된다. 배경·렌즈 구멍 등은 검정 마스크로 남긴다.
4. 마스크 알파는 255를 권장한다. 반투명 마스크는 색칠 강도를 줄인다. 빨강+초록 채널 합계는 최대255, 경계 반올림 오차256까지 허용한다. 노랑·흰색 마스크처럼 두 영역이 완전히 겹치는 데이터는 오류로 처리한다. 파랑 채널은 사용하지 않는다.
5. PNG는 각 변1~2048px, 파일당16MiB 이하. 원본·마스크 크기가 달라지면 오류다. 경로는 `Coloring/` 아래 상대 PNG 경로만 허용한다. URL·상위 폴더 경로는 거부한다.
6. 파일과 `.meta`를 함께 관리한다. 설정 변경 후 Play/앱을 다시 시작한다. 런타임에 PNG를 직접 읽으므로 Texture Importer의 Read/Write 설정은 필요 없다.

색상은 같은 설정의 colors에서 `id`와 `#RRGGBB` 값을 지정한다. Black/White/Blue/Red/Green/Yellow 블록 ID는 기존 BlockCatalog와 일치해야 한다. 기본값은 `#000000/#FFFFFF/#2068E8/#E83030/#27AD55/#FFD52A`이다. 프롬프트 문구는 색칠에 사용하지 않는다.

Equipment도 나잇대별로 설정한다. 현재 샘플은 Child=robots, Student=laptops, Working Adult=keyboards, Senior=laptops. `productId`는 equipment를 유지하고 `resolvedProductId`를 robots/keyboards/laptops 중 하나로 지정한다. 임의 추첨을 하지 않아 색상만 바꿀 때 외형이 바뀌지 않는다. 일반 물품의 resolvedProductId는 productId와 같아야 한다.

## 수명·실패 처리

`BlockExperienceController`가 비동기 설정 로드 후 `LocalBlockColoring.RenderAsync`를 호출한다. 원본/마스크를 로컬 파일로 읽고, 픽셀 계산은 작업 스레드에서 처리하며 Unity 객체 생성·해제는 메인 스레드에서 수행한다. 매 프레임 색칠하지 않는다. 원본 PNG·배열은 수정하지 않는다.

선택 변경·취소·화면 비활성화·세션 복귀에서 작업 토큰을 취소한다. 선택 revision이 달라진 결과는 거부한다. 중간 텍스처는 해제하며 최종 텍스처만 컨트롤러 또는 GameManager가 소유한다. Result 전환 시 유지하고 Idle에서 파괴한다. 타이머는 색칠 중에도 계속 동작한다.

파일 누락·PNG 해독 실패·크기 불일치·마스크 오류는 영문 상태와 원본 예외 로그로 확인한다. 오래된 이미지를 지우고 안내 상태를 유지한다. 에디터 시뮬레이터의 **Apply colors / Retry**, 기존 `GenerateImage()` UnityEvent, 또는 블록 재선택으로 재시도한다. Cancel coloring과 `CancelGeneration()`으로 중단한다.

선택 저널의 기존 제한(30파일, 파일당4MiB)은 유지한다. 성공 시 `coloring_completed`에 선택/revision/확정 물품/원본·마스크 경로/두 RGB/결과 크기를 기록한다. 이미지 자체는 디스크에 누적 저장하지 않는다. 서버용 generation_* 이벤트는 발행하지 않는다.

## 개발용 샘플 재생성·검증

Pillow가 있는 Python으로 `Tools/Coloring/build_samples.py`를 실행한다. 자체 도형 코드로 `Coloring/Samples`의 48쌍을 재생성하며 기존 `BlockColoring.json`은 덮어쓰지 않는다. 사용자 최종 삽화는 Samples 대신 Artwork 등 별도 폴더에 보관한다. 샘플 제작 도구는 Unity/배포 실행에 필요하지 않다.

EditMode의 `BlockColoringTests`는 두 영역·선·명암·알파·흰색/검정색·취소·잘못된 설정/마스크 및 모든 샘플 파일을 검증한다. `Tools/Coloring/verify_runtime.cs`는 Idle에서 Play한 상태로 Unity MCP execute_code에 메서드 본문으로 전달하는 개발용 실행 확인이다. 이 검증은 의도적으로 누락 경로를 메모리에 넣어 오류 메시지와 원본 예외 로그를 발생시킨 뒤 복구한다. 저장소 원본 PNG/설정은 변경하지 않는다. 실행 결과는 `docs/validation/coloring_runtime.json`에 기록한다.

실제 수행한 검증·남은 배포/장비 검수는 [HANDOFF](agents/HANDOFF.md)의 COLOR-001 기록을 확인한다.
