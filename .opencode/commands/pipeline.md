---
description: Запускает полный pipeline разработки: plan → implement → test → review → docs → critic → PR. Используй для нового функционала, архитектурных изменений, сложных фич.
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
5. **Docs** — обнови документацию
6. **Critic** — финальная проверка перед PR
7. **PR** — создай feature-branch и Pull Request

WORKFLOW_STATE.md в `.opencode/workflow/current/`.
Retry: 3 цикла auto-fix, затем escalate пользователю.
