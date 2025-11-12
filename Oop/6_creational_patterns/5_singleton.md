
### Singletone (одиночка)

Синглтон - объект, для которого мы гарантируем, что одновременно будет существовать не больше одного экземпляра.

Давайте представим реальную ситуацию. Вы разрабатываете приложение, которое должно работать с единственным подключением к базе данных, конфигурацией приложения или логером. Такие объекты по своей природе должны существовать в одном экземпляре — создание нескольких копий приведёт к несогласованности данных, дублированию подключений и ошибкам.

Проблема в том, что если просто создавать такой объект через обычный конструктор, ничто не мешает разработчику создать его несколько раз:

Давайте представим реальную ситуацию. Вы разрабатываете приложение, которое должно работать с единственным подключением к базе данных, конфигурацией приложения или логером. Такие объекты по своей природе должны существовать в одном экземпляре — создание нескольких копий приведёт к несогласованности данных, дублированию подключений и ошибкам.

Проблема в том, что если просто создавать такой объект через обычный конструктор, ничто не мешает разработчику создать его несколько раз:
```csharp
var config1 = new DatabaseConfig();
var config2 = new DatabaseConfig();
// Теперь у нас есть две разные конфигурации!
// Какую использовать? Неопределённость и баги гарантированы.
```

![](src/singleton/circle.png)
###### Клиенты могут не подозревать, что работают с одним и тем же объектом.

Вот здесь на помощь приходит Singleton — паттерн проектирования, который гарантирует, что класс будет иметь ровно один экземпляр и предоставит глобальную точку доступа к этому экземпляру.

Идея проста:

- Запретить прямое создание объекта — сделать конструктор приватным, чтобы никто снаружи не мог вызвать new Singleton().
- Контролировать создание через статический метод — предоставить специальное свойство или метод, которое сначала проверит, создан ли объект, и если нет, создаст его ровно один раз.
- Гарантировать потокобезопасность — если приложение многопоточное (а в 2025 году это стандарт), нужно убедиться, что даже если несколько потоков одновременно попросят Singleton, они получат один и тот же объект. 
Это решает нашу проблему: разработчик не может случайно создать несколько экземпляров, потому что конструктор для него недоступен.

#### Первая реализация: Singleton с Double-Checked Locking:

```csharp
public class Singleton 
{ 
    private static readonly object _lock = new(); 
    private static Singleton? _instance; 
    
    private Singleton() { }  // Конструктор приватный!
    
    public static Singleton Instance 
    { 
        get 
        { 
            // Первая проверка (без блокировки)
            if (_instance is not null) 
                return _instance; 
            
            // Блокируем доступ для других потоков
            lock (_lock) 
            { 
                // Вторая проверка (внутри блокировки)
                if (_instance is not null) 
                    return _instance; 
                
                // Создаём объект ровно один раз
                return _instance = new Singleton(); 
            } 
        } 
    } 
}
```

- private Singleton() — приватный конструктор блокирует создание экземпляров извне. Только код внутри самого класса может вызвать new Singleton().

- private static readonly object _lock — это объект, используемый для синхронизации потоков. Представьте его как "замок двери": когда один поток входит в lock(_lock), остальные ждут снаружи.

- private static Singleton? _instance — статическая переменная, которая хранит единственный экземпляр. Слово static означает, что эта переменная существует один раз для всего класса, а не отдельно для каждого объекта.

Почему две проверки (if)?
Это называется Double-Checked Locking. Представьте очередь в магазин:

Первая проверка (без блокировки): "Стой, стоит ли уже охранник у двери? Если да, значит магазин открыт, я могу войти." Это быстро и не требует ждать.

Если охранника нет, ждём возможности войти через lock.

Вторая проверка (внутри блокировки): "Ещё раз проверим, может быть, пока я ждал, другой поток уже создал экземпляр?" Это гарантирует, что объект будет создан только один раз, даже если несколько потоков одновременно ждали входа.

Практическое применение:
```csharp
// Где-то в main:
var singleton1 = Singleton.Instance;
var singleton2 = Singleton.Instance;

Console.WriteLine(ReferenceEquals(singleton1, singleton2)); // true — один и тот же объект!
```

#### Вторая реализация: Singleton с Lazy<T>
В современной C# (начиная с .NET Framework 4.0) есть встроенный класс Lazy<T>, который автоматически решает все проблемы потокобезопасности:
```csharp
public class Singleton 
{ 
    private static readonly Lazy<Singleton> _instance;
    
    static Singleton() 
    { 
        _instance = new Lazy<Singleton>(
            () => new Singleton(), 
            LazyThreadSafetyMode.ExecutionAndPublication
        ); 
    } 
    
    private Singleton() { } 
    
    public static Singleton Instance => _instance.Value; 
}
```

- static Singleton() — это статический конструктор класса. Он вызывается ровно один раз при первом обращении к классу, ещё до того, как кто-то попросит Instance. Здесь мы создаём объект Lazy<Singleton>.

- new Lazy<Singleton>(...) — мы передаём лямбда-выражение () => new Singleton(), которое говорит: "Когда потребуется экземпляр, выполни это выражение". Но выполнится оно только один раз!

- LazyThreadSafetyMode.ExecutionAndPublication — это флаг потокобезопасности. Об этом подробнее дальше.

- public static Singleton Instance => _instance.Value; — просто свойство, которое возвращает значение из Lazy<T>. При первом обращении _instance.Value запустит лямбду и создаст объект, при последующих — вернёт уже созданный.

Почему это лучше?
1. Меньше кода — не нужно писать свою синхронизацию.
2. Правильнее — Lazy<T> разработана Microsoft специально для этого.
3. Понятнее — намерение очевидно: "Это ленивый синглтон, инициализируется по требованию".

Рассмотрим типы потокобезопасности:
 
#### LazyThreadSafetyMode.None

***None*** - не гарантируется потокобезопасность, при инициализации несколькими потоками, объект будет создан несколько раз, сохранённое значение не определено

```csharp
new Lazy<Singleton>(() => new Singleton(), LazyThreadSafetyMode.None)
```

Что происходит: Если два потока одновременно обращаются к .Value, лямбда может выполниться несколько раз. Каждый поток создаст свой объект.

Результат: Нарушается гарантия Singleton! Будет несколько экземпляров.

Когда использовать: Практически никогда, если вам нужен Singleton. Только в однопоточных приложениях.

#### LazyThreadSafetyMode.PublicationOnly
***PublicationOnly*** - при инициализации несколькими потоками, объект будет создан несколько раз, сохранённое значение – созданное последним потоком начавшим инициализацию

```csharp
new Lazy<Singleton>(() => new Singleton(), LazyThreadSafetyMode.PublicationOnly)
```

Что происходит: Лямбда может выполниться несколько раз (несколько потоков создадут объекты), но .Value вернёт объект, созданный последним потоком, начавшим инициализацию.

Результат: Остальные созданные объекты будут отброшены. В памяти останется один экземпляр, но процесс создания был избыточным.

Когда использовать: Когда создание объекта очень дешёво (например, простая структура данных) и не имеет побочных эффектов. Плюс: нет блокировок, быстрее. Минус: впустую создаём объекты.

#### LazyThreadSafetyMode.ExecutionAndPublication

***ExecutionAndPublication*** - полная потокобезопасность, при инициализации несколькими потоками, объект будет создан лишь один раз

```csharp
new Lazy<Singleton>(() => new Singleton(), LazyThreadSafetyMode.ExecutionAndPublication)
```

Что происходит: Лямбда выполняется ровно один раз, даже если несколько потоков одновременно обращаются к .Value. Используется внутренняя синхронизация (примерно как Double-Checked Locking).

Результат: Идеальное поведение Singleton.

Когда использовать: Всегда, для настоящего Singleton! Это значение по умолчанию в новых версиях .NET.

Пример:
```csharp
// Потоки 1, 2, 3 одновременно обращаются:
var task1 = Task.Run(() => Singleton.Instance);
var task2 = Task.Run(() => Singleton.Instance);
var task3 = Task.Run(() => Singleton.Instance);

Task.WaitAll(task1, task2, task3);

var s1 = task1.Result;
var s2 = task2.Result;
var s3 = task3.Result;

// Все три переменные указывают на ОДИН и ТОТ ЖЕ объект
Console.WriteLine(ReferenceEquals(s1, s2) && ReferenceEquals(s2, s3)); // true
```

#### Недостатки Singleton
1. ***тестирование***- приватный конструктор не даёт возможности контролировать объект в тестах
2. ***внедрение зависимостей*** -приватный конструктор не даёт возможности передавать значения извне
3. ***время жизни объекта*** - т.к. объект инициализируется статически, его время жизни нельзя явно контролировать
4. ***статический стейт*** - объект можно получить из любого места приложения, без какого-либо контроля

Examples:

Проблема (Тестирование):

```csharp
public class DatabaseLogger 
{ 
    public static DatabaseLogger Instance { /* ... */ }
    private DatabaseLogger() { }
}

// Вы хотите протестировать класс, использующий логер:
public class UserService 
{ 
    public void CreateUser(string name) 
    { 
        // UserService закодирована на конкретный Singleton!
        DatabaseLogger.Instance.Log($"Creating user: {name}"); 
    } 
}

// Тест:
[Test]
public void CreateUser_ShouldLogEvent()
{
    var service = new UserService();
    service.CreateUser("John");
    
    // Как проверить, что логировалось? 
    // Нельзя "подменить" DatabaseLogger на фейковый логер!
    // Приватный конструктор не позволит нам создать mock объект.
}
```
Почему: Singleton имеет приватный конструктор, поэтому в тесте вы не можете создать контролируемый экземпляр или передать фейковый (mock) объект.

Решение: Использовать инъекцию зависимостей (Dependency Injection) вместо Singleton, или, если Singleton необходим, делать его конструктор protected и создавать тестовые подклассы.

Проблема (Внедрение зависимостей):
```csharp
public class AppConfig 
{ 
    private string _connectionString;
    
    private AppConfig() 
    { 
        _connectionString = "hardcoded_string"; // Жёстко кодируем значение
    }
    
    public static AppConfig Instance { /* ... */ }
}
```
Если конфигурация зависит от вашего Singleton, вы не можете передать её извне. Это значит, что:

- В разных окружениях (development, staging, production) приложение будет вести себя одинаково.
- Нельзя легко переконфигурировать приложение при запуске.
- Сложно работать с контейнерами зависимостей (IoC containers).

Решение: Передавать зависимости через конструктор (DI pattern):
```csharp
public class AppConfig 
{ 
    private readonly string _connectionString;
    
    // Зависимость передаётся снаружи
    public AppConfig(string connectionString) 
    { 
        _connectionString = connectionString;
    }
}
```

Проблема (Время жизни объекта):
```csharp
public class ResourceManager 
{ 
    private List<NativeResource> _resources = new();
    
    private ResourceManager() 
    { 
        // Инициализация...
    }
    
    public static ResourceManager Instance { /* ... */ }
    
    public void Cleanup() 
    { 
        // Освобождаем ресурсы
        foreach (var res in _resources) 
        { 
            res.Dispose(); 
        }
    }
}

// Когда вызвать Cleanup()? 
// Объект создан статически, существует всю жизнь приложения.
// Когда приложение завершается, Cleanup() может не вызваться вообще!
```

Singleton живёт столько же, сколько приложение. Вы не можете контролировать его время жизни: создание, очистку ресурсов, пересоздание.

Решение: Использовать pattern "Scoped" или "Transient" в DI контейнере:

```csharp
// Каждый раз создаётся новый экземпляр (Transient)
services.AddTransient<ResourceManager>();

// Каждый раз в рамках одного запроса один экземпляр (Scoped)
services.AddScoped<ResourceManager>();
```

Проблема (Статический стейт и отсутствие контроля):
```csharp
var logger = Singleton.Instance;
// Но кто создал этот объект? Когда? С какими параметрами?
// Неизвестно! Singleton скрывает логику инициализации.

// Ещё хуже:
DatabaseConnection.Instance.Connect();
CacheManager.Instance.Clear();
SessionManager.Instance.Reset();

// Теперь ваша логика разбросана везде в коде.
// Сложнее отследить, кто за что отвечает.
// Это называется "глобальным состоянием" и часто ведёт к ошибкам.
```
Когда Singleton используется везде в приложении, возникает глобальное состояние — все компоненты зависят друг от друга опосредованно, через общие объекты. Это делает код хаотичным и трудным для отладки.

Несмотря на недостатки, есть случаи, когда Singleton оправдан:
- Logger — логирование действительно должно быть в одном месте.
- Database Connection Pool — пул подключений должен быть единственным.
- Configuration Manager — конфигурация приложения одна.
- Cache — кэш должен быть общий для всех компонентов.

Главное правило: Singleton должен быть простым объектом, который предоставляет доступ к ресурсу, но не содержит сложную бизнес-логику.

Давайте соберём всё вместе и покажем, как это работает:
```csharp
// Singleton для логирования
public class Logger 
{ 
    private static readonly Lazy<Logger> _instance = 
        new(() => new Logger(), LazyThreadSafetyMode.ExecutionAndPublication);
    
    private Logger() 
    { 
        Console.WriteLine("Logger initialized"); 
    }
    
    public static Logger Instance => _instance.Value;
    
    public void Log(string message) 
    { 
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {message}"); 
    }
}

// Singleton для конфигурации
public class AppSettings 
{ 
    private static readonly Lazy<AppSettings> _instance = 
        new(() => new AppSettings(), LazyThreadSafetyMode.ExecutionAndPublication);
    
    private readonly Dictionary<string, string> _settings;
    
    private AppSettings() 
    { 
        _settings = new Dictionary<string, string>
        { 
            { "DatabaseUrl", "Server=localhost;Database=MyApp" },
            { "ApiKey", "secret123" }
        };
    }
    
    public static AppSettings Instance => _instance.Value;
    
    public string GetSetting(string key) => _settings[key];
}

// Использование в main:
static void Main()
{
    // Первый поток
    var task1 = Task.Run(() =>
    {
        Logger.Instance.Log("Task 1: Starting");
        var dbUrl = AppSettings.Instance.GetSetting("DatabaseUrl");
        Logger.Instance.Log($"Task 1: Got DB URL = {dbUrl}");
    });

    // Второй поток
    var task2 = Task.Run(() =>
    {
        Logger.Instance.Log("Task 2: Starting");
        var apiKey = AppSettings.Instance.GetSetting("ApiKey");
        Logger.Instance.Log($"Task 2: Got API Key = {apiKey}");
    });

    Task.WaitAll(task1, task2);

    // Проверяем, что это один и тот же объект
    Logger.Instance.Log("Main: Verification");
    var logger1 = Logger.Instance;
    var logger2 = Logger.Instance;
    Logger.Instance.Log($"Same logger? {ReferenceEquals(logger1, logger2)}"); // true

    var settings1 = AppSettings.Instance;
    var settings2 = AppSettings.Instance;
    Logger.Instance.Log($"Same settings? {ReferenceEquals(settings1, settings2)}"); // true
}

/* Вывод
Logger initialized
[HH:mm:ss] Task 1: Starting
[HH:mm:ss] Task 1: Got DB URL = Server=localhost;Database=MyApp
[HH:mm:ss] Task 2: Starting
[HH:mm:ss] Task 2: Got API Key = secret123
[HH:mm:ss] Main: Verification
[HH:mm:ss] Same logger? True
[HH:mm:ss] Same settings? True
*/
```

Применимость:
1.  **Когда в программе должен быть единственный экземпляр какого-то класса, доступный всем клиентам (например, общий доступ к базе данных из разных частей программы).**
Одиночка скрывает от клиентов все способы создания нового объекта, кроме специального метода. Этот метод либо создаёт объект, либо отдаёт существующий объект, если он уже был созда
2. Когда вам хочется иметь больше контроля над глобальными пер**еменными.**
В отличие от глобальных переменных, Одиночка гарантирует, что никакой другой код не заменит созданный экземпляр класса, поэтому вы всегда уверены в наличии лишь одного объекта-одиночки.
Тем не менее, в любой момент вы можете расширить это ограничение и позволить любое количество объектов-одиночек, поменяв код в одном месте (метод getInstance).