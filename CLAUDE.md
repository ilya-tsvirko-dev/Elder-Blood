# Elder Blood — Project Context for Claude

## Проект
2D stealth/action игра на Unity. Архитектура: **Morpeh ECS** + **Odin Inspector**.

---

## Технологии
- **Morpeh ECS** — вся игровая логика
- **VContainer** — DI (composition root `GameLifetimeScope`)
- **Unity AI Navigation (NavMesh)** — движение охранников
- **Cinemachine** — камера
- **Unity Input System** (новый) — ввод игрока
- **Addressables** — загрузка ассетов (уровни + игрок)
- **URP** — рендер
- **2D**: Tilemap, SpriteShape, Aseprite, PixelPerfect

---

## Структура проекта

**Разделение:** ECS-код — в `Scripts/ElderBlood/Ecs/<Domain>/` под неймспейсами `ElderBlood.Ecs.<Domain>`. Unity-слой (`GameManager`, `UI`, `Camera`, `Bootstrap`), сервисы (`Save`), SO-конфиги (`Configs`) и константы (`Common`) — в `ElderBlood.*`.

```
Assets/
├── Scripts/ElderBlood/
│   ├── WorldCreator.cs             — строит ECS-систем-группу ОДИН раз (CreateWorld(), флаг _created)
│   ├── Bootstrap/BootstrapEntryPoint.cs — стартовая сцена → грузит Main
│   ├── GameManager/
│   │   ├── GameLifetimeScope.cs    — VContainer root (регистрирует View/LevelManager/Camera/WorldCreator/SaveService + презентеры)
│   │   ├── LevelManager.cs         — петля забега: StartRun/AdvanceFloor/RestartRun/WinRun; ОДИН игрок на забег
│   │   └── LevelBootstrap.cs       — точка спавна игрока на этаже (PlayerSpawnPoint)
│   ├── UI/                         — MainMenuView + MainMenuPresenter (MVP)
│   ├── Camera/                     — CameraController + CameraPresenter (не ECS)
│   ├── Save/                       — SaveService (JSON в persistentDataPath), PlayerProgressData, VillageStateData
│   ├── Common/                     — AnimatorParams, ShaderIds, Layers
│   ├── Configs/                    — MainConfig (ссылки на все SO), AbilityConfig, CharacterConfig, UpgradeConfig, VillageConfig, Guard/GuardConfig, Levels/LevelContainer
│   └── Ecs/
│       ├── Character/              — Input/, Animation/ (компоненты+системы+провайдеры)
│       ├── Movement/               — Move*, MoveNavMesh* (NavMesh используется стражей)
│       ├── Health/                 — Health*, HealthShader*
│       ├── Blood/                  — Blood/BloodSource/Shield, Damage/Drain (системы+события), SourceState
│       ├── Abilities/              — AbilitiesComponent/System, Invisibility/BloodPuddle/BloodSight, AbilityType, AbilityCastEvent
│       ├── Guard/                  — Component/Provider, GuardConfiguredComponent (тег), Behavior/, Patrol/, Vision/, Alert/, Animation/
│       ├── Progression/            — PlayerProgress*, GodOfBlood*, Upgrade* (вендор)
│       ├── Village/                — VillageState/Reaction (ОТКЛЮЧЕНО в WorldCreator, ждёт переделки)
│       └── Level/                  — Exit*, ObjectiveSystem (цель этажа + переходы)
├── Configs/                        — .asset файлы конфигов
├── Controls/Controls.cs           — сгенерённый Input System (Move/Run/Interact/Invisibility/BloodPuddle/BloodSight/Upgrade)
├── Scenes/Bootstrap.unity, Main.unity — две сцены (Bootstrap → Main); уровни = Addressable-префабы
├── Prefabs/Characters/, Enemy/, Levels/
└── Sprites/, Tilemaps/, Animation/
```

### Поток загрузки / петля забега
1. `Bootstrap.unity` → `BootstrapEntryPoint` грузит сцену `Main`
2. `Main.unity`: `GameLifetimeScope` (VContainer) собирает зависимости; `MainMenuView` (кнопка Start) + `LevelManager`
3. Start → `MainMenuPresenter` → `LevelManager.StartRun()`: `WorldCreator.CreateWorld()` (один раз) → спавн игрока (Addressables, ОДИН на забег) → `LoadFloor(0)`
4. Игрок дошёл до выхода → `ObjectiveSystem` → `LevelManager.AdvanceFloor()` → выгрузка геометрии + загрузка след. этажа + телепорт игрока на `PlayerSpawnPoint` (кровь/кулдауны переносятся)
5. Последний этаж + выход = `WinRun()`; смерть (`PlayerDeathEvent`) → `RestartRun()` (новый игрок, этаж 1; мета-прогресс уже сохранён)

---

## ECS Паттерны (Morpeh)

### Структура компонента
```csharp
[Serializable]
[Il2CppSetOption(Option.NullChecks, false)]
[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
public struct MyComponent : IComponent {
    public float someValue;
}
```

### Структура системы
```csharp
public class MySystem : ISystem {  // или IFixedSystem
    private Filter _filter;
    private Stash<MyComponent> _stash;

    public World World { get; set; }

    public void OnAwake() {
        _filter = World.Filter.With<MyComponent>().Build();
        _stash = World.GetStash<MyComponent>();
    }

    public void OnUpdate(float deltaTime) {
        foreach (var entity in _filter) {
            ref var c = ref _stash.Get(entity);
            // логика
        }
    }

    public void Dispose() { }
}
```

### События
```csharp
var evt = World.GetEvent<MyEvent>();
evt.NextFrame(new MyEvent { data = value }); // отправить
// в другой системе:
foreach (var e in evt.publishedChanges) { ... }
```

### MonoProvider
```csharp
public class MyProvider : MonoProvider<MyComponent> { }
```

---

## Порядок систем (WorldCreator.cs → CreateWorld(), создаётся один раз)
1. `HealthShaderSystem`
2. `InputSystem`
3. `MoveSystem`
4. `CharacterAnimationSystem`
5. `AbilitySystem`
6. `InvisibilitySystem`
7. `BloodPuddleSystem`
8. `BloodSightSystem`
9. `GuardVisionSystem`
10. `GuardAlertSystem`
11. `GuardBehaviorSystem`
12. `GuardAnimationSystem`
13. `GuardSetupSystem`
14. `GuardPatrolSystem`
15. `PlayerProgressSystem`
16. `UpgradeSystem`
17. `DamageSystem`
18. `DrainSystem`
19. `ObjectiveSystem`
20. ~~`VillageReactionSystem`~~ — закомментирована (ждёт переделки)

---

## Игровые системы

### Игрок
- `InputSystem` читает ввод → обновляет `MoveComponent.moveVector` + `CharacterAnimationComponent`
- `MoveSystem` применяет velocity к `Rigidbody2D`
- `CharacterAnimationSystem` управляет аниматором (Idle/Move/Run)

### Способности (тратят кровь = единый ресурс)
- `AbilitySystem` — ввод → проверка стоимости/кулдауна → списание `BloodComponent.current` → запуск (тег + таймер). Невидимость / Кровавая лужа / Кровавое зрение
- `InvisibilitySystem` / `BloodPuddleSystem` / `BloodSightSystem` — активные состояния

### Кровь
- `DamageSystem` — урон: щит (если `unlocked`) поглощает первым, остаток в кровь; кровь ≤ 0 → `Death`-анимация + `PlayerDeathEvent`
- `DrainSystem` — по `Interact`/F мгновенно сливает ближайший `BloodSource` в радиусе (блок при полном пуле); пропускает источники со `state == Awake`

### Охранник — обнаружение / тревога
- `GuardVisionSystem` — конус зрения + LoS-рейкаст → `GuardPerceptionComponent.awareness/state`; на `Alert` пишет цель
- `GuardAlertSystem` — FSM Patrol→Suspicious→Chase→Search→Return, выбирает destination, гейтит патруль
- `GuardBehaviorSystem` — чистый исполнитель: NavMesh к destination + атака в `minMagnitude` (поднимает `DamageEvent`)
- `GuardPatrolSystem` — обход `points` с ожиданием `waitDuration`, продвигает маршрут только в `Patrol`

### Прогрессия / забег
- `PlayerProgressSystem` — применяет сохранённый мета-прогресс (бонусы крови/щита) к каждому новому игроку; сохраняет на `PlayerDeathEvent`
- `UpgradeSystem` — вендор Бога Крови: клавиша `Upgrade` рядом → покупка следующего пункта `UpgradeConfig` за кровь
- `GuardSetupSystem` — применяет `MaxHealth` из `GuardConfig` к каждому новому стражу (фильтр `Without<GuardConfiguredComponent>`)
- `ObjectiveSystem` — проксимити игрок→`ExitComponent` → `LevelManager.AdvanceFloor`; `PlayerDeathEvent` → `RestartRun`

### DI вместо событийной раздачи конфигов
- Конфиги собраны в `MainConfig`, инжектятся через VContainer; системы получают их в конструкторе (`WorldCreator` раздаёт). Старые `ConfigSetupSystem`/`GuardSetupConfigEvent` удалены.

---

## Правила кодинга

- **Нет LINQ** — только for-циклы
- Компоненты — `struct`, системы — `class`
- IL2CPP директивы на всех компонентах (отключить null/bounds checks)
- Называть: `*Component`, `*System`, `*Provider`, `*Config`, `*Event`
- Перед изменениями >20 строк — согласовать план

---

## Состояние реализации
Фазы 0–7 готовы в коде (детект, FSM тревоги, кровь, способности, Бог Крови+сохранения, поток этажей). Многие фичи требуют editor-части (провайдеры на префабах, input-экшены, SO-ассеты). Детальный статус — `Assets/Docs/Roadmap.md`.

**Отключено/не реализовано:**
- `VillageReactionSystem` (реактивность деревни) — закомментирована в `WorldCreator`, ждёт переделки/удаления
- Авто-выдача способностей по этажам — нет (все способности доступны с старта)
- `MoveNavMeshSystem` — не добавлена (но `MoveNavMeshComponent`/`Provider` используются стражей)
- Экран победы / задержка рестарта под death-анимацию — Фаза 8 (HUD/полиш)
