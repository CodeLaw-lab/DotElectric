---
description: Запускает полный pipeline разработки: plan → implement → test → review → docs → verify → critic → PR. Используй для нового функционала, архитектурных изменений, сложных фич.
agent: conductor
subtask: true
---

Запусти полный pipeline (full) для: $ARGUMENTS

Режим: full (с Critic в конце)

Фазы:
1. **Plan** — исследуй код, напиши план
2. **Implement** — реализуй по плану
3. **Test** — напиши и запусти тесты
4. **Review** — независимое код-ревью
5. **Docs** — обнови ВСЮ документацию (documenter subagent + DOCS_MANIFEST.md)
6. **Verify** — verification gate: проверить CONSISTENCY_REPORT.md
7. **Critic** — финальная проверка перед PR (включая Documentation Consistency gate)
8. **PR** — создай feature-branch и Pull Request

WORKFLOW_STATE.md в `.opencode/workflow/current/`.
Consistency report: `.opencode/workflow/current/CONSISTENCY_REPORT.md`.
Retry: 3 цикла auto-fix, затем escalate пользователю.
