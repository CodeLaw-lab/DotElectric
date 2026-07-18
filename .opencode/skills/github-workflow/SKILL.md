---
name: github-workflow
description: Use when performing git and GitHub operations — branch management, commits, pull requests, GitHub CLI operations, and repository management.
---

# GitHub Workflow — Инструкции для git/GitHub операций

Ты — gh-ops агент. Твоя задача — все операции с git и GitHub.

## 1. Branch naming

```
feature/<short-description>   # новая функциональность
fix/<short-description>        # исправление бага
refactor/<short-description>   # рефакторинг
test/<short-description>       # добавление тестов
docs/<short-description>       # документация
```

Всегда создавай branch от `main` (или `master`, смотря что актуально).

## 2. Commits

### Convention (Conventional Commits)

```
<type>(<scope>): <description>

[body]

[footer]
```

### Types
- `feat` — новая функциональность
- `fix` — исправление бага
- `refactor` — рефакторинг без изменения функциональности
- `test` — добавление/изменение тестов
- `docs` — документация
- `chore` — сборка, CI, зависимости

### Scope (опционально)
- `wpf` — UI/XAML changes
- `vm` — ViewModel changes
- `model` — domain model changes
- `service` — service layer
- `test` — test infrastructure
- `ci` — CI/CD changes

### Examples
```
feat(vm): add ExportToPdf command
fix(model): correct ContainsPoint rotation for 90° angles
refactor(service): extract IDialogFileService from FileService
test(wpf): add STA-infrastructure tests for TextBoxLostFocusBehavior
```

## 3. Pull Requests

### Создание PR через `gh`

```powershell
gh pr create `
  --base main `
  --head feature/my-feature `
  --title "feat(vm): add ExportToPdf command" `
  --body "## Description
...
## Related issues
Closes #123"
```

### PR body template

```markdown
## Description
<что сделано>

## Type of change
- [ ] feat
- [ ] fix
- [ ] refactor
- [ ] test
- [ ] docs

## Checklist
- [ ] Build: 0 errors, 0 warnings
- [ ] Tests: all pass
- [ ] Coverage: ≥75%
- [ ] AGENTS.md updated
- [ ] Changelog updated

## Pipeline
pipeline: full | quick
critic: approved | changes-requested
```

## 4. GitHub CLI — установка и авторизация

### Проверка
```powershell
gh auth status
```

### Установка (если `gh` не найден)
```powershell
winget install --id GitHub.cli
```
или скачай с https://cli.github.com/

### Авторизация с браузером
```powershell
gh auth login
# Выбери: GitHub.com → HTTPS → Login with a browser
```

### Авторизация с токеном
```powershell
gh auth login --with-token
# (вставь Personal Access Token из stdin)
```

Требуемые scopes для `gh-ops`: `repo`, `workflow`, `pull-requests: write`.

### Fallback (если авторизация не удалась)
1. Запиши ошибку в WORKFLOW_STATE.md
2. Сообщи conductor: "GitHub CLI не авторизован. Выполни `gh auth login`."
3. Pipeline приостанавливается до ручной авторизации

## 5. Рецензирование через `gh`

```powershell
# Добавить review комментарий
gh pr review <number> --comment --body "..."

# Approve
gh pr review <number> --approve

# Request changes
gh pr review <number> --request-changes --body "..."
```

## 5. Merge

```powershell
# Squash and merge
gh pr merge <number> --squash

# или rebase
gh pr merge <number> --rebase
```

## 6. CI pipeline (GitHub Actions)

Не редактируй CI workflow вручную — CI запускается автоматически.
Если CI упал:
1. Проверь вывод `gh run view <run-id> --log`
2. Если проблема в тестах → вернись к tester
3. Если проблема в билде → вернись к implementor

## 7. Чеклист перед PR

- [ ] Branch создан от main
- [ ] Commits следуют Conventional Commits
- [ ] Все изменения закоммичены (git status чист)
- [ ] PR body заполнен по шаблону
- [ ] gh CLI авторизован (`gh auth status`)
