
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
