# Рефактор MainMenu / Level loading — план

## Цель
- Ввести **Presenter** (отделить View от логики меню).
- Вынести список уровней в **LevelContainer (ScriptableObject)**.
- Убрать **синглтон** `LevelManager.Instance`.
- Всё связывать через **EntryPoint** — единую точку запуска в каждой сцене (composition root).

## Согласованные решения
- `LevelManager` остаётся **MonoBehaviour** (нужен `StartCoroutine`), но **без** `static Instance` / `DontDestroyOnLoad`.
- **EntryPoint в каждой сцене**: сохраняем `Bootstrap.unity → Main.unity`. В `Main` добавляем `MainEntryPoint` (composition root). `BootstrapEntryPoint` остаётся как есть.
- `LevelContainer` — **упорядоченный список** `AssetReference` (загрузка по индексу + `LoadFirst`/`LoadNext`).
- DI-фреймворка нет → composition root ручной.

---

## Изменения по файлам

### Новые

**1. `Assets/Scripts/ElderBlood/Configs/Levels/LevelContainer.cs`**
- `ScriptableObject`, `[CreateAssetMenu(menuName = "Configs/LevelContainer")]` (в стиле `MainConfig`).
- `[SerializeField] List<AssetReference> _levels`.
- Аксессоры: `int Count`, `AssetReference First`, `AssetReference GetByIndex(int index)`.

**2. `Assets/Scripts/ElderBlood/UI/MainMenuView.cs`** (заменяет `MainMenuUI`)
- Чистая View.
- `[SerializeField] Button _startButton`.
- `event Action StartClicked` → инвок в `onClick`; отписка в `OnDestroy`.

**3. `Assets/Scripts/ElderBlood/UI/MainMenuPresenter.cs`**
- Обычный C#-класс (не MonoBehaviour).
- `ctor(MainMenuView view, LevelManager levelManager)`.
- Подписка `view.StartClicked → levelManager.LoadFirstLevel()`.
- `Dispose()` — отписка.

**4. `Assets/Scripts/ElderBlood/GameManager/MainEntryPoint.cs`**
- Composition root в `Main`-сцене (MonoBehaviour).
- `[SerializeField] MainMenuView _view; LevelManager _levelManager;`
- `Awake`: создаёт `MainMenuPresenter`. `OnDestroy`: `presenter.Dispose()`.

### Изменяемые

**5. `Assets/Scripts/ElderBlood/GameManager/LevelManager.cs`**
- Убрать `Instance`, `Awake`, `DontDestroyOnLoad` (Main живёт всю игру; уровни — дочерние префабы через Addressables, синглтон не нужен).
- Добавить `[SerializeField]`: `LevelContainer _levelContainer`, `Transform _levelsParent` (+ существующие `_playerPrefab`, `_worldCreator`).
- Методы: `LoadFirstLevel()`, `LoadNextLevel()`, `LoadLevel(int index)` (хранит `_currentIndex`).
- `LoadLevelRoutine` использует `_levelsParent` вместо параметра.

### Удаляемые

**6. `Assets/Scripts/ElderBlood/UI/MainMenuUI.cs`** (+ `.meta`) — заменён View + Presenter.

---

## Ручная работа в Unity (после кода)
Редактирование `.unity` / `.asset` из кода не делаем — нужно в редакторе:
- Создать ассет `LevelContainer` (Create → Configs/LevelContainer), заполнить уровнями.
- На GameObject меню заменить компонент `MainMenuUI` → `MainMenuView`, привязать кнопку.
- Добавить `MainEntryPoint`, проставить ссылки на `MainMenuView` и `LevelManager`.
- В `LevelManager` проставить `LevelContainer`, `_levelsParent`, `_playerPrefab`, `_worldCreator`.

---

## Опционально: переезд на VContainer (позже)
Структура (View / Presenter / EntryPoint / Service) уже DI-дружественная, переход механический:
- `MainEntryPoint` → наследник `LifetimeScope` (один на сцену = "EntryPoint в каждой сцене").
- `Configure(IContainerBuilder)`: `RegisterComponent(_view)`, `RegisterComponent(_levelManager)`, `RegisterEntryPoint<MainMenuPresenter>()`.
- `MainMenuPresenter` реализует `IStartable` (подписка в `Start()`) + `IDisposable` (отписка) — VContainer сам вызовет.
- Плюсы: убирает ручную проводку и `Awake`-связывание, масштабируется с ростом систем.
- Минусы: новая зависимость + настройка IL2CPP/codegen; для текущих 3-4 классов ручная проводка проще.
- Рекомендация: сделать ручной рефактор сейчас, VContainer ввести отдельным шагом, когда зависимостей станет больше.
