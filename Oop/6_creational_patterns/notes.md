# Порождающие паттерны

[Фабричный метод](#фабричный-метод)
[(Абстрактная) фабрика](#абстрактная-фабрика)

В ходе разработки возникают классы, объекты которых создаются уж слишком тяжело и громоздко. Для этих случаев разрабатывают другие методы/объекты, за которыми лежит ответственность за их созданием

Порождающие паттерны отвечают за удобное и безопасное создание новых объектов или даже целых семейств объектов.

### Фабричный метод

> вариативность создания объектов при помощи наследования и полиморфизма

Фабричный метод — это порождающий паттерн проектирования, который определяет общий интерфейс для создания объектов в суперклассе, позволяя подклассам изменять тип создаваемых объектов.

Представьте, что вы создаёте программу управления грузовыми перевозками. Сперва вы рассчитываете перевозить товары только на автомобилях. Поэтому весь ваш код работает с объектами класса `Грузовик`.

В какой-то момент ваша программа становится настолько известной, что морские перевозчики выстраиваются в очередь и просят добавить поддержку морской логистики в программу. Отличные новости, правда? Но как насчёт кода? Большая часть существующего кода жёстко привязана к классам `Грузовиков`

![](src/factory_method/factory_method_proble.png)

Чтобы добавить в программу классы морских `Судов`, понадобится перелопатить всю программу. Более того, если вы потом решите добавить в программу ещё один вид транспорта, то всю эту работу придётся повторить.

В итоге вы получите ужасающий код, наполненный условными операторами, которые выполняют то или иное действие, в зависимости от класса транспорта.

Решить эту проблему можно при помощи фабричного метода:

Паттерн Фабричный метод предлагает создавать объекты не напрямую, используя оператор new, а косвенно через вызов особого фабричного метода.

![](src/factory_method/factory_method_logistics.png)

На первый взгляд, это может показаться бессмысленным: мы просто переместили вызов конструктора из одного конца программы в другой.

Чтобы эта система заработала, все возвращаемые объекты должны иметь общий интерфейс. Подклассы смогут производить объекты различных классов, следующих одному и тому же интерфейсу.

![](src/factory_method/factory_method_interface.png)

Теперь вы сможете переопределить фабричный метод в подклассе, чтобы изменить тип создаваемого продукта.

Например, классы `Truck` и `Ship` реализуют интерфейс `Transport` с методом доставить. Каждый из этих классов реализует метод по-своему: грузовики везут грузы по земле, а суда — по морю. Фабричный метод в классе `RoadLogistics` вернёт объект-грузовик, а класс `SeaLogistics` — объект-судно

![](src/factory_method/factory_method_result.png)

Для клиента фабричного метода нет разницы между этими объектами, так как он будет трактовать их как некий абстрактный `Transport`. Для него будет важно, чтобы объект имел метод `Deliver`, а как конкретно он работает — не важно.

![](src/factory_method/factory_method_logisitics_scheme.png)

1. Product определяет общий интерфейс объектов, которые может произвести создатель и его подклассы.
2. `Concrete Products` содержат код различных продуктов. Продукты будут отличаться реализацией, но интерфейс у них будет общий.
3. `Creator` объявляет фабричный метод, который должен возвращать новые объекты продуктов. Важно, чтобы тип результата совпадал с общим интерфейсом `Product`.
Зачастую фабричный метод объявляют абстрактным, чтобы заставить все подклассы реализовать его по-своему. Но он может возвращать и некий стандартный продукт.
Несмотря на название, важно понимать, что создание продуктов не является единственной функцией создателя. Обычно он содержит и другой полезный код работы с продуктом. Аналогия: большая софтверная компания может иметь центр подготовки программистов, но основная задача компании — создавать программные продукты, а не готовить программистов.
4. `Concrete Creators` по-своему реализуют фабричный метод, производя те или иные конкретные продукты.
Фабричный метод не обязан всё время создавать новые объекты. Его можно переписать так, чтобы возвращать существующие объекты из какого-то хранилища или кэша.

Напишем всевдокод для какой-нибудь абстрактного примера:

![](src/factory_method/factory_method_pseudo_example.png)

В этом примере Фабричный метод помогает создавать кросс-платформенные элементы интерфейса, не привязывая основной код программы к конкретным классам элементов.

Фабричный метод объявлен в классе диалогов. Его подклассы относятся к различным операционным системам. Благодаря фабричному методу, вам не нужно переписывать логику диалогов под каждую систему. Подклассы могут наследовать почти весь код из базового диалога, изменяя типы кнопок и других элементов, из которых базовый код строит окна графического пользовательского интерфейса.

Базовый класс диалогов работает с кнопками через их общий программный интерфейс. Поэтому, какую вариацию кнопок ни вернул бы фабричный метод, диалог останется рабочим. Базовый класс не зависит от конкретных классов кнопок, оставляя подклассам решение о том, какой тип кнопок создавать.

Такой подход можно применить и для создания других элементов интерфейса (но чем больше таких элементов, тем ближе это будет уже к другому паттерну под названием "абстрактной фабрика")

```csharp
// Интерфейс продукта
public interface IButton
{
    void Render();
    void OnClick(Action handler);
}

// Конкретный продукт: кнопка Windows
public class WindowsButton : IButton
{
    public void Render()
    {
        Console.WriteLine("Отрисовать кнопку в стиле Windows.");
    }

    public void OnClick(Action handler)
    {
        Console.WriteLine("Навешан обработчик событий Windows.");
        handler?.Invoke();
    }
}

// Конкретный продукт: кнопка HTML
public class HtmlButton : IButton
{
    public void Render()
    {
        Console.WriteLine("Вернуть HTML-код кнопки.");
    }

    public void OnClick(Action handler)
    {
        Console.WriteLine("Навешан обработчик событий браузера.");
        handler?.Invoke();
    }
}

// Абстрактная фабрика
public abstract class Dialog
{
    public void Render()
    {
        // Используем фабричный метод для создания кнопки
        IButton okButton = CreateButton();
        okButton.OnClick(CloseDialog);
        okButton.Render();
    }

    // Фабричный метод
    protected abstract IButton CreateButton();

    protected void CloseDialog()
    {
        Console.WriteLine("Закрытие диалога...");
    }
}

// Конкретная фабрика для Windows
public class WindowsDialog : Dialog
{
    protected override IButton CreateButton()
    {
        return new WindowsButton();
    }
}

// Конкретная фабрика для Web
public class WebDialog : Dialog
{
    protected override IButton CreateButton()
    {
        return new HtmlButton();
    }
}

// Пример конфигурации
public class AppConfig
{
    public string OS { get; set; }
}

// Приложение
public class Application
{
    private Dialog _dialog;

    public void Initialize()
    {
        // Здесь можно читать настройки из файла или окружения
        var config = new AppConfig { OS = "Windows" }; // пример

        if (config.OS == "Windows")
            _dialog = new WindowsDialog();
        else if (config.OS == "Web")
            _dialog = new WebDialog();
        else
            throw new Exception("Error! Unknown operating system.");
    }

    public void Main()
    {
        Initialize();
        _dialog.Render();
    }
}

// Точка входа
public static class Program
{
    public static void Main()
    {
        var app = new Application();
        app.Main();
    }
}
```

Применимость: 

- **Когда заранее неизвестны типы и зависимости объектов, с которыми должен работать ваш код**
Фабричный метод отделяет код производства продуктов от остального кода, который эти продукты использует.
Благодаря этому, код производства можно расширять, не трогая основной. Так, чтобы добавить поддержку нового продукта, вам нужно создать новый подкласс и определить в нём фабричный метод, возвращая оттуда экземпляр нового продукта.
- **Когда вы хотите дать возможность пользователям расширять части вашего фреймворка или библиотеки**.
Пользователи могут расширять классы вашего фреймворка через наследование. Но как сделать так, чтобы фреймворк создавал объекты из этих новых классов, а не из стандартных?
Решением будет дать пользователям возможность расширять не только желаемые компоненты, но и классы, которые создают эти компоненты. А для этого создающие классы должны иметь конкретные создающие методы, которые можно определить.
Например, вы используете готовый UI-фреймворк для своего приложения. Но вот беда — требуется иметь круглые кнопки, вместо стандартных прямоугольных. Вы создаёте класс `RoundButton`. Но как сказать главному классу фреймворка `UIFramework`, чтобы он теперь создавал круглые кнопки, вместо стандартных?
Для этого вы создаёте подкласс `UIWithRoundButtons` из базового класса фреймворка, переопределяете в нём метод создания кнопки (а-ля createButton) и вписываете туда создание своего класса кнопок. Затем используете `UIWithRoundButtons` вместо стандартного `UIFramework`.
- **Когда вы хотите экономить системные ресурсы, повторно используя уже созданные объекты, вместо порождения новых**.
Такая проблема обычно возникает при работе с тяжёлыми ресурсоёмкими объектами, такими, как подключение к базе данных, файловой системе и т. д.
Представьте, сколько действий вам нужно совершить, чтобы повторно использовать существующие объекты:
Сначала вам следует создать общее хранилище, чтобы хранить в нём все создаваемые объекты. При запросе нового объекта нужно будет заглянуть в хранилище и проверить, есть ли там неиспользуемый объект, а затем вернуть его клиентскому коду, но если свободных объектов нет — создать новый, не забыв добавить его в хранилище.
Весь этот код нужно куда-то поместить, чтобы не засорять клиентский код.
Самым удобным местом был бы конструктор объекта, ведь все эти проверки нужны только при создании объектов. Но, увы, конструктор всегда создаёт новые объекты, он не может вернуть существующий экземпляр.
Значит, нужен другой метод, который бы отдавал как существующие, так и новые объекты. Им и станет фабричный метод.

Шаги реализации:
1. Приведите все создаваемые продукты к общему интерфейсу.
2. В классе, который производит продукты, создайте пустой фабричный метод. В качестве возвращаемого типа укажите общий интерфейс продукта.
3. Затем пройдитесь по коду класса и найдите все участки, создающие продукты. Поочерёдно замените эти участки вызовами фабричного метода, перенося в него код создания различных продуктов.
В фабричный метод, возможно, придётся добавить несколько параметров, контролирующих, какой из продуктов нужно создать.
На этом этапе фабричный метод, скорее всего, будет выглядеть удручающе. В нём будет жить большой условный оператор, выбирающий класс создаваемого продукта. Но не волнуйтесь, мы вот-вот исправим это.
4. Для каждого типа продуктов заведите подкласс и переопределите в нём фабричный метод. Переместите туда код создания соответствующего продукта из суперкласса.
5. Если создаваемых продуктов слишком много для существующих подклассов создателя, вы можете подумать о введении параметров в фабричный метод, которые позволят возвращать различные продукты в пределах одного подкласса.
Например, у вас есть класс `Mail` с подклассами `AviaMail` и `GroundMail`, а также классы продуктов `Plane`, `Bus` и `Train`. `AviaMail` соответствует `Plane`, но для `GroundMail` есть сразу два продукта. Вы могли бы создать новый подкласс почты для поездов, но проблему можно решить и по-другому. Клиентский код может передавать в фабричный метод `GroundMail` аргумент, контролирующий тип создаваемого продукта.
6. Если после всех перемещений фабричный метод стал пустым, можете сделать его абстрактным. Если в нём что-то осталось — не беда, это будет его реализацией по умолчанию.

Структура паттена:
- creator -тип, в котором содержится логика, в рамках которой создаются объекты наследники реализуют логику создания объектов
- product тип, создаваемых объектов наследники создаются в конкретных creator’ах
![](src/factory_method/factory_method_scheme.png)

Давайте рассмотрим ещё один пример: представьте, что мы пишем систему обработки заказов
```csharp
public record OrderItem(decimal Price, int Amount)
{
    public decimal Cost => Price * Amount;
}

public record Order(IEnumerable<OrderItem> Items)
{
    public decimal TotalCost => Items.Sum(x => x.Cost);
}

public record CashPayment(decimal Amount);

public class PaymentCalculator
{
    public CashPayment Calculate(Order order)
    {
        var totalCost = order.TotalCost;

        // Apply discounts and coupons
        ...

        return new CashPayment(totalCost);
    }
}
```
Здесь у нас есть класс `PaymentCalculator`. Его задача — получить заказ `Order`, посчитать итоговую стоимость `totalCost`, применить какие-то скидки (бизнес-логика) и вернуть платёж: `return new CashPayment(totalCost);.`

В чём здесь проблема? Наш `PaymentCalculator` привязан к конкретному типу оплаты — `CashPayment`. По факту мы применяем какие-то абстрактные купоны и скидки, а потом всё равно возвращаем оплату наличными `CashPayment` 

А что, если завтра бизнес скажет: "Мы хотим добавить оплату банковским переводом"? Или криптовалютой? Нам придётся:
- Либо лезть внутрь PaymentCalculator и дописывать if/switch (что нарушает принцип Открытости/Закрытости).
- Либо копировать всю логику Calculate в новый класс, скажем, BankPaymentCalculator, и менять там только одну строку — return new BankPayment(...). Это приведёт к дублированию кода.

Проблема в том, что логика расчёта (что мы делаем) и логика создания (какой объект мы в итоге получаем) смешаны в одном месте. Фабричный метод нужен, чтобы эту связь разорвать.

Паттерн вводит два ключевых понятия: product и creator
- `Product` — это абстрактный тип объектов, которые мы хотим создавать. В нашем примере это будет не `CashPayment`, а некий общий `IPayment`. Наследники этого типа (`CashPayment`, `BankPayment`, `CryptoPayment`) — это продукты под каждую нужную нам ситуацию (где-то будет информация об аккаунте плательщика, где-то будет номер крипто-кошелька и тд. А в наличке нам вообще ничего знать не нужно - да и невозможно).
- `Creator` — это абстрактный тип, в котором содержится основная логика, в рамках которой требуется создание `Product`. Например, `PaymentCalculator`. Он будет содержать метод `Calculate`.

Ключевая идея в том, что `Creator` не знает, какой экземпляр `Product` он создаёт. Он просто говорит: "Мне нужен продукт".
А вот наследники creator'а (конкретные создатели) — они-то и будут реализовывать логику создания конкретных продуктов.
То есть `Creator` делегирует создание product'а своим наследникам. Метод, который он делегирует, и называется Фабричный метод

Давайте посмотрим, как наш код преображается с применением этого паттерна:

```csharp
public interface IPayment
{
    decimal Amount { get; }
}

public record CashPayment(
    decimal Amount) : IPayment;

public record BankPayment(
    decimal Amount,
    string ReceiverAccountId) : IPayment;
```

Мы ввели интерфейс `IPayment`. Это наш абстрактный product. Теперь `CashPayment` и новый `BankPayment` реализуют этот интерфейс. Это даёт нам полиморфизм: мы можем работать с любым типом оплаты через общий интерфейс `IPayment`.

Теперь введём создателя:
```csharp
public abstract class PaymentCalculator
{
    public IPayment Calculate(Order order)
    {
        var totalCost = order.TotalCost;

        // Apply discounts and coupons
        ...

        return CreatePayment(totalCost);
    }

    protected abstract IPayment CreatePayment(decimal amount);
}
```
Теперь у нас осталась вся логика вычисления скидок и применения купонов (бизнес-логика), однако Calculate теперь возвращает общий интерфейс `IPayment`.
Самая главное, что у нас появился абстрактный метод `CreatePayment`, который классы-наследники вынуждены переопределить.
Сам `PaymentCalculator` не знает, как его реализовать. Он заставляет своих наследников предоставить эту реализацию. Он protected — потому что это "внутренняя кухня" класса. `Calculate` использует его, но снаружи никому не нужно знать о том, как создаётся платёж.

Теперь посмотрим на то, как реализовать наших создателей под конкретные типы:
```csharp
public class CashPaymentCalculator : PaymentCalculator
{
    protected override IPayment CreatePayment(decimal amount) 
        => new CashPayment(amount);
}

public class BankPaymentCalculator : PaymentCalculator
{
    private readonly string _currentReceiverAccountId;

    public BankPaymentCalculator(string currentReceiverAccountId)
    {
        _currentReceiverAccountId = currentReceiverAccountId;
    }

    protected override IPayment CreatePayment(decimal amount)
    {
        return new BankPayment(amount, _currentReceiverAccountId);
    }
}
```

Обратите внимание на `BankPaymentCalculator`. Ему для создания `BankPayment` нужен `_currentReceiverAccountId`. И это не проблема! Конкретный создатель может иметь собственное состояние и конструкторы, которые нужны именно ему для создания его конкретного продукта. Абстрактный `PaymentCalculator` об этом даже не знает.

Итак, как это применять? Соберём всё вместе:

1. Клиентский код теперь сам решает, какой тип калькулятора ему нужен.
```csharp
PaymentCalculator calculator = new CashPaymentCalculator();

// Или: PaymentCalculator calculator = new BankPaymentCalculator("123456");
```
2. Клиент вызывает один и тот же метод: IPayment payment = calculator.Calculate(myOrder);.
3. Выполняется общая логика из PaymentCalculator.Calculate.
4. Когда Calculate доходит до строки return CreatePayment(totalCost);, благодаря полиморфизму вызывается override-версия метода того класса, который мы создали в пункте 1.
5. Если calculator — это CashPaymentCalculator, вызовется его CreatePayment и вернётся CashPayment. Если calculator — это BankPaymentCalculator, вызовется его CreatePayment и вернётся BankPayment.
6. Creator (PaymentCalculator) отвечает за "когда" создавать объект (в конце расчёта). Конкретные creator'ы (CashPaymentCalculator, BankPaymentCalculator) отвечают за "что" создавать. Логика создания инкапсулирована внутри конкретных создателей.

Схема использования:
![](src/factory_method/factory_method_kruglov_sheme.png)

Самое главное, что мы получили — гибкость и расширяемость.

Завтра нам понадобится CryptoPayment. Что мы делаем?

1. Создаём record CryptoPayment(...) : IPayment (новый product).
2. Создаём class CryptoPaymentCalculator : PaymentCalculator.
3. Реализуем в нём protected override IPayment CreatePayment(...) => new CryptoPayment(...).

Мы не тронули ни одной строки существующего кода (PaymentCalculator, CashPaymentCalculator и т.д.). Мы только добавили новый. Система расширяется, оставаясь "закрытой" для модификаций. Это принцип Открытости/Закрытости (Open/Closed Principle) в чистом виде.

Однако у этого паттерна есть и недостатки:

**1.) Фабричный метод может привести к созданию больших параллельных иерархий классов, так как для каждого класса продукта надо создать свой подкласс создателя.**

**2.) сильная связанность конкретных создателей с базовым типом: переиспользуется логика базового типа, но не логика конкретных создателей**

Пример: CashPaymentCalculator связан с PaymentCalculator через наследование. Он обязан быть его наследником.

Представьте, что у нас появляется другая иерархия калькуляторов, например, FixedPricePaymentCalculator, у которого другая логика Calculate (например, он всегда возвращает 100). То есть он не применяет скидки и купоны и не делает никакой другой бизнес-логики, в отличии от обычного PaymentCalculator.
Если мы захотим, чтобы он тоже создавал CashPayment, нам придётся создать FixedCashPaymentCalculator : FixedPricePaymentCalculator и заново написать в нём override CreatePayment ... => new CashPayment().

Мы не можем переиспользовать логику создания из CashPaymentCalculator, потому что она используется в другой иерархии. Логика базового типа (Calculate) переиспользуется наследниками, а вот логика конкретных создателей (CreatePayment) — нет.

**3.) неявное нарушение SRP: объект конкретного создателя ответственен как за реализацию логики,
так и за создание продуктов**

creator (например, BankPaymentCalculator) становится ответственен и за реализацию логики (он наследует Calculate), и за создание продуктов (он реализует CreatePayment).

Это две разные "причины для изменения".
1. Если изменится общая логика расчёта (в базовом PaymentCalculator), это коснётся всех наследников.
2. Если изменится способ создания BankPayment (например, ему понадобится новый параметр), нам придётся менять BankPaymentCalculator.

Класс BankPaymentCalculator и "калькулятор", и "фабрика" одновременно. Это может усложнить поддержку, если и логика, и создание становятся слишком сложными.


Вывод:
Фабричный метод — это классический паттерн, который решает проблему связи с оператором new путём делегирования создания наследникам через абстрактный метод. Он идеально подходит, когда у вас есть общая логика, но в конце этой логики нужно создавать разные объекты, тип которых определяется подклассом.

### (Абстрактная) фабрика

> вариативность создания объектов при помощи композиции и полиморфизма

Вообще фабричный метод применяется достаточно редко. Куда чаще применеятся абстрактная фабрика, или же просто - фабрика. Давайте разбираться что это.

Какая проблема есть у фабричного метода?
Вспомним код: 
```csharp
public abstract class Dialog
{
    public void Render()
    {
        // Используем фабричный метод для создания кнопки
        IButton okButton = CreateButton();

        okButton.Render();
    }

    // Фабричный метод
    protected abstract IButton CreateButton();
}
```
А что если помимо кнопки мы захоти добавить ещё, например, текстовое поле ITextBox?

```csharp
public abstract class Dialog
{
    public void Render()
    {
        // Нам нужно ДВА продукта, и они должны быть из ОДНОЙ СЕМЬИ
        // (оба должны быть 'Windows' или оба 'Html')
        IButton okButton = CreateButton();
        ITextBox nameBox = CreateTextBox(); // <-- Вторая точка создания
        
        okButton.Render();
        nameBox.Render();
    }

    // Наш Creator "распухает" от фабричных методов
    protected abstract IButton CreateButton();
    protected abstract ITextBox CreateTextBox(); // <-- Добавили второй метод
}
```
А если захотим добавить третий?
```csharp
public abstract class Dialog
{
    public void Render()
    {
        IButton okButton = CreateButton();
        ITextBox nameBox = CreateTextBox(); 
        IScrollBar vScroll = CreateScrollBar(); // третий...
        
        okButton.Render();
        nameBox.Render();
    }

    // Уже целых три абстрактных метода
    protected abstract IButton CreateButton();
    protected abstract ITextBox CreateTextBox(); 
    protected abstract IScrollBar CreateScrollBar(); 
}
```
И так далее.

После таких изменений в каждом Creator'е нам нужно будет добавить реализацию этих методов. То есть пробежаться во всем таким классам и вручную из изменить. Очевидно, что если таких классов будет достаточно много, то сделать это будет крайне тяжело.

То есть фабричный метод не работает, когда нам нужно создать не один объект, а семейство каких-то связанных между собой объектов.

Решить эту проблему можно через композицию. Создадим фабрику, которая будет выполнять все эти методы.

```csharp
// Это "Абстрактная Фабрика"
// Она описывает СЕМЕЙСТВО продуктов
public interface IUIFactory
{
    IButton CreateButton();
    ITextBox CreateTextBox();
    IScrollBar CreateScrollBar();
}

// Фабрика 1
public class WindowsFactory : IUIFactory
{
    public IButton CreateButton() => new WindowsButton();
    public ITextBox CreateTextBox() => new WindowsTextBox();
    public IScrollBar CreateScrollBar() => new WindowsScrollBar();
}

// Фабрика 2
public class WebFactory : IUIFactory
{
    public IButton CreateButton() => new HtmlButton();
    public ITextBox CreateTextBox() => new HtmlTextBox();
    public IScrollBar CreateScrollBar() => new HtmlScrollBar();
}

// Теперь уже не абстрактный
public class Dialog
{
    // 1. (Композиция)
    private readonly IUIFactory _factory;

    public Dialog(IUIFactory factory)
    {
        _factory = factory;
    }

    // 3. Логика Render() не изменилась
    public void Render()
    {
        // но теперь она ДЕЛЕГИРУЕТ создание фабрике
        IButton okButton = _factory.CreateButton();
        ITextBox nameBox = _factory.CreateTextBox();
        IScrollBar scrollBar = _factory.CreateScrollBar();
        
        okButton.Render();
        nameBox.Render();
        scrollBar.Render();
    }
}
```

Рассмотрим ещё несколько примеров:
Создаём продукты:
```csharp
// Это общий интерфейс продуктов
public interface IPayment
{
    decimal Amount { get; }
}

// Это продукт 1
public record BankPayment(
    decimal Amount,
    string ReceiverAccountId) : IPayment;

// Это продукт 2
public record CashPayment(
    decimal Amount) : IPayment;
```
Теперь мы определяем контракт для создателя:
```csharp
public interface IPaymentFactory
{
    IPayment Create(decimal amount);
}
```
Теперь мы пишем классы, которые реализуют этот интерфейс. Каждая фабрика будет "заточена" под создание своего конкретного продукта.
```csharp
// "Конкретная фабрика 1"
public class BankPaymentFactory : IPaymentFactory
{
    private readonly string _currentReceiverAccountId;

    // Фабрика может иметь свое состояние!
    public BankPaymentFactory(string currentReceiverAccountId)
    {
        _currentReceiverAccountId = currentReceiverAccountId;
    }

    public IPayment Create(decimal amount)
    {
        // Эта фабрика знает, как создать BankPayment.
        // Она инкапсулирует эту логику.
        return new BankPayment(amount, _currentReceiverAccountId);
    }
}

// "Конкретная фабрика 2"
public class CashPaymentFactory : IPaymentFactory
{
    public IPayment Create(decimal amount)
    {
        // Эта фабрика знает, как создать CashPayment.
        return new CashPayment(amount);
    }
}
```

Теперь посмотрим на то, как это можно применить:
```csharp
// интерфейс для калькулятора
public interface IPaymentCalculator
{
    IPayment Calculate(Order order);
}

public class PaymentCalculator : IPaymentCalculator
{
    private readonly IPaymentFactory _paymentFactory;

    public PaymentCalculator(IPaymentFactory paymentFactory)
    {
        _paymentFactory = paymentFactory;
    }

    public IPayment Calculate(Order order)
    {
        var totalCost = order.TotalCost;
        
        // Apply discounts and coupons 

        // ...

        return _paymentFactory.Create(totalCost);
    }
}

// Другой калькулятор
public class FixedPaymentCalculator : IPaymentCalculator
{
    private readonly decimal _fixedPrice;
    private readonly IPaymentFactory _paymentFactory;

    public FixedPaymentCalculator(decimal fixedPrice, IPaymentFactory paymentFactory)
    {
        _fixedPrice = fixedPrice;
        _paymentFactory = paymentFactory;
    }
    public IPayment Calculate(Order order)
    {
        var totalCost = order.Items.Sum(item =>_fixedPrice * item.Amount);

        // Apply discounts and coupons

        // ...

        return _paymentFactory.Create(totalCost);
    }
}
```
Мы можем легко добавить новую фабрику:
```csharp
public record CryptoPayment(decimal Amount, string Wallet) : IPayment;

public class CryptoPaymentFactory : IPaymentFactory
{
    private readonly string _companyWallet;
    public CryptoPaymentFactory(string companyWallet) { _companyWallet = companyWallet; }

    public IPayment Create(decimal amount)
    {
        return new CryptoPayment(amount, _companyWallet);
    }
}
```

Пример использования в коде:
```csharp
public static class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("--- Сценарий 1: Банк ---");

        // 1. Создаем конкретную фабрику
        IPaymentFactory bankFactory = new BankPaymentFactory("UA1234567890");

        // 2. Внедряем фабрику в калькулятор
        IPaymentCalculator bankCalculator = new PaymentCalculator(bankFactory);

        // 3. Используем калькулятор
        Order order1 = new Order { TotalCost = 250.50m };
        IPayment payment1 = bankCalculator.Calculate(order1);
        
        // 4. Проверяем результат
        Console.WriteLine($"Создан платеж типа: {payment1.GetType().Name}"); // Выведет: BankPayment
        Console.WriteLine($"Сумма: {payment1.Amount}");

        
        // === СЦЕНАРИЙ 2: Конфигурация для наличных ===
        Console.WriteLine("\n--- Сценарий 2: Наличные ---");

        // 1. Создаем ДРУГУЮ фабрику
        IPaymentFactory cashFactory = new CashPaymentFactory();

        // 2. Внедряем эту фабрику в тот же тип калькулятора
        IPaymentCalculator cashCalculator = new PaymentCalculator(cashFactory);

        // 3. Используем
        Order order2 = new Order { TotalCost = 99.00m };
        IPayment payment2 = cashCalculator.Calculate(order2);

        // 4. Проверяем
        // На этот раз код калькулятора создал CashPayment!
        Console.WriteLine($"Создан платеж типа: {payment2.GetType().Name}"); // Выведет: CashPayment
        Console.WriteLine($"Сумма: {payment2.Amount}");
    }
}
```

В общем основная идея, что фабрика применяется в тех случаях, когда мы желаем создавать целые семейства объектов, а не что-то одно. Есть редкие исключения, когда вполне уместно применить фабричный метод, но в основном применяют именно фабрику.

Рассмотрим ещё один пример:

Представьте, что вы пишете симулятор мебельного магазина. Ваш код содержит:

- Семейство зависимых продуктов, cкажем, Armchair + Sofa + Coffee table.
- Несколько вариаций этого семейства. Например, продукты Armchair, Sofa, Coffee table представлены в трёх разных стилях: [Ар-деко, Ар-нуво (модерн)](https://artdoart.com/news/ar-nuvo-i-ar-deko-v-cem-raznica) и Викторианском.

![](src/abstract_factory/furniture.png)

Вам нужен такой способ создавать объекты продуктов, чтобы они сочетались с другими продуктами того же семейства. Это важно, так как клиенты расстраиваются, если получают несочетающуюся мебель.

![](src/abstract_factory/client_sad.png)

Кроме того, вы не хотите вносить изменения в существующий код при добавлении новых продуктов или семейcтв в программу. Поставщики часто обновляют свои каталоги, и вы бы не хотели менять уже написанный код каждый раз при получении новых моделей мебели

Паттерн Абстрактная фабрика предлагает выделить общие интерфейсы для отдельных продуктов, составляющих семейства. Так, все вариации кресел получат общий интерфейс Armchair, все диваны реализуют интерфейс Sofa и так далее. В примерах выше это были Button, ScrollBar и так далее
![](src/abstract_factory/IChair.png)

Далее вы создаёте абстрактную фабрику — общий интерфейс, который содержит методы создания всех продуктов семейства (например, createArmchair, createSofa и createCoffeeTable). Эти операции должны возвращать абстрактные типы продуктов, представленные интерфейсами, которые мы выделили ранее — Armchair, Sofa и Coffee table.

Для каждой вариации семейства продуктов мы должны создать свою собственную фабрику, реализовав абстрактный интерфейс. Фабрики создают продукты одной вариации. Например, ModernFactory будет возвращать только ModernArmchair ,ModernSofa и ModernCoffeeTable.

![](src/abstract_factory/IFactory.png)

(В примерах выше это была фабрика всяких UI элементов, вроде кнопок)

Клиентский код должен работать как с фабриками, так и с продуктами только через их общие интерфейсы. Это позволит подавать в ваши классы любой тип фабрики и производить любые продукты, ничего не ломая.

![](src/abstract_factory/Not_funny.png)

Например, клиентский код просит фабрику сделать стул. Он не знает, какого типа была эта фабрика. Он не знает, получит викторианский или модерновый стул. Для него важно, чтобы на стуле можно было сидеть и чтобы этот стул отлично смотрелся с диваном той же фабрики.

То есть схема следующая:
![](src/abstract_factory/scheme.png)

1. Абстрактные продукты объявляют интерфейсы продуктов, которые связаны друг с другом по смыслу, но выполняют разные функции.

2. Конкретные продукты — большой набор классов, которые относятся к различным абстрактным продуктам (кресло/столик), но имеют одни и те же вариации (Викторианский/Модерн).

3. Абстрактная фабрика объявляет методы создания различных абстрактных продуктов (кресло/столик).

4. Конкретные фабрики относятся каждая к своей вариации продуктов (Викторианский/Модерн) и реализуют методы абстрактной фабрики, позволяя создавать все продукты определённой вариации.

5. Несмотря на то, что конкретные фабрики порождают конкретные продукты, сигнатуры их методов должны возвращать соответствующие абстрактные продукты. Это позволит клиентскому коду, использующему фабрику, не привязываться к конкретным классам продуктов. Клиент сможет работать с любыми вариациями продуктов через абстрактные интерфейсы.

Приведём пример кода:

```csharp
// -----------------------------
// 1. Абстрактные продукты
// -----------------------------

public interface IArmchair
{
    void SitOn();
}

public interface ISofa
{
    void LieOn();
}

public interface ICoffeeTable
{
    void PutCoffee();
}

// -----------------------------
// 2. Конкретные продукты
// Ар-деко
// -----------------------------

public class ArtDecoArmchair : IArmchair
{
    public void SitOn() => Console.WriteLine("Вы сидите на кресле Ар-деко.");
}

public class ArtDecoSofa : ISofa
{
    public void LieOn() => Console.WriteLine("Вы лежите на диване Ар-деко.");
}

public class ArtDecoCoffeeTable : ICoffeeTable
{
    public void PutCoffee() => Console.WriteLine("Вы ставите кофе на столик Ар-деко.");
}

// -----------------------------
// 2. Конкретные продукты
// Ар-нуво (модерн)
// -----------------------------

public class ModernArmchair : IArmchair
{
    public void SitOn() => Console.WriteLine("Вы сидите на кресле в стиле модерн.");
}

public class ModernSofa : ISofa
{
    public void LieOn() => Console.WriteLine("Вы лежите на диване в стиле модерн.");
}

public class ModernCoffeeTable : ICoffeeTable
{
    public void PutCoffee() => Console.WriteLine("Вы ставите кофе на столик в стиле модерн.");
}

// -----------------------------
// 2. Конкретные продукты
// Викторианский
// -----------------------------

public class VictorianArmchair : IArmchair
{
    public void SitOn() => Console.WriteLine("Вы сидите на викторианском кресле.");
}

public class VictorianSofa : ISofa
{
    public void LieOn() => Console.WriteLine("Вы лежите на викторианском диване.");
}

public class VictorianCoffeeTable : ICoffeeTable
{
    public void PutCoffee() => Console.WriteLine("Вы ставите кофе на викторианский столик.");
}

// -----------------------------
// 3. Абстрактная фабрика
// -----------------------------

public interface IFurnitureFactory
{
    IArmchair CreateArmchair();
    ISofa CreateSofa();
    ICoffeeTable CreateCoffeeTable();
}

// -----------------------------
// 4. Конкретные фабрики
// -----------------------------

public class ArtDecoFurnitureFactory : IFurnitureFactory
{
    public IArmchair CreateArmchair() => new ArtDecoArmchair();
    public ISofa CreateSofa() => new ArtDecoSofa();
    public ICoffeeTable CreateCoffeeTable() => new ArtDecoCoffeeTable();
}

public class ModernFurnitureFactory : IFurnitureFactory
{
    public IArmchair CreateArmchair() => new ModernArmchair();
    public ISofa CreateSofa() => new ModernSofa();
    public ICoffeeTable CreateCoffeeTable() => new ModernCoffeeTable();
}

public class VictorianFurnitureFactory : IFurnitureFactory
{
    public IArmchair CreateArmchair() => new VictorianArmchair();
    public ISofa CreateSofa() => new VictorianSofa();
    public ICoffeeTable CreateCoffeeTable() => new VictorianCoffeeTable();
}

// -----------------------------
// 5. Клиентский код
// -----------------------------

public class FurnitureShowroom
{
    private readonly IFurnitureFactory _factory;

    private IArmchair _armchair;
    private ISofa _sofa;
    private ICoffeeTable _table;

    public FurnitureShowroom(IFurnitureFactory factory)
    {
        _factory = factory;
    }

    public void CreateFurnitureSet()
    {
        _armchair = _factory.CreateArmchair();
        _sofa = _factory.CreateSofa();
        _table = _factory.CreateCoffeeTable();
    }

    public void Demo()
    {
        _armchair.SitOn();
        _sofa.LieOn();
        _table.PutCoffee();
    }
}

// -----------------------------
// 6. Конфигуратор приложения
// -----------------------------

public static class AppConfig
{
    public static void Main()
    {
        string style = "ArtDeco"; // читаем из конфига

        IFurnitureFactory factory = style switch
        {
            "ArtDeco" => new ArtDecoFurnitureFactory(),
            "Modern" => new ModernFurnitureFactory(),
            "Victorian" => new VictorianFurnitureFactory(),
            _ => throw new Exception("Неизвестный стиль мебели!")
        };

        var showroom = new FurnitureShowroom(factory);

        showroom.CreateFurnitureSet();
        showroom.Demo();
    }
}
```
**Применимость**
1. **Когда бизнес-логика программы должна работать с разными видами связанных друг с другом продуктов, не завися от конкретных классов продуктов.**
Абстрактная фабрика скрывает от клиентского кода подробности того, как и какие конкретно объекты будут созданы. Но при этом клиентский код может работать со всеми типами создаваемых продуктов, поскольку их общий интерфейс был заранее определён.
2. **Когда в программе уже используется Фабричный метод, но очередные изменения предполагают введение новых типов продуктов.**
В хорошей программе каждый класс отвечает только за одну вещь. Если класс имеет слишком много фабричных методов, они способны затуманить его основную функцию. Поэтому имеет смысл вынести всю логику создания продуктов в отдельную иерархию классов, применив абстрактную фабрику.

**Шаги реализации**
1. Создайте таблицу соотношений типов продуктов к вариациям семейств продуктов.
2. Сведите все вариации продуктов к общим интерфейсам.
3. Определите интерфейс абстрактной фабрики. Он должен иметь фабричные методы для создания каждого из типов продуктов.
4. Создайте классы конкретных фабрик, реализовав интерфейс абстрактной фабрики. Этих классов должно быть столько же, сколько и вариаций семейств продуктов.
5. Измените код инициализации программы так, чтобы она создавала определённую фабрику и передавала её в клиентский код.
6. Замените в клиентском коде участки создания продуктов через конструктор вызовами соответствующих методов фабрики.


Преимущества:
- настоящее соблюдение SRP, ведь в такой реализации нет прямой связанности между реализациями
- соблюдение OCP: мы можем добавить в систему новые виды платежей и реализовать фабрики для них, тем самым, расширить логику не меняя реализацию калькуляторов

Недостатки:
- Усложняет код программы из-за введения множества дополнительных классов.
- Требует наличия всех типов продуктов в каждой вариации.

