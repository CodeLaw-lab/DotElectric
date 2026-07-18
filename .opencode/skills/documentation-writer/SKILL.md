---
name: documentation-writer
description: Use when updating project documentation — AGENTS.md, changelog, docs/ files, sprint reports, and spec documents in a WPF/MVVM project.
---

# Documentation Writer — Инструкции по обновлению документации

Ты отвечаешь за поддержание документации проекта в актуальном состоянии.

## 1. Какие файлы обновлять

| Файл | Когда обновлять | Формат |
|------|----------------|--------|
| `AGENTS.md` | Всегда — любое изменение | Markdown |
| `docs/<номер>_<название>.md` | Спецификация, архитектура | Markdown |
| `CHANGELOG.md` | Всегда, если нет — создать | Keep a Changelog |
| `README.md` | Изменение сборки, зависимостей, структуры | Markdown |

## 2. AGENTS.md — правила обновления

### Структура

```
# AGENTS.md — <Project Name>

## Current Focus
- последние успешные изменения
- build/tests статус

## Sprint <N> — <краткое название>
### Fix/Feature <ID>: <Описание>
**Проблема:** <что было не так>
**Исправление:** <что сделали>
**Файлы:** <перечень>
**Build:** <статус>
**Tests:** <статус>
```

### Common Mistakes — добавление
Если в изменении выявлено новое правило, которое стоит запомнить — добавь его:
1. В секцию `## Common Mistakes to Avoid` в конце списка
2. С номером (следующий по порядку)
3. Формат: `N. <правило> — <explanation>`

### Build/Tests статус
Всегда обновляй строки:
```markdown
**Build:** 0 errors, 0 warnings
**Tests:** <count> passed, <skip> pre-existing skip
```

## 3. Changelog — Keep a Changelog

```markdown
# Changelog

## [Unreleased]

### Added
- ...

### Changed
- ...

### Fixed
- ...

### Removed
- ...
```

### Типы изменений
| Тип | Что |
|-----|-----|
| Added | Новая функциональность |
| Changed | Изменение существующего API/поведения |
| Fixed | Исправление бага |
| Removed | Удаление функциональности |
| Deprecated | Помечено на удаление |

## 4. Sprint reports (docs/)

Если требуется — обнови:
- `docs/00_Индекс_документов.md` — добавить новый документ в индекс
- Статус проекта — обновить счётчики тестов, build status

## 5. НЕ делай

- Не создавай новые файлы документации без явной необходимости
- Не добавляй emoji в документацию
- Не переписывай историю Sprint'ов (секции в AGENTS.md — это история, её не редактируют)
- Не меняй формат существующих документов

## 6. Чеклист перед завершением

- [ ] AGENTS.md: Sprint секция добавлена (если это новый Sprint)
- [ ] AGENTS.md: Common Mistakes обновлён (если есть новое правило)
- [ ] AGENTS.md: Build/Tests статус актуален
- [ ] CHANGELOG.md: запись есть
- [ ] docs/: связанные документы обновлены
- [ ] README.md: изменения не требуются (или обновлён)
