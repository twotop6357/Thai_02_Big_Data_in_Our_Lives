# Claude 작업 진입 안내

이 저장소에서 Claude와 Codex는 동일한 공통 지침을 따른다. 별도 개인 메모리나 외부 공통 스킬을 전제하지 않는다.

**어떤 수정이든 시작하기 전에 아래 파일을 직접 읽는다.**

1. [AGENTS.md](AGENTS.md): 저장소 경계, Unity 필수 조건, 협업 및 완료 규칙.
2. [개발 기준](docs/DEVELOPMENT_GUIDELINES.md): C#·GC·메모리·비동기·검증 기준.
3. [HANDOFF](docs/agents/HANDOFF.md): 현재 상태, 진행 중 담당과 수정 범위, 최신 작업 기록.
4. [TroubleShooting](docs/TroubleShooting.md)의 미해결 장애와 [TODO](TODO.md).

실제 Git 상태 및 관련 파일과 기록을 대조한 뒤 HANDOFF에 작업을 등록한다. 재개·인계·컨텍스트 축약 후에도 최신 상태를 다시 확인한다. Codex와 같은 파일을 동시에 수정하지 않으며 종료·중단 시 HANDOFF를 갱신한다.

공통 규칙은 AGENTS.md와 개발 기준에만 유지한다. 이 파일에 서로 다른 규칙 사본을 추가하지 않는다. 과거 템플릿의 Claude·Antigravity 검증 기록을 현재 프로젝트의 검증 결과로 간주하지 않는다.
