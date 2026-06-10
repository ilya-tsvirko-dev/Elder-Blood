# Elder Blood — Архитектура

Технический блюпринт проекта, выведенный из `Roadmap.md` и `DesignNotes.md`.
Это целевая архитектура (куда идём), а не снимок текущего кода.

> **Статус (2026-06-10):** Фазы 0–7 реализованы в коде (детект, FSM тревоги, кровь, способности, Бог Крови + сохранения, поток этажей). Актуальный пофазовый статус — `Roadmap.md`. Отличия реализации от этого блюпринта: **реактивность деревни (Фаза 6) отключена** (ждёт переделки); `RunComponent`/`LevelObjectiveComponent` не заведены (run-стейт держит `LevelManager`); авто-выдача способностей по этажам не сделана (все доступны с старта). Осталась Фаза 8 (HUD/UX).

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
- Ручная рассылка конфигов (`ConfigSetupSystem`/`ConfigSetupEvent`) заменяется инъекцией, где это упрощает.

---

## ECS-модули по доменам

Ниже — целевой состав компонентов/систем. Помечено, в какой фазе появляется (см. `Roadmap.md`).

### Character — игрок
- **Ввод**: `InputComponent`, `InputSystem` *(есть)*. Действия: Move, Run, экшены (Jump/Magic/Death/Sit), Fade, Hide, способности.
- **Движение**: `MoveComponent`, `MoveSystem` *(есть, Rigidbody2D)*. NavMesh игрока — удаляется (Фаза 0).
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
- **Поведение/патруль** *(есть)*: `GuardBehaviorComponent`/`GuardBehaviorSystem` → чистый исполнитель (двигайся к destination + атакуй в `minMagnitude`); `GuardPatrolComponent`/`GuardPatrolSystem` → продвигает маршрут только в Patrol.
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
- Паттерн MVP: `MainMenuView/Presenter` *(есть)*, `HudView/HudPresenter`, `PauseView/PausePresenter`, презентер индикаторов тревоги над стражей.
- Презентеры подписаны на события (`BloodChangedEvent`, смена `GuardAlertState`, кулдауны).
- HUD v1: шкала крови + кулдауны способностей + индикаторы тревоги + пауза.

---

## Каталог событий (Morpeh `Event<T>`)

| Событие | Издатель | Потребители |
|---|---|---|
| `DamageEvent` | Guard attack (Chase в радиусе) | `DamageSystem` |
| `DrainEvent` | `DrainSystem`/интеракция | Blood, Village |
| `PlayerDeathEvent` | `DamageSystem` (кровь ≤ 0) | Run flow (конец забега), анимация Death |
| `BloodChangedEvent` | Blood/Damage/Drain | `HudPresenter` |
| `ConfigSetupEvent`/`GuardSetupConfigEvent` *(есть)* | Config setup | Guard setup *(может уйти в DI)* |
| `DebugTriggerEvent` *(есть)* | Debug | `GuardVisionSystem` (визуализация) |

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

**Забег:**
```
RunComponent старт → LevelManager.LoadFirstLevel
  → ExitComponent достигнут → ObjectiveSystem → FloorTransitionSystem
  → хаб Бога Крови (UpgradeSystem, трата крови) → LoadNextLevel
  → вершина → победа | PlayerDeathEvent → мета (SaveSystem) → рестарт
```

---

## Порядок систем (целевой, расширение `WorldCreator`)

Текущий порядок (есть): HealthShader → Input → Move → CharacterAnimation → GuardTrigger → GuardBehavior → GuardAnimation → GuardSetup → ConfigSetup → GuardPatrol.

Целевой порядок добавляет (примерно): Input → Abilities → активные способности → Move → CharacterAnimation → **GuardVision** → **GuardAlert** → GuardBehavior → GuardAnimation → Drain/Damage → Death → Village → Objective/FloorTransition → HealthShader/HUD-фид. Точный порядок уточняется при реализации фаз.

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
