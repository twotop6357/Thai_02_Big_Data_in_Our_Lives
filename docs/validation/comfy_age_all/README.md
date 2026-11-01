# AGE-GALLERY-001 — 전체10물건/4연령 비교

현재 승인된SDXL-Turbo4/512 설정으로10종×4연령40개를 한 장에 배치했다. [전체 이미지](all_products_by_age.png),2248×5330 PNG. 열은 아동/학생/직장인/시니어,행은 가방/신발/모자/선글라스/헤드폰/상의/자전거/텀블러/손목시계/우산이다.

색 입력red/blue와시드1206501을고정해연령디자인조건만바꿨다. 현재API해시가동일한기존16장+추가생성24장이다. report.json에요청ID/입력/실제시간/PNG해시를보존했다. 보기좋은시드로교체하거나내용을수정하지않았다. 일부항목의연령차이는작고일부출력에는잔여부분/잘린부분이보일수있다.

Unity MCP는Transport closed로사용할수없어에디터/씬조작없이프로젝트GeneratedImageFraming.cs의원본순수배열코드를PowerShell Add-Type으로실행했다. Color32는동일4바이트필드/생성자shim을쓰고Unity의상하행순서를맞췄다. 기존Unity프레이밍16장과픽셀배열이완전히같음을확인했다. helper해시와호스트는framing_method.json/framing_host.cs에보존했다. 새Unity실행/컴파일검증으로보고하지않는다.

40/40셀·모든물건/연령조합·색/시드·PNG무결성확인및상/하반부직접검토. 원본RGBA는본폴더,전경선택/중앙보정결과는framed/. 모델/카탈로그/앱코드/씬은변경하지않았다. 증거verification.json/배치layout.json. 원시RGBA중간자료는삭제하지않고보존한다.
