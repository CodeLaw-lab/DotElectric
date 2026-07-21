---
name: documentation-writer
description: Use when updating project documentation — AGENTS.md, changelog, docs/ files, sprint reports, and spec documents in a WPF/MVVM project. Enforces cross-source consistency using DOCS_MANIFEST.md.
---

# Documentation Writer — Инструкции по обновлению документации

Ты отвечаешь за поддержание документации проекта в **полностью консистентном** состоянии. Обновление только очевидных файлов (AGENTS.md, CHANGELOG.md) — **недопустимо**. Используй `.opencode/DOCS_MANIFEST.md` как single source of truth.

## 0. Обязательный manifest

**Перед любым обновлением** прочитай `.opencode/DOCS_MANIFEST.md`:
- Список 14 зарегистрированных файлов (не считая архив)
- 6 метрик (test_count, coverage, sprint_id, build_status, last_update, fr_coverage)
- 8 consistency rules
- Grep-команды для верификации
- Anti-patterns

**Все 14 файлов** обязаны обновляться согласованно когда изменение касается их scope.

---

## 1. 5-step алгоритм (ОБЯЗАТЕЛЬНЫЙ)

### Шаг 1: DISCOVER
- Прочитай `WORKFLOW_STATE.md` → секция Implementation + Tests (знай что изменилось)
- Прочитай `DOCS_MANIFEST.md` → какие файлы могут быть затронуты
- Составь список файлов которые **точно** нужно обновить

### Шаг 2: GREP (до изменений)
Запусти ВСЕ команды из манифеста секция 5. Запиши результаты — это baseline.

```powershell
rg -n '\b\d{3,4}\s*(тест(ов|а|)|passed|tests)\b' AGENTS.md CHANGELOG.md docs/00_Индекс_документов.md docs/19_Статус_проекта.md
rg -n '\b\d{1,3}\.?\d*\s*%' AGENTS.md CHANGELOG.md docs/00_Индекс_документов.md docs/19_Статус_проекта.md
rg -n 'Sprint\s+[A-Z0-9.]+' AGENTS.md CHANGELOG.md docs/19_Статус_проекта.md
rg -n 'Build:\s*\d+\s*errors?,\s*\d+\s*warnings?' AGENTS.md
rg -n '\b\d{2}\.\d{2}\.\d{4}\b' docs/00_Индекс_документов.md docs/19_Статус_проекта.md CHANGELOG.md
```

### Шаг 3: COMPARE
Построй таблицу "источник → значение" для каждой метрики. Найди расхождения.

### Шаг 4: UPDATE
Обнови **ВСЕ** источники до согласованных значений. **Не останавливайся** после AGENTS.md + CHANGELOG.md.

Типичные каскады:
- Sprint N добавлен → AGENTS.md (Sprint секция) + AGENTS.md (Current Focus) + CHANGELOG.md [Unreleased] + docs/19 (sprint list) + docs/19 (FR coverage) + docs/00 (status)
- test_count изменился → AGENTS.md + CHANGELOG.md + docs/19 + docs/00
- coverage изменился → AGENTS.md + CHANGELOG.md + docs/19 + docs/00
- Новая фича → README.md (если user-facing) + docs/01-49 (по scope) + AGENTS.md (Sprint)

### Шаг 5: VERIFY
Повтори ВСЕ grep из шага 2. **Все значения должны совпадать** во всех required sources.

Создай `.opencode/workflow/current/CONSISTENCY_REPORT.md` (формат в манифесте секция 6).

---

## 2. Какие файлы обновлять

| Файл | Когда обновлять | Формат |
|------|----------------|--------|
| `AGENTS.md` | Любое изменение (Sprint секция, Current Focus, Common Mistakes) | Markdown |
| `CHANGELOG.md` | Любое изменение (Added/Changed/Fixed в [Unreleased]) | Keep a Changelog |
| `README.md` | Изменение сборки, зависимостей, структуры, user-facing фичи | Markdown |
| `docs/00_Индекс_документов.md` | Изменение даты, статуса, тестов, coverage, добавление файла | Markdown |
| `docs/19_Статус_проекта.md` | Изменение тестов, coverage, sprint list, FR coverage | Markdown |
| `docs/01_Техническое_задание_Этап1.md` | Изменение требований, новые FR | Markdown |
| `docs/02_User_Stories_Этап1.md` | Новые US, изменение acceptance criteria | Markdown |
| `docs/03_Спецификация_требований_Этап1.md` | Архитектурные изменения, новая валидация (V-XXX) | Markdown |
| `docs/05_Руководство_пользователя_черновик.md` | Новые user-facing фичи, hotkeys, настройки | Markdown |
| `docs/09_UI_решения.md` | Изменения layout, диалогов, XAML-binding | Markdown |
| `docs/47_План_развития_Этап2.md` | Новые FR в backlog | Markdown |
| `docs/48_Архитектурный_анализ_и_план_рефакторинга.md` | Архитектурные изменения, новые замечания | Markdown |
| `docs/49_План_рефакторинга_R1-R4.md` | Детальный план (по sprint) | Markdown |
| `docs/План_ручного_тестирования.md` | Новые test scenarios, FR coverage | Markdown |

---

## 3. AGENTS.md — правила обновления

### Структура Sprint секции

```markdown
## Sprint <N> — <краткое название>

### Fix/Feature <ID>: <Описание>
**Проблема:** <что было не так>
**Исправление:** <что сделали>
**Файлы:** <перечень>
**Build:** <статус>
**Tests:** <статус>
```

### Common Mistakes — добавление
Если в изменении выявлено новое правило:
1. В секцию `## Common Mistakes to Avoid` в конце списка
2. С номером (следующий по порядку)
3. Формат: `N. <правило> — <explanation>`

### Build/Tests статус
В **Current Focus** секции всегда:
```markdown
**Build:** 0 errors, 0 warnings
**Tests:** <count> passed, <skip> pre-existing skip
```

---

## 4. Changelog — Keep a Changelog

```markdown
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

---

## 5. НЕ делай (anti-patterns)

- ❌ Не обновляй только AGENTS.md + CHANGELOG.md — это **главная ошибка** которую мы устраняем
- ❌ Не забывай docs/19_Статус_проекта.md — там test_count и sprint_list
- ❌ Не забывай docs/00_Индекс_документов.md — там дата и статус
- ❌ Не хардкодь дату вручную — используй `Get-Date -Format "dd.MM.yyyy"`
- ❌ Не удаляй Sprint секции в AGENTS.md (история)
- ❌ Не создавай файлы в docs/ без обновления 00_Индекс
- ❌ Не добавляй FR-XXX в ТЗ без записи в US или backlog
- ❌ Не переписывай файлы целиком — используй `edit` для точечных правок
- ❌ Не меняй формат существующих Sprint секций
- ❌ Не добавляй emoji в документацию
- ❌ Не правь docs/archive/

---

## 6. Чеклист (12 пунктов — обязательный)

Перед завершением:

- [ ] 1. Прочитал DOCS_MANIFEST.md
- [ ] 2. Прочитал WORKFLOW_STATE.md (знаю scope изменений)
- [ ] 3. Запустил GREP до изменений (baseline)
- [ ] 4. Построил comparison table
- [ ] 5. Обновил AGENTS.md (Sprint секция + Current Focus)
- [ ] 6. Обновил CHANGELOG.md ([Unreleased])
- [ ] 7. Обновил docs/19_Статус_проекта.md (если метрики)
- [ ] 8. Обновил docs/00_Индекс_документов.md (дата/статус)
- [ ] 9. Обновил релевантные docs/01-49 (по scope)
- [ ] 10. Обновил README.md (если user-facing)
- [ ] 11. Запустил GREP после изменений (всё совпадает)
- [ ] 12. Создал CONSISTENCY_REPORT.md со статусом PASS

Если хотя бы один пункт не выполнен → status: FAIL → возврат на Phase 5.

---

## 7. Возврат результата conductor'у

```markdown
## Documentation Update Report

### Status
PASS | FAIL

### Files changed
| File | Change type | Lines |
|------|-------------|-------|
| AGENTS.md | modified | +5 -2 |
| CHANGELOG.md | modified | +12 -0 |
| docs/19_Статус_проекта.md | modified | +3 -1 |
| docs/00_Индекс_документов.md | modified | +1 -1 |

### Metrics verified
- test_count: 2069 (4/4 sources match)
- coverage: 75.3% (4/4 sources match)
- sprint_list: Sprint 61 added (3/3 sources match)
- last_update: 20.07.2026 (3/3 sources match)

### Consistency report
Файл: .opencode/workflow/current/CONSISTENCY_REPORT.md
Статус: PASS

### Mismatches (если FAIL)
- <file>:<line> — <metric>=<value>, expected=<value>
```
