
# Строитель (Builder Pattern)

## Определение паттерна

**Builder** (Строитель) — это порождающий паттерн проектирования, который инкапсулирует логику пошагового создания сложных объектов. Паттерн отделяет конструирование объекта от его представления, позволяя использовать один и тот же процесс создания для получения различных конфигураций объектов.

### Основные термины

Прежде чем углубиться в паттерн, важно различать два понятия:

**Parameter** (Параметр) — это переменная в сигнатуре метода, которая определяет тип и имя ожидаемого значения:

```csharp
// int x и string s — параметры метода M
public void M(int x, string s)
{
    // Тело метода
}
```

**Argument** (Аргумент) — это конкретное значение, передаваемое в метод при его вызове:

```csharp
// 10 и "hi" — аргументы, передаваемые в метод M
M(10, "hi");
```

## Проблема: сложность создания объектов

Представьте класс, описывающий дом. У дома может быть множество различных характеристик: адрес, количество этажей, наличие гаража, сада, бассейна и других элементов. При создании такого объекта мы сталкиваемся с несколькими проблемами.

### Проблема 1: Конструктор с большим числом параметров

Если передавать все параметры через конструктор, получается громоздкая конструкция, где многие аргументы приходится заполнять значениями по умолчанию или `null`:
```csharp
// Конструктор с множеством параметров - неудобен в использовании
var house = new House(
    address: "ул. Пушкина, д. 10",
    floors: 2,
    hasGarage: false,
    hasGarden: true,
    hasPool: true,
    hasFancyStatues: false
);
```

### Проблема 2: Телескопические конструкторы

Чтобы упростить создание объекта, разработчики создают множество перегруженных конструкторов — так называемый **Telescoping Constructor** (Телескопический конструктор). Это приводит к матрёшке из конструкторов:

```csharp
public class House
{
    public string Address { get; }
    public int Floors { get; }
    public bool HasGarage { get; }
    public bool HasGarden { get; }
    public bool HasPool { get; }
    public bool HasFancyStatues { get; }

    // Базовый конструктор с полным набором параметров
    public House(string address, int floors, bool hasGarage, bool hasGarden, bool hasPool, bool hasFancyStatues)
    {
        Address = address;
        Floors = floors;
        HasGarage = hasGarage;
        HasGarden = hasGarden;
        HasPool = hasPool;
        HasFancyStatues = hasFancyStatues;
    }

    // Минимальный дом - только адрес
    public House(string address)
        : this(address, 1, false, false, false, false) { }

    // Дом с указанием этажности
    public House(string address, int floors)
        : this(address, floors, false, false, false, false) { }

    // Фабричные методы для различных конфигураций
    public static House WithGarage(string address, int floors = 1)
        => new House(address, floors, hasGarage: true, hasGarden: false, hasPool: false, hasFancyStatues: false);

    public static House WithGarden(string address, int floors = 1)
        => new House(address, floors, false, hasGarden: true, false, false);

    public static House WithPool(string address, int floors = 1)
        => new House(address, floors, false, false, hasPool: true, false);

    // ...и ещё множество вариантов для каждой комбинации
} 
```

Этот подход создаёт экспоненциальный рост числа конструкторов при добавлении новых параметров.

### Проблема 3: Иерархия подклассов

Ещё один неудачный подход — создание отдельного подкласса для каждой конфигурации объекта. Это приводит к взрыву количества классов и жёсткой привязке к конкретным комбинациям параметров.

```mermaid
graph TD
    A[House] --> B[HouseWithGarage]
    A --> C[HouseWithGarden]
    A --> D[HouseWithPool]
    B --> E[HouseWithGarageAndGarden]
    B --> F[HouseWithGarageAndPool]
    C --> G[HouseWithGardenAndPool]
    E --> H[HouseWithGarageGardenAndPool]
```

Эта структура становится неуправляемой при увеличении числа опций.

## Решение: паттерн Builder

Паттерн Builder предлагает вынести логику создания объекта в отдельный класс-строитель. Вместо вызова конструктора с множеством параметров, клиентский код пошагово конфигурирует объект через понятные методы.

```mermaid
graph LR
    A[Клиент] --> B[Builder]
    B --> C[WithAddress]
    C --> D[WithFloors]
    D --> E[WithPool]
    E --> F[Build]
    F --> G[House]
```

### Основной принцип

Builder разбивает процесс создания на отдельные шаги. Каждый метод строителя устанавливает один аспект будущего объекта и возвращает сам строитель, позволяя выстроить цепочку вызовов. Это называется **Fluent Interface** (Текучий интерфейс).

Сравните код до и после применения паттерна:
```csharp
// ДО: Громоздкий конструктор с множеством параметров
var house = new House(
    address: "ул. Пушкина, д. 10",
    floors: 2,
    hasGarage: false,
    hasGarden: true,
    hasPool: true,
    hasFancyStatues: true
);

// ДО: Фабричные методы для каждой комбинации
var house = House.WithPoolAndGardenAndFancyStatues(
    address: "ул. Пушкина, д. 10",
    floors: 2
);
// Проблема: при добавлении новых опций нужно создавать
// новые фабричные методы или менять конструктор

// ПОСЛЕ: Пошаговое создание через Builder
var house = new HouseBuilder()
    .WithAddress("ул. Пушкина, д. 10")
    .WithFloors(2)
    .WithPool()
    .WithGarden()
    .WithStatues()
    .Build();

// Преимущество: для добавления новых опций достаточно
// добавить новый метод в Builder, например:
var advancedHouse = new HouseBuilder()
    .WithAddress("ул. Ленина, д. 5")
    .WithFloors(3)
    .WithGarden()
    .WithFountain()    // Новая опция
    .WithPlayground()  // Новая опция
    .Build();
```

Логика создания вынесена в отдельный класс, который собирает объект по принципу конструктора, где каждый метод добавляет новую деталь.

## Практический пример: система заказов

Рассмотрим реальный сценарий применения паттерна Builder на примере системы интернет-магазина.

### Эволюция требований

Изначально класс `Order` (Заказ) был простым:
```csharp
// Простая модель заказа: только список товаров
public record Order(IEnumerable<OrderItem> Items);

public record OrderItem(string Name, decimal Price, int Amount);
```

Создание заказа было простым и понятным:

```csharp
var order = new Order(
    new[]
    {
        new OrderItem(Name: "Бананы", Price: 42, Amount: 20),
        new OrderItem(Name: "Телефон", Price: 30000, Amount: 1),
        new OrderItem(Name: "Маркеры", Price: 201, Amount: 3),
        new OrderItem(Name: "Стейк", Price: 1337, Amount: 1)
    }
);
```

### Рост сложности

Со временем требования усложняются. Помимо списка товаров, заказу требуются дополнительные данные: комментарии для магазина и доставки, контактная информация получателя, временные метки и другие поля:
```csharp
// Расширенная модель заказа со множеством полей
public record Order(
    string CommentForShop,
    string CommentForDelivery,
    IEnumerable<OrderItem> Items,
    DateTimeOffset CreatedAt,
    string? ReceiverPhoneNumber
    // ...и ещё множество других полей
);
```

Теперь создание заказа превращается в громоздкую операцию:

```csharp
var order = new Order(
    CommentForShop: "Срочная доставка",
    CommentForDelivery: "Оставить у двери",
    Items: new[]
    {
        new OrderItem(Name: "Бананы", Price: 42, Amount: 20),
        new OrderItem(Name: "Телефон", Price: 30000, Amount: 1),
        new OrderItem(Name: "Маркеры", Price: 201, Amount: 3),
        new OrderItem(Name: "Стейк", Price: 1337, Amount: 1)
    },
    CreatedAt: DateTimeOffset.UtcNow,
    ReceiverPhoneNumber: "+7-900-123-45-67"
);
```

### Дополнительные сложности

Помимо неудобства синтаксиса, возникают вопросы бизнес-логики:

- Как обеспечить порядок добавления товаров? Например, продукты питания должны добавляться перед бытовой техникой.
- Где проверять ограничения? Например, в заказе не может быть больше 20 товаров.
- Как учитывать различные правила для разных типов клиентов? Премиум-клиенты могут иметь расширенные лимиты.
- Как избежать дублирования логики валидации в разных местах кода?     

### Применение Builder

Паттерн Builder решает все перечисленные проблемы, выделяя логику создания и валидации в отдельный класс.

## Структура паттерна Builder

Builder — это вспомогательный класс, который выполняет три ключевые функции:

1. **Накапливает состояние** — хранит промежуточные значения всех параметров будущего объекта
2. **Предоставляет текучий интерфейс** — методы вида `WithXxx()` возвращают сам строитель, позволяя выстраивать цепочки вызовов (Fluent Interface)
3. **Выполняет финальное создание** — метод `Build()` валидирует собранные данные и создаёт неизменяемый объект

### Базовая реализация Builder

```csharp
public class OrderBuilder
{
    // 1. Внутреннее изменяемое состояние
    // Builder хранит промежуточные данные в изменяемых структурах
    private readonly List<OrderItem> _items = new List<OrderItem>();

    // 2. Метод конфигурации с Fluent Interface
    // Возвращает this, позволяя выстроить цепочку вызовов
    public OrderBuilder WithItem(OrderItem item)
    {
        _items.Add(item);
        return this; // Ключевой момент для построения цепочки
    }

    // 3. Финальное создание объекта
    // Метод Build() создаёт неизменяемый (immutable) объект
    public Order Build()
    {
        return new Order(_items.ToArray());
    }
}
```

### Использование Builder

Теперь код клиента становится выразительным и гибким:
```csharp
// Создаём строитель и последовательно конфигурируем заказ
var orderBuilder = new OrderBuilder()
    .WithItem(new OrderItem(Name: "Бананы", Price: 42, Amount: 20))
    .WithItem(new OrderItem(Name: "Телефон", Price: 30000, Amount: 1))
    .WithItem(new OrderItem(Name: "Маркеры", Price: 201, Amount: 3))
    .WithItem(new OrderItem(Name: "Стейк", Price: 1337, Amount: 1));

// Builder можно передавать в другие методы для дополнительной конфигурации
// Это позволяет распределить логику построения между разными компонентами
AddDefaultItems(orderBuilder);
AddRequestedItems(orderBuilder);
AddForecastedItems(orderBuilder);

// Финальное создание неизменяемого объекта заказа
Order order = orderBuilder.Build();
```

Преимущество такого подхода — логика создания разделена на отдельные методы, но весь процесс централизован в одном строителе. Это улучшает читаемость и поддерживаемость кода.

## Типы строителей

Существует несколько подходов к реализации паттерна Builder, каждый из которых решает специфические задачи.

### 1. Convenience Builder

**Convenience Builder** (Удобный строитель) — это тип строителя, основная задача которого упростить создание объектов с большим числом параметров.

Ключевые характеристики:

- Модель не зависит от строителя — они существуют независимо
- Предоставляет значения по умолчанию для опциональных параметров
- Используется для улучшения эргономики API

#### Проблема

Вернёмся к расширенной модели заказа с множеством полей:
```csharp
// Модель заказа с множеством полей
public record Order(
    string CommentForShop,
    string CommentForDelivery,
    IEnumerable<OrderItem> Items,
    DateTimeOffset CreatedAt,
    string? ReceiverPhoneNumber
);

// Неудобное создание: приходится передавать все параметры,
// даже если нужен только список товаров
var order = new Order(
    CommentForShop: string.Empty,           // значение по умолчанию
    CommentForDelivery: string.Empty,       // значение по умолчанию
    Items: new[] { new OrderItem(Price: 1337, Amount: 2) }, // единственное нужное поле
    CreatedAt: DateTimeOffset.UtcNow,       // значение по умолчанию
    ReceiverPhoneNumber: null               // значение по умолчанию
);
```

#### Решение через Convenience Builder

Convenience Builder инкапсулирует значения по умолчанию внутри себя, избавляя клиентский код от необходимости их указывать:

```csharp
public class OrderBuilder
{
    // Все поля инициализированы значениями по умолчанию
    private readonly List<OrderItem> _items = new List<OrderItem>();
    private string _commentForShop = string.Empty;
    private string _commentForDelivery = string.Empty;
    private DateTimeOffset _createdAt = DateTimeOffset.UtcNow;
    private string? _receiverPhoneNumber = null;

    // Метод для добавления товара в заказ
    public OrderBuilder WithItem(OrderItem item)
    {
        _items.Add(item);
        return this;
    }

    // Метод для установки комментария магазину
    public OrderBuilder WithCommentForShop(string value)
    {
        _commentForShop = value;
        return this;
    }

    // Метод для установки комментария доставке
    public OrderBuilder WithCommentForDelivery(string value)
    {
        _commentForDelivery = value;
        return this;
    }

    // Метод для установки номера телефона получателя
    public OrderBuilder WithReceiverPhoneNumber(string? value)
    {
        _receiverPhoneNumber = value;
        return this;
    }

    // Создание финального объекта заказа
    public Order Build()
    {
        return new Order(
            CommentForShop: _commentForShop,
            CommentForDelivery: _commentForDelivery,
            Items: _items,
            CreatedAt: _createdAt,
            ReceiverPhoneNumber: _receiverPhoneNumber
        );
    }
}
```

#### Использование Convenience Builder

Клиентский код становится лаконичным. Указываются только параметры, отличающиеся от значений по умолчанию:

```csharp
// Минимальный заказ - только товары
var order = new OrderBuilder()
    .WithItem(new OrderItem(Price: 1337, Amount: 2))
    .Build();

// Заказ с дополнительной конфигурацией
var customOrder = new OrderBuilder()
    .WithItem(new OrderItem(Price: 100, Amount: 5))
    .WithCommentForShop("Упаковать аккуратно!")
    .WithReceiverPhoneNumber("+7-900-000-00-00")
    .Build();
```

Convenience Builder избавляет от необходимости передавать множество параметров с одинаковыми значениями, делая код более читаемым и поддерживаемым. 

### 2. Stateful Constructor

**Stateful Constructor** (Строитель с состоянием) — это тип строителя, который инкапсулирует логику валидации и бизнес-правила в процессе создания объекта.

Ключевые характеристики:

- **Fail-fast подход** — ошибки обнаруживаются немедленно при добавлении некорректных данных, а не в момент вызова `Build()`
- **Инкапсуляция валидации** — вся логика проверки сосредоточена в строителе, а не в модели
- **Упрощение отладки** — точно известно, на каком шаге произошла ошибка

#### Проблема: где размещать валидацию

Представим новое требование: в заказе не может быть больше 20 товаров. Возникает вопрос — где выполнять эту проверку?

Варианты размещения валидации и их недостатки:

1. **В конструкторе модели** — нарушает принцип единственной ответственности (SRP). Модель отвечает за представление данных, а не за бизнес-правила их создания.
2. **В методе `Build()`** — ошибка обнаруживается поздно, после добавления всех товаров. Сложно определить, какой именно вызов метода `WithItem()` стал проблемным.

#### Решение через Stateful Constructor

Stateful Constructor выполняет валидацию в момент добавления данных:

```csharp
public class Order
{
    // Приватный конструктор - объект можно создать только через Builder
    private Order(IEnumerable<OrderItem> items)
    {
        Items = items;
    }

    public IEnumerable<OrderItem> Items { get; }

    // Builder как вложенный класс
    // Это подчёркивает тесную связь между моделью и её строителем
    public class OrderBuilder
    {
        private const int MaxOrderItemCount = 20;
        private readonly List<OrderItem> _items = new List<OrderItem>();

        // Валидация выполняется сразу при добавлении товара
        public OrderBuilder WithItem(OrderItem item)
        {
            // Fail-fast: проверка до добавления элемента
            if (_items.Count >= MaxOrderItemCount)
            {
                throw new ArgumentException(
                    $"Невозможно добавить более {MaxOrderItemCount} товаров в заказ");
            }
            
            _items.Add(item);
            return this;
        }

        // В методе Build() валидация уже не требуется
        // Все проверки выполнены на этапе добавления данных
        public Order Build()
        {
            return new Order(_items.ToArray());
        }
    }
}
```

#### Использование Stateful Constructor

```csharp
var orderBuilder = new Order.OrderBuilder();

// Добавляем 20 товаров - всё работает корректно
for (int i = 0; i < 20; i++)
{
    orderBuilder.WithItem(new OrderItem(Price: i * 10, Amount: 1));
}

// Попытка добавить 21-й товар немедленно вызовет исключение
// Мы точно знаем, где произошла ошибка
orderBuilder.WithItem(new OrderItem(Price: 1000, Amount: 1)); // ArgumentException!
```

#### Преимущества Stateful Constructor

1. **Fail-fast** — ошибка обнаруживается немедленно в момент нарушения правила, а не при вызове `Build()`
2. **Локализация валидации** — вся логика проверок находится в строителе, модель остаётся чистой
3. **Точность диагностики** — известно, какой именно вызов метода вызвал проблему
4. **Гарантия инвариантов** — приватный конструктор модели гарантирует, что объект может быть создан только через валидирующий строитель

#### Зачем приватный конструктор

Приватный конструктор модели `Order` обеспечивает важный архитектурный инвариант: все объекты `Order` в системе гарантированно проходят валидацию через строитель. Это предотвращает создание некорректных объектов и упрощает рассуждения о корректности системы.

### Полиморфизм строителей через интерфейсы

В реальных системах часто существуют различные типы объектов с разной логикой создания. Например, заказы для обычных пользователей имеют лимит в 20 товаров, а премиум-заказы не ограничены.

#### Антипаттерн: жёсткая привязка к конкретным типам

Неправильный подход — создать два независимых строителя и использовать их напрямую:

```csharp
// Плохо: клиентский код зависит от конкретных реализаций
var regularBuilder = new LimitedOrderBuilder();
var premiumBuilder = new UnlimitedOrderBuilder();
```

Это создаёт жёсткую связанность (tight coupling) между клиентским кодом и конкретными реализациями строителей.

#### Правильный подход: программирование через интерфейс
Определим общий интерфейс для всех типов строителей заказов:

```csharp
// Общий интерфейс для всех строителей заказов
public interface IOrderBuilder
{
    IOrderBuilder WithItem(OrderItem item);
    Order Build();
}

// Строитель с ограничением на количество товаров
public class LimitedOrderBuilder : IOrderBuilder
{
    private const int MaxOrderItemCount = 20;
    private readonly List<OrderItem> _items = new List<OrderItem>();

    public IOrderBuilder WithItem(OrderItem item)
    {
        if (_items.Count >= MaxOrderItemCount)
        {
            throw new ArgumentException($"Превышен лимит товаров: {MaxOrderItemCount}");
        }
        
        _items.Add(item);
        return this;
    }

    public Order Build()
    {
        return new Order(_items.ToArray());
    }
}

// Строитель без ограничений для премиум-пользователей
public class UnlimitedOrderBuilder : IOrderBuilder
{
    private readonly List<OrderItem> _items = new List<OrderItem>();

    public IOrderBuilder WithItem(OrderItem item)
    {
        _items.Add(item);
        return this;
    }

    public Order Build()
    {
        return new Order(_items.ToArray());
    }
}
```

#### Использование полиморфных строителей

Клиентский код работает с интерфейсом, не зная о конкретной реализации:

```csharp
public class OrderService
{
    // Метод зависит от интерфейса, а не от конкретной реализации
    public Order CreateOrder(User user, IOrderBuilder builder)
    {
        // Код не знает, какой именно строитель используется
        return builder
            .WithItem(new OrderItem(Price: 100, Amount: 1))
            .WithItem(new OrderItem(Price: 200, Amount: 2))
            .Build();
    }
}

// Использование в клиентском коде
var orderService = new OrderService();

// Для обычного пользователя - строитель с ограничениями
var regularBuilder = new LimitedOrderBuilder();
var regularOrder = orderService.CreateOrder(regularUser, regularBuilder);

// Для премиум-пользователя - строитель без ограничений
var premiumBuilder = new UnlimitedOrderBuilder();
var premiumOrder = orderService.CreateOrder(premiumUser, premiumBuilder);
```

#### Архитектурные преимущества

1. **Dependency Inversion Principle (Принцип инверсии зависимостей)** — высокоуровневый код зависит от абстракции (`IOrderBuilder`), а не от конкретных реализаций
2. **Open/Closed Principle (Принцип открытости/закрытости)** — можно добавлять новые типы строителей без изменения существующего кода
3. **Гибкость** — выбор конкретного строителя происходит на границе системы, обеспечивая централизованное управление политиками создания объектов

### 3. Director Pattern

**Director** (Директор) — это компонент паттерна Builder, который инкапсулирует алгоритм построения объекта. Директор знает, в каком порядке вызывать методы строителя для создания конкретной конфигурации объекта.

#### Проблема: повторяющиеся последовательности построения

Рассмотрим систему заказа пиццы. Часто клиенты хотят заказать стандартные рецепты, например, пиццу "Пепперони" или "Маргариту". Каждый рецепт — это определённая последовательность шагов:

```csharp
// Модель пиццы
public record Pizza(
    PizzaSize Size,
    DoughType DoughType,
    Sauce Sauce,
    IReadOnlyCollection<Topping> Toppings
);

// Перечисления для параметров
public enum PizzaSize { Small, Medium, Large }
public enum DoughType { Standard, Thin, Thick }
public enum Sauce { Tomato, BBQ, White }
public enum Topping { Cheese, Pepperoni, Mushrooms, Olives, Jalapeno, Bacon, Pineapple }

// Строитель пиццы
public class PizzaBuilder
{
    private readonly List<Topping> _toppings = new List<Topping>();
    private PizzaSize _size = PizzaSize.Medium;
    private DoughType _doughType = DoughType.Standard;
    private Sauce _sauce = Sauce.Tomato;

    public PizzaBuilder WithTopping(Topping topping)
    {
        _toppings.Add(topping);
        return this;
    }

    public PizzaBuilder WithSize(PizzaSize size)
    {
        _size = size;
        return this;
    }

    public PizzaBuilder WithDoughType(DoughType type)
    {
        _doughType = type;
        return this;
    }

    public PizzaBuilder WithSauce(Sauce sauce)
    {
        _sauce = sauce;
        return this;
    }

    public Pizza Build()
    {
        return new Pizza(
            Size: _size,
            DoughType: _doughType,
            Sauce: _sauce,
            Toppings: _toppings.AsReadOnly()
        );
    }
}
```

Без директора клиентский код должен каждый раз повторять последовательность создания стандартной пиццы:

```csharp
// Повторяющийся код для создания пиццы "Пепперони"
var pepperoni1 = new PizzaBuilder()
    .WithDoughType(DoughType.Standard)
    .WithSauce(Sauce.Tomato)
    .WithSize(PizzaSize.Medium)
    .WithTopping(Topping.Cheese)
    .WithTopping(Topping.Pepperoni)
    .Build();

// Та же последовательность повторяется снова
var pepperoni2 = new PizzaBuilder()
    .WithDoughType(DoughType.Standard)
    .WithSauce(Sauce.Tomato)
    .WithSize(PizzaSize.Medium)
    .WithTopping(Topping.Cheese)
    .WithTopping(Topping.Pepperoni)
    .Build();
```

#### Решение: класс Director

Director инкапсулирует алгоритм построения конкретной конфигурации:

```csharp
// Интерфейс директора
public interface IPizzaDirector
{
    PizzaBuilder Direct(PizzaBuilder builder);
}

// Директор для пиццы "Пепперони"
public class PepperoniPizzaDirector : IPizzaDirector
{
    public PizzaBuilder Direct(PizzaBuilder builder)
    {
        // Директор знает точный рецепт пиццы "Пепперони"
        return builder
            .WithDoughType(DoughType.Standard)
            .WithSauce(Sauce.Tomato)
            .WithSize(PizzaSize.Medium)
            .WithTopping(Topping.Cheese)
            .WithTopping(Topping.Pepperoni);
    }
}

// Директор для вегетарианской пиццы
public class VegetarianPizzaDirector : IPizzaDirector
{
    public PizzaBuilder Direct(PizzaBuilder builder)
    {
        return builder
            .WithDoughType(DoughType.Thin)
            .WithSauce(Sauce.White)
            .WithSize(PizzaSize.Medium)
            .WithTopping(Topping.Cheese)
            .WithTopping(Topping.Mushrooms)
            .WithTopping(Topping.Olives);
    }
}
```

#### Использование Director

```csharp
var pizzaBuilder = new PizzaBuilder();
var pepperoniDirector = new PepperoniPizzaDirector();

// Создаём стандартную пиццу "Пепперони" с дополнительными топпингами
var myPizza = pepperoniDirector
    .Direct(pizzaBuilder)
    .WithTopping(Topping.Jalapeno)      // Дополнительная кастомизация после директора
    .WithSize(PizzaSize.Large)          // Изменяем размер
    .Build();
```

#### Альтернатива: Extension Methods в C#

В C# часто применяют методы расширения (Extension Methods) вместо отдельных классов директоров. Этот подход более лаконичен:

```csharp
public static class PizzaBuilderExtensions
{
    // Метод расширения для создания базовой пиццы "Пепперони"
    public static PizzaBuilder AsPepperoni(this PizzaBuilder builder)
    {
        return builder
            .WithDoughType(DoughType.Standard)
            .WithSauce(Sauce.Tomato)
            .WithSize(PizzaSize.Medium)
            .WithTopping(Topping.Cheese)
            .WithTopping(Topping.Pepperoni);
    }

    // Метод расширения для вегетарианской пиццы
    public static PizzaBuilder AsVegetarian(this PizzaBuilder builder)
    {
        return builder
            .WithDoughType(DoughType.Thin)
            .WithSauce(Sauce.White)
            .WithSize(PizzaSize.Medium)
            .WithTopping(Topping.Cheese)
            .WithTopping(Topping.Mushrooms)
            .WithTopping(Topping.Olives);
    }
}
```

Использование методов расширения:

```csharp
// Более естественный синтаксис для C#
var myPizza = new PizzaBuilder()
    .AsPepperoni()
    .WithTopping(Topping.Jalapeno)
    .WithSize(PizzaSize.Large)
    .Build();
```

#### Когда использовать классы Director, а когда Extension Methods

- **Используйте классы Director**, когда:
  - Нужен полиморфизм — различные реализации директоров должны быть взаимозаменяемы
  - Директор имеет состояние или зависимости
  - Требуется внедрение зависимостей (Dependency Injection)

- **Используйте Extension Methods**, когда:
  - Директор не имеет состояния — это просто последовательность вызовов
  - Важна простота и лаконичность кода
  - Достаточно статической конфигурации без полиморфизма

#### Расширенный пример: Director с ограничениями

Рассмотрим более сложный случай, когда Director устанавливает базовую конфигурацию, которую клиент не может изменить. Например, тип теста определяется рецептом и не может быть изменён, а количество дополнительных топпингов ограничено пятью.

```csharp
using System;
using System.Collections.Generic;
using System.Linq;

public class AdvancedPizzaBuilder
{
    private const int MaxAdditionalToppings = 5;
    
    private readonly List<Topping> _toppings = new List<Topping>();
    private PizzaSize _size = PizzaSize.Medium;
    private DoughType _doughType = DoughType.Standard;
    private Sauce _sauce = Sauce.Tomato;
    
    // Флаг блокировки типа теста после установки директором
    private bool _doughTypeLocked = false;
    
    // Счётчик базовых топпингов, установленных директором
    // Эти топпинги не входят в лимит дополнительных топпингов
    private int _baseToppingsCount = 0;

    // Публичные методы для клиента с проверками
    
    public AdvancedPizzaBuilder WithTopping(Topping topping)
    {
        // Вычисляем количество дополнительных топпингов
        int additionalToppingsCount = _toppings.Count - _baseToppingsCount;
        
        if (additionalToppingsCount >= MaxAdditionalToppings)
        {
            throw new InvalidOperationException(
                $"Невозможно добавить более {MaxAdditionalToppings} дополнительных топпингов");
        }
        
        _toppings.Add(topping);
        return this;
    }

    public AdvancedPizzaBuilder WithSize(PizzaSize size)
    {
        _size = size;
        return this;
    }

    public AdvancedPizzaBuilder WithDoughType(DoughType type)
    {
        if (_doughTypeLocked)
        {
            throw new InvalidOperationException(
                "Тип теста заблокирован рецептом и не может быть изменён");
        }
        
        _doughType = type;
        return this;
    }

    public AdvancedPizzaBuilder WithSauce(Sauce sauce)
    {
        _sauce = sauce;
        return this;
    }

    public Pizza Build()
    {
        if (_toppings.Count == 0)
        {
            throw new InvalidOperationException("Пицца должна содержать хотя бы один топпинг");
        }

        return new Pizza(
            Size: _size,
            DoughType: _doughType,
            Sauce: _sauce,
            Toppings: _toppings.AsReadOnly()
        );
    }

    // Внутренние методы для использования директором
    // Эти методы обходят валидацию и используются только при построении базовой конфигурации
    
    internal AdvancedPizzaBuilder SetBaseDoughType(DoughType type)
    {
        _doughType = type;
        _doughTypeLocked = true;  // Блокируем изменение после установки
        return this;
    }

    internal AdvancedPizzaBuilder AddBaseTopping(Topping topping)
    {
        _toppings.Add(topping);
        _baseToppingsCount++;  // Увеличиваем счётчик базовых топпингов
        return this;
    }

    internal AdvancedPizzaBuilder SetBaseSauce(Sauce sauce)
    {
        _sauce = sauce;
        return this;
    }

    internal AdvancedPizzaBuilder SetBaseSize(PizzaSize size)
    {
        _size = size;
        return this;
    }
}

// Директор с использованием внутренних методов
public class StrictPepperoniDirector
{
    public AdvancedPizzaBuilder Direct(AdvancedPizzaBuilder builder)
    {
        // Используем внутренние методы для установки базовой конфигурации
        return builder
            .SetBaseDoughType(DoughType.Standard)  // Устанавливаем и блокируем тип теста
            .SetBaseSauce(Sauce.Tomato)
            .SetBaseSize(PizzaSize.Medium)
            .AddBaseTopping(Topping.Cheese)        // Базовый топпинг из рецепта
            .AddBaseTopping(Topping.Pepperoni);    // Базовый топпинг из рецепта
    }
}

// Пример использования
public class DirectorExamples
{
    public static void Demo()
    {
        Console.WriteLine("Пример 1: Успешная кастомизация");
        var builder1 = new AdvancedPizzaBuilder();
        var director = new StrictPepperoniDirector();

        var customPizza = director
            .Direct(builder1)
            .WithTopping(Topping.Jalapeno)      // Дополнительный топпинг 1/5
            .WithTopping(Topping.Mushrooms)     // Дополнительный топпинг 2/5
            .WithSize(PizzaSize.Large)          // Размер можно изменить
            .Build();

        PrintPizza(customPizza);

        Console.WriteLine("\nПример 2: Попытка изменить заблокированное тесто");
        try
        {
            var builder2 = new AdvancedPizzaBuilder();
            var failedPizza = director
                .Direct(builder2)
                .WithDoughType(DoughType.Thick)  // Вызовет исключение
                .Build();
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"Ошибка: {ex.Message}");
        }

        Console.WriteLine("\nПример 3: Превышение лимита дополнительных топпингов");
        try
        {
            var builder3 = new AdvancedPizzaBuilder();
            var overloadedPizza = director
                .Direct(builder3)
                .WithTopping(Topping.Bacon)
                .WithTopping(Topping.Mushrooms)
                .WithTopping(Topping.Olives)
                .WithTopping(Topping.Jalapeno)
                .WithTopping(Topping.Pineapple)
                .WithTopping(Topping.Bacon)  // Шестой дополнительный - вызовет исключение
                .Build();
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"Ошибка: {ex.Message}");
        }
    }

    private static void PrintPizza(Pizza pizza)
    {
        Console.WriteLine($"Пицца:");
        Console.WriteLine($"  Размер: {pizza.Size}");
        Console.WriteLine($"  Тесто: {pizza.DoughType}");
        Console.WriteLine($"  Соус: {pizza.Sauce}");
        Console.WriteLine($"  Топпинги ({pizza.Toppings.Count}): {string.Join(", ", pizza.Toppings)}");
    }
}
```

Этот подход демонстрирует, как Director может устанавливать неизменяемые параметры базовой конфигурации, оставляя клиенту возможность кастомизации в установленных рамках.

### 4. Interface-Driven Builder

**Interface-Driven Builder** (Строитель с управлением через интерфейсы) — это продвинутый подход, который использует систему типов для обеспечения корректности построения объектов на этапе компиляции.

#### Проблема: отложенная проверка обязательных параметров

Рассмотрим модель электронного письма:

```csharp
public record Email(
    string Address,
    string Subject,
    string Body
);
```

Бизнес-правило: адрес (`Address`) обязателен, а тема (`Subject`) и тело (`Body`) опциональны.

При использовании обычного строителя ошибка обнаружится только в момент вызова `Build()`:

```csharp
var email = new EmailBuilder()
    .WithBody("Привет!")
    .Build();  // InvalidOperationException: адрес не установлен
```

Проблема в том, что компилятор не может предотвратить создание некорректного объекта. Ошибка обнаруживается во время выполнения (runtime), а не на этапе компиляции (compile-time).

#### Решение: Interface-Driven Builder

Идея заключается в использовании интерфейсов для представления различных состояний строителя. Каждый метод возвращает интерфейс, который определяет доступные на следующем шаге операции:

```csharp
// Интерфейс начального состояния: только установка адреса доступна
public interface IEmailAddressBuilder
{
    IEmailBuilder WithAddress(string address);
}

// Интерфейс после установки адреса: доступны опциональные поля и финализация
public interface IEmailBuilder
{
    IEmailBuilder WithSubject(string subject);
    IEmailBuilder WithBody(string body);
    Email Build();
}

// Фабрика и реализация строителя
public static class Email
{
    // Точка входа для создания строителя
    public static IEmailAddressBuilder Builder => new EmailBuilder();

    // Приватная реализация, реализующая оба интерфейса
    private class EmailBuilder : IEmailAddressBuilder, IEmailBuilder
    {
        private string? _address;
        private string _subject = string.Empty;
        private string _body = string.Empty;

        // Метод из IEmailAddressBuilder
        // Возвращает IEmailBuilder, разблокируя доступ к остальным методам
        public IEmailBuilder WithAddress(string address)
        {
            _address = address;
            return this;  // Возвращаем this, но как IEmailBuilder
        }

        // Методы из IEmailBuilder
        public IEmailBuilder WithSubject(string subject)
        {
            _subject = subject;
            return this;
        }

        public IEmailBuilder WithBody(string body)
        {
            _body = body;
            return this;
        }

        public Email Build()
        {
            // На этом этапе адрес гарантированно установлен благодаря типам
            // Но для надёжности оставляем проверку
            if (_address is null)
                throw new InvalidOperationException("Адрес должен быть установлен");

            return new Email(
                Address: _address,
                Subject: _subject,
                Body: _body
            );
        }
    }
}
```

#### Использование Interface-Driven Builder

```csharp
// Это НЕ скомпилируется - компилятор не позволит вызвать Build() без WithAddress()
// var email = Email.Builder
//     .WithBody("Привет!")
//     .Build();  // Ошибка компиляции: у IEmailAddressBuilder нет метода WithBody()

// Правильное использование: сначала обязательный адрес
var email = Email.Builder
    .WithAddress("user@example.com")
    .WithBody("Привет!")
    .Build();  // Компилируется успешно

// Порядок опциональных параметров не важен
var email2 = Email.Builder
    .WithAddress("user@example.com")
    .WithSubject("Приветствие")
    .WithBody("Добрый день")
    .Build();

// Можно установить только адрес
var email3 = Email.Builder
    .WithAddress("admin@example.com")
    .Build();
```

#### Преимущества Interface-Driven Builder

1. **Compile-time safety (Безопасность на этапе компиляции)** — компилятор гарантирует установку обязательных параметров
2. **Self-documenting API (Самодокументируемый API)** — из типов интерфейсов ясно, какие операции доступны на каждом этапе
3. **Невозможность создания некорректных объектов** — система типов предотвращает ошибки до выполнения программы
4. **IntelliSense support (Поддержка автодополнения)** — IDE показывает только доступные на текущем этапе методы

#### Когда использовать Interface-Driven Builder

**Используйте**, когда:
- Есть строгие требования к порядку установки параметров
- Критична безопасность типов на этапе компиляции
- Важно предотвратить создание некорректных объектов
- API должен явно направлять разработчика

**Не используйте**, когда:
- Все параметры опциональны
- Порядок установки параметров не имеет значения
- Сложность реализации не оправдывает преимущества (overkill для простых случаев)


## Важные принципы проектирования

### Правило: избегайте смешивания типов строителей

Технически возможно использовать несколько строителей для одного объекта, но такая необходимость часто указывает на проблемы в дизайне модели.

#### Антипаттерн: множественные строители для одной модели

```csharp
// Плохо: один заказ собирается разными строителями
var builder1 = new OrderBuilder();
var builder2 = new LimitedOrderBuilder();

builder1.WithItem(item1);
builder2.WithItem(item2);

// Проблема: как собрать заказ? Какой строитель использовать?
// Данные распределены между двумя объектами
```

#### Решение: декомпозиция модели

Если требуются различные строители, это сигнал о том, что модель слишком сложна и должна быть разделена:

```csharp
// Хорошо: разные модели для разных типов заказов
public record RegularOrder(IEnumerable<OrderItem> Items);
public record PremiumOrder(IEnumerable<OrderItem> Items, string VipStatus);

// Каждая модель имеет свой строитель
public class RegularOrderBuilder 
{
    // Логика для обычного заказа
}

public class PremiumOrderBuilder 
{
    // Логика для премиум-заказа
}
```

### Золотое правило зависимостей

**Строитель зависит от модели, а не наоборот.**

Модель должна оставаться независимой от способов её создания. Builder — это вспомогательный компонент, который существует для удобства, но не влияет на саму доменную модель.

## Практическое применение Builder

Паттерн Builder широко используется в реальных приложениях. Рассмотрим типичные сценарии.

### 1. HTTP запросы

```csharp
// Построение HTTP-запроса через Builder
var request = new HttpRequestBuilder()
    .WithUrl("https://api.example.com/orders")
    .WithMethod(HttpMethod.Post)
    .WithHeader("Authorization", "Bearer eyJhbGciOiJIUzI1NiIs...")
    .WithHeader("Content-Type", "application/json")
    .WithBody(jsonPayload)
    .WithTimeout(TimeSpan.FromSeconds(30))
    .Build();
```

### 2. SQL запросы

```csharp
// Построение SQL-запроса через Builder
var query = new SqlQueryBuilder()
    .Select("id", "name", "email")
    .From("users")
    .Where("age > @minAge")
    .WithParameter("@minAge", 18)
    .OrderBy("name ASC")
    .Limit(100)
    .Build();
```

### 3. Конфигурация UI

```csharp
// Построение формы через Builder
var form = new FormBuilder()
    .AddField(new TextField("name", required: true, maxLength: 100))
    .AddField(new EmailField("email", required: true))
    .AddField(new PasswordField("password", minLength: 8))
    .AddButton("Зарегистрироваться", ButtonType.Submit)
    .WithValidation(new FormValidator())
    .Build();
```

### 4. Конфигурация приложения

```csharp
// Построение конфигурации приложения
var config = new AppConfigBuilder()
    .WithDatabase("Server=localhost;Database=mydb")
    .WithLogging(LogLevel.Information)
    .WithCache(CacheType.Redis, "localhost:6379")
    .WithFeatureFlag("NewCheckout", enabled: true)
    .Build();
```

## Философия паттерна и уроки проектирования

Паттерн Builder учит нас важным принципам разработки программного обеспечения.

### 1. Разделение ответственности

Builder отделяет логику создания объекта от самой модели. Модель остаётся чистой и сфокусированной на представлении данных, а вся сложность процесса создания инкапсулирована в строителе.

```mermaid
graph LR
    A[Модель] -->|чистая, без логики создания| B[Представление данных]
    C[Builder] -->|инкапсулирует| D[Логика создания]
    C -->|создаёт| A
```

### 2. Гибкость без сложности

Вместо множества перегруженных конструкторов (constructor hell) Builder предоставляет чистый, читаемый интерфейс. Код становится самодокументируемым.

### 3. Валидация в правильном месте

Логика валидации находится там, где происходит сбор данных — в строителе. Это позволяет:

- Обнаруживать ошибки немедленно (fail-fast)
- Избежать дублирования проверок
- Сохранить модель свободной от бизнес-логики валидации

### 4. Fluent Interface

Возвращая `this` из каждого метода, Builder создаёт текучий интерфейс. Код читается как предложение естественного языка:

```csharp
order.WithItem(item1).WithComment("Срочно").WithDelivery(express).Build();
```

### 5. Инверсия зависимостей

Используя интерфейсы строителей, высокоуровневый код зависит от абстракций, а не от конкретных реализаций. Это упрощает тестирование и расширение системы.

### 6. Типобезопасность

Interface-Driven Builder использует систему типов для гарантии корректности на этапе компиляции. Компилятор становится союзником, предотвращая ошибки до запуска программы.

## Комплексный пример: система заказов

Рассмотрим полный пример, объединяющий все изученные концепции:

```csharp
using System;
using System.Collections.Generic;
using System.Linq;

// Доменные модели
public record OrderItem(string Name, decimal Price, int Amount);

public record Order(
    string CommentForShop,
    string CommentForDelivery,
    IReadOnlyCollection<OrderItem> Items,
    DateTimeOffset CreatedAt,
    string? ReceiverPhoneNumber
)
{
    // Вычисляемое свойство - общая стоимость заказа
    public decimal TotalPrice => Items.Sum(i => i.Price * i.Amount);
}

// Builder с валидацией
public class OrderBuilder
{
    private const int MaxOrderItemCount = 20;
    private readonly List<OrderItem> _items = new List<OrderItem>();
    private string _commentForShop = string.Empty;
    private string _commentForDelivery = string.Empty;
    private DateTimeOffset _createdAt = DateTimeOffset.UtcNow;
    private string? _receiverPhoneNumber = null;

    // Добавление товара с валидацией
    public OrderBuilder WithItem(OrderItem item)
    {
        if (_items.Count >= MaxOrderItemCount)
        {
            throw new ArgumentException(
                $"Невозможно добавить более {MaxOrderItemCount} товаров в заказ");
        }
        
        _items.Add(item);
        return this;
    }

    public OrderBuilder WithCommentForShop(string comment)
    {
        _commentForShop = comment ?? string.Empty;
        return this;
    }

    public OrderBuilder WithCommentForDelivery(string comment)
    {
        _commentForDelivery = comment ?? string.Empty;
        return this;
    }

    public OrderBuilder WithReceiverPhoneNumber(string? phoneNumber)
    {
        _receiverPhoneNumber = phoneNumber;
        return this;
    }

    // Финальное создание заказа
    public Order Build()
    {
        if (_items.Count == 0)
        {
            throw new InvalidOperationException("Заказ должен содержать хотя бы один товар");
        }

        return new Order(
            CommentForShop: _commentForShop,
            CommentForDelivery: _commentForDelivery,
            Items: _items.AsReadOnly(),
            CreatedAt: _createdAt,
            ReceiverPhoneNumber: _receiverPhoneNumber
        );
    }
}

// Примеры использования
public class OrderExamples
{
    public static void Demo()
    {
        Console.WriteLine("=== Пример 1: Простой заказ ===");
        var simpleOrder = new OrderBuilder()
            .WithItem(new OrderItem(Name: "Ноутбук", Price: 50000, Amount: 1))
            .WithItem(new OrderItem(Name: "Мышь", Price: 1500, Amount: 2))
            .Build();

        PrintOrder(simpleOrder);

        Console.WriteLine("\n=== Пример 2: Заказ с комментариями ===");
        var detailedOrder = new OrderBuilder()
            .WithItem(new OrderItem(Name: "Книга", Price: 800, Amount: 3))
            .WithCommentForShop("Упаковать аккуратно, это подарок")
            .WithCommentForDelivery("Позвонить за 30 минут")
            .WithReceiverPhoneNumber("+7-900-123-45-67")
            .Build();

        PrintOrder(detailedOrder);

        Console.WriteLine("\n=== Пример 3: Пошаговое построение ===");
        var builder = new OrderBuilder();
        
        // Различные методы могут добавлять товары
        AddBasicItems(builder);
        AddRequestedItems(builder);
        AddRecommendedItems(builder);
        
        var complexOrder = builder
            .WithCommentForShop("Комплексный заказ")
            .Build();

        PrintOrder(complexOrder);

        Console.WriteLine("\n=== Пример 4: Обработка ошибок - пустой заказ ===");
        try
        {
            var invalidOrder = new OrderBuilder().Build();
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"Ошибка: {ex.Message}");
        }

        Console.WriteLine("\n=== Пример 5: Обработка ошибок - превышение лимита ===");
        try
        {
            var overloadedBuilder = new OrderBuilder();
            for (int i = 0; i < 21; i++)
            {
                overloadedBuilder.WithItem(
                    new OrderItem(Name: $"Товар {i}", Price: 100, Amount: 1));
            }
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Ошибка: {ex.Message}");
        }
    }

    // Вспомогательные методы для добавления товаров
    private static void AddBasicItems(OrderBuilder builder)
    {
        builder.WithItem(new OrderItem(Name: "Хлеб", Price: 50, Amount: 2));
    }

    private static void AddRequestedItems(OrderBuilder builder)
    {
        builder
            .WithItem(new OrderItem(Name: "Молоко", Price: 80, Amount: 1))
            .WithItem(new OrderItem(Name: "Яйца", Price: 120, Amount: 1));
    }

    private static void AddRecommendedItems(OrderBuilder builder)
    {
        builder.WithItem(new OrderItem(Name: "Сыр", Price: 350, Amount: 1));
    }

    // Вывод информации о заказе
    private static void PrintOrder(Order order)
    {
        Console.WriteLine($"Заказ создан: {order.CreatedAt:dd.MM.yyyy HH:mm:ss}");
        Console.WriteLine($"Товаров в заказе: {order.Items.Count}");
        Console.WriteLine($"Общая сумма: {order.TotalPrice:N2} руб.");
        
        if (!string.IsNullOrEmpty(order.CommentForShop))
            Console.WriteLine($"Комментарий магазину: {order.CommentForShop}");
        
        if (!string.IsNullOrEmpty(order.CommentForDelivery))
            Console.WriteLine($"Комментарий доставке: {order.CommentForDelivery}");
        
        if (order.ReceiverPhoneNumber != null)
            Console.WriteLine($"Телефон получателя: {order.ReceiverPhoneNumber}");
    }
}
```

## Когда применять паттерн Builder

### 1. Избавление от телескопических конструкторов

Используйте Builder, когда конструктор имеет множество опциональных параметров, что приводит к созданию десятков перегруженных версий:

```csharp
// Антипаттерн: телескопические конструкторы
public class House
{
    public House(string address) { }
    public House(string address, int floors) { }
    public House(string address, int floors, bool hasGarage) { }
    public House(string address, int floors, bool hasGarage, bool hasPool) { }
    // ...ещё десятки вариантов
}
```

Паттерн Builder позволяет собирать объекты пошагово, вызывая только необходимые шаги.

### 2. Создание различных представлений объекта

Builder полезен, когда нужно создавать различные конфигурации объекта с общими этапами построения. Например, дома из разных материалов (дерево, бетон, кирпич) строятся по одинаковым этапам, но с разными деталями.

```mermaid
graph TD
    A[Директор] -->|управляет| B[Builder]
    B --> C[Деревянный дом]
    B --> D[Каменный дом]
    B --> E[Кирпичный дом]
```

Интерфейс строителей определяет этапы конструирования. Каждому представлению соответствует своя реализация. Директор задаёт порядок этапов.

### 3. Построение сложных составных объектов

Builder идеален для создания древовидных структур (например, паттерн Composite), где построение происходит рекурсивно. Builder выполняет шаги последовательно, а не за один проход.

Важное преимущество: Builder не позволяет клиентскому коду получить доступ к объекту до завершения построения. Это защищает от работы с незавершёнными объектами.

## Преимущества и недостатки

### Преимущества

1. **Пошаговое создание** — объект собирается шаг за шагом, что делает процесс понятным и контролируемым
2. **Повторное использование кода** — один строитель может создавать различные представления объектов
3. **Изоляция сложной логики** — детали построения скрыты от клиентского кода
4. **Single Responsibility Principle** — модель не знает о деталях своего создания
5. **Fluent Interface** — код становится читаемым и выразительным

### Недостатки

1. **Усложнение кода** — для каждого продукта нужен отдельный строитель
2. **Избыточность для простых случаев** — если объект простой, Builder может быть излишним
3. **Дублирование полей** — строитель дублирует поля модели

## Связь с другими паттернами

### Builder и Abstract Factory

- **Abstract Factory** создаёт семейства связанных объектов
- **Builder** фокусируется на пошаговом создании одного сложного объекта

### Builder и Prototype

- **Prototype** копирует существующие объекты
- **Builder** создаёт новые объекты с нуля по шагам

### Builder и Composite

Builder часто используется для построения структур Composite, так как позволяет рекурсивно создавать вложенные объекты.

### Builder и Director

Director инкапсулирует алгоритм создания, используя Builder для выполнения шагов. Director знает, какие шаги и в каком порядке выполнять.

## Заключение

Паттерн Builder — это мощный инструмент для создания сложных объектов. Он решает проблемы телескопических конструкторов, обеспечивает чистоту кода и следует принципам SOLID.

Ключевые моменты для запоминания:

1. **Builder отделяет создание от представления** — модель остаётся чистой
2. **Fluent Interface делает код читаемым** — цепочки вызовов естественны
3. **Валидация в строителе** — ошибки обнаруживаются рано (fail-fast)
4. **Гибкость через интерфейсы** — полиморфизм строителей расширяет возможности
5. **Interface-Driven Builder** — типобезопасность на этапе компиляции

Используйте Builder, когда создание объекта требует множества шагов или когда нужно создавать различные представления одного объекта. Избегайте избыточного применения паттерна для простых случаев, где достаточно обычного конструктора.
