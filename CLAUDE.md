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

**Разделение:** ECS-код — в `Scripts/ElderBlood/Ecs/<Domain>/` под неймспейсами `ElderBlood.Ecs.<Domain>`. Unity-слой (`GameManager`, `UI`, `Camera`, `Bootstrap`), сервисы (`Save`, `Services`), SO-конфиги (`Configs`) и константы (`Common`) — в `ElderBlood.*`.

```
Assets/
├── Scripts/ElderBlood/
│   ├── WorldCreator.cs             — строит ECS-систем-группу ОДИН раз (CreateWorld(), флаг _created)
│   ├── Bootstrap/BootstrapEntryPoint.cs — стартовая сцена → грузит Main
│   ├── GameManager/
│   │   ├── GameLifetimeScope.cs    — VContainer root (View'ы/LevelManager/Camera/WorldCreator + SaveService, AudioService, HudChannel, RunChannel + презентеры)
│   │   ├── LevelManager.cs         — петля забега: StartRun/AdvanceFloor/RestartRun/WinRun/EndRunDeath; ОДИН игрок на забег
│   │   └── LevelBootstrap.cs       — точка спавна игрока на этаже (PlayerSpawnPoint)
│   ├── UI/                         — MVP: MainMenu/Hud/Pause/RunEnd/Settings (View + Presenter каждый); экраны-панели наследуют PanelView (Show/Hide дочернего `_panel`); мосты ECS→UI: HudChannel, RunChannel
│   ├── Camera/                     — CameraController + CameraPresenter (не ECS)
│   ├── Save/                       — SaveService (JSON в persistentDataPath), PlayerProgressData, VillageStateData, SettingsData
│   ├── Services/                   — AudioService (SFX), GameLogger (каналы логов)
│   ├── Common/                     — AnimatorParams, ShaderIds, Layers
│   ├── Configs/                    — MainConfig (ссылки на все SO), AbilityConfig, CharacterConfig, UpgradeConfig, VillageConfig, AudioConfig, LogConfig, Guard/GuardConfig, Levels/LevelContainer
│   └── Ecs/
│       ├── Character/              — Input/, Animation/ (компоненты+системы+провайдеры)
│       ├── Movement/               — Move*, MoveNavMesh* (NavMesh используется стражей)
│       ├── Health/                 — Health*, HealthShader*
│       ├── Blood/                  — Blood/BloodSource/Shield, Damage/Drain (системы+события), SourceState
│       ├── Abilities/              — AbilitiesComponent/System, Invisibility/BloodPuddle/BloodSight, AbilityType, AbilityCastEvent
│       ├── Guard/                  — Component/Provider, GuardConfiguredComponent (тег), Behavior/, Patrol/, Vision/, Alert/, Animation/
│       ├── Progression/            — PlayerProgress*, GodOfBlood*, Upgrade* (вендор)
│       ├── Village/                — VillageState/Reaction (ОТКЛЮЧЕНО в WorldCreator, ждёт переделки)
│       ├── Hud/                    — HudFeedSystem (кровь + щит + кулдауны + кровавое зрение → HudChannel; всё квантуется в 32 шага и сбрасывается разом, когда сущность игрока исчезла)
│       ├── Audio/                  — SfxEvent (IEventData) + AudioSystem (играет через Services/AudioService)
│       └── Level/                  — Exit*, ObjectiveSystem (цель этажа + переходы)
├── Configs/                        — .asset файлы конфигов
├── Controls/Controls.cs           — сгенерённый Input System (Move/Run/Interact/Invisibility/BloodPuddle/BloodSight/Upgrade/Pause)
├── Scenes/Bootstrap.unity, Main.unity — две сцены (Bootstrap → Main); уровни = Addressable-префабы
├── Prefabs/Characters/, Enemy/, Levels/
└── Sprites/, Tilemaps/, Animation/
```

### Поток загрузки / петля забега
1. `Bootstrap.unity` → `BootstrapEntryPoint` грузит сцену `Main`
2. `Main.unity`: `GameLifetimeScope` (VContainer) собирает зависимости; `MainMenuView` (кнопка Start) + `LevelManager`
3. Start → `MainMenuPresenter` → `LevelManager.StartRun()`: `WorldCreator.CreateWorld()` (один раз) → спавн игрока (Addressables, ОДИН на забег) → `LoadFloor(0)`
4. Игрок дошёл до выхода → `ObjectiveSystem` → `LevelManager.AdvanceFloor()` → выгрузка геометрии + загрузка след. этажа + телепорт игрока на `PlayerSpawnPoint` (кровь/кулдауны переносятся)
5. Последний этаж + выход = `WinRun()` → `RunChannel.RaiseWon()` → экран «Victory»
6. Смерть (`PlayerDeathEvent`) → `EndRunDeath()` → `RunChannel.RaiseLost()` → экран «You Died»; `timeScale = 0`. Рестарт — **по кнопке**: `RestartRun()` (новый игрок, этаж 1; мета-прогресс сохранён на смерти через `PlayerProgressSystem`)

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
13. `GuardAlertIndicatorSystem`
14. `GuardSetupSystem`
15. `GuardPatrolSystem`
16. `PlayerProgressSystem`
17. `UpgradeSystem`
18. `DamageSystem`
19. `DrainSystem`
20. `ObjectiveSystem`
21. `HudFeedSystem`
22. `AudioSystem`
23. ~~`VillageReactionSystem`~~ — закомментирована (ждёт переделки)

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
- `GuardBehaviorSystem` — чистый исполнитель: NavMesh к destination + атака (поднимает `DamageEvent`). Досягаемость — **зазор между коллайдерами** (`Collider2D.Distance`), а не дистанция центров: тела разной формы, и круг от центра достаёт вбок втрое дальше, чем по вертикали. Проверка атаки идёт **до** расчёта пути — иначе в упор путь схлопывается в одну точку и атака пропускается
- `GuardPatrolSystem` — обход `points` с ожиданием `waitDuration`, продвигает маршрут только в `Patrol`

### Прогрессия / забег
- `PlayerProgressSystem` — применяет сохранённый мета-прогресс (бонусы крови/щита) к каждому новому игроку; сохраняет на `PlayerDeathEvent`
- `UpgradeSystem` — вендор Бога Крови: клавиша `Upgrade` рядом → покупка следующего пункта `UpgradeConfig` за кровь
- `GuardSetupSystem` — применяет `MaxHealth` из `GuardConfig` к каждому новому стражу (фильтр `Without<GuardConfiguredComponent>`)
- `ObjectiveSystem` — проксимити игрок→`ExitComponent` → `LevelManager.AdvanceFloor`; `PlayerDeathEvent` → `EndRunDeath` (экран конца забега, рестарт по кнопке). Проксимити опрашивается каждый кадр, поэтому `LevelManager` держит флаги `_transitioning`/`_runOver` — без них победа срабатывала десятки раз подряд

### Аудио
- `SfxEvent` — **`IEventData`**, не `IComponent`. Морфовский `Event<T>` принимает только `IEventData`; на `IComponent` проект не компилится
- `AudioSystem` — единственный подписчик, дёргает `AudioService.Play`. Сам ничего не тикает (`OnUpdate` пустой)
- **Повторы гасит `AudioService`, не геймплейные системы.** `AudioConfig.repeatCooldown` — минимальный зазор между двумя проигрываниями одного клипа; непрерывный DPS стража иначе бьёт каждый кадр. Считается по `Time.unscaledTime`, чтобы пауза не ломала счёт
- **Громкость — тоже `AudioService`** (`SetMasterVolume`), и применяется в его конструкторе из `SaveService.Settings`. `SettingsPresenter` только двигает ползунок: если экрана настроек нет в сцене, презентер не регистрируется, и сохранённая громкость иначе не применилась бы вообще. `AudioMixerGroup` в `AudioConfig` лежит на вырост

### DI вместо событийной раздачи конфигов
- Конфиги собраны в `MainConfig`, инжектятся через VContainer; системы получают их в конструкторе (`WorldCreator` раздаёт). Старые `ConfigSetupSystem`/`GuardSetupConfigEvent` удалены.

---

## Правила кодинга

- **Нет LINQ** — только for-циклы (индексный `for` предпочтительнее `foreach`)
- Компоненты — `struct`, системы — `class`
- IL2CPP директивы на всех компонентах (отключить null/bounds checks)
- Odin: inspector-поля сверху, runtime-поля — под `[Title("Runtime State")]` + `[ReadOnly]`
- `Filter`/`Stash` строить в `OnAwake`, не в `OnUpdate`
- Новая система → регистрация в `WorldCreator.CreateWorld()` + обновить список порядка выше в этом файле
- Новый SO-конфиг → ссылка в `MainConfig` → в систему через конструктор (раздаёт `WorldCreator`)
- Называть: `*Component`, `*System`, `*Provider`, `*Config`, `*Event`
- Комментарии в коде — на английском и по минимуму (ориентир — плотность в текущем файле)
- Порядок методов — caller выше callee (желательно)

## Язык

Отвечать в чат **на русском** — всегда, включая саммари скилов и выводы сабагентов. Комментарии в коде — английские (единственное исключение).

## Планирование

- Перед любой нетривиальной реализацией (задача сложная или правок больше ~20 строк) — план в отдельном `.md` и явное подтверждение пользователя перед исполнением
- Планы лежат **вне гита**: `.claude/agent-memory/plans/<FEATURE>_PLAN.md`, НЕ в `Assets/`. Описания реализованных фич — `.claude/agent-memory/features/<FEATURE>.md`
- Полный цикл (Plan → Execute → обязательная фаза Reflect) — скил `/plan-feature`
- Ревью дифа по правилам проекта — скил `/eb-review`; навигация по проекту — `/where-is`
- Новая пиксель-арт анимация/поза для персонажа — скил `/pixel-anim` (собирает кадры из пикселей существующего атласа, а не генерит нейросетью)
- Иконки способностей, элементы HUD, проверка арта на палитру — скил `/pixel-ui` (рисует примитивами в палитре проекта). Граница с `/pixel-anim`: стыкуется попиксельно с атласом персонажа → `pixel-anim`, самостоятельный элемент UI → `pixel-ui`
- **Генерация и внедрение разделены.** Оба арт-скила в `Assets/` не пишут: результат уезжает в `C:\Users\Phagos\Desktop\Temp\<slug>\` с `MANIFEST.md` (куда класть, PPU, Sprite Mode, `spriteBorder`, что связать). Внедряет скил `/art-apply <slug>` — он единственный трогает проект

## Вопросы

Если задача неоднозначная или есть важные развилки — сначала задать вопросы в чате, и только после ответов приступать к плану или реализации.

## Git

Git — зона ответственности пользователя. Не выполнять операции, меняющие состояние репо или удалёнку: commit, push, fetch/pull, branch, worktree, комментирование PR/issue. Читающие команды (`git status`, `git diff`, `git log`, `git show`) — можно. Редактировать любые файлы можно свободно, запрет касается только git-команд.

## Файлы Unity

- Не создавать `.meta` файлы вручную — Unity генерирует их сама
- Новые скрипты — по существующей структуре: ECS-код в `Ecs/<Domain>/` с неймспейсом `ElderBlood.Ecs.<Domain>`, Unity-слой и сервисы — в `ElderBlood.*`. Неймспейс должен соответствовать папке
- После правок кода, требующих editor-части (провайдеры на префабах, SO-ассеты, input-экшены, Addressables-ключи), — отдельным блоком перечислить, что пользователь собирает руками

## Мутации внешних систем

Читать состояние внешних систем (IDE-диагностика, мост в Unity Editor) можно свободно. Любые мутации через них — сцены, префабы, ассеты, SO-конфиги, скрипты, play mode, запуск тестов, импорт ассетов, операции с пакетами — **только с явного согласия пользователя**. Причина: правки уезжают в реальный проект и молча меняют сериализованные ассеты, а откатывать их руками дорого. Правки обычных `.cs` через Edit/Write — обычный флоу, отдельного согласия не требуют.

## Unity CLI — мост в Unity Editor

Официальный стек Unity (с 2026-09-26, заменил `com.coplaydev.unity-mcp`): CLI `unity` + пакет `com.unity.pipeline` в проекте + плагин `unity@unity-agent-plugin` в Claude Code (скилы). Мост — локальный HTTP к открытому редактору, не удалёнка.

- **Каталог команд и грабли — `.claude/agent-memory/unity-cli-commands.md`.** Там же разделение на читающие и мутирующие. Полный список пересниать через `unity list --format json` (160 команд).
- **Вызов:** `unity command <name> --key value`, булевы как `--confirm true`. `unity status --format json` — жив ли редактор (порт плавает между перезагрузками, не хардкодить).
- **Компиляция:** `unity command console_status` отдаёт `groundTruth.compiling`, `.compilationFailed` и счётчик ошибок одним вызовом; записи — `console --level error --tail 20`. Unity подхватывает правки `.cs` при фокусе окна; принудительно — `recompile` + `recompile_status`.
- **`eval` (Roslyn) удобен для чтения состояния** — дамп тайлмапы, значения `SerializedProperty`, рефлексия по API. Это не мутация, согласия не требует; согласие нужно на запись.
- **Перед написанием Unity-кода** сверять API — прямого аналога `unity_reflect` у CLI нет, поэтому рефлексия через `eval` либо `docs.unity.com`. По памяти не писать.
- **Allow-лист** (`.claude/settings.local.json`) держать только на читающих командах. **Отсутствие запроса разрешения ≠ разрешение менять проект:** правило «Мутации внешних систем» выше действует независимо от allow-листа.
- **Safe Mode** (ошибки компиляции) не даёт загрузиться pipeline-пакету — мост пропадает целиком. Чинить `.cs` и рестартить Unity, а не править файлы вслепую.
- **Команда, вызывающая domain reload, падает с `Network error`, но успевает примениться.** Не повторять вслепую — сначала проверить результат.

### Скилы Unity из плагина

Ставятся все 32 разом (поштучно плагин не режется), зовутся с префиксом `unity:`. Ненужные погашены через `skillOverrides` в `.claude/settings.local.json`: `"off"` — не видно ни мне, ни в меню `/`; `"name-only"` — видно имя без описания (дёшево по контексту, но остаётся находимым). Наши:

| Скил | Когда |
|---|---|
| `unity:2d-pixel-perfect` | PPU/Filter/Cinemachine/микро-статтер — диагностика пиксель-перфекта |
| `unity:sprite-editor` | нарезка листов, пивоты, `spriteBorder` через `ISpriteEditorDataProvider` |
| `unity:sprite-segment-3x3grid` + `unity:tilemap-ruletile-createfromsegment` | автотайлинг: из существующих спрайтов террейна собрать RuleTile |
| `unity:tilemap-palette-create`, `unity:tilemap-ruletile-createempty` | палитры и пустые RuleTile |
| `unity:manage-sprite-atlas` | SpriteAtlas V2 + Addressables (атласов пока нет, на вырост) |
| `unity:ui` → `unity:ui-ugui` | UI проекта на uGUI (Canvas/RectTransform/префабы). `ui` — роутер, сам выберет |
| `unity:optimize-text-mesh-pro` | пиксельный SDF-шрифт `BoldPixels`: padding/sampling, AutoSize, атласы |
| `unity:urp-postprocessing` | Volume-эффекты — под оверлей кровавого зрения (остаток Фазы 8) |
| `unity:shader-graph-create-custom-node` | `Assets/Shaders/CharactersHeals.shadergraph` — кастомные ноды из HLSL |
| `unity:validate-urp-render-graph-renderer-feature` | ревью ScriptableRendererFeature на Render Graph (Unity 6) |
| `unity:optimize-audio` | импорт-настройки звука, Load Type, микшер — под остаток Фазы 8 |
| `unity:generate-editor-search-query` | найти ассеты/объекты запросом Unity Search |
| `unity:unity-cli` | сам CLI: установка, проекты, билды, тесты |

Погашено в `"off"` (13): IAP, LevelPlay, multiplayer-services, Vivox, build-live-game, optimize-web, new-unity-project, physics-3d-collision, migrate-birp-to-urp (уже URP), asset-transformer-toolkit, `ui-uitk`/`ui-imgui` (в проекте нет `.uxml`/`.uss`), initialize-ai-navigation.
В `"name-only"` (2): `localization`, `unity-package-management` — могут понадобиться, но описания у них длинные.

Оговорки:
- **`unity:initialize-ai-navigation` погашен намеренно** — он только про 3D, а у нас `com.h8man.2d.navmeshplus`; включать его — значит получить советы не по нашему стеку.
- **`unity:audio-setup-mixers` уже микшеры не создаёт** — только инвентаризует `AudioSource` и раскладывает по **существующим** группам. Сами группы и громкости — руками.
- **`unity:unity-package-management` утверждает, что CLI не управляет UPM-пакетами** — это про `unity` как таковой. Через мост пакеты ставятся и снимаются: `unity command package_add` / `package_remove` (`--confirm true`, есть `--dry_run true`), проверено на снятии coplay.

## Окружение

Шелл **работает** — и Bash (git, grep, sed, find, ls, rm), и PowerShell 7.6. Проверено 2026-09-16; более ранняя запись про «заблокирован политикой sandbox» была неверной и стоила лишних обходных путей.

Чем пользоваться: файлы — Read/Write/Edit/Glob/Grep (они лучше шелла для чтения и поиска), состояние Unity — через `unity command`, git-чтение — Bash (`git status`/`diff`/`log`). Шелл хорош там, где инструмента нет: `wc`, `sed -n` по диапазону строк, разбор больших логов.

`unity` попадает в PATH только новых терминалов; бинарь — `C:\Users\Phagos\AppData\Local\Unity\bin\unity.exe`.

**Логи Unity — важный источник.** Консоль редактора очищается, а `C:\Users\Phagos\AppData\Local\Unity\Editor\Editor.log` хранит всё со стеками. `console` отдаёт буфер моста (он живёт с момента загрузки pipeline-пакета и переживает очистку консоли), но всё, что было до, и полные стеки — только в файле: `grep -n "\[Run\]\|\[Damage\]\|Exception"`.

---

## Состояние реализации
Фазы 0–8d готовы в коде **и собраны в редакторе**. Петля забега проверена в play mode целиком (2026-09-16): старт → HUD → слив → способности → тревога → урон/щит → вендор и сохранение → переход на этаж 2 → смерть → экран «You Died» → рестарт с мета-прогрессом → «Victory». Детальный статус — `Assets/Docs/Roadmap.md`.

**Фаза 8 (2026-09-27): editor-часть почти собрана, в play mode не проверена.** `MainConfig` держит **все** восемь конфигов, включая `LogConfig` и `AudioConfig`. Осталось три галочки в инспекторе — см. «Щит и оверлей» ниже.

- **Обёртка над логами** — `Services/GameLogger` (каналы по доменам) + `Configs/LogConfig` (SO с флагами, ссылка в `MainConfig`). Все игровые `Debug.Log` переведены на канальные вызовы. Стрипается тройным `[Conditional]`: `UNITY_EDITOR` / `DEVELOPMENT_BUILD` / `ENABLE_LOGS` — в редакторе и dev-билде логи живут, в релизе исчезают, если не добавить `ENABLE_LOGS` в Scripting Define Symbols. `GameLogger.Error` **не** условный и не гейтится каналом. `Init()` зовётся в `GameLifetimeScope.Configure` (раньше всего остального); до него конфига нет — вызовы печатают всё подряд, а не молчат. Флаги гасят запись в консоль, но не сборку строки — логам не место в `OnUpdate`
- **Аудио** — `Services/AudioService` (VContainer-синглтон, свой `AudioSource` + `DontDestroyOnLoad`, `IDisposable`; владеет громкостью и кулдауном на повтор клипа), `Configs/AudioConfig` (SO: клипы + `repeatCooldown` + `AudioMixerGroup`), `Ecs/Audio/SfxEvent` (`IEventData`) + `AudioSystem`. SFX поднимают `AbilitySystem` (каст), `DamageSystem` (урон), `DrainSystem` (слив), `GuardAlertSystem` (вход в Chase — проверка ребра после switch, а не по веткам). Ассет `AudioConfig` собран и стоит в `MainConfig`, клипы назначены (`hurt`/`throw`/`ghost`/`grow_big`), `mixerGroup` → `Audio/BaseMixer`
- **Щит и оверлей кровавого зрения в HUD** — `HudFeedSystem` → `HudChannel.RaiseShield`/`RaiseBloodSight` → `HudView.SetShield`/`SetBloodSight`. Элементы на Canvas **есть и привязаны**: `_shieldFill` → `Hud/Substrate/ShieldFill`, `_bloodSightOverlay` → `/UI/Canvas/Overllay` (опечатка в имени — так в сцене), растянут на весь экран. `_shieldText` пуст — необязателен, `HudView` его чекает на null.
  Осталось руками: (1) назначить оверлею спрайт `Sprites/UI/Hud/BloodVignette`; (2) снять у него **Raycast Target** — сейчас `true`, полноэкранный `Image` съест клики по HUD; (3) поднять `HudView._bloodSightPeakAlpha` — **в сцене он 0**, и оверлей не покажется вообще, даже со спрайтом (дефолт в коде 0.15 рассчитан на сплошную заливку; под брызги рабочее ~0.75)
- **Экран настроек** — `UI/SettingsView` + `SettingsPresenter`, громкость через `AudioListener.volume`, персист в `SaveService.Settings` (`Save/SettingsData`). Открывается кнопкой из меню паузы (`PauseView.SettingsClicked`), закрывается вместе с ним (`PauseView.Closed`). Запись в `save.json` — на закрытии экрана, не на каждый тик слайдера. В `GameLifetimeScope` вью и презентер регистрируются **только если `_settingsView` проставлен**: `RegisterComponent` читает `instance.GetType()` и на null валит весь контейнер. Сцена собрана (объект `SettingView`, кнопка Settings в паузе), в play mode не проверено

**Отключено/не реализовано:**
- `VillageReactionSystem` (реактивность деревни) — закомментирована в `WorldCreator`, ждёт переделки/удаления
- Авто-выдача способностей по этажам — нет (все способности доступны с старта)
- `MoveNavMeshSystem` — не добавлена (но `MoveNavMeshComponent`/`Provider` используются стражей)
- Задержка рестарта под death-анимацию — нет (экран конца забега встаёт сразу, `timeScale = 0` морозит анимацию)
- **Выходы невидимы** — `Exit` на обоих уровнях это пустой Transform без спрайта
