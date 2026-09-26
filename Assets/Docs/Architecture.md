# Elder Blood — Архитектура

Технический блюпринт проекта, выведенный из `Roadmap.md` и `DesignNotes.md`.
Это целевая архитектура (куда идём), а не снимок текущего кода.

> **Статус (2026-09-16):** Фазы 0–8d реализованы в коде и собраны в редакторе; петля забега проверена в play mode целиком. Актуальный пофазовый статус — `Roadmap.md`.
>
> Отличия реализации от этого блюпринта:
> - **реактивность деревни (Фаза 6) отключена** — закомментирована в `WorldCreator`, ждёт переделки;
> - `RunComponent`/`LevelObjectiveComponent`/`FloorTransitionSystem` **не заведены** — run-стейт держит `LevelManager`, переход делает `ObjectiveSystem`;
> - авто-выдача способностей по этажам не сделана (все доступны с старта);
> - **ECS→UI идёт не событиями Morpeh, а C#-мостами** (`HudChannel`, `RunChannel`): презентеры VContainer не умеют подписываться на `Event<T>`. `BloodChangedEvent` так и не появился;
> - `NavMesh` игрока **не удалён** — `MoveNavMeshComponent`/`Provider` используются стражей;
> - Фаза 8 дописана в коде и почти собрана в редакторе (логи, аудио, щит, оверлей зрения, настройки); в play mode не проверена. Остаток — три поля в инспекторе `HudView`/оверлея, см. Roadmap 8g.

---

## Принципы

- **Morpeh ECS** — вся игровая логика. Компоненты = данные (`struct : IComponent`), системы = логика (`class : ISystem`/`IFixedSystem`), провайдеры (`MonoProvider<T>`) связывают сцену с ECS.
- **DI: VContainer** — композиция зависимостей. Системы, сервисы, менеджеры и конфиги собираются через контейнер, а не ручным `new`.
- **MVP для UI** — `View` (отображение) + `Presenter` (логика), презентеры читают ECS **через события**.
- **Конфиги — ScriptableObject** (тюнинг данных вне кода).
- **События Morpeh** — связь между системами и из ECS в UI.
- **Правила кодинга**: без LINQ, for-циклы, IL2CPP-директивы на компонентах, нейминг `*Component/*System/*Provider/*Config/*Event`, комментарии на английском.

## Разделение ECS / не-ECS
- **ECS-код** — в `Assets/Scripts/ElderBlood/Ecs/` под неймспейсами `ElderBlood.Ecs.<Domain>` (по доменам, схлопнуто): `Ecs.Character`, `Ecs.Movement`, `Ecs.Health`, `Ecs.Guard`, `Ecs.Config`. Сюда входят компоненты, системы, провайдеры (мост), события, enum'ы компонентов.
- **Unity-слой** — в текущих неймспейсах `ElderBlood.*`: `GameManager` (`LevelManager`, `WorldCreator`, `GameLifetimeScope`, `LevelBootstrap`), `Bootstrap`, `UI`, `Camera`.
- **Данные/общее** — SO-конфиги в `ElderBlood.Configs(.Guard)` (`Configs/`), константы в `ElderBlood.Common` (`Common/`).
- Папки-подкаталоги внутри домена (напр. `Ecs/Guard/Vision`) оставлены для навигации, но неймспейс у домена один.

---

## Слои

```
┌──────────────────────────────────────────────┐
│ Composition Root (VContainer LifetimeScopes)  │  ← сборка зависимостей
├──────────────────────────────────────────────┤
│ Game Flow / Managers (LevelManager, Run)      │  ← забег, этажи, переходы
├──────────────────────────────────────────────┤
│ ECS World (Morpeh: components / systems)      │  ← вся игровая логика
│   Providers (MonoBehaviour ↔ ECS bridge)      │
├──────────────────────────────────────────────┤
│ UI (MVP, event-driven)                        │  ← меню, HUD, пауза, вендор
├──────────────────────────────────────────────┤
│ Services: Save (JSON), Audio                  │  ← кросс-сквозные
│ Configs (ScriptableObject)                    │
└──────────────────────────────────────────────┘
```

---

## DI-композиция (VContainer)

- **RootLifetimeScope** (сцена Main / приложение): сервисы (`SaveSystem`, аудио), `LevelManager`, конфиги (`MainConfig`, `GuardConfig`, `AbilityConfig`, `UpgradeConfig`), глобальное состояние (`PlayerProgress`, `VillageState`).
- **LevelLifetimeScope** (на уровень/забег): то, что живёт в пределах этажа; здесь же собирается Morpeh `SystemsGroup` с инъекцией зависимостей в системы.
- `WorldCreator` берёт системы из контейнера (а не `new`), системы получают конфиги/сервисы через конструктор.
- Ручная рассылка конфигов (`ConfigSetupSystem`/`ConfigSetupEvent`) **заменена инъекцией и удалена** (Фаза 5).

---

## ECS-модули по доменам

Ниже — целевой состав компонентов/систем. Помечено, в какой фазе появляется (см. `Roadmap.md`).

### Character — игрок
- **Ввод**: `InputComponent`, `InputSystem` *(есть)*. Действия: Move, Run, экшены (Jump/Magic/Death/Sit), Fade, Hide, способности.
- **Движение**: `MoveComponent`, `MoveSystem` *(есть, Rigidbody2D)*. Удаление NavMesh игрока **отменено** — `MoveNavMeshComponent`/`Provider` используются стражей.
- **Анимация**: `CharacterAnimationComponent`, `CharacterAnimationSystem` *(есть)*. Экшены через таймерную блокировку (`isAction` + `actionTimer`).
- **Заметность** *(Фаза 1)*: в v1 отдельного компонента-профиля нет — `awareness` считает охранник от дистанции. Множители (бег/Sit/Fade) и влияние невидимости приходят позже (Фаза 4).

### Blood — экономика крови *(Фаза 3)*
- `BloodComponent` (игрок: `current`, `capacity`) — единый пул HP/мана/валюта.
- `BloodSourceComponent` (жители/животные/павшие: `amount`, `state`, `canDrain`).
- `ShieldComponent` (отложен; открывается в Фазе 5).
- `HealthComponent` *(есть)* — у тех, кто получает урон (стража).
- Системы: `DamageSystem` (урон → кровь/HP → смерть), `DrainSystem` (мгновенный слив в радиусе, блок при полном пуле), опц. `DeathSystem` (павший NPC → `BloodSource`).
- События: `DamageEvent`, `DrainEvent`, `PlayerDeathEvent`, `BloodChangedEvent` (для HUD).

### Abilities — способности *(Фаза 4)*
- `AbilityConfig` (SO: cost/cooldown/duration/параметры).
- `AbilitiesComponent` (слоты + кулдауны).
- Тег-компоненты активных состояний: `InvisibilityActiveComponent`, `BloodPuddleComponent`.
- Системы: `AbilitySystem` (ввод → проверка стоимости/кулдауна → списание крови → запуск), `InvisibilitySystem` (Fade + скорость, гасит awareness), `BloodPuddleSystem` (каст «если не видят», движение = отмена, наступивший → смерть + слив), `BloodSightSystem` (подсветка крови в радиусе, импульс).

### Guard — охранник
- **Зрение/восприятие** *(Фаза 1)*: `GuardVisionComponent` (radius, angle, eyeOffset, obstacleMask, facing), `GuardPerceptionComponent` (`awareness`, `state`, `lastSeenPosition`), `PerceptionState{Unaware,Suspicious,Alert}`, `GuardVisionSystem` (заменяет детект в `GuardTriggerSystem`).
- **FSM тревоги** *(Фаза 2)*: `GuardAlertState{Patrol,Suspicious,Chase,Search,Return}`, `GuardAlertComponent`, `GuardAlertSystem` (выбирает destination, гейтит патруль).
- **Поведение/патруль** *(есть)*: `GuardBehaviorComponent`/`GuardBehaviorSystem` → чистый исполнитель (двигайся к destination + атакуй в `attackRange`, где `attackRange` — зазор между коллайдерами, а не дистанция центров); `GuardPatrolComponent`/`GuardPatrolSystem` → продвигает маршрут только в Patrol.
- **Анимация** *(есть)*: `GuardAnimationComponent`/`GuardAnimationSystem`.
- **Конфиг** *(есть)*: `GuardConfig` (+ `damage` в Фазе 3), `GuardSetupSystem`.

### Progression — Бог Крови *(Фаза 5)*
- `UpgradeConfig` (SO-каталог), `PlayerProgressComponent`, `GodOfBloodComponent`, `UpgradeSystem` (списать кровь → применить апгрейд).
- Здесь открывается щит; здесь же бэкенд **сохранений** (JSON в `persistentDataPath`): `SaveSystem` сериализует `PlayerProgress` (+ `VillageState`).

### Village — реактивность *(Фаза 6)*
- `VillageStateComponent` (`notoriety`, `tier`, счётчики), `VillageReactionSystem` (notoriety → tier → будит/разгоняет жителей). Питается событиями слива/смерти. Персистит (мета).

### Run / Level flow *(Фаза 7)*
- `LevelManager` *(есть)* + `LevelContainer` (упорядоченные этажи). Расширяется `RunComponent`, `ExitComponent`, `LevelObjectiveComponent`, `ObjectiveSystem`, `FloorTransitionSystem`.
- Петля: старт → этаж → цель (дойти до выхода) → хаб Бога Крови → следующий этаж → вершина = победа; `PlayerDeathEvent` → конец забега → мета персистит → старт с этажа 1.
- **Фикс жизненного цикла World**: систем-группа создаётся один раз и переиспользуется (сейчас `CreateWorld()` зовётся на каждой загрузке).

### UI *(Фаза 8)*
- Паттерн MVP, всё *(есть)*: `MainMenuView/Presenter`, `HudView/HudPresenter`, `PauseView/PausePresenter`, `RunEndView/RunEndPresenter`.
- **Индикаторы тревоги над стражей сделаны не презентером, а ECS** (`GuardAlertIndicatorSystem`, по образцу `GuardAnimationSystem`) — они per-guard, презентер тут лишний.
- Презентеры кормятся **C#-мостами** `HudChannel` (кровь, кулдауны) и `RunChannel` (победа/поражение), а не событиями Morpeh: презентеры VContainer не подписываются на `Event<T>`.
- HUD v1 готов: шкала крови + кулдауны способностей + индикаторы тревоги + пауза + экран конца забега. Щит и оверлей кровавого зрения дописаны и привязаны в сцене, но в игре ещё не показывались — остаток по ним в Roadmap 8g.
- **Оверлей кровавого зрения — обычный полноэкранный `Image`, не post-processing.** Volume-эффект URP выглядел бы богаче, но потребовал бы своего профиля и рендер-фичи ради одного импульса; спрайт `Sprites/UI/Hud/BloodVignette` (320×180, пиксель-арт) лежит на том же Canvas и гасится одной альфой.

---

## Каталог событий (Morpeh `Event<T>`)

| Событие | Издатель | Потребители |
|---|---|---|
| `DamageEvent` | Guard attack (Chase в радиусе) | `DamageSystem` |
| `DrainEvent` | `DrainSystem`/интеракция | Blood, Village |
| `PlayerDeathEvent` | `DamageSystem` (кровь ≤ 0) | Run flow (конец забега), анимация Death |
| ~~`BloodChangedEvent`~~ | — | **не заведён**: HUD кормится через `HudChannel` (C#-мост), а не событием Morpeh |
| ~~`ConfigSetupEvent`/`GuardSetupConfigEvent`~~ | — | **удалены** — конфиги раздаются через DI (`WorldCreator` → конструкторы систем) |
| ~~`DebugTriggerEvent`~~ | — | **удалён** вместе со старым `GuardTriggerSystem` в Фазе 1 |
| `AbilityCastEvent` | `InputSystem` | `AbilitySystem` |
| `UpgradeEvent` | `InputSystem` | `UpgradeSystem` |
| `SourceDrainedEvent` | `DrainSystem` (по факту слива) | `VillageReactionSystem` *(отключена)* |

---

## Ключевые потоки данных

**Детект → тревога → бой → смерть:**
```
GuardVisionSystem (конус+LoS) → awareness/state
  → GuardAlertSystem (FSM) → destination/Chase
  → GuardBehaviorSystem (move+attack в radius) → DamageEvent
  → DamageSystem → Blood/Health; ≤0 → PlayerDeathEvent → конец забега
```

**Слив → кровь → деревня:**
```
интеракция → DrainSystem → BloodComponent (cap) + DrainEvent
  → VillageReactionSystem (notoriety↑ → tier → жители просыпаются)
  → BloodChangedEvent → HUD
```

**Забег** (как реализовано; `RunComponent`/`FloorTransitionSystem` не заведены):
```
MainMenuPresenter → LevelManager.StartRun (CreateWorld один раз + спавн игрока + этаж 0)
  → ExitComponent достигнут → ObjectiveSystem → LevelManager.AdvanceFloor
  → вендор Бога Крови стоит на самом этаже (UpgradeSystem, трата крови), отдельного хаба нет
  → вершина → WinRun → RunChannel → экран «Victory»
  | PlayerDeathEvent → EndRunDeath → RunChannel → экран «You Died» → рестарт по кнопке
```

---

## Порядок систем (целевой, расширение `WorldCreator`)

Фактический порядок живёт в `WorldCreator.CreateWorld()`, список продублирован в `CLAUDE.md` — **там источник правды**, здесь не повторяем, чтобы не разъезжалось.

Целевой блюпринт был: Input → Abilities → активные способности → Move → CharacterAnimation → GuardVision → GuardAlert → GuardBehavior → GuardAnimation → Drain/Damage → Village → Objective → HUD-фид. Реализация ему соответствует, кроме отключённой `VillageReactionSystem`.

---

## Зависимости фаз

```
Фаза 0 (DI, техдолг) — фундамент для всего
Фаза 1 (детект) → Фаза 2 (FSM) → Фаза 3 (кровь)
Фаза 3 → Фаза 4 (способности тратят кровь) и Фаза 6 (события слива)
Фаза 5 (Бог Крови, сохранения) ← нужна Фазам 5/6 для меты
Фаза 7 (забег) связывает всё в петлю
Фаза 8 (HUD) поверх готовых данных/событий
```
