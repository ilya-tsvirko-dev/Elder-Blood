# Elder Blood — Роадмап

Фазовый план разработки. Основан на дизайн-видении (см. `DesignNotes.md`).
Модель: **стелс-экшен-рогалик**. Прямой атаки нет — только способности; кровь = HP = мана = валюта (единый пул).

## Текущее состояние

**2026-09-16: петля забега проверена в play mode целиком.** Пройдено подряд: старт → HUD крови → слив
источника → все три способности с кулдаунами → индикаторы `?`/`!` над стражей → погоня, урон, щит →
покупка у Бога Крови и запись `save.json` → выход на этаж 2 с переносом крови и кулдаунов → смерть →
экран «You Died» → Restart с сохранённым мета-прогрессом → выход на последнем этаже → «Victory».
Консоль чистая (единственные предупреждения — от пакета UnityMCP, не от игрового кода).

Пометки «ждёт editor-части» по Фазам 1–8 ниже **устарели**: editor-часть собрана, `GameLifetimeScope`
в `Main.unity` держит все ссылки (меню, HUD, пауза, экран конца забега, LevelManager, камера,
WorldCreator, MainConfig).

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
- [ ] **`MoveSystem`: убрать `deltaTime` из присвоения скорости.** Сейчас `rigidbody.linearVelocity = moveVector * (deltaTime * speed)`, а `linearVelocity` — это уже скорость, домножать её на шаг времени не надо. Из-за этого числа в инспекторе ровно в 50 раз больше реальных (шаг физики 0.02): у игрока `speed = 30` даёт 0.6 ед/с, `runSpeed = 80` — 1.6 ед/с, у стража `speed = 40` — 0.8 ед/с. Работает только потому, что шаг фиксированный; при смене Fixed Timestep вся игра поедет по скоростям. Правка сама по себе однострочная, но требует **пересчёта всех скоростей на префабах** (поделить текущие на 50) — поэтому отдельной задачей, не по ходу дела.

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

**5b (сохранения + PlayerProgress) — код готов, ждёт проверки в Editor.** `Save/PlayerProgressData` (plain `[Serializable]` DTO, модель **бонусов поверх базы префаба**: `bloodCapacityBonus`, `shieldUnlocked`, `shieldMaxBonus`), `Save/SaveService` (VContainer-синглтон, JSON в `persistentDataPath/save.json`, `Load`/`Save`/`Progress`, грузит в конструкторе), `Ecs/Progression/PlayerProgressComponent` (runtime-зеркало на игроке, добавляется системой — провайдер не нужен), `Ecs/Progression/PlayerProgressSystem` (инжектит `SaveService`; применяет прогресс к `BloodComponent.capacity`+`ShieldComponent` однократно в `OnUpdate`, т.к. игрок спавнится асинхронно после `CreateWorld`; подписка на `PlayerDeathEvent` → `Save()`). `SaveService` зарегистрирован в `GameLifetimeScope`; `WorldCreator` инжектит его и добавляет `PlayerProgressSystem` (после `GuardPatrolSystem`, до `DamageSystem`). **Объём 5b:** только кровь+щит; ранги способностей/дальность зрения — в 5c с каталогом апгрейдов.
**Editor-часть:** провайдеров добавлять не нужно. Проверить: в `Main.unity` `GameLifetimeScope` резолвит `SaveService` без ошибок; файл `save.json` появляется в `persistentDataPath` после смерти игрока.

**5c (вендор Бога Крови) — код готов, ждёт editor-части.** Интеракция v1: **одна клавиша `Upgrade`** рядом с вендором покупает следующий пункт из **плоского упорядоченного каталога**. `Configs/UpgradeType` (enum: BloodCapacity/ShieldUnlock/ShieldMax), `Configs/UpgradeConfig` (SO: `List<UpgradeEntry{ id, type, cost, amount }>`; добавлен в `MainConfig`). `Ecs/Progression/GodOfBloodComponent`(+Provider: `transform`+`interactRadius`), `UpgradeEvent`, `UpgradeSystem` (инжектит `MainConfig`+`SaveService`; по событию: игрок в радиусе вендора → берёт `upgrades[purchasedCount]` → хватает ли крови → списывает `cost` из живого пула → применяет эффект к `Blood`/`Shield` + к `PlayerProgressComponent` + к `SaveService.Progress` → `purchasedCount++` → `Save()`). `PlayerProgressData` получил `purchasedCount`. `InputSystem`: `Upgrade`→`UpgradeEvent`. `WorldCreator`: +`UpgradeSystem` (после `PlayerProgressSystem`, до `DamageSystem`). **Объём 5c:** только кровь+щит-апгрейды (совпадает с 5b); ранги способностей/дальность зрения отложены. UI вендора — Phase 8.
**Editor-часть:** (1) добавить input-экшен `Upgrade` в `Controls.inputactions` и перегенерить `Controls.cs` — **иначе `InputSystem` не скомпилится** (`_controls.Main.Upgrade`); (2) создать ассет `UpgradeConfig`, заполнить упорядоченный список покупок (тиры = несколько записей подряд); (3) проставить `UpgradeConfig` в `MainConfig`; (4) в тестовом уровне создать объект Бога Крови + `GodOfBloodProvider` (задать `interactRadius`).

**Фаза 5 завершена в коде** (5a Щит, 5b сохранения, 5c вендор) — остаётся editor-часть.

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

**Код готов, ждёт editor-части.** Эффект v1 = **только не-сливаемость** (без AI-бегства): на росте `tier` спящие источники просыпаются → `state = Awake`, `DrainSystem` их пропускает (лужа 4b консумит по-прежнему — у неё свой путь). Драйвер notoriety v1 = **успешные сливы** (убийств стражи в игре нет). Состояние **персистит сразу**. Новые элементы: `Ecs/Blood/SourceState`(enum Asleep/Awake) + поле `state` в `BloodSourceComponent`; `Ecs/Blood/SourceDrainedEvent{amount}` (поднимается `DrainSystem` по факту слива, не по нажатию); `Configs/VillageConfig`(SO: `notorietyPerDrain`, `notorietyPerTier`) → в `MainConfig`; `Ecs/Village/VillageStateComponent`(notoriety/tier/drainedCount/killedCount); `Ecs/Village/VillageReactionSystem`(инжектит `MainConfig`+`SaveService`; создаёт/сидит сущность деревни из сейва — `World.Default` переживает `CreateWorld`, поэтому переиспользует существующую; по `SourceDrainedEvent` растит notoriety/счётчик → пересчёт tier → на росте будит спящих → персист). `SaveService` отрефакторен в корень `SaveData{ player, village }` (+ `Village`-свойство; `Save/VillageStateData`). `WorldCreator` +`VillageReactionSystem`. Логи `[Village]`.
**Editor-часть:** (1) создать ассет `VillageConfig`, проставить в `MainConfig`; (2) у источников-жителей `state = Asleep` (дефолт enum = Asleep — ок); если страж-источник не должен сливаться вручную после тревоги — ему тоже сгодится `Asleep`; (3) **формат `save.json` сменился** (корень `SaveData`) — старый файл подхватится с дефолтами (можно удалить).

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

**Код готов, ждёт editor-части.** Решения v1: **один игрок на забег** (спавнится раз, между этажами телепортируется на spawn-point — кровь/кулдауны переносятся; новый игрок только на рестарте); **вендор на этаже** (отдельного хаб-уровня нет); **детект выхода — проксимити в системе**.
- **Фикс пере-наслоения сделан:** `WorldCreator.CreateWorld()` под флагом `_created` (систем-группа создаётся один раз, `World.Default` живёт между этажами, фильтры подхватывают новые сущности).
- **Регрессия от фикса исправлена** в системах, что применяли конфиг «один раз» глобальным флагом: `PlayerProgressSystem` и `GuardSetupSystem` переведены на фильтр `Without<тег>` (`PlayerProgressComponent` / новый `GuardConfiguredComponent`), чтобы применяться к каждому новому игроку/стражу (в т.ч. на след. этажах и рестарте).
- **`LevelManager`** переписан: `StartRun` (CreateWorld раз + спавн игрока + этаж 0), `AdvanceFloor` (след. этаж или `WinRun`), `RestartRun` (на смерть: уничтожить игрока + StartRun), `LoadFloor`/`WinRun`; выгружается только геометрия (`Addressables.ReleaseInstance`), игрок живёт; телепорт через `TryPlacePlayer` (гонка спавн-игрока/загрузки-уровня разрешена общим `_pendingSpawnPoint`).
- **Новое ECS:** `Ecs/Level/ExitComponent`(+Provider: `transform`+`reachRadius`), `Ecs/Level/ObjectiveSystem` (инжектит `LevelManager`; проксимити игрок→выход → `AdvanceFloor`; подписка `PlayerDeathEvent` → `RestartRun`). `Ecs/Guard/GuardConfiguredComponent` (тег). `WorldCreator` инжектит `LevelManager`, +`ObjectiveSystem`. `MainMenuPresenter`: `LoadFirstLevel`→`StartRun`. Логи `[Run]`.
- **Отложено/упрощено:** `RunComponent`/`LevelObjectiveComponent` не заведены (run-стейт держит `LevelManager`, цель v1 = только выход); **авто-выдача способностей по этажам не реализована** (все способности и так доступны с старта — гейтинг отложен); победа = лог `[Run] WIN` (экран победы — Фаза 8); рестарт на смерть мгновенный (задержка под death-анимацию — полиш Фазы 8).
**Editor-часть:** (1) на каждом уровне-префабе на объект-выход повесить `ExitProvider` (задать `reachRadius`); (2) `LevelContainer` — список этажей по порядку (как есть); (3) на каждом этаже разместить вендор Бога Крови (`GodOfBloodProvider`, из 5c); (4) `LevelBootstrap.PlayerSpawnPoint` на каждом этаже. **Внимание:** `CLAUDE.md` описывает старый поток загрузки (спавн игрока на каждом уровне) — устарел.

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

**8a (ядро HUD: кровь + кулдауны) — код готов, ждёт editor-части.** Мост ECS→UI: `UI/HudChannel` (DI-синглтон, C#-события `BloodChanged(current,capacity)` / `CooldownsChanged(HudCooldowns)`) + `Ecs/Hud/HudFeedSystem` (инжектит `HudChannel`+`MainConfig`; читает `BloodComponent`/`AbilitiesComponent` игрока, дёргает события только при изменении; кулдауны нормализует remaining/total из `AbilityConfig`). UI (MVP): `UI/HudView` (MonoBehaviour: `Image _bloodFill` + `TMP_Text` + 3 `Image` кулдаун-оверлея, `SetBlood`/`SetCooldowns`), `UI/HudPresenter` (entry point, подписан на `HudChannel`). Регистрация в `GameLifetimeScope` (`_hudView` RegisterComponent, `HudChannel` Singleton, `HudPresenter` EntryPoint); `WorldCreator` инжектит `HudChannel`, +`HudFeedSystem`. **Решение:** вместо Morpeh-`BloodChangedEvent` использован C#-мост `HudChannel` (презентеры VContainer не подписываются на Morpeh-`Event<T>`).
**Editor-часть:** собрать Canvas с HUD — шкала крови (filled `Image`) + опц. текст + 3 кулдаун-иконки (filled `Image`), повесить `HudView`, проставить ссылки; в `GameLifetimeScope` назначить `_hudView`.
**8b (индикаторы тревоги над стражей) — код готов, ждёт editor-части.** Per-guard ECS (как `GuardAnimationSystem`, не презентер): `Ecs/Guard/Alert/GuardAlertIndicatorComponent` (`renderer` + `suspiciousSprite`«?» + `chaseSprite`«!» + runtime lastState/initialized), `GuardAlertIndicatorProvider`, `GuardAlertIndicatorSystem` (читает `GuardAlertComponent.state` → Suspicious/Search=«?», Chase=«!», Patrol/Return=скрыт; трогает рендерер только на смене состояния). `WorldCreator`: +система после `GuardAnimationSystem`.
**Editor-часть:** на префаб `Guard` добавить дочерний `SpriteRenderer` (иконка над головой), повесить `GuardAlertIndicatorProvider`, задать `renderer` + спрайты «?»/«!».

**8c (меню паузы) — код готов, ждёт editor-части.** MVP: `UI/PauseView` (панель + кнопки Resume/Restart/Quit, `Show`/`Hide`, события), `UI/PausePresenter` (`IStartable`/`IDisposable`; заводит свой `Controls`, тоггл по `Main.Pause.performed`; `Time.timeScale` 0/1; Restart → `LevelManager.RestartRun`, Quit → `Application.Quit`/стоп в эдиторе; `Dispose` снимает подписку, `Disable()` и всегда возвращает `timeScale=1`). Регистрация в `GameLifetimeScope` (`_pauseView` RegisterComponent, `PausePresenter` EntryPoint).
**Editor-часть:** (1) ⚠️ добавить input-экшен `Pause` в `Controls.inputactions` (карта `Main`, напр. `Esc`) и перегенерить `Controls.cs` — **иначе `PausePresenter` не скомпилится** (`_controls.Main.Pause`); (2) собрать панель паузы (скрытый `GameObject` + 3 кнопки Resume/Restart/Quit), повесить `PauseView`, проставить ссылки; (3) назначить `_pauseView` в `GameLifetimeScope`.
**Известные упрощения:** при `timeScale=0` колбэки `Controls` всё ещё срабатывают (игрок может кастовать/двигать ввод на паузе — ECS-движение стоит из-за dt=0, но события идут); пауза не гейтится по «идёт ли забег» (работает и в меню). Полировать позже при необходимости.

**8d (экран победы/смерти) — код готов, ждёт editor-части.** Мост `UI/RunChannel` (DI-синглтон, события `RunWon`/`RunLost`). `UI/RunEndView` (панель + `TMP_Text` заголовок + кнопки Restart/Quit, `Show(title)`/`Hide`), `UI/RunEndPresenter` (подписан на `RunChannel`: won→Show("Victory"), lost→Show("You Died"), `Time.timeScale=0`; Restart→`timeScale=1`+`LevelManager.RestartRun`+Hide; Quit→стоп). `LevelManager`: инжектит `RunChannel`; `WinRun`→`RaiseWon`; новый `EndRunDeath`→`RaiseLost`. `ObjectiveSystem` на `PlayerDeathEvent` теперь зовёт `EndRunDeath` (раньше — мгновенный `RestartRun`; рестарт теперь по кнопке экрана). Регистрация в `GameLifetimeScope` (`_runEndView` RegisterComponent, `RunChannel` Singleton, `RunEndPresenter` EntryPoint).
**Editor-часть:** собрать оверлей (скрытый `GameObject` + заголовок TMP + кнопки Restart/Quit), повесить `RunEndView`, проставить ссылки; назначить `_runEndView` в `GameLifetimeScope`.
**Упрощение:** при смерти оверлей показывается сразу (`timeScale=0` морозит death-анимацию на середине) — задержку под анимацию можно добавить позже.

**Осталось в Фазе 8:** аудио-сервис, UI слотов/настроек, щит/оверлей зрения в HUD.

**8e (обёртка над логами) — заметка, к реализации не приступали.** Логов будет много: сейчас `Debug.Log` раскидан
по системам с префиксами `[Run]`, `[Damage]`, `[Village]` и т.п., и дальше их станет только больше. Нужна обёртка,
чтобы включать/выключать вывод по надобности. Ориентиры на момент обсуждения (решать при старте задачи):
- каналы по доменам (Run / Damage / Guard / Blood / Save / Hud) с отдельными флагами
- полное вырезание из релизного билда через `[Conditional("ENABLE_LOGS")]` — тогда и строки не собираются,
  и `string`-интерполяция в вызовах не стоит ничего
- флаги держать там же, где остальная конфигурация (SO рядом с `MainConfig`), чтобы переключать без рекомпиляции
- заодно пройтись по существующим `Debug.Log` и перевести на обёртку

---

## Сюжетный контекст (проходит через Фазы 6–7)
Игрок — кровь древнего вампира, освобождает возлюбленную из заточения Инквизиции; поднимается по этажам замка, получая способности. Подробнее — `DesignNotes.md`.
