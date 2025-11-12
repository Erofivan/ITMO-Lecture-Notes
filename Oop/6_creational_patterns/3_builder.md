
### Строитель (билдер)

> выделение отдельного типа, инкапсулирующего логику сбора данных и создания объекта

Параметр - набор тип+имя находящийся в сигнатуре метода 
Ex.: `public void A(int a, char b); // int a и char b — это параметры`

Аргумент - конкретное значение передающееся в метод
Ex.: `obj.A(1, '2'); // 1 и '2' - аргументы метода A()`

```csharp
void M(int x, string s)   // int x и string s — параметры
{
}
M(10, "hi");               // 10 и "hi" — аргументы
```

Строитель — это порождающий паттерн проектирования, который позволяет создавать сложные объекты пошагово. Строитель даёт возможность использовать один и тот же код строительства для получения разных представлений объектов.

![](src/builder/intro.png)

Представьте объект (например, дом) у которого может быть очень много разных конфигураций. Два варианта: либо передавать каким-то образом всё через огромный конструктор:
![](src/builder/constructor.png)
Далеко не все аргументы всегда используются и приходится делать их null. Либо создаётся проблема "телескопических конструкторов", то есть матрёшки из конструкторов:
```csharp
public class House
{
    public string Address { get; }
    public int Floors { get; }
    public bool HasGarage { get; }
    public bool HasGarden { get; }
    public bool HasPool { get; }
    public bool HasFancyStatues { get; }

    // Базовый конструктор, куда сводятся все остальные
    public House(string address, int floors, bool hasGarage, bool hasGarden, bool hasPool, bool hasFancyStatues)
    {
        Address = address;
        Floors = floors;
        HasGarage = hasGarage;
        HasGarden = hasGarden;
        HasPool = hasPool;
        HasFancyStatues = hasFancyStatues;
    }

    // Только адрес → минимальный дом
    public House(string address)
        : this(address, 1, false, false, false, false) { }

    // Адрес + этажность
    public House(string address, int floors)
        : this(address, floors, false, false, false, false) { }

    // Дом с гаражом
    public static House WithGarage(string address, int floors = 1)
        => new House(address, floors, hasGarage: true, hasGarden: false, hasPool: false, hasFancyStatues: false);

    // Дом с садом
    public static House WithGarden(string address, int floors = 1)
        => new House(address, floors, false, hasGarden: true, false, false);

    // Дом с бассейном
    public static House WithPool(string address, int floors = 1)
        => new House(address, floors, false, false, hasPool: true, false);

    // Дом со статуями
    public static House WithFancyStatues(string address, int floors = 1)
        => new House(address, floors, false, false, false, hasFancyStatues: true);
 
    // И так далее...
} 
```
Либо же, что возможно ещё хуже, придется наследовать кучу разных подклассов для всех возможных комбинаций параметров

![](src/builder/inheritance.png)

Паттерн Строитель предлагает разбить весь этот процесс на отдельные шаги. Пришём, чтобы создать объект, вам нужно лишь поочерёдно вызывать методы строителя. И теперь не нужно запускать все шаги, а только те, что нужны для производства объекта определённой конфигурации.

![](src/builder/usage.png)

И в нашем коде получится что-то вроде:
```csharp

// Раньше: создавали дом в явном виде
var house = House(
    adress: "ulitsa- pushkina-dom-kalatush",
    floors: 2,
    HasGarage = false,
    HasGarden = true,
    HasPool = true,
    HasFancyStatues = true
    )

// Либо: создавали конкретный дом
var house = houseWithPoolAndGardenAndFancyStatues(
    adress: "ulitsa- pushkina-dom-kalatush",
    floors: 2
    )
// И если бы мы хотели добавить что-то новое,
// то пришлось бы либо 
// менять конструктор, либо вызывать новый дом

// Теперь можем собрать дом "по кусочкам":
var house = HouseBuilder()
    .WithAddress("ulitsa-pushkina-dom-kalatush")
    .WithFloors(2)
    .WithPool()
    .WithGarden()
    .WithStatues()
    .Build()

// Если бы мы захотели добавить что-то ещё
// , то достаточно было бы просто прописать ещё 
// один метод, например WithFountain() и WithPlayground()
```

То есть мы вынесли логику создания из чего-то цельного в "как бы лего конструктор", куда мы при желании можешь в любой момент присоединить что-то новое.

Рассмотрим пример того, как применить этот паттерн в реальности:

У вас есть класс Order, который описывает заказ в интернет-магазине. Изначально это было бы просто:
```csharp
public record Order(IEnumerable<OrderItem> Items);
```

То есть просто список каких-то заказов:
```csharp
var order = new Order(
    [
        new OrderItem(Name: "banana", Price: 42, Amount: 20),
        new OrderItem(Name: "phone", Price: 30000, Amount: 1),
        new OrderItem(Name: "marker", Price: 201, Amount: 3),
        new OrderItem(Name: "steak", Price: 1337, Amount: 1)
    ]
);
```

Но со временем требования растут. Помимо просто списка заказов становятся нужными комментарий для магазина, комментарий для доставки, номер телефона получателя, время создания и ещё десяток полей:
```csharp
public record Order(
    string CommentForShop,
    string CommentForDelivery,
    IEnumerable<OrderItem> Items,
    DateTimeOffset CreatedAt,
    string? ReceiverPhoneNumber,
    // и ещё много...
);
```
Теперь, когда мы захотим создать объект, конструктор потребует множество параметров:
```csharp
var order = new Order(
    CommentForShop: "Срочно!",
    CommentForDelivery: "Оставить у двери",
    Items: [
        new OrderItem(Name: "banana", Price: 42, Amount: 20),
        new OrderItem(Name: "phone", Price: 30000, Amount: 1),
        new OrderItem(Name: "marker", Price: 201, Amount: 3),
        new OrderItem(Name: "steak", Price: 1337, Amount: 1)
    ],
    CreatedAt: DateTimeOffset.UtcNow,
    ReceiverPhoneNumber: "+7-900-123-45-67"
);
```

Это становится неудобно. Кроме того, создание заказа часто требует логики: иногда товары надо добавлять в строго определённом порядке, например, сначала продукты, а только потом бытовую технику. Где проверять, что в заказе, например, не больше 20 товаров? Некоторые клиеты (например с вип-статусом) могут иметь больше привелегий. Как всё это организовать?     

Вот здесь появляется паттерн Builder. Как мы уже знаем его основная идея выделить отдельный тип, который инкапсулирует логику сбора данных и создания объекта. 

Builder — это вспомогательный класс, который:
- Накапливает состояние — хранит промежуточные значения параметров
- Предоставляет удобный API — методы вида WithXxx(), которые возвращают сам builder для цепочки вызовов (fluent interface)
- Выполняет финальное создание — метод Build() создаёт финальный объект

```csharp
// Строитель
public class OrderBuilder
{
    // 1. Он хранит *внутреннее, изменяемое состояние*
    private readonly List<OrderItem> _items = [];

    // 2. Он предоставляет "текучий" (fluent) интерфейс
    public OrderBuilder WithItem(OrderItem item)
    {
        _items.Add(item);
        return this; // <- Ключевой момент для "цепочки" вызовов
    }

    // 3. Он имеет финальный метод "Build"
    public Order Build()
    {
        // В этот момент он создает иммутабельный продукт
        return new Order(_items.ToArray());
    }
}
```

Теперь можем удобно использовать это билдер:
```csharp
var orderBuilder = new OrderBuilder() // Заказ
    .WithItem(Name: "banana", Price: 42, Amount: 20) // с бананами
    .WithItem(Name: "phone", Price: 30000, Amount: 1) // и телефоном
    .WithItem(Name: "marker", Price: 201, Amount: 3) // и тремя маркерами
    .WithItem(Name: "steak", Price: 1337, Amount: 1); // и одним стейком

// Мы можем *передавать строителя* в другие методы,
// чтобы они его "до-настроили".
AddDefaultItems(orderBuilder);
AddRequestedItems(orderBuilder); 
AddForecastedItems(orderBuilder);

Order order = orderBuilder.Build(); // соберём заказ
```
Видите красоту? Логика добавления товаров разнесена по разным методам, но всё аккуратно собирается в одном builder'е.

Давайте рассмотрим два вида билдеров:

#### 1. Convenience builder (Удобный Строитель)

> упрощенное создание объектов с большим конструктором

Это тип можно охарактеризовать так:
- модель никак не связана с билдером
- несёт в себе вспомогательный функционал
- используется для упрощения создания объектов

Вспоним ситуацию с огромным конструктором:
```csharp
// в заказе много полей
public record Order(
    string CommentForShop,
    string CommentForDelivery,
    IEnumerable<OrderItem> Items,
    DateTimeOffset CreatedAt,
    string? ReceiverPhoneNumber
);

// пропуск кода

// Это то, чего мы хотим избежать!
var order = new Order(
    CommentForShop: string.Empty, // по умолчанию
    CommentForDelivery: string.Empty, // по умолчанию
    Items: [new OrderItem(Price: 1337, Amount: 2)], // < - единственное нужное поле
    CreatedAt: DateTimeOffset.UtcNow, // по умолчанию
    ReceiverPhoneNumber: null // по умолчанию
);
```
Convinience builder решает эту проблему. Он сам, внутри себя, хранит эти значения по умолчанию.

```csharp
// Builder должен хранить значения всех полей со значениями по умолчанию
public class OrderBuilder
{
    private readonly List<OrderItem> _items = [];
    private string _commentForShop = string.Empty;
    private string _commentForDelivery = string.Empty;
    private DateTimeOffset _createdAt = DateTimeOffset.UtcNow;
    private string? _receiverPhoneNumber = null;

    public OrderBuilder WithItem(OrderItem item)
    {
        _items.Add(item);
        return this;
    }

    public OrderBuilder WithCommentForShop(string value)
    {
        _commentForShop = value;
        return this;
    }

    public OrderBuilder WithCommentForDelivery(string value)
    {
        _commentForDelivery = value;
        return this;
    }

    public OrderBuilder WithReceiverPhoneNumber(string? value)
    {
        _receiverPhoneNumber = value;
        return this;
    }

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
Использование:

Теперь код клиента становится чистым. Клиент указывает только то, что отклоняется от нормы.

```csharp
var order = new OrderBuilder()
    .WithItem(new OrderItem(Price: 1337, Amount: 2))
    .Build();

// Или с большей кастомизацией:
var customOrder = new OrderBuilder()
    .WithItem(new OrderItem(Price: 100, Amount: 5))
    .WithCommentForShop("Упаковать аккуратно!")
    .WithReceiverPhoneNumber("+7-900-000-00-00")
    .Build();
```

То есть с помощью Convenience Builder мы упрощаем создание объектов с гигантским конструктором, предполагая, что некоторые аргумент можем сделать по умолчанию. 

Следующий уровень: Stateful Constructor 

#### 2. Stateful Constructor (Строитель с состоянием и валидацией)

> используется как конструктор, имеющий состояние (валидации)

- в билдер выносятся валидации входных данных
- позволяет выполнять валидации во время сбора данных
    - fail-fast
    - упрощение логики валидации
    - упрощение определения момента добавления некорректных данных

Двигаемся дальше. Вот появляется новое требование: в заказе не может быть больше 20 товаров. Где это проверять?

Если добавить проверку в конструктор Order, это нарушит принцип единственной ответственности — модель станет отвечать за валидацию правил бизнеса. Кроме того, если 21-й товар добавлен, мы поймём об этом только при Build().

Stateful Constructor решает это так: валидация происходит во время сбора данных (fail-fast).

```csharp
public class Order
{
    private Order(IEnumerable<OrderItem> items)
    {
        Items = items;
    }

    public IEnumerable<OrderItem> Items { get; }

    // Builder — вложенный класс
    public class OrderBuilder
    {
        private const int MaxOrderItemCount = 20;
        private readonly List<OrderItem> _items = [];

        public OrderBuilder WithItem(OrderItem item)
        {
            if (_items.Count >= MaxOrderItemCount)
                throw new ArgumentException(
                    $"Cannot add more than {MaxOrderItemCount} items");
            
            _items.Add(item);
            return this;
        }

        public Order Build()
        {
            return new Order(_items.ToArray());
        }
    }
}
```
использование:
```csharp
var orderBuilder = new Order.OrderBuilder();
for (int i = 0; i < 20; i++)
{
    orderBuilder.WithItem(new OrderItem(Price: i, Amount: 1));
}

// Это выбросит исключение сразу же!
orderBuilder.WithItem(new OrderItem(Price: 1000, Amount: 1));
```
Ключевые преимущества:
- Fail-fast: ошибка обнаруживается немедленно, а не при Build()
- Упрощение валидации: логика валидации находится в одном месте (в builder'е)
- Ясная причина ошибки: вы точно знаете, какой именно WithItem() вызов вызвал проблему
Почему конструктор приватный? Потому что мы хотим, чтобы все создавали заказ только через builder, в котором есть валидация. Это обеспечивает инвариант: любой Order в системе гарантированно содержит не более 20 товаров.

#### Архитектурное решение: Полиморфизм через интерфейсы

Иногда в системе есть разные типы заказов. Например, заказы для обычных пользователей (с лимитом 20 товаров) и премиум-заказы (без лимита).

Неправильный подход: создать UnlimitedOrderBuilder и LimitedOrderBuilder и использовать их отдельно.

Правильный подход: использовать интерфейс:
```csharp
public interface IOrderBuilder
{
    IOrderBuilder WithItem(OrderItem item);
    Order Build();
}

public class LimitedOrderBuilder : IOrderBuilder
{
    private const int MaxOrderItemCount = 20;
    private readonly List<OrderItem> _items = [];

    public IOrderBuilder WithItem(OrderItem item)
    {
        if (_items.Count >= MaxOrderItemCount)
            throw new ArgumentException("Limit exceeded");
        _items.Add(item);
        return this;
    }

    public Order Build()
    {
        return new Order(_items.ToArray());
    }
}

public class UnlimitedOrderBuilder : IOrderBuilder
{
    private readonly List<OrderItem> _items = [];

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
Использование:
```csharp
public class OrderService
{
    public Order CreateOrder(User user, IOrderBuilder builder)
    {
        // Код не знает, ограниченный это builder или нет
        return builder
            .WithItem(new OrderItem(Price: 100, Amount: 1))
            .WithItem(new OrderItem(Price: 200, Amount: 2))
            .Build();
    }
}

// Для обычного пользователя
var regularBuilder = new LimitedOrderBuilder();
var regularOrder = orderService.CreateOrder(user, regularBuilder);

// Для премиум-пользователя
var premiumBuilder = new UnlimitedOrderBuilder();
var premiumOrder = orderService.CreateOrder(user, premiumBuilder);
```
Архитектурный смысл:
- Code не зависит от конкретных типов builder'ов
- Выбор типа builder'а происходит на границе системы (где-то выше)
- Это инвертирует зависимость: высокоуровневый код зависит от интерфейса, а не от реализации

#### Директор

Теперь рассмотрим ситуацию, когда процесс построения объекта имеет определённый порядок и логику.

Например, мы собираем пиццу:

```csharp
public record Pizza(
    PizzaSize Size,
    DoughType DoughType,
    Sauce Sauce,
    IReadOnlyCollection<Topping> Toppings
);

public class PizzaBuilder
{
    private readonly List<Topping> _toppings = [];
    private PizzaSize _size = PizzaSize.Medium;
    private DoughType _doughType = DoughType.Standard;
    private Sauce _sauce = Sauce.Tomato;

    public PizzaBuilder WithTopping(Topping topping) { /* ... */ }
    public PizzaBuilder WithSize(PizzaSize size) { /* ... */ }
    public PizzaBuilder WithDoughType(DoughType type) { /* ... */ }
    public PizzaBuilder WithSause(Sauce sauce) { /* ... */ }

    public Pizza Build() { /* ... */ }
}
```

Проблема: часто нужно создать пиццу по рецепту. Например, пепперони пицца — это всегда определённая комбинация:

1. Стандартное тесто
2. Томатный соус
3. Средний размер
4. Сыр и пепперони

Решение: Директор — класс, который знает алгоритм построения и направляет builder:

```csharp
public interface IPizzaDirector
{
    PizzaBuilder Direct(PizzaBuilder builder);
}

public class PepperoniPizzaDirector : IPizzaDirector
{
    public PizzaBuilder Direct(PizzaBuilder builder)
    {
        return builder
            .WithDoughType(DoughType.Standard)
            .WithSause(Sauce.Tomato)
            .WithSize(PizzaSize.Medium)
            .WithTopping(Topping.Cheese)
            .WithTopping(Topping.Pepperoni);
    }
}
```
использование:
```csharp
var pizzaBuilder = new PizzaBuilder();
var pepperoniDirector = new PepperoniPizzaDirector();

var myPizza = pepperoniDirector
    .Direct(pizzaBuilder)
    .WithTopping(Topping.Jalapeno)      // Кастомизируем после директора
    .WithSize(PizzaSize.Large)
    .Build();
```
Альтернатива через Extension Methods:

В C# часто используют extension methods вместо отдельного класса директора:

```csharp
public static class PizzaBuilderExtensions
{
    public static PizzaBuilder DirectPepperoni(this PizzaBuilder builder)
    {
        return builder
            .WithDoughType(DoughType.Standard)
            .WithSause(Sauce.Tomato)
            .WithSize(PizzaSize.Medium)
            .WithTopping(Topping.Cheese)
            .WithTopping(Topping.Pepperoni);
    }
}
```
Использование:
```csharp
var myPizza = new PizzaBuilder()
    .DirectPepperoni()
    .WithTopping(Topping.Jalapeno)
    .WithSize(PizzaSize.Large)
    .Build();
```
Это более простой и изящный способ в C#. Директор как класс нужен, когда нужен полиморфизм (разные реализации директора).

Пример директора с ограничениями (нельзя менять тесто и не больше 5 топпингов):
```csharp
using System;
using System.Collections.Generic;

// === Доменные модели ===
public enum PizzaSize { Small, Medium, Large }
public enum DoughType { Standard, Thin, Thick }
public enum Sauce { Tomato, BBQ, White }
public enum Topping { Cheese, Pepperoni, Mushrooms, Olives, Jalapeno, Bacon, Pineapple }

public record Pizza(
    PizzaSize Size,
    DoughType DoughType,
    Sauce Sauce,
    IReadOnlyCollection<Topping> Toppings
);

// === Builder с ограничениями ===
public class PizzaBuilder
{
    private const int MaxAdditionalToppings = 5;
    
    private readonly List<Topping> _toppings = [];
    private PizzaSize _size = PizzaSize.Medium;
    private DoughType _doughType = DoughType.Standard;
    private Sauce _sauce = Sauce.Tomato;
    
    // Флаг, который блокирует изменение теста после работы директора
    private bool _doughTypeLocked = false;
    
    // Счётчик базовых топпингов (которые добавил директор)
    // Их не учитываем в лимите дополнительных топпингов
    private int _baseToppingsCount = 0;

    // === Публичные методы для клиента ===
    
    public PizzaBuilder WithTopping(Topping topping)
    {
        // Вычисляем сколько дополнительных топпингов уже добавлено
        int additionalToppingsCount = _toppings.Count - _baseToppingsCount;
        
        if (additionalToppingsCount >= MaxAdditionalToppings)
        {
            throw new InvalidOperationException(
                $"Cannot add more than {MaxAdditionalToppings} additional toppings");
        }
        
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
        // Проверяем, не заблокирован ли тип теста директором
        if (_doughTypeLocked)
        {
            throw new InvalidOperationException(
                "Cannot change dough type - it was locked by the recipe");
        }
        
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
        if (_toppings.Count == 0)
        {
            throw new InvalidOperationException("Pizza must have at least one topping");
        }

        return new Pizza(
            Size: _size,
            DoughType: _doughType,
            Sauce: _sauce,
            Toppings: _toppings.AsReadOnly()
        );
    }

    // === Внутренние методы для директора ===
    // Эти методы используются только директором и игнорируют ограничения
    
    internal PizzaBuilder SetBaseDoughType(DoughType type)
    {
        _doughType = type;
        _doughTypeLocked = true;  // Блокируем изменение теста
        return this;
    }

    internal PizzaBuilder AddBaseTopping(Topping topping)
    {
        _toppings.Add(topping);
        _baseToppingsCount++;  // Увеличиваем счётчик базовых топпингов
        return this;
    }

    internal PizzaBuilder SetBaseSauce(Sauce sauce)
    {
        _sauce = sauce;
        return this;
    }

    internal PizzaBuilder SetBaseSize(PizzaSize size)
    {
        _size = size;
        return this;
    }
}

// === Директор ===
public interface IPizzaDirector
{
    PizzaBuilder Direct(PizzaBuilder builder);
}

public class PepperoniPizzaDirector : IPizzaDirector
{
    public PizzaBuilder Direct(PizzaBuilder builder)
    {
        // Используем внутренние методы для установки базовой конфигурации
        // Эти методы не подчиняются ограничениям
        return builder
            .SetBaseDoughType(DoughType.Standard)  // Тесто блокируется!
            .SetBaseSauce(Sauce.Tomato)
            .SetBaseSize(PizzaSize.Medium)
            .AddBaseTopping(Topping.Cheese)        // Базовый топпинг
            .AddBaseTopping(Topping.Pepperoni);    // Базовый топпинг
    }
}

public class VegetarianPizzaDirector : IPizzaDirector
{
    public PizzaBuilder Direct(PizzaBuilder builder)
    {
        return builder
            .SetBaseDoughType(DoughType.Thin)
            .SetBaseSauce(Sauce.White)
            .SetBaseSize(PizzaSize.Medium)
            .AddBaseTopping(Topping.Cheese)
            .AddBaseTopping(Topping.Mushrooms)
            .AddBaseTopping(Topping.Olives);
    }
}

// === Примеры использования ===
public class Program
{
    public static void Main()
    {
        Console.WriteLine("=== Пример 1: Успешная кастомизация пепперони ===");
        var pizzaBuilder1 = new PizzaBuilder();
        var pepperoniDirector = new PepperoniPizzaDirector();

        var customPepperoni = pepperoniDirector
            .Direct(pizzaBuilder1)
            .WithTopping(Topping.Jalapeno)      // +1 доп топпинг (1/5)
            .WithTopping(Topping.Mushrooms)     // +2 доп топпинг (2/5)
            .WithSize(PizzaSize.Large)          // Размер можно менять
            .Build();

        PrintPizza(customPepperoni);

        Console.WriteLine("\n=== Пример 2: Попытка изменить тесто (ОШИБКА) ===");
        try
        {
            var pizzaBuilder2 = new PizzaBuilder();
            var failedPizza = pepperoniDirector
                .Direct(pizzaBuilder2)
                .WithDoughType(DoughType.Thick)  // Это вызовет исключение!
                .Build();
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"❌ Ошибка: {ex.Message}");
        }

        Console.WriteLine("\n=== Пример 3: Превышение лимита топпингов (ОШИБКА) ===");
        try
        {
            var pizzaBuilder3 = new PizzaBuilder();
            var overloadedPizza = pepperoniDirector
                .Direct(pizzaBuilder3)
                .WithTopping(Topping.Bacon)         // +1 (1/5)
                .WithTopping(Topping.Mushrooms)     // +2 (2/5)
                .WithTopping(Topping.Olives)        // +3 (3/5)
                .WithTopping(Topping.Jalapeno)      // +4 (4/5)
                .WithTopping(Topping.Pineapple)     // +5 (5/5)
                .WithTopping(Topping.Bacon)         // +6 - ОШИБКА!
                .Build();
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"❌ Ошибка: {ex.Message}");
        }

        Console.WriteLine("\n=== Пример 4: Максимум дополнительных топпингов ===");
        var pizzaBuilder4 = new PizzaBuilder();
        var maxPizza = pepperoniDirector
            .Direct(pizzaBuilder4)
            .WithTopping(Topping.Bacon)         // +1
            .WithTopping(Topping.Mushrooms)     // +2
            .WithTopping(Topping.Olives)        // +3
            .WithTopping(Topping.Jalapeno)      // +4
            .WithTopping(Topping.Pineapple)     // +5 - максимум!
            .WithSize(PizzaSize.Large)
            .Build();

        PrintPizza(maxPizza);

        Console.WriteLine("\n=== Пример 5: Вегетарианская пицца с базовыми 3 топпингами ===");
        var pizzaBuilder5 = new PizzaBuilder();
        var vegetarianDirector = new VegetarianPizzaDirector();
        
        var veggiePizza = vegetarianDirector
            .Direct(pizzaBuilder5)
            .WithTopping(Topping.Jalapeno)      // +1 доп топпинг
            .WithTopping(Topping.Pineapple)     // +2 доп топпинг
            .Build();

        PrintPizza(veggiePizza);
    }

    private static void PrintPizza(Pizza pizza)
    {
        Console.WriteLine($"🍕 Пицца:");
        Console.WriteLine($"   Размер: {pizza.Size}");
        Console.WriteLine($"   Тесто: {pizza.DoughType}");
        Console.WriteLine($"   Соус: {pizza.Sauce}");
        Console.WriteLine($"   Топпинги ({pizza.Toppings.Count}): {string.Join(", ", pizza.Toppings)}");
    }
}
```

#### Interface-Driven builder 

Вернёмся к простому примеру с Email. У нас есть модель:

```csharp
public record Email(
    string Address,
    string Subject,
    string Body
);
```
Требование: Address обязателен, а Subject и Body опциональны. Если написать:
```csharp
var email = new EmailBuilder()
    .WithBody("Hello!")
    .Build();  // Ошибка! Address не установлен
```

То ошибка произойдёт при Build(). Но хотелось бы поймать это на этапе компиляции.

Решение: Interface-Driven Builder.

Идея: каждый интерфейс представляет состояние builder'а, и методы возвращают следующее состояние:

```csharp
// Первый шаг: нужно установить адрес
public interface IEmailAddressBuilder
{
    IEmailBuilder WithAddress(string address);
}

// Второй шаг: можно установить Subject, Body или Build
public interface IEmailBuilder
{
    IEmailBuilder WithSubject(string subject);
    IEmailBuilder WithBody(string body);
    Email Build();
}

public static class Email
{
    public static IEmailAddressBuilder Builder => new EmailBuilder();

    private class EmailBuilder : IEmailAddressBuilder, IEmailBuilder
    {
        private string? _address;
        private string _subject = string.Empty;
        private string _body = string.Empty;

        // Первый метод: только один способ начать — установить адрес
        public IEmailBuilder WithAddress(string address)
        {
            _address = address;
            return this;  // Возвращаем IEmailBuilder, а не IEmailAddressBuilder
        }

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
            if (_address is null)
                throw new ArgumentNullException(nameof(_address));

            return new Email(
                Address: _address,
                Subject: _subject,
                Body: _body
            );
        }
    }
}
```
Использование:
```csharp
// Это не скомпилируется! WithBody() возвращает IEmailBuilder, но нет метода Build() без WithAddress()
// var email = Email.Builder
//     .WithBody("Hello!")
//     .Build();  // Ошибка компилятора!

// Только так работает:
var email = Email.Builder
    .WithAddress("user@example.com")
    .WithBody("Hello!")
    .Build();  // OK!

// И порядок методов не важен, важен только первый вызов:
var email2 = Email.Builder
    .WithAddress("user@example.com")
    .WithSubject("Greeting")
    .WithBody("Hi there!")
    .Build();
```
Архитектурный смысл:
- Типобезопасность: компилятор гарантирует, что обязательные поля установлены
- Self-documenting code: из сигнатуры интерфейса видно, какие методы доступны на каждом шаге
- Fail-fast на уровне компиляции, а не времени выполнения

Когда использовать: когда у вас есть строгие требования к порядку или обязательности установки полей. В простых случаях это overkill.

Важное замечание: Когда НЕ смешивать типы Builder'ов
- смешивать типы builder’ов можно
- НО! необходимость смешения скорее всего свидетельствует о необходимости декомпозиции модели
- стоит помнить что реализация builder’а должна зависеть от модели, а не наоборот

Например:
```csharp
// Плохо: один заказ собирается двумя builder'ами
var builder1 = new OrderBuilder();
var builder2 = new LimitedOrderBuilder();

builder1.WithItem(...);
builder2.WithItem(...);

// Как теперь собрать заказ? Который из них использовать?
```
Если вам нужны оба типа builder'ов, это часто означает, что модель слишком сложная и её нужно разложить:
```csharp
// Лучше: разные заказы — разные модели
public record RegularOrder(IEnumerable<OrderItem> Items);
public record PremiumOrder(IEnumerable<OrderItem> Items, string VipStatus);

// У каждого свой builder
public class RegularOrderBuilder { }
public class PremiumOrderBuilder { }
```
**Золотое правило: реализация builder'а зависит от модели, а не наоборот.**
Практический смысл и применение
Где Builder используется в реальном коде?

1. HTTP запросы
```csharp
var request = new HttpRequestBuilder()
    .WithUrl("https://api.example.com/orders")
    .WithMethod(HttpMethod.Post)
    .WithHeader("Authorization", "Bearer token")
    .WithBody(jsonPayload)
    .Build();
```
2. SQL queries
```csharp
var query = new SqlQueryBuilder()
    .Select("id", "name", "email")
    .From("users")
    .Where("age > 18")
    .OrderBy("name")
    .Build();
```
3. UI/Configuration
```csharp
var form = new FormBuilder()
    .AddField(new TextField("name", required: true))
    .AddField(new EmailField("email"))
    .AddButton("Submit")
    .Build();
```

Философия паттерна и уроки проектирования
Что нас учит Builder?
1. Разделение ответственности
Builder отделяет логику создания от самого объекта. Model остаётся чистой, а вся сложность находится в builder'е.

2. Гибкость без сложности
Вместо множества перегруженных конструкторов (конструктор hell) мы получаем чистый, читаемый код.

3. Валидация в нужном месте
Не в конструкторе модели, не в какой-то службе валидации, а в builder'е, где собираются данные.

4. Fluent Interface
Код читается как предложение: .WithItem(...).WithComment(...).Build() — это естественно.

5. Инверсия зависимостей
Высокоуровневый код зависит от интерфейса builder'а, а не от конкретной реализации.

6. Типобезопасность
Interface-driven подход позволяет компилятору гарантировать корректность.

Давайте соберём всё вместе в реалистичный пример:
```csharp
using System;
using System.Collections.Generic;
using System.Linq;

// === Доменные модели ===
public record OrderItem(decimal Price, int Amount);

public record Order(
    string CommentForShop,
    string CommentForDelivery,
    IReadOnlyCollection<OrderItem> Items,
    DateTimeOffset CreatedAt,
    string? ReceiverPhoneNumber
)
{
    public decimal TotalPrice => Items.Sum(i => i.Price * i.Amount);
}

// === Builder с валидацией ===
public class OrderBuilder
{
    private const int MaxOrderItemCount = 20;
    private readonly List<OrderItem> _items = [];
    private string _commentForShop = string.Empty;
    private string _commentForDelivery = string.Empty;
    private DateTimeOffset _createdAt = DateTimeOffset.UtcNow;
    private string? _receiverPhoneNumber = null;

    public OrderBuilder WithItem(OrderItem item)
    {
        if (_items.Count >= MaxOrderItemCount)
            throw new ArgumentException(
                $"Cannot add more than {MaxOrderItemCount} items to an order");
        
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

    public Order Build()
    {
        if (_items.Count == 0)
            throw new InvalidOperationException("Order must contain at least one item");

        return new Order(
            CommentForShop: _commentForShop,
            CommentForDelivery: _commentForDelivery,
            Items: _items.AsReadOnly(),
            CreatedAt: _createdAt,
            ReceiverPhoneNumber: _receiverPhoneNumber
        );
    }
}

// === Пример использования в main ===
public class Program
{
    public static void Main()
    {
        Console.WriteLine("=== Пример 1: Простой заказ ===");
        var simpleOrder = new OrderBuilder()
            .WithItem(new OrderItem(Price: 100, Amount: 2))
            .WithItem(new OrderItem(Price: 50, Amount: 1))
            .Build();

        PrintOrder(simpleOrder);

        Console.WriteLine("\n=== Пример 2: Заказ с комментариями ===");
        var complexOrder = new OrderBuilder()
            .WithItem(new OrderItem(Price: 1337, Amount: 2))
            .WithCommentForShop("Упаковать аккуратно!")
            .WithCommentForDelivery("Оставить у двери")
            .WithReceiverPhoneNumber("+7-900-123-45-67")
            .Build();

        PrintOrder(complexOrder);

        Console.WriteLine("\n=== Пример 3: Пошаговое построение ===");
        var builder = new OrderBuilder();
        
        // Добавляем базовые товары
        AddDefaultItems(builder);
        
        // Добавляем товары по запросу
        AddRequestedItems(builder);
        
        // Добавляем прогнозируемые товары
        AddForecastedItems(builder);
        
        var finalOrder = builder.Build();
        PrintOrder(finalOrder);

        Console.WriteLine("\n=== Пример 4: Обработка ошибок ===");
        try
        {
            var badOrder = new OrderBuilder()
                .Build();  // Ошибка: нет товаров
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"Ошибка: {ex.Message}");
        }

        Console.WriteLine("\n=== Пример 5: Превышение лимита ===");
        try
        {
            var tooManyItems = new OrderBuilder();
            for (int i = 0; i < 21; i++)
            {
                tooManyItems.WithItem(new OrderItem(Price: 10, Amount: 1));
            }
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Ошибка: {ex.Message}");
        }
    }

    private static void AddDefaultItems(OrderBuilder builder)
    {
        builder.WithItem(new OrderItem(Price: 100, Amount: 1));
    }

    private static void AddRequestedItems(OrderBuilder builder)
    {
        builder
            .WithItem(new OrderItem(Price: 200, Amount: 2))
            .WithItem(new OrderItem(Price: 150, Amount: 1));
    }

    private static void AddForecastedItems(OrderBuilder builder)
    {
        builder.WithItem(new OrderItem(Price: 75, Amount: 3));
    }

    private static void PrintOrder(Order order)
    {
        Console.WriteLine($"Заказ создан: {order.CreatedAt:dd.MM.yyyy HH:mm:ss}");
        Console.WriteLine($"Товаров в заказе: {order.Items.Count}");
        Console.WriteLine($"Сумма заказа: {order.TotalPrice}₽");
        Console.WriteLine($"Комментарий магазину: {order.CommentForShop}");
        Console.WriteLine($"Комментарий доставке: {order.CommentForDelivery}");
        Console.WriteLine($"Телефон получателя: {order.ReceiverPhoneNumber ?? "не указан"}");
    }
}
/* Вывод
=== Пример 1: Простой заказ ===
Заказ создан: 31.10.2025 15:33:45
Товаров в заказе: 2
Сумма заказа: 250₽
Комментарий магазину: 
Комментарий доставке: 
Телефон получателя: не указан

=== Пример 2: Заказ с комментариями ===
Заказ создан: 31.10.2025 15:33:45
Товаров в заказе: 1
Сумма заказа: 2674₽
Комментарий магазину: Упаковать аккуратно!
Комментарий доставке: Оставить у двери
Телефон получателя: +7-900-123-45-67

=== Пример 3: Пошаговое построение ===
Заказ создан: 31.10.2025 15:33:45
Товаров в заказе: 6
Сумма заказа: 875₽
Комментарий магазину: 
Комментарий доставке: 
Телефон получателя: не указан

=== Пример 4: Обработка ошибок ===
Ошибка: Order must contain at least one item

=== Пример 5: Превышение лимита ===
Ошибка: Cannot add more than 20 items to an order
*/
```

Применимость:
1. **Когда вы хотите избавиться от «телескопического конструктора».**

 Допустим, у вас есть один конструктор с десятью опциональными параметрами. Его неудобно вызывать, поэтому вы создали ещё десять конструкторов с меньшим количеством параметров. Всё, что они делают — это переадресуют вызов к базовому конструктору, подавая какие-то значения по умолчанию в параметры, которые пропущены в них самих.

```csharp
class Pizza {
    Pizza(int size) { ... }
    Pizza(int size, boolean cheese) { ... }
    Pizza(int size, boolean cheese, boolean pepperoni) { ... }
    // ...
```

Такого монстра можно создать только в языках, имеющих механизм перегрузки методов, например, C# или Java.

Паттерн Строитель позволяет собирать объекты пошагово, вызывая только те шаги, которые вам нужны. А значит, больше не нужно пытаться «запихнуть» в конструктор все возможные опции продукта.

2. **Когда ваш код должен создавать разные представления какого-то объекта. Например, деревянные и железобетонные дома.**

Строитель можно применить, если создание нескольких представлений объекта состоит из одинаковых этапов, которые отличаются в деталях.

Интерфейс строителей определит все возможные этапы конструирования. Каждому представлению будет соответствовать собственный класс-строитель. А порядок этапов строительства будет задавать класс-директор.

3. **Когда вам нужно собирать сложные составные объекты, например, деревья Компоновщика.**

Строитель конструирует объекты пошагово, а не за один проход. Более того, шаги строительства можно выполнять рекурсивно. А без этого не построить древовидную структуру, вроде Компоновщика.

Заметьте, что Строитель не позволяет посторонним объектам иметь доступ к конструируемому объекту, пока тот не будет полностью готов. Это предохраняет клиентский код от получения незаконченных «битых» объектов.
