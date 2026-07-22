# План: Оставшиеся задачи (после P2)

**Дата:** 22.07.2026
**Статус:** Утверждено к выполнению
**Исключено:** Этап 2 — Редактор УГО (отдельное планирование)

---

## Обзор

После завершения Архитектурного рефакторинга P2 остались задачи по
«гигиене» документации и финализации Этапа 1. FR-021/022 из `docs/1.md`
фактически относятся к Этапу 2 (см. `docs/47_План_развития_Этап2.md`),
поэтому в данном плане не участвуют.

---

## Задача 1: Удалить docs/1.md

**Файл:** `docs/1.md` (14 строк)

**Причина:** Все 4 пункта техдолга решены:
1. ✅ STA-тесты (Sprint 62)
2. ✅ Template.Clone regression test (Sprint 63)
3. ✅ Command naming consistency (Sprint P2)
4. ✅ MainViewModel DI reduction (Sprint P2)

FR-021/022 — отложенные фичи Этапа 2, не актуальны для текущего спринта.

**После удаления:** проверить DOCS_MANIFEST.md и консистентность документации.

---

## Задача 2: Исправить кодировку CHANGELOG.md

**Файл:** `CHANGELOG.md`

**Проблема:** UTF-8 double-encoding (mojibake) в русском тексте.
Пример: `СѓРІРµР»РёС‡РµРЅРёРµ` → `увеличение`

**Решение:** Декодировать через Windows-1251 → перекодировать в UTF-8
(аналогично фиксу AGENTS.md).

---

## Задача 3: Обновить AGENTS.md

**Файл:** `AGENTS.md`

**Изменения:**
- Убрать "FR-021 Drag&Drop из библиотеки" и "FR-022 Preview шаблонов"
  из секции Next Steps (это часть Этапа 2, не входит в план)
- Убрать ссылку на `docs/1.md` если есть

---

## Задача 4: Проверка консистентности документации

**Что проверить:**
- DOCS_MANIFEST.md — убрать ссылку на `docs/1.md`
- Все метрики (test_count=2095, coverage=75.3%) синхронизированы
- Обновить CONSISTENCY_REPORT.md

---

## Задача 5: Build + Test

```bash
dotnet build src/DotElectric.TemplateEditor.slnx
dotnet test src/DotElectric.TemplateEditor.Tests
```

---

## Критерии приёмки

- [ ] `docs/1.md` удалён
- [ ] `CHANGELOG.md` без mojibake
- [ ] `AGENTS.md` Next Steps чисты
- [ ] DOCS_MANIFEST.md не ссылается на удалённый файл
- [ ] Build: 0 errors, 0 warnings
- [ ] Tests: 2095 passed (1 pre-existing skip)
- [ ] CONSISTENCY_REPORT.md: PASS
