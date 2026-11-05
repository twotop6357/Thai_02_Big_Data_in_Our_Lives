# 블록 이미지 교체

- 영어 설정: `blocks_en.json`
- 태국어 설정: `blocks_th.json`
- 영어 이미지: `Images/en/*_Eng.png`
- 태국어 이미지: `Images/th/*_Thai.png`

같은 파일명으로 이미지를 덮어쓰거나 JSON의 `image`를 새 이미지의 상대 경로로 바꾸세요. 예:

```json
{ "kind": "product", "id": "bags", "image": "Images/th/BagBlock_Thai.png" }
```

`image`는 이 BlockArtwork 폴더 기준 경로입니다. PNG/JPG/JPEG를 지원하며 투명도를 유지하고 원래 비율로 표시합니다. 기존 카드와 같은 형태를 원하면 물건/색상은252×252, 연령은261×261로 준비하세요. 파일은16MiB 이하, 가로/세로4096픽셀 이하로 사용하세요.

`kind`, `id`, `language`, `schemaVersion`은 인식 연결용이므로 유지하세요. 태국어 연령4개는 아직 번역 이미지가 없어 영어 이미지 경로를 명시해 두었습니다. 태국어 파일을 추가한 뒤 해당 경로만 바꾸면 됩니다. 언어별로 같은 이미지를 공유해도 됩니다.

앱 시작 시와 홈으로 돌아올 때 다시 읽습니다. 체험 중 파일을 바꿨다면 홈으로 돌아온 뒤 다음 체험을 시작하세요. 빌드된 앱에서는 `앱이름_Data/StreamingAssets/BlockArtwork`의 파일을 수정하면 됩니다. Unity 재빌드는 필요 없습니다.

잘못된 JSON은 해당 언어의 기본 이미지로, 읽지 못한 이미지 한 개는 해당 블록의 기본 이미지로 복구하고 영문 오류/원래 예외를 기록합니다. 인식 ID나 생성 색상은 이미지 교체로 바뀌지 않습니다. 생성 색상을 바꾸려면 `../BlockGeneration/BlockCatalog.json`의 대응 색상 HEX/설정을 별도로 수정하세요.
