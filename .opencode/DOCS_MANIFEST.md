# Documentation Manifest

**Назначение:** Single source of truth для консистентности всей документации проекта. Используется `documenter` subagent на фазе 5 (Docs) пайплайна. Conductor проверяет результат на фазе 5.5. Critic валидирует как CRITICAL gate.

**Обновляется:** при добавлении/удалении документа в проекте, при изменении формата метрик.

---

## 1. Зарегистрированные файлы

| # | Path | Category | Что содержит | Last verified |
|---|------|----------|--------------|---------------|
| 1 | `AGENTS.md` | status | Sprint history, Common Mistakes, build status, test count, coverage | 2026-08-14 |
| 2 | `CHANGELOG.md` | release | [Unreleased] секция, Added/Changed/Fixed/Removed, метрики | 2026-08-14 |
| 3 | `README.md` | readme | Описание проекта, build, dependencies, structure | 2026-07-20 |
| 4 | `docs/00_Индекс_документов.md` | index | Реестр всех docs/ файлов, метрики, даты, статус | 2026-08-14 |
| 5 | `docs/01_Техническое_задание_Этап1.md` | spec | FR-001 — FR-074, критерии приёмки, требования | 2026-07-20 |
| 6 | `docs/02_User_Stories_Этап1.md` | spec | US-1.1 — US-7.x, 25 stories в 7 эпиках | 2026-07-20 |
| 7 | `docs/03_Спецификация_требований_Этап1.md` | spec | Архитектура, V-XXX (валидация), модель данных | 2026-07-20 |
| 8 | `docs/05_Руководство_пользователя_черновик.md` | user | Инструкции для пользователя, hotkeys, FR-XXX | 2026-07-20 |
| 9 | `docs/09_UI_решения.md` | ui | Layout, Material Design, диалоги, XAML-binding | 2026-07-20 |
| 10 | `docs/19_Статус_проекта.md` | status | Test count, coverage, sprint list, FR coverage, динамика | 2026-08-14 |
| 11 | `docs/47_План_развития_Этап2.md` | roadmap | Этап 2 направления, FR-XXX | 2026-07-20 |
| 12 | `docs/48_Архитектурный_анализ_и_план_рефакторинга.md` | arch | 25 замечаний, file:line, метрики | 2026-07-20 |
| 13 | `docs/49_План_рефакторинга_R1-R4.md` | arch | 30 задач, file:line, XAML-маппинги | 2026-07-20 |
| 14 | `docs/План_ручного_тестирования.md` | test | Manual test scenarios, FR-XXX | 2026-07-20 |
| 15 | `docs/plan_next_steps.md` | roadmap | Следующие шаги, backlog, follow-ups; scope: test_count, coverage | 2026-08-01 |

**Исключения** (не требуют консистентности):
- `docs/archive/` — исторические материалы, заморожены
- `docs/*/bin/`, `docs/*/obj/` — build artifacts (не коммитятся)
- `WORKFLOW_STATE.md` (`.opencode/workflow/current/`) — runtime state, не документация

---

## 2. Метрики (cross-source consistency)

Каждая метрика обязана иметь **одинаковое значение** во всех `Required sources`. Documenter обязан проверить и обновить все источники согласованно.

### 2.1. Test count

| Aspect | Value |
|--------|-------|
| Pattern | `\b\d{3,4}\s*(тест(ов|а|)|passed|tests)\b` |
| Required sources | `AGENTS.md`, `CHANGELOG.md`, `docs/19_Статус_проекта.md`, `docs/00_Индекс_документов.md` |
| Example | "2035 passed", "2069 tests" |
| Skip count | `\d+\s*(pre-existing\s*)?skip` — должен быть указан если > 0 |

### 2.2. Coverage (line-rate)

| Aspect | Value |
|--------|-------|
| Pattern | `\b\d{1,3}\.?\d*\s*%\s*(line-rate\|покрыти\|coverage)?` |
| Required sources | `AGENTS.md`, `CHANGELOG.md`, `docs/19_Статус_проекта.md`, `docs/00_Индекс_документов.md` |
| Example | "75.3%", "75.15%" |
| Range | обычно 70% — 85% |

### 2.3. Sprint ID

| Aspect | Value |
|--------|-------|
| Pattern | `Sprint\s+([A-Z0-9.]+)` |
| Required sources | `AGENTS.md`, `CHANGELOG.md`, `docs/19_Статус_проекта.md` |
| Example | "Sprint 60", "Sprint R3.1", "Sprint STA" |
| Rule | Каждый Sprint N в AGENTS.md должен быть в CHANGELOG.md [Unreleased] |

### 2.4. Build status

| Aspect | Value |
|--------|-------|
| Pattern | `Build:\s*\d+\s*errors?,\s*\d+\s*warnings?` |
| Required sources | `AGENTS.md` (только Current Focus) |
| Example | "Build: 0 errors, 0 warnings" |
| Rule | Значение в Current Focus = последний известный build |

### 2.5. Last update date

| Aspect | Value |
|--------|-------|
| Pattern | `\b\d{2}\.\d{2}\.\d{4}\b` |
| Required sources | `docs/00_Индекс_документов.md`, `docs/19_Статус_проекта.md`, `CHANGELOG.md` |
| Example | "20.07.2026" |
| Rule | Должна совпадать во всех 3 источниках в пределах 1 дня |

### 2.6. FR coverage

| Aspect | Value |
|--------|-------|
| Pattern | `FR-\d{3}` |
| Required sources | `docs/01_*`, `docs/02_*`, `docs/05_*`, `docs/19_*` |
| Rule | FR-XXX упомянутый в ТЗ (01) должен быть либо реализован, либо в backlog (47) |

---

## 3. Consistency rules (обязательные)

При обновлении документации documenter обязан обеспечить выполнение **всех** правил:

| # | Rule | Severity (если нарушено) |
|---|------|-------------------------|
| 1 | `test_count` одинаков во всех 4 required sources | CRITICAL |
| 2 | `coverage` одинаков во всех 4 required sources | CRITICAL |
| 3 | Каждый Sprint N в AGENTS.md (Current Focus или Sprint section) — в CHANGELOG.md [Unreleased] | MAJOR |
| 4 | Каждый файл в `docs/` перечислен в `00_Индекс_документов.md` | MAJOR |
| 5 | `last_update` дата одинакова в docs/00, docs/19, CHANGELOG.md (±1 день) | MAJOR |
| 6 | Нет "мёртвых" ссылок: Sprint без CHANGELOG записи, FR-XXX без US, US без FR | MAJOR |
| 7 | `Build: 0 errors, X warnings` в AGENTS.md Current Focus актуален | MINOR |
| 8 | Common Mistakes в AGENTS.md пронумерованы подряд (1, 2, 3, …) | MINOR |

---

## 4. Anti-patterns (запрещено)

Documenter **не должен**:

- ❌ Обновлять только `AGENTS.md`, забыв `docs/19_Статус_проекта.md` и `00_Индекс_документов.md`
- ❌ Обновлять только `CHANGELOG.md`, забыв проектные docs
- ❌ Хардкодить сегодняшнюю дату вручную — использовать `Get-Date -Format "dd.MM.yyyy"` или дату из git log последнего коммита
- ❌ Удалять Sprint секцию в AGENTS.md вместо пометки её как исторической
- ❌ Добавлять FR-XXX в `01_Техническое_задание_Этап1.md` без записи в `02_User_Stories_Этап1.md` или `47_План_развития_Этап2.md`
- ❌ Создавать новые файлы в `docs/` без обновления `00_Индекс_документов.md`
- ❌ Менять формат существующих Sprint секций (только добавлять новые в конец)
- ❌ Удалять/изменять секции архива `docs/archive/`

---

## 5. Grep-команды для верификации

Используются documenter'ом в фазе VERIFY и critic'ом для финальной проверки.

### Test count
```powershell
rg -n '\b\d{3,4}\s*(тест(ов|а|)|passed|tests)\b' AGENTS.md CHANGELOG.md docs/00_Индекс_документов.md docs/19_Статус_проекта.md
```

### Coverage
```powershell
rg -n '\b\d{1,3}\.?\d*\s*%' AGENTS.md CHANGELOG.md docs/00_Индекс_документов.md docs/19_Статус_проекта.md
```

### Sprints в AGENTS.md
```powershell
rg -n 'Sprint\s+[A-Z0-9.]+' AGENTS.md
```

### Sprints в CHANGELOG.md
```powershell
rg -n 'Sprint\s+[A-Z0-9.]+' CHANGELOG.md
```

### Build status
```powershell
rg -n 'Build:\s*\d+\s*errors?,\s*\d+\s*warnings?' AGENTS.md
```

### Last update dates
```powershell
rg -n '\b\d{2}\.\d{2}\.\d{4}\b' docs/00_Индекс_документов.md docs/19_Статус_проекта.md CHANGELOG.md
```

### Files in docs/ vs index
```powershell
# Файлы в docs/ (исключая archive):
rg --files docs/ | rg -v '^docs/archive/' | rg -v '\.(bin|obj)/'
# Файлы перечисленные в 00_Индекс:
rg -o '`docs/[^`]+`' docs/00_Индекс_документов.md
```

### FR-XXX coverage
```powershell
rg -on 'FR-\d{3}' docs/01_Техническое_задание_Этап1.md | Sort-Object -Unique
rg -on 'FR-\d{3}' docs/02_User_Stories_Этап1.md | Sort-Object -Unique
rg -on 'FR-\d{3}' docs/19_Статус_проекта.md | Sort-Object -Unique
```

---

## 6. Формат отчёта (CONSISTENCY_REPORT.md)

Documenter создаёт `.opencode/workflow/current/CONSISTENCY_REPORT.md` со следующей структурой:

```markdown
# Documentation Consistency Report

**Workflow:** <description from WORKFLOW_STATE.md>
**Generated:** <dd.MM.yyyy HH:mm>
**Documenter:** <agent version>

## Status
PASS | FAIL

## Metrics table
| Metric | AGENTS.md | CHANGELOG.md | docs/19 | docs/00 | Match? |
|--------|-----------|--------------|---------|---------|--------|
| test_count | 2069 | 2069 | 2069 | 2069 | ✅ |
| coverage | 75.3% | 75.3% | 75.3% | 75.3% | ✅ |
| sprint_count | 61 | 61 | 61 | N/A | ✅ |
| last_update | N/A | 20.07.2026 | 20.07.2026 | 20.07.2026 | ✅ |

## Files changed
| File | Lines | Description |
|------|-------|-------------|
| AGENTS.md | +5 -2 | Sprint 61 section added |
| CHANGELOG.md | +12 -0 | Sprint 60/61 entries |
| docs/19_Статус_проекта.md | +3 -1 | Test count 2069 |
| docs/00_Индекс_документов.md | +1 -1 | Date updated |

## Mismatches found
- (если есть) список расхождений

## Cross-references verified
- [x] 14 файлов в docs/ перечислены в 00_Индекс
- [x] Все Sprint N в AGENTS.md имеют CHANGELOG запись
- [x] Нет мёртвых ссылок

## Conclusion
- Если PASS → "Все метрики консистентны. Ready for Critic."
- Если FAIL → "Найдены расхождения: <count>. Требуется доработка."
```

---

## 7. История изменений манифеста

| Date | Change | Reason |
|------|--------|--------|
| 2026-07-20 | Initial creation (15 files, 6 metrics, 8 rules) | Phase 5 docs consistency fix |
| 2026-08-01 | Removed `docs/1.md` (orphan deleted), registered `docs/plan_next_steps.md` | Docs hygiene |
| 2026-08-10 | Last verified обновлён для AGENTS.md, CHANGELOG.md, docs/00, docs/19 (Sprint "AppSettings → GridSettings chain": test_count 2160, coverage 76.4%) | Sprint docs sync |
| 2026-08-14 | Last verified обновлён для AGENTS.md, CHANGELOG.md, docs/00, docs/19 (Coverage series + Docs/CI: test_count 2515, coverage 88.45%, CI gate 80%) | Sprint docs sync |
| 2026-08-14 | Last verified обновлён для AGENTS.md, CHANGELOG.md, docs/00, docs/19 (Sprint "Coverage weak zones": test_count 2636, coverage 90.18%, 6 слабых зон закрыты) | Sprint docs sync |
| 2026-08-17 | Last verified обновлён для AGENTS.md, CHANGELOG.md [Unreleased], README.md, CONTRIBUTING.md, docs/00, docs/19, CODING_STANDARDS.md, .coverage-baseline.txt (рефакторинг панелей свойств «Глубокая база» #45–#48: test_count 2649, coverage 89.99%; устаревшие упоминания CustomResizeCommand/ComputeLineResize почищены) | Sprint docs sync (#49) |
| 2026-08-17 | Last verified обновлён для AGENTS.md, CHANGELOG.md [Unreleased], README.md, CONTRIBUTING.md, docs/00, docs/19, CODING_STANDARDS.md, .coverage-baseline.txt (рефакторинг инструментов «ToolRegistry» #53–#57: test_count 2646, coverage 89.98%; ToolManager → ToolRegistry в current-state секциях, Common Mistakes #76, исторические спринт-логи не изменялись) | Sprint docs sync (#62) |
| 2026-08-17 | Last verified обновлён для AGENTS.md, CHANGELOG.md [Unreleased], README.md, CONTRIBUTING.md, docs/00, docs/19, CONTEXT.md, .coverage-baseline.txt (кандидат 7 обзора №3 — deletion-пара #70–#71, PR #73: FitToScreen на живом viewport, рамка выделения через полиморфный GetBoundingBox: test_count 2622, coverage 89.86%; CODING_STANDARDS без числовых метрик — изменений не потребовал, исторические спринт-логи не изменялись) | Sprint docs sync (#72) |
| 2026-08-18 | Last verified обновлён для AGENTS.md, CHANGELOG.md [Unreleased], README.md, CONTRIBUTING.md, docs/00, docs/19, CODING_STANDARDS.md, CONTEXT.md, .coverage-baseline.txt (кандидат 2 обзора №4 — preview-сейм #93, тикет #94, PR #96: PreviewManager единственный источник истины, ре-ассайн-трюк удалён структурно, IEditorContext 29→27: test_count 2691, coverage 90.13%; CODING_STANDARDS §4.6/§7.3 переписаны под контракт P2 (HARD-находка Standards-ревью), Common Mistakes #13/#60 аннотированы историческими; исторические спринт-логи не изменялись) | Sprint docs sync (#95) |
| 2026-08-18 | Last verified обновлён для AGENTS.md, CHANGELOG.md [Unreleased], README.md, CONTRIBUTING.md, docs/00, docs/19, CODING_STANDARDS.md, .coverage-baseline.txt (кандидат 3 обзора №4 — CommandHistory единолично владеет грязностью #98, тикет #99, PR #101: Push/Undo/Redo стреляют markDirty, команды делегат не носят, компенсация в VM удалена, IEditorContext 27→26: test_count 2674, coverage 90.08%; CODING_STANDARDS §5.4/§15#16 переписаны под новое правило (документационный конфликт Standards-ревью), Common Mistakes #6 переписан + #79 добавлен, исторические спринт-логи не изменялись) | Sprint docs sync (#100) |
| 2026-08-18 | Last verified обновлён для AGENTS.md, CHANGELOG.md [Unreleased], README.md, CONTRIBUTING.md, docs/00, docs/19, .coverage-baseline.txt (кандидат 4 обзора №4 — раскладка маркеров один модуль #103, тикет #104, PR #106: геометрия маркеров в MarkerLayout (каталог/позиции/hit с допуском #82/классификация/курсоры), HitTestHelper — хит по телу (6 мёртвых методов удалены), ResizeMath — чистая математика, ResizeTool — единственный ResizeState, IEditorContext 26→25: test_count 2614, coverage 90.25%; CODING_STANDARDS не затронут (таблицы хелперов нет), CONTEXT.md «Маркер выделения» добавлен в реализации, Common Mistakes #80 добавлен, исторические спринт-логи не изменялись) | Sprint docs sync (#105) |
| 2026-08-18 | Last verified обновлён для AGENTS.md, CHANGELOG.md [Unreleased], README.md, CONTRIBUTING.md, docs/00, docs/19, CODING_STANDARDS.md, .coverage-baseline.txt (кандидат 8 обзора №4 — мёртвые швы deletion sweep #108, тикет #109, PR #111: удалены callback-параметры ZoomPanManager, мёртвые зависимости EditorViewModel, ITextToolSettings, DI-регистрация IFontMetrics, 4 конвертера без биндингов, ValidateRotation; ресурсы конвертеров консолидированы в корневом словаре (каждый класс ровно один раз, InverseBooleanConverter view-локально для STA-тестов); контракт Автосохранения типизирован, механизм сохранён (Q5-b): test_count 2576, coverage 90.6%; CODING_STANDARDS §9.1 — правило единой точки объявления конвертеров, Common Mistakes #81 добавлен, CONTEXT.md «Автосохранение» добавлен в реализации, исторические спринт-логи не изменялись) | Sprint docs sync (#110) |
| 2026-08-10 | CHANGELOG.md: исправлен UTF-8 double-encoding (mojibake) в старых строках (21 строка: в†’→→, вЂ”→—, В°→°, Вµ→µ, С‘→ё); BOM/CRLF сохранены, метрики не менялись | Encoding fix (backlog docs/plan_next_steps.md Задача 2) |
