# ADR / MADR — саммари

> Разбор по ссылкам [MADR](https://adr.github.io/madr/), [ADR genealogy](https://github.com/architecture-decision-record/architecture-decision-record)
> и папке `Desktop\ADR`. Дата: 2026-06-13.

---

## 1. Что такое ADR

**ADR (Architecture Decision Record)** — короткий markdown-файл, фиксирующий **одно** значимое
архитектурное решение: контекст, рассмотренные варианты, что выбрали и какие последствия.

Цель — чтобы через год-два было понятно **почему** так сделали, а не только *что* сделали.

Правила:
- файлы лежат рядом с кодом (`docs/adr/` или `docs/decisions/`);
- нумеруются последовательно;
- **не редактируются задним числом** — устаревшее решение помечают `superseded by …`, а не переписывают.

## 2. Что такое MADR

**MADR** — популярный *шаблон* для ADR.
- Аббревиатура: изначально «**M**arkdown **A**rchitectural **D**ecision **R**ecords»,
  сейчас шире — «Markdown **Any** Decision Records».
- Текущая версия — **4.0.0** (сен 2024).
- Вторая ссылка (репозиторий `architecture-decision-record`) — «генеалогия» форматов ADR вообще;
  `adr.github.io/madr` — сам MADR.

**Конвенция имени файла:** `NNNN-title-with-dashes.md` (4 цифры + kebab-case заголовок).

**Секции MADR (full):**
1. YAML-фронтматтер (метаданные)
2. Context and Problem Statement
3. Decision Drivers *(опц.)*
4. Considered Options
5. **Decision Outcome**
6. Consequences *(опц.)*
7. Confirmation *(опц.)*
8. Pros and Cons of the Options *(опц.)*
9. More Information *(опц.)*

**Статусы:** proposed → accepted → deprecated / superseded (+ rejected).
**Варианты шаблона:** full, minimal, bare.

## 3. Что лежит в папке `Desktop\ADR`

| Файл | Что это | Связь с MADR |
|---|---|---|
| `0000-template.md` | **ADR-шаблон** — технические/архитектурные решения | MADR full, слегка перекроен: добавлены `Tags`, в шапку вынесены `Deciders/Consulted/Informed`, статус со ссылками на supersede. Твой «диалект» MADR. |
| `0000-template (2).md` | **PDR-шаблон** (Product Decision Record) — продуктовые/фичевые решения | Не MADR, а отдельный жанр: «зачем фича», scope (in/out), персоны, user journeys (J1, J2…), binding commitments, «что значит done». Сделан как *upstream* к ADR. |
| `graphify-summary.md` | Конспект видео про инструмент Graphify | К ADR отношения не имеет — лежит в той же папке. |

## 4. Ключевое наблюдение: связка PDR → ADR

У тебя выстроена цепочка **PDR → ADR**:
- **PDR** описывает продуктовое решение и его границы, затем «передаёт» обязательства
  (по id: журналы `J1..`, решения `D1..`, commitments) в технический ADR.
- В PDR-шаблоне есть поле `Related` и финальная секция «Handoff to the technical spec»
  со ссылкой на `ADR-0017` из папки `../adr/`.

Подразумеваемая структура каталогов:

```
docs/
├── pdr/   0000-template (2).md  → 0001-…, 0002-…
└── adr/   0000-template.md      → 0001-…, 0002-…
```

Отличия твоих шаблонов от чистого MADR — косметика и расширения под локальные реалии
(упоминания «152-ФЗ», «self-hosted в Russian-friendly infra»), плюс PDR как продуктовый
слой, которого в MADR нет в принципе.

## 5. Возможные следующие шаги

- Завести реальную структуру `docs/adr/` + `docs/pdr/` и переименовать шаблоны по конвенции.
- Написать первый настоящий ADR/PDR по конкретному решению.
- Причесать шаблоны под MADR 4.0 / привести к единому виду.
