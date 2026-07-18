---
description: Запускает только фазу code review для уже существующих изменений (без реализации). Проверяет код по Common Mistakes и архитектурным правилам.
agent: conductor
subtask: true
---

Запусти независимое код-ревью текущих изменений.

Процесс:
1. Прочитай WORKFLOW_STATE.md (если есть)
2. Получи diff изменений через `git diff main...HEAD`
3. Запусти reviewer с diff
4. Обнови WORKFLOW_STATE.md секция Review
5. Покажи результат пользователю
