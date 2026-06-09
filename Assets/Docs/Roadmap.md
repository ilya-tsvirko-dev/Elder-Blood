# Elder Blood — Роадмап

Фазовый план разработки. Основан на дизайн-видении (см. `DesignNotes.md`).
Модель: **стелс-экшен-рогалик**. Прямой атаки нет — только способности; кровь = HP = мана = валюта (единый пул).

## Текущее состояние
Работает: загрузка (Bootstrap → Main → LevelManager/Addressables → спавн), движение/анимации игрока, экшены (Jump/Magic/Death/Sit/Hide), Fade (прозрачность через шейдер), детект охранника (`OverlapCircle` 360°, мгновенный/бинарный), преследование по NavMesh, патруль, здоровье охранника из конфига.

---

## Фаза 0 — Технический долг *(в работе)*
- [x] Неймспейсы `DefaultNamespace.*` → `ElderBlood.*`.
- [~] **Внедрить DI — VContainer.** Фундаментальный пункт (все следующие системы будут принимать зависимости через DI).
  - [x] Подключить пакет VContainer (1.18.0, git URL в `manifest.json`).
  - [x] Корневой `GameLifetimeScope : LifetimeScope` — регистрирует `MainMenuView`/`LevelManager`/`CameraController` + презентеры как entry points. `MainEntryPoint` удалён.
  - [x] **Editor-часть:** пакет подтянут; в `Main.unity` `GameLifetimeScope` повешен и ссылки проставлены; кнопка Start и камера работают.
  - [x] **Инъекция конфигов (сделано в начале Фазы 5):** `WorldCreator` получает конфиги через VContainer (`[Inject]`, `RegisterComponent`) и передаёт их в конструкторы систем. `AbilityConfig` → `AbilitySystem` (убрано поле `AbilitiesComponent.config`); `GuardConfig` → `GuardSetupSystem` (применяет один раз). Событийная рассылка конфигов удалена: `ConfigSetupSystem/Component/Provider/Event`, `GuardSetupConfigEvent`. `MainConfig.Reset` (live-tuning) теперь инертна.
  - Системы по-прежнему `new`-аются в `WorldCreator`, но получают зависимости-конфиги через инъекцию в `WorldCreator`. Доп. сервисы (`SaveSystem`, аудио) регистрируются по мере надобности.
- [x] Централизовать магические строки → `Common/AnimatorParams.cs`, `Common/ShaderIds.cs`, `Common/Layers.cs`. Переведены `CharacterAnimationSystem`, `GuardAnimationSystem`, `HealthShaderSystem`, `GuardTriggerSystem`.
- [x] Удалён мёртвый `Guard/GuardAnimation.cs` (MonoBehaviour, легаси до ECS, заменён `GuardAnimationSystem`).
- [~] ~~Удалить мёртвый NavMesh игрока~~ — **отменено**: `MoveNavMeshComponent`/`MoveNavMeshProvider` используются охранниками (`GuardBehaviorSystem` + `Guard.prefab`), это не мёртвый код.
- [x] Организация: корневые файлы разложены в `Movement/` и `Health/`. `WorldCreator` оставлен в корне.
- [x] Разделение ECS / не-ECS: весь ECS-код перенесён в `Ecs/` под неймспейсы `ElderBlood.Ecs.<Domain>` (Character/Movement/Health/Guard/Config, схлопнуто по доменам). Unity-слой и SO-конфиги/Common остались в `ElderBlood.*`. guid'ы целы (ссылки в сценах/префабах по guid).
- [x] `Fade` добавлен в `Controls.inputactions` (Tab) — `Controls.cs` перегенерён, проект компилится.

## Фаза 1 — Ядро детекта
Конус зрения + линия видимости (LoS) + плавная шкала обнаружения.

**Статус: код готов, ждёт editor-части.** Созданы `Guard/Vision/`: `PerceptionState`, `GuardVisionComponent`(+Provider), `GuardPerceptionComponent`(+Provider), `GuardVisionSystem`. В `WorldCreator` `GuardTriggerSystem` заменён на `GuardVisionSystem`. Старый модуль детекта удалён (`GuardTriggerSystem/Component/Provider`, `DebugTriggerEvent`). Дебаг-конус рисуется через `Debug.DrawRay` (Scene view), цвет по `state`.
**Editor-часть (на разработчике):** на префабе `Guard` убрать missing-`GuardTriggerProvider`, добавить `GuardVisionProvider` (задать `viewRadius`, `viewAngle`, `eyeOffset`, `obstacleMask` = слой стен-тайлмапа, `fillSpeed`, `decaySpeed`) и `GuardPerceptionProvider`.

**Объём v1 (зафиксирован):**
- Заметность зависит **только от дистанции** (без множителей за бег/Sit/Fade).
- LoS рвут **только стены** (`obstacleMask`); зон укрытия нет.
- **Без осматривания** — `facing` держит последнее направление движения.
- Отложено: Fade/невидимость как фактор детекта → уедет в способность Невидимости (Фаза 4); зоны укрытия и вращение конуса — позже.

**Новые элементы:**
- `GuardVisionComponent` — `viewRadius`, `viewAngle`, `eyeOffset`, `obstacleMask`, runtime `facing`.
- `GuardPerceptionComponent` — `awareness` (0..1), `state`, `lastSeenPosition`.
- `PerceptionState { Unaware, Suspicious, Alert }`.
- `GuardVisionSystem` — broad-phase `OverlapCircle` → угол конуса → LoS-рейкаст → набор/спад `awareness` по дистанции → состояние по порогам (с гистерезисом). На `Alert` пишет цель в `GuardBehaviorComponent`.
- Дебаг: расширить `LineRenderer` — рисовать конус, красить по `state`.

**Выход:** `state` + `lastSeenPosition` + цель → вход Фазы 2.

## Фаза 2 — FSM тревоги
Состояния: Patrol → Suspicious → Chase → Search → Return.

**Статус: код готов, ждёт editor-части.** Созданы `Ecs/Guard/Alert/`: `GuardAlertState`, `GuardAlertComponent`(+Provider), `GuardAlertSystem`. `GuardPatrolSystem` теперь продвигает маршрут только в `Patrol` (требует `GuardAlertComponent` в фильтре). `GuardAlertSystem` добавлен в `WorldCreator` (Vision → Alert → Behavior). Suspicious = стоп + конус к источнику; Search = идёт к `lastSeenPosition`, таймер → Return; Return → к `patrol.currentPoint` → Patrol. `targetTransform` (Chase) по-прежнему ставит `GuardVisionSystem`.
**Editor-часть (на разработчике):** на префабе `Guard` добавить `GuardAlertProvider`, задать `searchDuration` (напр. 3) и `returnReachDistance` (напр. 0.3). **Без `GuardAlertProvider` охранник перестанет патрулировать** (патруль теперь требует этот компонент).

**Объём v1 (зафиксирован):**
- **Suspicious** — стоит на месте и поворачивается к источнику (без шагов навстречу).
- **Search** — идёт только к `lastSeenPosition`, осматривается, таймаут → Return (без обхода нескольких точек).
- **Общая тревога** (охранники оповещают друг друга) — отложена в отдельную фазу.

**Переходы:**
```
Patrol     → Suspicious : perception.state == Suspicious
Suspicious → Patrol      : awareness → 0
Suspicious → Chase       : awareness == full (Alert)
Chase      → Search      : цель потеряна (запоминаем lastSeenPosition)
Search     → Chase       : снова Alert
Search     → Return      : searchTimer истёк
Return     → Patrol      : дошёл до маршрута
```

**Новые элементы:**
- `GuardAlertState { Patrol, Suspicious, Chase, Search, Return }`.
- `GuardAlertComponent` — `state`, `searchTimer`, `lookTimer`, `searchPoint`, `homePatrolIndex`.
- `GuardAlertSystem` — FSM: читает perception, выбирает destination, гейтит патруль.

**Рефактор:** `GuardBehaviorSystem` → чистый исполнитель (двигайся к destination + атакуй в радиусе); `GuardPatrolSystem` продвигает маршрут только в Patrol.

## Фаза 3 — Система крови (ядро экономики)
Единый пул Кровь = HP + мана + валюта, с потолком ёмкости (~3 человек, качается). Слив с источников (спящие жители, стражники, мёртвые животные). Урон → кровь; кровь = 0 → смерть.

**Разбито на 3a (урон/смерть) и 3b (слив).**

**3a — код готов, ждёт editor-части.** Домен `Ecs/Blood/`: `BloodComponent`(+Provider, `current`/`capacity`, float, старт = capacity), `DamageEvent`, `PlayerDeathEvent`, `DamageSystem`. `GuardBehaviorComponent` получил `attackDamage`; `GuardBehaviorSystem` в атаке (Chase + minMagnitude) поднимает `DamageEvent{attackDamage*dt}` (непрерывный DPS, пока цель в радиусе). `DamageSystem` (Subscribe) вычитает из крови игрока; при ≤0 → ставит `Death`-анимацию + `isAction` и поднимает `PlayerDeathEvent`. Игрок получает `BloodComponent` рядом с `HealthComponent` (heal-шейдер пока на Health; свяжем с кровью в HUD/Фаза 8). Реальный рестарт по смерти — Фаза 7.
**Editor-часть (на разработчике):** на префаб игрока добавить `BloodProvider` (`capacity`, напр. 100); на префабе `Guard` задать `attackDamage` (напр. 30 — это DPS, пока бьёт).

**3b — код готов, ждёт editor-части.** `BloodSourceComponent`(+Provider: `transform` ставит провайдер, `amount` в инспекторе), `DrainEvent`, `DrainSystem` (по `Interact`/F находит ближайший источник в `BloodComponent.drainRadius`, мгновенно переливает с учётом потолка, блок при полном пуле). `InputSystem` поднимает `DrainEvent` по `Interact`. `BloodComponent` получил `drainRadius`. Поле `state` у источника отложено в Фазу 6.
**Editor-часть:** игроку задать `drainRadius` (напр. 1.5); создать тестовый источник — GameObject + `BloodSourceProvider` (`amount`, напр. 50).

**Объём v1 (зафиксирован):**
- **Раздельные компоненты:** `BloodComponent` (игрок: `current`, `capacity`), `BloodSourceComponent` (`amount`, `state`, `canDrain`), `HealthComponent` оставляем для тех, кто получает урон (стражники).
- **Слив — мгновенный** по интеракции рядом с источником.
- **При полной ёмкости слив блокируется** (надо сначала потратить кровь у Бога Крови).
- **Щит отложен в Фазу 5** — пока урон идёт сразу в кровь/HP, слоя щита нет.

**Новые элементы:**
- Компоненты: `BloodComponent`, `BloodSourceComponent`. *(Щит — позже.)*
- События: `DamageEvent { target, amount }`, `DrainEvent`/интеракция, `PlayerDeathEvent`. *(на будущее: `BloodChangedEvent` для HUD.)*
- Системы:
  - `DamageSystem` — применяет урон в кровь/HP; кровь игрока ≤ 0 → `PlayerDeathEvent` (→ `Death`-анимация + перезагрузка уровня заглушкой).
  - `DrainSystem` — мгновенный перелив из ближайшего `BloodSource` в радиусе в `Blood.current` с учётом потолка; блок при полном пуле.
  - *(возможно)* `DeathSystem` — поверженный NPC → труп с `BloodSourceComponent`.

**Хук:** страж в Chase, попав в `minMagnitude`, поднимает `DamageEvent(player, guardDamage)`; в `GuardConfig` добавится `damage`.

## Фаза 4 — Способности
Общий каркас (стоимость в крови = мана, кулдауны). Первые три: Невидимость (+скорость, враги теряют из вида), Кровавая лужа-капкан (движение = отмена; жертва наступила = смерть + кровь; не работает, если видели при превращении), Кровавое зрение (подсветка крови в радиусе, дальность качается).

**Разбито на 4a (каркас+Невидимость), 4b (Лужа), 4c (Зрение).**

**4a — код готов, ждёт editor-части.** `Configs/AbilityConfig` (SO), `Ecs/Abilities/`: `AbilityType`, `AbilityCastEvent`, `AbilitiesComponent`(+Provider), `InvisibilityActiveComponent`, `AbilitySystem`, `InvisibilitySystem`. Буст скорости через новое `MoveComponent.speedMultiplier` (MoveProvider→1, MoveSystem умножает). Скрытие от стражи: `GuardVisionSystem` через `EntityProvider` проверяет `InvisibilityActiveComponent` у цели → «не видно». `InputSystem`: `Invisibility`→`AbilityCastEvent`. `WorldCreator`: +AbilitySystem,+InvisibilitySystem.
**Editor-часть:** добавить input-экшен `Invisibility` (напр. клавиша 1); создать ассет `AbilityConfig` (cost/cooldown/duration/speedMult); на игрока добавить `AbilitiesProvider` с этим конфигом.
- Ручной `Fade` из `InputSystem` удалён (рудимент — невидимость владеет `isFaded`); action `Fade` в Controls можно убрать.

**4b (Лужа) — код готов, ждёт editor-части.** `BloodPuddleActiveComponent`, `BloodPuddleSystem`; `AbilityConfig`+`AbilitiesComponent` получили puddle-поля; `AbilitySystem.TryBloodPuddle` (каст если ни один страж не aware); `InputSystem`: `BloodPuddle`→cast; `WorldCreator`: +BloodPuddleSystem. Логика: каст → игрок замирает «лужей» в точке; движение → отмена; `BloodSource` в радиусе → слив его `amount` + уничтожение источника, лужа расходуется. **Визуал лужи (свап спрайта) отложен; «кто наступит» упрощено до «любой источник в радиусе»; лужа не прячет от стражи.**
**Editor-часть:** input-экшен `BloodPuddle` (напр. 2); задать puddle-поля в `AbilityConfig`; для жертвы-стража — `BloodSourceProvider` на префаб `Guard`.
- Выход из лужи переделан на **тоггл** (повторное нажатие `2` → `WakeUp`); движение не отменяет. Sit полностью переименован в Puddle (enum/AnimatorParams/триггер аниматора), ручной Sit (Ctrl) убран.

**4c (Кровавое зрение) — код готов, ждёт editor-части.** `BloodSightActiveComponent`, `BloodSightSystem`; `AbilityConfig`/`AbilitiesComponent` + bloodSight-поля; `BloodSourceComponent` получил `renderer`+highlight-поля; `AbilitySystem.TryBloodSight`; `InputSystem`: `BloodSight`→cast; `WorldCreator`: +BloodSightSystem. Импульс на `duration`: тинт `SpriteRenderer` источников в радиусе, возврат при выходе/истечении.
**Editor-часть:** input-экшен `BloodSight` (напр. 3); bloodSight-поля в `AbilityConfig`; подсветка видна только у источников со `SpriteRenderer`.

**Фаза 4 завершена** (4a Невидимость, 4b Лужа, 4c Кровавое зрение).

**Объём v1 (зафиксирован):**
- **Гибридный каркас:** `AbilityConfig` (SO — на способность: `cost`, `cooldown`, `duration`, параметры) + `AbilitiesComponent` (слоты + кулдауны) + **тег-компоненты активных состояний** (`InvisibilityActiveComponent`, `BloodPuddleComponent`).
- **Ввод — отдельная клавиша на способность.**
- **Кровавое зрение — импульс на время** (разовая стоимость, подсветка N секунд).
- **Все 3 способности доступны с старта** (реальный гейтинг по этажам/у Бога Крови — Фазы 5/7).

**Системы:**
- `AbilitySystem` — ввод → проверка `cost`/кулдаун → списание крови → запуск (тег + таймер), тик кулдаунов.
- `InvisibilitySystem` — переиспользует Fade (`isFaded`) + множитель скорости; гасит `awareness` (Фаза 1).
- `BloodPuddleSystem` — каст только если «не видят» (perception Фазы 1); ввод движения = отмена; наступивший → смерть + слив крови (Фаза 3).
- `BloodSightSystem` — подсветка `BloodSource`/стражей в радиусе на время.

**Связки:** кровь (Фаза 3) — стоимость и награда; детект (Фаза 1) — невидимость гасит awareness, лужа требует «не видят».

## Фаза 5 — Бог Крови + рогалик-прогрессия
Трата крови на апгрейды (HP/ёмкость, Щит, способности, дальность кровавого зрения).

**Объём v1 (зафиксирован):**
- **Прогрессия персистит (мета):** купленные апгрейды сохраняются между забегами → нужны сохранения и `PlayerProgressComponent`.
- **Валюта — живой пул крови:** тратишь текущую кровь (= жизнь/мана). Это и есть задуманное напряжение единого ресурса.
- **Бог Крови — хаб между этажами.**
- **Трактовка:** кровь — расходник в пределах забега (набираешь сливом каждый забег); купленные на неё апгрейды — постоянные.
- **Щит открывается здесь** (отложен из Фазы 3). **5a сделано:** `ShieldComponent`(+Provider), `DamageSystem` поглощает урон щитом перед кровью (если `unlocked`). Не саморегенится. Editor: `ShieldProvider` на игрока (для теста `unlocked=true`, `max`); реальная разблокировка — вендор 5c.
- **Бэкенд сохранений — здесь** (JSON в `persistentDataPath`, персистит `PlayerProgress` + `VillageState` Фазы 6). В Фазе 8 — только UI настроек/слотов.

**Новые элементы:**
- `UpgradeConfig` (SO-каталог: `id`, стоимость крови, эффект, тиры).
- `SaveSystem` — сериализация `PlayerProgress` (+ `VillageState`) в JSON.
- `PlayerProgressComponent` — что куплено/прокачано.
- `GodOfBloodComponent` — точка вендора.
- `UpgradeSystem` — списать кровь → применить к `BloodComponent`/`ShieldComponent`/`AbilitiesComponent`/зрению.
- UI вендора — Фаза 8; в v1 простая интеракция.

**Развязка:** цикл этажей, переходы и выдача способностей по этажам — Фаза 7.

## Фаза 6 — Реактивность деревни
Состояние деревни меняется от действий игрока (сколько слито/убито).

**Объём v1 (зафиксирован):**
- **Состояние персистит (мета)** — стыкуется с сохранениями/`PlayerProgress` (Фаза 5).
- **Единственный эффект v1: жители просыпаются/разбегаются** с ростом `notoriety` (меньше лёгкой добычи).
- **Только рост** — без восстановления состояния в v1.

**Новые элементы:**
- `VillageStateComponent` — `notoriety`, `tier`, счётчики `drained`/`killed`.
- `VillageReactionSystem` — `notoriety` → `tier` → будит/разгоняет жителей.
- Вход: события слива/смерти из Фазы 3.

**Отложено:** доп./тревожная стража, истощение источников, цены у Бога Крови, восстановление состояния.

## Фаза 7 — Поток этажей замка
Подъём по этажам, способности по этажам, цели уровней, победа на вершине (освободить возлюбленную), смерть → конец забега. Опирается на существующий `LevelManager` (`LoadFirstLevel`/`LoadNextLevel`/`_currentIndex`) + `LevelContainer` (упорядоченный список).

**Объём v1 (зафиксирован):**
- **Фиксированная авторская последовательность этажей** (LevelContainer как сейчас, без рандома).
- **Цель этажа — дойти до выхода** (`ExitComponent`-триггер → «этаж пройден»).
- **Способности выдаются авто** по порядку при подъёме.

**Петля забега:**
```
старт → этаж 1 → (цель) → хаб Бога Крови → LoadNextLevel → … → вершина + цель = ПОБЕДА
смерть (PlayerDeathEvent) → конец забега → мета персистит → старт с этажа 1
```

**Новые элементы:** `RunComponent`, `ExitComponent`, `LevelObjectiveComponent`, `ObjectiveSystem`, `FloorTransitionSystem` (или в `LevelManager`).

**Обязательный фикс:** `LevelManager.LoadLevelRoutine` зовёт `WorldCreator.CreateWorld()` на каждой загрузке, а тот каждый раз добавляет новую систем-группу → при переходах системы наслаиваются/дублируются. Нужно создавать системы один раз и переиспользовать (или диспозить между этажами).

## Фаза 8 — HUD / UX и полиш
HUD, индикаторы тревоги, пауза, аудио, UI сохранений. UI в существующем паттерне MVP (View + Presenter, как меню).

**Объём v1 (зафиксирован):**
- **UI читает ECS через события** (`BloodChangedEvent` и т.п.), не опросом.
- **HUD v1:** шкала крови + кулдауны способностей + индикаторы тревоги (`?`/`!`) + меню паузы. *(Щит и оверлей кровавого зрения — позже.)*

**Новые элементы:**
- `HudView` / `HudPresenter`.
- `PauseView` / `PausePresenter` (resume / restart / quit).
- Презентер индикаторов тревоги над стражей (по `GuardAlertState`).
- Аудио-сервис (SFX по событиям: урон, тревога, каст, слив).

**Сохранения:** бэкенд сохранений **перенесён в Фазу 5** (JSON в `persistentDataPath`, персистит `PlayerProgress` + `VillageState`). В Фазе 8 — только UI настроек/слотов.

---

## Сюжетный контекст (проходит через Фазы 6–7)
Игрок — кровь древнего вампира, освобождает возлюбленную из заточения Инквизиции; поднимается по этажам замка, получая способности. Подробнее — `DesignNotes.md`.
