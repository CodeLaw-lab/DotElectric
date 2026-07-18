---
description: Запускает быстрый pipeline без Critic: implement → test → review → PR. Используй для hotfix'ов, багов, простых UI-правок, типовых задач.
agent: conductor
subtask: true
---

Запусти быстрый pipeline (quick) для: $ARGUMENTS

Режим: quick (без Critic)

Фазы:
1. **Implement** — реализуй
2. **Test** — тесты
3. **Review** — ревью
4. **PR** — Pull Request

WORKFLOW_STATE.md в `.opencode/workflow/current/`.
Retry: 3 цикла auto-fix, затем escalate пользователю.
