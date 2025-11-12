# Структурыне паттерны

Эти паттерны отвечают за построение удобных в
поддержке иерархий классов.

### Adapter (Адаптер)

Адаптер — это промежуточный тип, использующий объект одного типа, для реализации интерфейса
другого типа.
Другими словами это структурный паттерн проектирования, который позволяет объектам с несовместимыми интерфейсами работать вместе.

Вся суть этого паттерна заключается в следующей картинке:
![](src/adapter/car-to-reil-adapter.png)

То есть у нас есть два каких-то объекта, которые несовместимы между собой (разные интерфейсы) и мы хотим написать какой-то промежуточный класс, который свяжет эти объекты между собой и будет как бы "перенапраплять" интерфейсы.

Банальный пример: вы пишите приложение для игры на бирже и все результаты записываете в форма .tsv или в формате markdown. 
То есть пока что это просто приложение, которые записывает свои логи в формате .md. Но затем вам хочется эти логи проанализировать и вы находите хорошую библиотеку для анализа логов. Однако загвоздка в том, что эта библиотека работает только с форматом json. Соответсвенно вы не можете напрямую передавать md в json. Тут на помощь и приходит адаптер: вы создаёте промежуточный объект, который конвертирует md в json и передаете так:
Логи (md) -> Промежуточный объект (конвертирует md в json) -> Библиотека (json)

Или же представьте это как переводчика на конференции: конференция ожидает, что все будут общаться на английском (это наш target), но один участник говорит только на русском (это наш adaptee). Переводчик встаёт посередине, слушает английский, переводит на русский, слушает русский, переводит обратно на английский. Переводчик — это наш Adapter.

Терминология:

*Target (Цель)* - целевой интерфейс, через который мы хотим взаимодействовать с объектом, изначально его не реализующий

*Adaptee (Адаптируемый)* - адаптируемый тип

*Adapter (Адаптер)* - тип-обёртка, реализует целевой интерфейс, содержит объект адаптируемого типа и перенаправляющая в него вызовы поведений целевого интерфейса 

![](src/adapter/hierarchy.png)

Давайте рассмотрим шуточный пример:
В этом шуточном примере Адаптер преобразует один интерфейс в другой, позволяя совместить квадратные колышки и круглые отверстия.
Адаптер вычисляет наименьший радиус окружности, в которую можно вписать квадратный колышек, и представляет его как круглый колышек с этим радиусом.

![](src/adapter/joke.png)
```csharp
using System;

//
// Классы с совместимыми интерфейсами: RoundHole и RoundPeg.
//
class RoundHole
{
    private double radius;

    public RoundHole(double radius)
    {
        this.radius = radius;
    }

    public double GetRadius()
    {
        // Вернуть радиус отверстия.
        return radius;
    }

    public bool Fits(RoundPeg peg)
    {
        return this.GetRadius() >= peg.GetRadius();
    }
}

class RoundPeg
{
    private double radius;

    public RoundPeg(double radius)
    {
        this.radius = radius;
    }

    public double GetRadius()
    {
        // Вернуть радиус круглого колышка.
        return radius;
    }
}

//
// Устаревший, несовместимый класс: SquarePeg.
//
class SquarePeg
{
    private double width;

    public SquarePeg(double width)
    {
        this.width = width;
    }

    public double GetWidth()
    {
        // Вернуть ширину квадратного колышка.
        return width;
    }
}

//
// Адаптер позволяет использовать квадратные колышки и круглые отверстия вместе.
//
class SquarePegAdapter : RoundPeg
{
    private SquarePeg peg;

    public SquarePegAdapter(SquarePeg peg) : base(0)
    {
        this.peg = peg;
    }

    public override double GetRadius()
    {
        // Вычислить половину диагонали квадратного колышка по теореме Пифагора.
        return peg.GetWidth() * Math.Sqrt(2) / 2;
    }
}

//
// Где-то в клиентском коде.
//
class Program
{
    static void Main()
    {
        var hole = new RoundHole(5);
        var rpeg = new RoundPeg(5);
        Console.WriteLine(hole.Fits(rpeg)); // TRUE

        var smallSqPeg = new SquarePeg(5);
        var largeSqPeg = new SquarePeg(10);

        // hole.Fits(smallSqPeg); // Ошибка компиляции, несовместимые типы

        var smallSqPegAdapter = new SquarePegAdapter(smallSqPeg);
        var largeSqPegAdapter = new SquarePegAdapter(largeSqPeg);

        Console.WriteLine(hole.Fits(smallSqPegAdapter)); // TRUE
        Console.WriteLine(hole.Fits(largeSqPegAdapter)); // FALSE
    }
}
```

Самое главное, что адаптер решает задачу совместимости интерфейсов, **не меняя** клиентский код и **не переписывая** сторонние библиотеки. То есть мы никак не должны менять уже написанное. Адаптер пишется поверх сущетсвующего кода. 

Рассмотрим ещё один пример использования:

Представьте, что вы разрабатываете систему логирования для вашего приложения. Изначально вы использовали PostgreSQL для хранения логов, и написали под него специальный класс.

Всё работает хорошо. Но потом бизнес требует: "Нам нужно масштабировать логирование. Добавим ElasticSearch для полнотекстового поиска!" И вот вам дают уже готовую библиотеку с классом ElasticSearch:

Nota: Elasticsearch — это распределенная поисковая и аналитическая система, основанная на движке Apache Lucene. Она используется для быстрого поиска и анализа больших объемов данных в реальном времени

```csharp
public class PostgresLogStorage
{
    public void Save(string message, DateTime timeStamp, int severity)
    {
        // ...
    }
}

public class ElasticSearchLogStorage
{
    public void Save(ElasticLogMessage message)
    {
        // ...
    }
}
```

Это были adaptee. То есть уже существующие классы, которые мы либо не можем, либо не хотим изменять. Они могут быть из внешних библиотек, легаси-кода или просто они имеют свою логику, которая не должна быть затронута.

Более того, представьте, что у нас в приложении уже есть сервис для логирования с одним единственным контрактом. Target Interface — то, что ожидает клиент. Это контракт, который клиент (в нашем случае LoggingService) ожидает видеть. Это единый стандарт, к которому мы хотим привести все реализации хранилищ логов.
```csharp
public interface ILogStorage
{
    void Save(LogMessage message);
}
```
Мы хотим как-то соединить эти две части между собой. Для этого мы можем написать класс-обертку. 
```csharp
public class PostgresLogStorageAdapter : ILogStorage
{
    private readonly PostgresLogStorage _storage;
    
    public PostgresLogStorageAdapter(PostgresLogStorage storage)
    {
        _storage = storage;
    }
    
    public void Save(LogMessage message)
    {
        // Трансформируем данные из целевого формата в формат adaptee
        _storage.Save(
            message.Message,
            message.DateTime,
            message.Severity.AsInteger()  // преобразуем Severity enum в int
        );
    }
}

public class ElasticLogStorageAdapter : ILogStorage
{
    private readonly ElasticSearchLogStorage _storage;
    
    public ElasticLogStorageAdapter(ElasticSearchLogStorage storage)
    {
        _storage = storage;
    }
    
    public void Save(LogMessage message)
    {
        // Трансформируем данные из целевого формата в формат adaptee
        _storage.Save(message.AsElasticLogMessage());
    }
}
```
Для PostgresLogStorageAdapter:

- Поле _storage — здесь мы храним объект, который нужно адаптировать. Это наша ссылка на "оригинального артиста", за спиной которого мы стоим.
- Конструктор — принимает объект PostgresLogStorage и сохраняет его. Это композиция (не наследование!). Мы не расширяем функциональность Postgres, мы оборачиваем его.
- Метод Save — это главное в адаптере:
    - Получает LogMessage в формате, который ожидает клиент
    - Трансформирует его в формат, который ожидает PostgresLogStorage
    - Вызывает оригинальный метод с преобразованными данными
- Для ElasticLogStorageAdapter логика та же, но трансформация другая. Заметьте, что мы вызываем message.AsElasticLogMessage() — это предполагает, что у LogMessage есть extension method или метод, который преобразует наше сообщение в формат Elastic.

Теперь давайте посмотрим на самую мощную часть этого паттерна — адаптивный рефакторинг.


Представьте, что вы пишите какую-то программу (аля бота в тг) и вам нужно перейти с синхронного API на асинхронный. Но у вас есть много кода, который уже использует ILogStorage. 
```csharp
public interface IAsyncLogStorage
{
    Task SaveAsync(LogMessage message);
}

public class AsyncLogStorageAdapter : IAsyncLogStorage
{
    private readonly ILogStorage _storage;
    
    public AsyncLogStorageAdapter(ILogStorage storage)
    {
        _storage = storage;
    }
    
    public Task SaveAsync(LogMessage message)
    {
        _storage.Save(message);
        return Task.CompletedTask;
    }
}
```

Вы можете:
- Вариант плохой: переписать весь код — опасно, долго, можно сломать.
- Вариант хороший: создать адаптер, который оборачивает синхронную реализацию и предоставляет асинхронный интерфейс.

Преимущество такого подхода:
```csharp
// Старый код продолжает работать как раньше
var logService = new LoggingService(new PostgresLogStorageAdapter(postgresStorage));

// А новый код может использовать асинхронный интерфейс
var asyncLogService = new AsyncLoggingService(
    new AsyncLogStorageAdapter(
        new PostgresLogStorageAdapter(postgresStorage)
    )
);
```
Видите? Мы можем комбинировать адаптеры! Это как слои (layers). Снизу у нас оригинальный PostgresLogStorage, затем мы оборачиваем его в PostgresLogStorageAdapter (чтобы он соответствовал ILogStorage), затем в AsyncLogStorageAdapter (чтобы он соответствовал IAsyncLogStorage).

А также с помощью адаптеров можно проводить адаптивный рефакторинг.

адаптивный рефакторинг
- позволяет проводить рефакторинг в два шага
    1. изменения использований
    2. изменение реализации
- позволяет локализовать изменения

Допустим такую ситуацию: все долгие годы мы юзали в проекте старый логгер, теперь пишем все асинхронно и нам нужен асинхронный логгер. Тогда сделаем все в 2 шага:

Меняем абстракцию - создаем крутой адаптер, интерфейс которого поддерживает и старую, и новую реализации, и используем этот адаптер в нашем коде

Меняем реализацию - засовываем в этот адаптер асинхронный логгер

Вся система использует старый синхронный API:
```csharp
class OrderService
{
    private ILogStorage _storage;
    
    public void ProcessOrder(Order order)
    {
        _storage.Save(new LogMessage { Message = "Order processed" });
    }
}

class PaymentService
{
    private ILogStorage _storage;
    
    public void ProcessPayment(Payment payment)
    {
        _storage.Save(new LogMessage { Message = "Payment processed" });
    }
}

// ... ещё 50 классов с таким же кодом
```
Теперь вам нужно перейти на асинхронный API, но это большой рефакторинг. Адаптер позволяет сделать это постепенно:
Шаг 1: Создаём адаптер (локализованное изменение)
```csharp
public class AsyncLogStorageAdapter : IAsyncLogStorage
{
    private readonly ILogStorage _storage;
    
    public AsyncLogStorageAdapter(ILogStorage storage)
    {
        _storage = storage;
    }
    
    public Task SaveAsync(LogMessage message)
    {
        _storage.Save(message);
        return Task.CompletedTask;
    }
}
```
Шаг 2: Постепенно меняем использования (один класс за раз)
```csharp
// Новая версия OrderService — асинхронная
class OrderService
{
    private IAsyncLogStorage _storage;
    
    public async Task ProcessOrderAsync(Order order)
    {
        await _storage.SaveAsync(new LogMessage { Message = "Order processed" });
    }
}

// PaymentService ещё старый, но может использовать адаптер!
class PaymentService
{
    private ILogStorage _storage;
    
    public void ProcessPayment(Payment payment)
    {
        _storage.Save(new LogMessage { Message = "Payment processed" });
    }
}
```
Шаг 3: При создании объектов используем адаптер для комбинирования
```csharp
var syncStorage = new PostgresLogStorageAdapter(postgresDb);
var asyncStorage = new AsyncLogStorageAdapter(syncStorage);

var orderService = new OrderService(asyncStorage);      // получает асинхронный
var paymentService = new PaymentService(syncStorage);   // получает синхронный
```

- Мы не меняем реализацию PostgresLogStorageAdapter
- Мы не переписываем все сразу
- Мы можем мигрировать постепенно, один класс за раз

Все объекты работают правильно, потому что они получают нужный интерфейс через адаптер

Это называется локализацией изменений — изменения сосредоточены в одном месте (в адаптере), а не разбросаны по всему коду.

Теперь давайте покажем, как всё это использовать в реальной программе:
```csharp
class Program
{
    static async Task Main(string[] args)
    {
        // Шаг 1: Создаём оригинальные объекты (adaptees)
        var postgresDb = new PostgresLogStorage();
        var elasticDb = new ElasticSearchLogStorage();
        
        // Шаг 2: Оборачиваем их в адаптеры (target interface)
        ILogStorage postgresAdapter = new PostgresLogStorageAdapter(postgresDb);
        ILogStorage elasticAdapter = new ElasticLogStorageAdapter(elasticDb);
        
        // Шаг 3: Используем оба хранилища через единый интерфейс
        var loggingService1 = new LoggingService(postgresAdapter);
        var loggingService2 = new LoggingService(elasticAdapter);
        
        // Логирование через Postgres
        loggingService1.LogError("Database connection failed", 2);
        
        // Логирование через Elastic
        loggingService2.LogError("API timeout", 3);
        
        Console.WriteLine("Synchronous logging completed.\n");
        
        // Шаг 4: Если нужен асинхронный API, используем адаптер адаптера!
        IAsyncLogStorage asyncPostgresAdapter = 
            new AsyncLogStorageAdapter(postgresAdapter);
        
        var asyncLoggingService = new AsyncLoggingService(asyncPostgresAdapter);
        
        // Асинхронное логирование
        await asyncLoggingService.LogErrorAsync("Async error occurred", 1);
        
        Console.WriteLine("Asynchronous logging completed.");
    }
}

// Вспомогательные классы для примера
public class LogMessage
{
    public string Message { get; set; }
    public DateTime DateTime { get; set; }
    public Severity Severity { get; set; }
}

public enum Severity
{
    Info = 0,
    Warning = 1,
    Error = 2,
    Critical = 3
}

public class LoggingService
{
    private readonly ILogStorage _storage;
    
    public LoggingService(ILogStorage storage)
    {
        _storage = storage;
    }
    
    public void LogError(string message, int severity)
    {
        var logMessage = new LogMessage
        {
            Message = message,
            DateTime = DateTime.Now,
            Severity = (Severity)severity
        };
        
        _storage.Save(logMessage);
    }
}

public class AsyncLoggingService
{
    private readonly IAsyncLogStorage _storage;
    
    public AsyncLoggingService(IAsyncLogStorage storage)
    {
        _storage = storage;
    }
    
    public async Task LogErrorAsync(string message, int severity)
    {
        var logMessage = new LogMessage
        {
            Message = message,
            DateTime = DateTime.Now,
            Severity = (Severity)severity
        };
        
        await _storage.SaveAsync(logMessage);
    }
}

// Extension method для трансформации в ElasticSearch формат
public static class LogMessageExtensions
{
    public static ElasticLogMessage AsElasticLogMessage(this LogMessage message)
    {
        return new ElasticLogMessage
        {
            Content = $"{message.DateTime:yyyy-MM-dd HH:mm:ss} [{message.Severity}] {message.Message}",
            Timestamp = message.DateTime,
            Level = message.Severity.ToString()
        };
    }
    
    public static int AsInteger(this Severity severity)
    {
        return (int)severity;
    }
}

public class ElasticLogMessage
{
    public string Content { get; set; }
    public DateTime Timestamp { get; set; }
    public string Level { get; set; }
}
```

1. Создание оригинальных объектов — мы инстанцируем PostgresLogStorage и ElasticSearchLogStorage.
2. Оборачивание в адаптеры — каждый из них оборачивается в соответствующий адаптер, который реализует ILogStorage.
3. Использование через интерфейс — клиентский код (LoggingService) вообще не знает, что он работает с адаптерами. Он просто получает ILogStorage и работает с ним.
4. Двойное оборачивание — мы даже можем создать AsyncLogStorageAdapter, который оборачивает postgresAdapter. Это показывает, как адаптеры можно комбинировать как матрёшки.

Применимость:

1. **Когда вы хотите использовать сторонний класс, но его интерфейс не соответствует остальному коду приложения.**

Адаптер позволяет создать объект-прокладку, который будет превращать вызовы приложения в формат, понятный стороннему классу.

2. **Когда вам нужно использовать несколько существующих подклассов, но в них не хватает какой-то общей функциональности, причём расширить суперкласс вы не можете.**

Вы могли бы создать ещё один уровень подклассов и добавить в них недостающую функциональность. Но при этом придётся дублировать один и тот же код в обеих ветках подклассов.

Более элегантным решением было бы поместить недостающую функциональность в адаптер и приспособить его для работы с суперклассом. Такой адаптер сможет работать со всеми подклассами иерархии. Это решение будет сильно напоминать паттерн Декоратор.
