# Elder Blood — Project Context for Claude

## Проект
2D stealth/action игра на Unity. Архитектура: **Morpeh ECS** + **Odin Inspector**.

---

## Технологии
- **Morpeh ECS** — вся игровая логика
- **Unity AI Navigation (NavMesh)** — движение охранников
- **Cinemachine** — камера
- **Unity Input System** (новый) — ввод игрока
- **Addressables** — загрузка ассетов
- **URP** — рендер
- **2D**: Tilemap, SpriteShape, Aseprite, PixelPerfect

---

## Структура проекта

```
Assets/
├── Scripts/ElderBlood/
│   ├── WorldCreator.cs             — инициализация ECS World, регистрация систем (CreateWorld())
│   ├── Bootstrap/BootstrapEntryPoint.cs — стартовая сцена → грузит Main
│   ├── GameManager/
│   │   ├── LevelManager.cs         — singleton, загрузка уровней через Addressables, спавн игрока
│   │   └── LevelBootstrap.cs       — точка спавна игрока на уровне (PlayerSpawnPoint)
│   ├── UI/MainMenuUI.cs            — главное меню, кнопка Start → LevelManager.LoadLevel
│   ├── Camera/                     — CameraController (MonoBehaviour, не ECS)
│   ├── Character/
│   │   ├── Input/                  — InputComponent/System/Provider
│   │   └── Animation/              — CharacterAnimationComponent/System/Provider
│   ├── Guard/
│   │   ├── GuardComponent/Provider — маркер охранника
│   │   ├── GuardTriggerComponent/System — обнаружение игрока (OverlapCircle)
│   │   ├── GuardBehavior/          — NavMesh-преследование цели / движение к patrolPoint
│   │   ├── GuardPatrol/            — патрулирование по точкам с ожиданием
│   │   └── Animation/              — анимация охранника
│   ├── Common/Enums/               — MoveAnimationType
│   └── Configs/
│       ├── MainConfig.cs           — ScriptableObject, ссылка на GuardConfig
│       ├── ConfigSetupComponent/Provider — носитель конфига
│       └── Guard/GuardConfig.cs    — MaxHealth охранника
├── Configs/                        — .asset файлы конфигов
├── Scenes/Bootstrap.unity, Main.unity — две сцены (Bootstrap → Main); уровни = Addressable-префабы
├── Prefabs/Characters/, Enemy/, Levels/
└── Sprites/, Tilemaps/, Animation/
```

### Поток загрузки
1. `Bootstrap.unity` → `BootstrapEntryPoint` грузит сцену `Main`
2. `Main.unity`: `MainMenuUI` (кнопка Start) + `LevelManager` (singleton, DontDestroyOnLoad)
3. Start → `LevelManager.LoadLevel(levelRef)` → Addressables-инстанс уровня → `LevelBootstrap.PlayerSpawnPoint` → спавн игрока (Addressables) → `WorldCreator.CreateWorld()`

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

## Порядок систем (WorldCreator.cs → CreateWorld())
1. `HealthShaderSystem`
2. `InputSystem`
3. `MoveSystem`
4. `CharacterAnimationSystem`
5. `GuardTriggerSystem`
6. `GuardBehaviorSystem`
7. `GuardAnimationSystem`
8. `GuardSetupSystem`
9. `ConfigSetupSystem`
10. `GuardPatrolSystem`

---

## Игровые системы

### Игрок
- `InputSystem` читает ввод → обновляет `MoveComponent.moveVector` + `CharacterAnimationComponent`
- `MoveSystem` применяет velocity к `Rigidbody2D`
- `CharacterAnimationSystem` управляет аниматором (Idle/Move/Run)

### Охранник — обнаружение
- `GuardTriggerSystem` делает `Physics2D.OverlapCircle` вокруг охранника
- При обнаружении ставит `GuardBehaviorComponent.targetTransform`

### Охранник — преследование
- `GuardBehaviorSystem` считает путь через NavMesh к цели
- Когда цель в `minMagnitude` — триггерит анимацию атаки

### Охранник — патруль
- `GuardPatrolSystem` циклично обходит `points` (List<Vector3>), на каждой точке ждёт `waitDuration`
- Пишет цель в `GuardBehaviorComponent.patrolPoint` (движение делает GuardBehaviorSystem)
- При обнаружении цели (`targetTransform != null`) патруль приостанавливается

### Конфиги
- `ConfigSetupSystem` слушает `ConfigSetupEvent` → рассылает `GuardSetupConfigEvent`
- `GuardSetupSystem` применяет `MaxHealth` из конфига ко всем охранникам

---

## Правила кодинга

- **Нет LINQ** — только for-циклы
- Компоненты — `struct`, системы — `class`
- IL2CPP директивы на всех компонентах (отключить null/bounds checks)
- Называть: `*Component`, `*System`, `*Provider`, `*Config`, `*Event`
- Перед изменениями >20 строк — согласовать план

---

## Что не реализовано (закомментировано)
- `MoveNavMeshSystem` — не добавлена в `WorldCreator` (есть `MoveNavMeshComponent` + `MoveNavMeshProvider`)
- Действия игрока: Sit, Jump, Hide, Magic, Death — есть в enum, нет в InputSystem
