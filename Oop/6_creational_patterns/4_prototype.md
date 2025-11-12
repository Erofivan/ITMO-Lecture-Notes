
### Prototype (Прототип)

Прототип — это порождающий паттерн проектирования, который позволяет копировать объекты, не вдаваясь в подробности их реализации.

Представьте себе задачу. У вас есть сложный объект, например `User`, со множеством полей:
```csharp
public class User
{
    public string Name { get; set; }
    public int Age { get; set; }
    public Email Email { get; set; }
    public List<Permission> Permissions { get; set; }
    public Dictionary<string, object> Settings { get; set; }
}
```
И вам нужно создать копию этого пользователя. Самое простое что можно сделать - это просто скопировать все поля через геттеры:

```csharp
var userCopy = new User 
{ 
    Name = originalUser.Name,
    Age = originalUser.Age,
    Email = originalUser.Email,
    Permissions = originalUser.Permissions,
    Settings = originalUser.Settings
};
```
Однако здесь есть сразу несколько проблем:
1. **логика копирования может быть необходима в нескольких места**
Представьте, что у вас есть сложный объект с 20 полями. Чтобы создать его точную копию, вам придется в нескольких местах программы писать код, который извлекает все 20 полей и передает их в конструктор нового объекта. Это дублирование кода и нарушение принципа DRY (Don't Repeat Yourself).
2. **данные могут быть сокрыты или модифицированны в конструкторе**
Что, если у объекта есть private поля, которые определяют его состояние, но у вас нет public "геттеров" для них? Вы, как клиентский код, просто не можете прочитать это состояние, чтобы передать его в конструктор. Более того, конструктор может модифицировать входные данные (например преобразовать их в другой формат), а мы хотим точную копию существующего состояния.
3. **объект находится в иерархии, при копировании конкретный тип не извествен**
Представьте, у вас есть массив Shape[], в котором лежат объекты Circle и Square. Вы хотите пройти по массиву и создать копию каждого элемента.
```csharp
foreach (Shape shape in shapes)
{
    // Что здесь писать?
    Shape copy = ???
}
```
Вы не можете написать new Circle() или new Square(), потому что вы оперируете только базовым типом Shape. Вам нужен механизм, позволяющий сказать самому объекту: "Пожалуйста, создай еще одного такого же, как ты".
Или другой пример, у нас есть админы и есть обычные пользователи
```csharp
public class User { }
public class AdminUser : User { }
public class GuestUser : User { }

public void DuplicateUser(User original)
{
    // Какой конструктор вызвать? Мы не знаем, это Admin или Guest!
    User copy = ??? // Тоже непонятно какой именно это у нас пользователь
}
```

Все такие попытки скопировать объект будут похожи на попытки воссоздать самолёт со всей его функциональностью, опираясь только на внешний вид:
![](src/prototype/plane.png)
Нам нужен какой-то *чертёж*. Это и есть суть паттерна Прототип. Prototype — это паттерн, когда объект сам знает, как себя копировать. Он переносит ответственность за создание копии с клиентского кода на сам объект-прототип.

Мы добавляем некий общий интерфейс, например метод Clone():
```csharp
public interface IPrototype
{
    IPrototype Clone();
}

public class User : IPrototype
{
    public string Name { get; set; }
    public int Age { get; set; }
    
    public IPrototype Clone()
    {
        return new User 
        { 
            Name = this.Name,
            Age = this.Age 
        };
    }
}
```
Теперь, где бы вам ни нужна была копия, мы просто вызываем:
```csharp
var copy = original.Clone();
```

Главное преимущество: вы не знаете и не обязаны знать, как внутри устроена копия. Объект сам берёт на себя эту ответственность.

Тем не менее идею с прототипами можно развить куда сильнее. Давайте рассмотрим несколько разных подходов:

#### Shallow copy (поверхностное копирование)

Представим, что у нас есть объект с примитивными полями и ссылками на другие объекты:

```csharp
public class Prototype
{
    private readonly IReadOnlyCollection<int> _relatedEntityIds;

    public Prototype(IReadOnlyCollection<int> relatedEntityIds)
    {
        _relatedEntityIds = relatedEntityIds;
    }

    public Prototype Clone()
    {
        // Просто копируем ссылку на коллекцию
        return new Prototype(_relatedEntityIds);
    }
}
```

Что здесь происходит:

- Мы создаём новый объект `Prototype`
- Но `_relatedEntityIds` — это одна и та же ссылка в оригинале и копии

То есть есть проблема: мы получили два разных объекта Prototype, но они оба ссылаются на одну и ту же коллекцию в памяти. Если мы изменим эту коллекцию в оригинале (предположим, она не IReadOnlyCollection), изменения увидит и копия.

Это поверхностное (shallow) копирование. Оно быстрое, но может привести к неожиданным побочным эффектам.

```
Original:    Clone:
┌─────────┐  ┌─────────┐
│Name: "A"│  │Name: "A"│
└─────────┘  └─────────┘
      │            │
      └────┬───────┘
           ↓
      [1, 2, 3, 4] (общая коллекция)
```

Это нормально, если:
- Коллекция неизменяемая (IReadOnlyCollection)
- Вы не планируете её модифицировать

Однако если коллекция может изменяться, это уже проблема.

#### Deep copy (глубокое копирование)

Если в объекте есть изменяемые вложенные структуры. Нам нужно скопировать не только сам объект, но и все вложенные объекты:
```csharp
// Обёрнутое значение (изменяемое)
public class WrappedValue
{
    public int Value { get; set; }

    public WrappedValue Clone()
    {
        return new WrappedValue { Value = this.Value };
    }
}

// Прототив, содержащий список обёрнутых значений
public class DeepCopyPrototype
{
    private readonly List<WrappedValue> _values;

    public DeepCopyPrototype(List<WrappedValue> values)
    {
        _values = values;
    }

    public DeepCopyPrototype Clone()
    {
        // Ключ: мы клонируем и сам список, и каждый элемент в нём
        List<WrappedValue> clonedValues = _values
            .Select(x => x.Clone())  // Каждое значение копируется!
            .ToList();
        /*
        1. Создаем новый список (.ToList()).

        2. Проходим по каждому элементу x в старом списке.

        3. Вызываем x.Clone() для каждого элемента, создавая новые объекты WrappedValue.
        */

        return new DeepCopyPrototype(clonedValues);
    }
}
```

Использовать это можно как-то так:
```csharp
var original = new DeepCopyPrototype(new List<WrappedValue>
{
    new WrappedValue { Value = 10 },
    new WrappedValue { Value = 20 }
});

var copy = original.Clone();

// Если изменить значение в копии, оригинал не пострадает
// Потому что это разные объекты WrappedValue
```
Когда нужен deep copy:
- Объект содержит списки, словари, другие коллекции
- Эти коллекции содержат изменяемые объекты
- Вы планируете модифицировать копию независимо от оригинала

#### Иерархии типов 

Теперь вернёмся к главной проблеме - иерархиям. 

Решить её можно так:
```csharp
public interface IHierarchyPrototype
{
    IHierarchyPrototype Clone();
}

// Тип A
public class FirstDerivedPrototype : IHierarchyPrototype
{
    private readonly string _name;
    private readonly int _age;

    public FirstDerivedPrototype(string name, int age)
    {
        _name = name;
        _age = age;
    }

    public IHierarchyPrototype Clone()
    {
        return new FirstDerivedPrototype(_name, _age);
    }
}

// Тип B
public class SecondDerivedPrototype : IHierarchyPrototype
{
    private readonly long _iterationCount;

    public SecondDerivedPrototype(long iterationCount)
    {
        _iterationCount = iterationCount;
    }

    public IHierarchyPrototype Clone()
    {
        return new SecondDerivedPrototype(_iterationCount);
    }
}
```
Теперь можно использовать так:
```csharp
public void ProcessPrototype(IHierarchyPrototype proto)
{
    // ...

    // Мы НЕ знаем, FirstDerivedPrototype это или SecondDerivedPrototype
    // Но мы можем создать копию!
    var clone = proto.Clone();
    
    // clone будет правильного типа автоматически
    // потому что каждый класс знает, как себя копировать

    // ...
}

// Использование:
var first = new FirstDerivedPrototype("Alice", 30);
var second = new SecondDerivedPrototype(1000);

ProcessPrototype(first);   // Создаст копию FirstDerivedPrototype
ProcessPrototype(second);  // Создаст копию SecondDerivedPrototype
```
Это решает проблему: теперь нам не нужно использовать typeof, is, as или switch для копирования. Каждый класс сам знает, как себя копировать.

Есть два способа организовать Prototype. Первый — через абстрактный класс:
```csharp
public abstract class Prototype
{
    public abstract Prototype Clone();
}

public class ClassPrototype : Prototype
{
    public override ClassPrototype Clone()
    {
        return new ClassPrototype();
    }
}
```
Второй — через интерфейсы:
```csharp
public interface IPrototype
{
    IPrototype Clone();
}

public class InterfacePrototype : IPrototype
{
    // Явная реализация через интерфейс
    IPrototype IPrototype.Clone()
    {
        return Clone();
    }

    // А сам метод возвращает конкретный тип
    public InterfacePrototype Clone()
    {
        return new InterfacePrototype();
    }
}
```
Обратите внимание, что во втором примере (реализации через интферйсы) два метода Clone(). Зачем нужна эта двойная реализация? Когда работаете с интерфейсом, Clone() возвращает IPrototype. Но часто нужно конкретный тип:
```csharp
IPrototype proto = new InterfacePrototype();
var clone = proto.Clone();  // Тип: IPrototype, теряем информацию!

// С двойной реализацией:
InterfacePrototype proto2 = new InterfacePrototype();
var clone2 = proto2.Clone();  // Тип: InterfacePrototype, сохранили!
```
Какой выбрать?
- Интерфейс — более гибкий, позволяет множественное наследование, лучше для разнородных типов
- Абстрактный класс — лучше, если есть общая логика для всех прототипов

В современном C# предпочитают интерфейсы.

#### Проблема переиспользования (наследование vs интерфейсы)

Тем не менее у обоих этих подходов (что через абстрактные классы, что через интерфейсы) есть общая проблема

Допустим, у нас есть логика, которую нужно выполнить для всех прототипов:
```csharp 
public abstract class Prototype
{
    public void DoSomeStuff()
    {
        // Общая логика для всех прототипов
        Console.WriteLine("Doing common work...");
    }

    public abstract Prototype Clone();
}

public class ClassPrototype : Prototype
{
    public void DoOtherStuff()
    {
        Console.WriteLine("Doing specific work...");
    }

    public override Prototype Clone()
    {
        return new ClassPrototype();
    }
}
```
Теперь рассмотрим сценарий, где нужно клонировать и выполнить общую логику:
```csharp
public class Scenario
{
    public static Prototype CloneAndDoSomeStuff(Prototype prototype)
    {
        var clone = prototype.Clone();
        clone.DoSomeStuff();  // Это работает, потому что это есть в базовом классе
        return clone;
    }

    public static void TopLevelScenario()
    {
        var prototype = new ClassPrototype();
        Prototype clone = CloneAndDoSomeStuff(prototype);
        
        // ПРОБЛЕМА: теперь clone — это Prototype, а не ClassPrototype
        // Мы потеряли информацию о конкретном типе!
        
        clone.DoOtherStuff();  // ОШИБКА компилятора! DoOtherStuff нет в Prototype
    }
}
```

Столкнёмся ли мы с той же проблемой если будем использовать интерфейсы?
```csharp
public interface IPrototype
{
    IPrototype Clone();
    void DoSomeStuff();
}

public class InterfacePrototype : IPrototype
{
    public IPrototype Clone()
    {
        return new InterfacePrototype();
    }

    public void DoSomeStuff()
    {
        Console.WriteLine("Doing common work...");
    }

    public void DoOtherStuff()
    {
        Console.WriteLine("Doing specific work...");
    }
}

public class Scenario
{
    public static IPrototype CloneAndDoSomeStuff(IPrototype prototype)
    {
        var clone = prototype.Clone();
        clone.DoSomeStuff();
        return clone;
    }

    public static void TopLevelScenario()
    {
        var prototype = new InterfacePrototype();
        IPrototype clone = CloneAndDoSomeStuff(prototype);
        
        // Мы можем привести к конкретному типу, но прихрдится делать cast
        InterfacePrototype specificClone = (InterfacePrototype)clone;
        specificClone.DoOtherStuff();  // OK!
    }
}
```
Тут уже лучше, но всё равно делать явные кастые кажется не очень красивым решением. 

Давайте рассмотрим ещё несколько примеров этой проблемы:
Представьте ситуацию: у нас есть система работы с геометрическими фигурами. Все фигуры можно копировать и рисовать:
```csharp
public abstract class Shape
{
    public string Color { get; set; }
    public double X { get; set; }
    public double Y { get; set; }

    protected Shape(string color, double x, double y)
    {
        Color = color;
        X = x;
        Y = y;
    }

    // Общая логика для всех фигур
    public void Draw()
    {
        Console.WriteLine($"Drawing {GetType().Name} at ({X}, {Y}) with color {Color}");
    }

    public void MoveTo(double newX, double newY)
    {
        X = newX;
        Y = newY;
    }

    // Метод клонирования
    public abstract Shape Clone();
}

```
Теперь добавляем конкретные фигуры:
```csharp
public class Circle : Shape
{
    public double Radius { get; set; }

    public Circle(string color, double x, double y, double radius)
        : base(color, x, y)
    {
        Radius = radius;
    }

    public override Circle Clone()
    {
        return new Circle(Color, X, Y, Radius);
    }

    // Специфичный метод только для круга
    public void Scale(double factor)
    {
        Radius *= factor;
        Console.WriteLine($"Circle scaled to radius {Radius}");
    }
}

public class Rectangle : Shape
{
    public double Width { get; set; }
    public double Height { get; set; }

    public Rectangle(string color, double x, double y, double width, double height)
        : base(color, x, y)
    {
        Width = width;
        Height = height;
    }

    public override Rectangle Clone()
    {
        return new Rectangle(Color, X, Y, Width, Height);
    }

    // Специфичный метод только для прямоугольника
    public void Resize(double newWidth, double newHeight)
    {
        Width = newWidth;
        Height = newHeight;
        Console.WriteLine($"Rectangle resized to {Width}x{Height}");
    }
}
```
Теперь проблема: хотим написать метод, который клонирует фигуру, перемещает её и возвращает:
```csharp
public class ShapeProcessor
{
    // Этот метод принимает любую фигуру
    public static Shape CloneAndMove(Shape shape, double newX, double newY)
    {
        var clone = shape.Clone();  // Клонируем
        clone.MoveTo(newX, newY);    // Перемещаем
        return clone;                // Возвращаем
    }
}
```
Использование:
```csharp
public static void Main()
{
    var circle = new Circle("Red", 10, 10, 5);
    
    // Вызываем наш метод
    Shape clonedShape = ShapeProcessor.CloneAndMove(circle, 50, 50);
    
    // clonedShape имеет тип Shape, а не Circle!
    // Мы не можем вызвать специфичный метод Scale()
    
    // clonedShape.Scale(2.0);  // Ошибка компиляции!
    
    // Приходится делать cast:
    if (clonedShape is Circle clonedCircle)
    {
        clonedCircle.Scale(2.0);  // Теперь работает
    }
}
```
То есть у нас всё равно неправильная сигнатура
```csharp
public static Shape CloneAndMove(Shape shape, ...)
                   ↑
            возвращаем Shape, а не конкретный тип!
```
Даже несмотря на то, что метод `Clone()` в `Circle` возвращает `Circle`, компилятор видит возвращаемый тип базового класса (`Shape`), потому что мы работаем через базовый класс.
```text
Что мы хотим:
Circle → CloneAndMove → Circle (сохранили тип!)
                         ↓
                    можем вызвать Scale()

Что мы получаем:
Circle → CloneAndMove → Shape (потеряли тип!)
                         ↓
                    не можем вызвать Scale() без cast
```
Та же самая проблема будет с интерфейсами:
```csharp
public interface IPrototype
{
    IPrototype Clone();
    void Draw();
}

public class Circle : IPrototype
{
    public double Radius { get; set; }

    public Circle(double radius)
    {
        Radius = radius;
    }

    // Явная реализация интерфейса
    IPrototype IPrototype.Clone()
    {
        return Clone();
    }

    // Конкретная реализация возвращает Circle
    public Circle Clone()
    {
        return new Circle(Radius);
    }

    public void Draw()
    {
        Console.WriteLine($"Drawing circle with radius {Radius}");
    }

    public void Scale(double factor)
    {
        Radius *= factor;
    }
}

public class Rectangle : IPrototype
{
    public double Width { get; set; }
    public double Height { get; set; }

    public Rectangle(double width, double height)
    {
        Width = width;
        Height = height;
    }

    IPrototype IPrototype.Clone()
    {
        return Clone();
    }

    public Rectangle Clone()
    {
        return new Rectangle(Width, Height);
    }

    public void Draw()
    {
        Console.WriteLine($"Drawing rectangle {Width}x{Height}");
    }

    public void Resize(double w, double h)
    {
        Width = w;
        Height = h;
    }
}
```
Использование:
```csharp
public class Scenario
{
    public static IPrototype CloneAndDraw(IPrototype prototype)
    {
        var clone = prototype.Clone();
        clone.Draw();
        return clone;
    }

    public static void Main()
    {
        var circle = new Circle(5.0);
        
        // Вызываем метод
        IPrototype clonedProto = CloneAndDraw(circle);
        
        // тип IPrototype, а не Circle!
        // clonedProto.Scale(2.0);  // Ошибка!
        
        // Снова нужен cast:
        if (clonedProto is Circle clonedCircle)
        {
            clonedCircle.Scale(2.0);  // Работает, но некрасиво
        }
    }
}
```
Та же проблема! Тип возвращаемого значения — IPrototype, а не конкретный класс.

Почему это плохо?
- Нарушается type safety — компилятор не может вам помочь
- Нужны runtime проверки — is, as, switch
- Возможны ошибки — если вы сделаете неправильный cast, получите null или InvalidCastException
- Код становится хрупким — легко ошибиться

Решение: использовать рекурсивные дженерики

#### Рекурсивные дженерики 

Рекурсивные параметр-тип - параметр-тип, ссылающийся на себя в ограничениях наложенных на допустимые агрументы-типы

Встречайте решение через CRTP (Curiously Recurring Template Pattern) — рекурсивный параметр-тип:
```csharp
public interface IPrototype<T> where T : IPrototype<T>
{
    T Clone();
    void DoSomeStuff();
}

public class Prototype : IPrototype<Prototype>
{
    public Prototype Clone()
    {
        return new Prototype();
    }

    public void DoSomeStuff()
    {
        Console.WriteLine("Doing common work...");
    }

    public void DoOtherStuff()
    {
        Console.WriteLine("Doing specific work...");
    }
}
```
Что здесь происходит?

- Параметр типа T в IPrototype<T> ограничен самим собой: where T : IPrototype<T>. Это означает:
- Если вы реализуете IPrototype<T>, то T должен быть классом, который реализует IPrototype<T>
- Это гарантирует, что Clone() вернёт правильный тип

```csharp
public interface IPrototype<T> where T : IPrototype<T>
                     ↑                         ↑
                     └─────────────────────────┘
                     Тип T должен реализовывать IPrototype<T>
```
Очень хорошо, что вы все помните CRTP ещё из плюсов и не нужно лишний раз объяснять что это и как это работает

То есть теперь можно использовать так:
```csharp
public class Scenario
{
    public static T CloneAndDoSomeStuff<T>(T prototype)
        where T : IPrototype<T>
    {
        var clone = prototype.Clone();  // Тип: T (правильный!)
        clone.DoSomeStuff();
        return clone;
    }

    public static void TopLevelScenario()
    {
        var prototype = new Prototype();
        Prototype clone = CloneAndDoSomeStuff(prototype);  // Тип сохранён!
        
        clone.DoOtherStuff();  // Никаких cast'ов!
    }
}
```

А что, если у вас есть иерархия? Например, SecondPrototype наследует Prototype:

```csharp
public class SecondPrototype : Prototype, IPrototype<SecondPrototype>
{
    public override SecondPrototype Clone()
    {
        return new SecondPrototype();
    }
}
```
Как это работает?
- SecondPrototype наследует от Prototype, который реализует IPrototype<Prototype>
- Но SecondPrototype явно говорит, что реализует IPrototype<SecondPrototype>
- Это означает, что Clone() в SecondPrototype возвращает именно SecondPrototype

```csharp
public static void Hierarchy()
{
    var second = new SecondPrototype();
    SecondPrototype clonedSecond = CloneAndDoSomeStuff(second);
    
    // Правильный тип!
}
```
Но тут появляется проблема со смешиванием обобщённых и необобщённых интерфейсов:
```csharp
// Это необобщённый интерфейс (без типизации)
public interface IPrototype
{
    void DoSomeStuff() { }
}

// Это обобщённый интерфейс (с типизацией)
public interface IPrototype<T> : IPrototype where T : IPrototype<T>
{
    T Clone();
}

public record Container(IPrototype Prototype);

static void NonGeneric()
{
    // Проблема: Container хранит IPrototype
    // Но IPrototype не имеет метода Clone()!
    var container = new Container(new Prototype());
    
    // Как теперь клонировать?
    // container.Prototype.Clone();  // Ошибка! Нет Clone() в IPrototype
}
```
Когда работаете через необобщённый интерфейс IPrototype, вы теряете информацию о типе и не можете вызвать обобщённый Clone().

Решение: если вам нужно работать с коллекциями разнородных типов, добавьте Clone() в необобщённый интерфейс:

```csharp
public interface IPrototype
{
    IPrototype Clone();  // Необобщённая версия
    void DoSomeStuff() { }
}

public interface IPrototype<T> : IPrototype where T : IPrototype<T>
{
    new T Clone();  // Переопределяем с обобщённой версией
}

// Теперь оба работают:
var proto = new Prototype();
IPrototype clone1 = proto.Clone();        // Из необобщённого интерфейса
Prototype clone2 = proto.Clone();         // Из обобщённого интерфейса
```

Давайте рассмотрим какой-нибудь пример, который объединяет это всё вместе:
```csharp
using System;
using System.Collections.Generic;
using System.Linq;

// === Интерфейсы ===
public interface IDocument
{
    IDocument Clone();
    void Print();
}

public interface IDocument<T> : IDocument where T : IDocument<T>
{
    new T Clone();
}

// === Базовый класс документа ===
public abstract class Document : IDocument<Document>
{
    protected string _title;
    protected DateTimeOffset _createdAt;

    protected Document(string title, DateTimeOffset createdAt)
    {
        _title = title;
        _createdAt = createdAt;
    }

    // Необобщённая версия для коллекций
    public IDocument Clone() => Clone();

    // Обобщённая версия, которую переопределяют дети
    public abstract Document Clone();

    public virtual void Print()
    {
        Console.WriteLine($"[{GetType().Name}] Title: {_title}");
        Console.WriteLine($"Created: {_createdAt:dd.MM.yyyy HH:mm:ss}");
    }
}

// === Конкретные типы документов ===
public class TextDocument : Document, IDocument<TextDocument>
{
    private readonly string _content;
    private readonly List<string> _tags;

    public TextDocument(string title, DateTimeOffset createdAt, string content, List<string> tags)
        : base(title, createdAt)
    {
        _content = content;
        _tags = tags;
    }

    public override TextDocument Clone()
    {
        // Deep copy списка тегов
        var clonedTags = _tags.Select(t => t).ToList();
        return new TextDocument(_title, _createdAt, _content, clonedTags);
    }

    public override void Print()
    {
        base.Print();
        Console.WriteLine($"Content: {_content}");
        Console.WriteLine($"Tags: {string.Join(", ", _tags)}");
    }

    public void EditContent(string newContent)
    {
        // Имитируем редактирование
        Console.WriteLine($"Edited: {_content} -> {newContent}");
    }
}

public class PresentationDocument : Document, IDocument<PresentationDocument>
{
    private readonly int _slideCount;
    private readonly List<string> _presenters;

    public PresentationDocument(string title, DateTimeOffset createdAt, int slideCount, List<string> presenters)
        : base(title, createdAt)
    {
        _slideCount = slideCount;
        _presenters = presenters;
    }

    public override PresentationDocument Clone()
    {
        // Deep copy списка ораторов
        var clonedPresenters = _presenters.Select(p => p).ToList();
        return new PresentationDocument(_title, _createdAt, _slideCount, clonedPresenters);
    }

    public override void Print()
    {
        base.Print();
        Console.WriteLine($"Slides: {_slideCount}");
        Console.WriteLine($"Presenters: {string.Join(", ", _presenters)}");
    }

    public void AddPresenter(string name)
    {
        Console.WriteLine($"Added presenter: {name}");
    }
}

public class SpreadsheetDocument : Document, IDocument<SpreadsheetDocument>
{
    private readonly int _rows;
    private readonly int _columns;
    private readonly Dictionary<string, object> _data;

    public SpreadsheetDocument(string title, DateTimeOffset createdAt, int rows, int columns)
        : base(title, createdAt)
    {
        _rows = rows;
        _columns = columns;
        _data = new Dictionary<string, object>();
    }

    public override SpreadsheetDocument Clone()
    {
        var clone = new SpreadsheetDocument(_title, _createdAt, _rows, _columns);
        
        // Deep copy данных
        foreach (var kvp in _data)
        {
            clone._data[kvp.Key] = kvp.Value;
        }

        return clone;
    }

    public override void Print()
    {
        base.Print();
        Console.WriteLine($"Dimensions: {_rows}x{_columns}");
        Console.WriteLine($"Data entries: {_data.Count}");
    }

    public void SetCellValue(string key, object value)
    {
        _data[key] = value;
        Console.WriteLine($"Set {key} = {value}");
    }
}

// === Хранилище документов ===
public class DocumentRepository
{
    private readonly List<IDocument> _documents = [];

    // Работаем с необобщённым интерфейсом — можем хранить любые документы
    public void AddDocument(IDocument doc)
    {
        _documents.Add(doc);
    }

    // Клонируем документ, не зная его конкретного типа
    public IDocument CloneDocument(int index)
    {
        if (index < 0 || index >= _documents.Count)
            throw new IndexOutOfRangeException();

        return _documents[index].Clone();
    }

    public void PrintAll()
    {
        foreach (var doc in _documents)
        {
            doc.Print();
            Console.WriteLine();
        }
    }
}

// === Сценарий использования ===
public class Program
{
    public static void Main()
    {
        Console.WriteLine("=== Пример 1: Клонирование разнородных документов ===\n");

        var textDoc = new TextDocument(
            title: "Report",
            createdAt: DateTimeOffset.Now,
            content: "This is a report",
            tags: new List<string> { "urgent", "finance" }
        );

        var presentation = new PresentationDocument(
            title: "Q4 Review",
            createdAt: DateTimeOffset.Now,
            slideCount: 20,
            presenters: new List<string> { "Alice", "Bob" }
        );

        var spreadsheet = new SpreadsheetDocument(
            title: "Budget",
            createdAt: DateTimeOffset.Now,
            rows: 100,
            columns: 10
        );

        // Добавляем данные в таблицу
        spreadsheet.SetCellValue("A1", 1000);
        spreadsheet.SetCellValue("B1", 2000);

        // Складываем в хранилище
        var repo = new DocumentRepository();
        repo.AddDocument(textDoc);
        repo.AddDocument(presentation);
        repo.AddDocument(spreadsheet);

        Console.WriteLine("--- Оригинальные документы ---");
        repo.PrintAll();

        Console.WriteLine("\n=== Пример 2: Клонирование через необобщённый интерфейс ===\n");

        var clonedText = repo.CloneDocument(0) as TextDocument;
        var clonedPresentation = repo.CloneDocument(1) as PresentationDocument;
        var clonedSpreadsheet = repo.CloneDocument(2) as SpreadsheetDocument;

        Console.WriteLine("--- Клонированные документы ---");
        clonedText?.Print();
        Console.WriteLine();
        clonedPresentation?.Print();
        Console.WriteLine();
        clonedSpreadsheet?.Print();

        Console.WriteLine("\n=== Пример 3: Типобезопасное клонирование ===\n");

        // Используем обобщённый интерфейс для типобезопасности
        ProcessTextDocument(textDoc);
        ProcessPresentation(presentation);

        Console.WriteLine("\n=== Пример 4: Независимое редактирование копий ===\n");

        Console.WriteLine("Редактируем копию текстового документа:");
        clonedText?.EditContent("Updated content");

        Console.WriteLine("\nДобавляем оратора к копии презентации:");
        clonedPresentation?.AddPresenter("Charlie");

        Console.WriteLine("\nДобавляем значение в копию таблицы:");
        clonedSpreadsheet?.SetCellValue("C1", 3000);

        Console.WriteLine("\n--- Оригинальные документы остались без изменений ---");
        repo.PrintAll();
    }

    // Типобезопасный метод работает с обобщённым интерфейсом
    private static T CloneAndModify<T>(T document) where T : IDocument<T>
    {
        var clone = document.Clone();
        Console.WriteLine($"Cloned {typeof(T).Name}");
        return clone;
    }

    private static void ProcessTextDocument(TextDocument doc)
    {
        var clone = CloneAndModify(doc);
        Console.WriteLine($"Cloned text doc: {typeof(TextDocument).Name}");
    }

    private static void ProcessPresentation(PresentationDocument doc)
    {
        var clone = CloneAndModify(doc);
        Console.WriteLine($"Cloned presentation: {typeof(PresentationDocument).Name}");
    }
}
/* Вывод
=== Пример 1: Клонирование разнородных документов ===

--- Оригинальные документы ---
[TextDocument] Title: Report
Created: 02.11.2025 22:36:45
Content: This is a report
Tags: urgent, finance

[PresentationDocument] Title: Q4 Review
Created: 02.11.2025 22:36:45
Slides: 20
Presenters: Alice, Bob

[SpreadsheetDocument] Title: Budget
Created: 02.11.2025 22:36:45
Dimensions: 100x10
Data entries: 2

=== Пример 2: Клонирование через необобщённый интерфейс ===

--- Клонированные документы ---
[TextDocument] Title: Report
Created: 02.11.2025 22:36:45
Content: This is a report
Tags: urgent, finance

[PresentationDocument] Title: Q4 Review
Created: 02.11.2025 22:36:45
Slides: 20
Presenters: Alice, Bob

[SpreadsheetDocument] Title: Budget
Created: 02.11.2025 22:36:45
Dimensions: 100x10
Data entries: 2

=== Пример 3: Типобезопасное клонирование ===

Cloned TextDocument
Cloned PresentationDocument

=== Пример 4: Независимое редактирование копий ===

Редактируем копию текстового документа:
Edited: This is a report -> Updated content

Добавляем оратора к копии презентации:
Added presenter: Charlie

Добавляем значение в копию таблицы:
Set C1 = 3000

--- Оригинальные документы остались без изменений ---
[TextDocument] Title: Report
Created: 02.11.2025 22:36:45
Content: This is a report
Tags: urgent, finance

[PresentationDocument] Title: Q4 Review
Created: 02.11.2025 22:36:45
Slides: 20
Presenters: Alice, Bob

[SpreadsheetDocument] Title: Budget
Created: 02.11.2025 22:36:45
Dimensions: 100x10
Data entries: 2
*/
```

Давайте рассмотрим ещё несколько примеров с рекурсивными дженериками и их проблемами. Для этого вернёмся к примеру с фигурами:
```csharp
public interface IPrototype<T> where T : IPrototype<T>
{
    T Clone();
    void Draw();
}

public class Circle : IPrototype<Circle>
{
    public double Radius { get; set; }

    public Circle(double radius)
    {
        Radius = radius;
    }

    // Метод Clone() возвращает именно Circle!
    public Circle Clone()
    {
        return new Circle(Radius);
    }

    public void Draw()
    {
        Console.WriteLine($"Drawing circle with radius {Radius}");
    }

    public void Scale(double factor)
    {
        Radius *= factor;
    }
}

public class Rectangle : IPrototype<Rectangle>
{
    public double Width { get; set; }
    public double Height { get; set; }

    public Rectangle(double width, double height)
    {
        Width = width;
        Height = height;
    }

    public Rectangle Clone()
    {
        return new Rectangle(Width, Height);
    }

    public void Draw()
    {
        Console.WriteLine($"Drawing rectangle {Width}x{Height}");
    }

    public void Resize(double w, double h)
    {
        Width = w;
        Height = h;
    }
}
```
Использование:
```csharp 
public class Scenario
{
    // Этот метод обобщённый и сохраняет тип!
    public static T CloneAndDraw<T>(T prototype) where T : IPrototype<T>
    {
        var clone = prototype.Clone();  // Тип: T (правильный!)
        clone.Draw();
        return clone;  // Возвращаем T, а не IPrototype
    }

    public static void Main()
    {
        var circle = new Circle(5.0);
        
        // Вызываем метод — тип сохранён!
        Circle clonedCircle = CloneAndDraw(circle);
        
        // Можем вызвать Scale() без cast
        clonedCircle.Scale(2.0);
        
        // Аналогично для прямоугольника:
        var rect = new Rectangle(10, 20);
        Rectangle clonedRect = CloneAndDraw(rect);
        
        // Можем вызвать Resize() без cast
        clonedRect.Resize(30, 40);
    }
}
```

Когда вы вызываете CloneAndDraw(circle), компилятор:

1. Видит, что circle имеет тип Circle

2. Проверяет, что Circle : IPrototype<Circle> — ✅ true

3. Подставляет T = Circle везде в методе

Получается:

```csharp 
// Так компилятор видит ваш вызов:
public static Circle CloneAndDraw(Circle prototype)
    where Circle : IPrototype<Circle>
{
    var clone = prototype.Clone();  // Тип: Circle
    clone.Draw();
    return clone;  // Тип: Circle
}
```
Вывод: тип сохраняется на уровне компиляции

Теперь усложним. Что если у нас есть иерархия?
```csharp
// Базовый класс для всех фигур
public abstract class Shape<T> : IPrototype<T> where T : Shape<T>
{
    public string Color { get; set; }
    public double X { get; set; }
    public double Y { get; set; }

    protected Shape(string color, double x, double y)
    {
        Color = color;
        X = x;
        Y = y;
    }

    public abstract T Clone();

    public void Draw()
    {
        Console.WriteLine($"Drawing {GetType().Name} at ({X}, {Y}) with color {Color}");
    }

    public void MoveTo(double newX, double newY)
    {
        X = newX;
        Y = newY;
    }
}

// Конкретные фигуры
public class Circle : Shape<Circle>
{
    public double Radius { get; set; }

    public Circle(string color, double x, double y, double radius)
        : base(color, x, y)
    {
        Radius = radius;
    }

    public override Circle Clone()
    {
        return new Circle(Color, X, Y, Radius);
    }

    public void Scale(double factor)
    {
        Radius *= factor;
    }
}

public class Rectangle : Shape<Rectangle>
{
    public double Width { get; set; }
    public double Height { get; set; }

    public Rectangle(string color, double x, double y, double width, double height)
        : base(color, x, y)
    {
        Width = width;
        Height = height;
    }

    public override Rectangle Clone()
    {
        return new Rectangle(Color, X, Y, Width, Height);
    }

    public void Resize(double w, double h)
    {
        Width = w;
        Height = h;
    }
}
```
Использование:
```csharp
public static void Main()
{
    var circle = new Circle("Red", 10, 10, 5);
    var rect = new Rectangle("Blue", 20, 20, 10, 15);

    // Типобезопасное клонирование
    Circle clonedCircle = CloneAndMove(circle, 50, 50);
    Rectangle clonedRect = CloneAndMove(rect, 100, 100);

    // Можем вызывать специфичные методы без cast!
    clonedCircle.Scale(2.0);
    clonedRect.Resize(20, 30);
}

public static T CloneAndMove<T>(T shape, double x, double y) 
    where T : Shape<T>
{
    var clone = shape.Clone();
    clone.MoveTo(x, y);
    return clone;
}
```
Более сложный случай: многоуровневая иерархия
Что если есть наследование между конкретными классами?
```csharp
// Базовая фигура
public class Circle : Shape<Circle>
{
    public double Radius { get; set; }

    public Circle(string color, double x, double y, double radius)
        : base(color, x, y)
    {
        Radius = radius;
    }

    public override Circle Clone()
    {
        return new Circle(Color, X, Y, Radius);
    }

    public virtual void Scale(double factor)
    {
        Radius *= factor;
    }
}

// Наследуемся от Circle
public class FilledCircle : Circle, IPrototype<FilledCircle>
{
    public string FillPattern { get; set; }

    public FilledCircle(string color, double x, double y, double radius, string fillPattern)
        : base(color, x, y, radius)
    {
        FillPattern = fillPattern;
    }

    // Переопределяем Clone() для возврата FilledCircle
    public new FilledCircle Clone()
    {
        return new FilledCircle(Color, X, Y, Radius, FillPattern);
    }

    public override void Scale(double factor)
    {
        base.Scale(factor);
        Console.WriteLine($"Filled with pattern: {FillPattern}");
    }
}
```
Использование:
```csharp
public static void Main()
{
    var filledCircle = new FilledCircle("Red", 10, 10, 5, "Stripes");
    
    // Типобезопасное клонирование
    FilledCircle cloned = CloneAndMove(filledCircle, 50, 50);
    
    cloned.Scale(2.0);
    Console.WriteLine($"Pattern: {cloned.FillPattern}");
}
```
Теперь самое сложное. Что если вам нужно хранить разные типы в одной коллекции?
```csharp
public interface IPrototype<T> where T : IPrototype<T>
{
    T Clone();
}

public class Circle : IPrototype<Circle> { ... }
public class Rectangle : IPrototype<Rectangle> { ... }

// Как создать коллекцию?
var shapes = new List<???>();  // Что тут написать?

// List<IPrototype<???>> не работает, потому что у каждого свой тип!
```
Circle реализует IPrototype<Circle>, а Rectangle реализует IPrototype<Rectangle>. Это разные типы, и нет общего предка!

Решение 1: Необобщённый базовый интерфейс
Добавляем общий необобщённый интерфейс:
```csharp
// Необобщённый интерфейс (для коллекций)
public interface IPrototype
{
    IPrototype Clone();  // Возвращает необобщённый тип
    void Draw();
}

// Обобщённый интерфейс (для типобезопасности)
public interface IPrototype<T> : IPrototype where T : IPrototype<T>
{
    new T Clone();  // Переопределяем с обобщённым типом
}

public class Circle : IPrototype<Circle>
{
    public double Radius { get; set; }

    public Circle(double radius)
    {
        Radius = radius;
    }

    // Явная реализация необобщённого интерфейса
    IPrototype IPrototype.Clone()
    {
        return Clone();
    }

    // Обобщённая версия
    public Circle Clone()
    {
        return new Circle(Radius);
    }

    public void Draw()
    {
        Console.WriteLine($"Circle with radius {Radius}");
    }

    public void Scale(double factor)
    {
        Radius *= factor;
    }
}

public class Rectangle : IPrototype<Rectangle>
{
    public double Width { get; set; }
    public double Height { get; set; }

    public Rectangle(double width, double height)
    {
        Width = width;
        Height = height;
    }

    IPrototype IPrototype.Clone()
    {
        return Clone();
    }

    public Rectangle Clone()
    {
        return new Rectangle(Width, Height);
    }

    public void Draw()
    {
        Console.WriteLine($"Rectangle {Width}x{Height}");
    }

    public void Resize(double w, double h)
    {
        Width = w;
        Height = h;
    }
}
```
Теперь можно создать коллекцию:
```csharp
public class ShapeManager
{
    private readonly List<IPrototype> _shapes = [];

    public void AddShape(IPrototype shape)
    {
        _shapes.Add(shape);
    }

    public void CloneAll()
    {
        var clones = _shapes.Select(s => s.Clone()).ToList();
        
        foreach (var clone in clones)
        {
            clone.Draw();
        }
    }

    // Но если нужен конкретный тип, приходится делать cast:
    public Circle? GetCircle(int index)
    {
        return _shapes[index] as Circle;
    }
}
```
Использование:
```csharp
public static void Main()
{
    var manager = new ShapeManager();
    
    manager.AddShape(new Circle(5.0));
    manager.AddShape(new Rectangle(10, 20));
    manager.AddShape(new Circle(7.5));

    Console.WriteLine("=== Клонирование всех фигур ===");
    manager.CloneAll();

    Console.WriteLine("\n=== Получение конкретного типа ===");
    var circle = manager.GetCircle(0);
    if (circle != null)
    {
        circle.Scale(2.0);
        circle.Draw();
    }
}
```

Решение 2: Visitor Pattern
Если нужно избежать cast'ов, можно использовать Visitor:
```csharp
public interface IShapeVisitor
{
    void Visit(Circle circle);
    void Visit(Rectangle rectangle);
}

public interface IPrototype
{
    IPrototype Clone();
    void Accept(IShapeVisitor visitor);
}

public class Circle : IPrototype<Circle>
{
    public double Radius { get; set; }

    public Circle(double radius)
    {
        Radius = radius;
    }

    IPrototype IPrototype.Clone() => Clone();
    
    public Circle Clone() => new Circle(Radius);

    public void Accept(IShapeVisitor visitor)
    {
        visitor.Visit(this);  // Передаём конкретный тип!
    }
}

public class ScaleVisitor : IShapeVisitor
{
    private readonly double _factor;

    public ScaleVisitor(double factor)
    {
        _factor = factor;
    }

    public void Visit(Circle circle)
    {
        circle.Radius *= _factor;
        Console.WriteLine($"Scaled circle to radius {circle.Radius}");
    }

    public void Visit(Rectangle rectangle)
    {
        rectangle.Width *= _factor;
        rectangle.Height *= _factor;
        Console.WriteLine($"Scaled rectangle to {rectangle.Width}x{rectangle.Height}");
    }
}
```
Использование:
```csharp
public static void Main()
{
    List<IPrototype> shapes = new()
    {
        new Circle(5.0),
        new Rectangle(10, 20),
        new Circle(7.5)
    };

    var scaleVisitor = new ScaleVisitor(2.0);

    foreach (var shape in shapes)
    {
        shape.Accept(scaleVisitor);
    }
}
```

То есть ещё раз проблема с необобщённым интерфейсом:
```csharp
// Необобщённый интерфейс
public interface IPrototype
{
    void DoSomeStuff();
    // Нет метода Clone()!
}

// Обобщённый интерфейс
public interface IPrototype<T> : IPrototype where T : IPrototype<T>
{
    T Clone();
}

public class Prototype : IPrototype<Prototype>
{
    public Prototype Clone()
    {
        return new Prototype();
    }

    public void DoSomeStuff()
    {
        Console.WriteLine("Doing stuff");
    }
}

// Контейнер хранит необобщённый тип
public record Container(IPrototype Prototype);

static void NonGeneric()
{
    var container = new Container(new Prototype());
    
    // ПРОБЛЕМА: как клонировать?
    // container.Prototype.Clone();  // ОШИБКА! Нет Clone() в IPrototype
    
    // Приходится делать cast:
    if (container.Prototype is IPrototype<Prototype> proto)
    {
        var clone = proto.Clone();  // Работает
    }
}
```

Когда вы храните объект как IPrototype (необобщённый), вы теряете доступ к обобщённым методам. Метод Clone() есть только в IPrototype<T>, а не в IPrototype.

Решение: Добавить Clone() в необобщённый интерфейс

```csharp
// Необобщённый интерфейс с Clone()
public interface IPrototype
{
    IPrototype Clone();  // Возвращает необобщённый тип
    void DoSomeStuff();
}

// Обобщённый интерфейс
public interface IPrototype<T> : IPrototype where T : IPrototype<T>
{
    new T Clone();  // Переопределяем с обобщённым типом
}

public class Prototype : IPrototype<Prototype>
{
    // Явная реализация необобщённого
    IPrototype IPrototype.Clone()
    {
        return Clone();
    }

    // Обобщённая реализация
    public Prototype Clone()
    {
        return new Prototype();
    }

    public void DoSomeStuff()
    {
        Console.WriteLine("Doing stuff");
    }
}

public record Container(IPrototype Prototype);

static void NonGeneric()
{
    var container = new Container(new Prototype());
    
    // Теперь работает
    IPrototype clone = container.Prototype.Clone();
    clone.DoSomeStuff();
    
    // Если нужен конкретный тип:
    if (clone is Prototype protoClone)
    {
        // Работаем с конкретным типом
    }
}
```
Соберём всё вместе в реалистичный пример:
```csharp
using System;
using System.Collections.Generic;
using System.Linq;

// === Интерфейсы ===

// Необобщённый (для коллекций)
public interface IDocument
{
    IDocument Clone();
    void Print();
    string GetTitle();
}

// Обобщённый (для типобезопасности)
public interface IDocument<T> : IDocument where T : IDocument<T>
{
    new T Clone();
}

// === Базовый класс ===

public abstract class Document<T> : IDocument<T> where T : Document<T>
{
    protected string _title;
    protected DateTimeOffset _createdAt;
    protected int _version;

    protected Document(string title, DateTimeOffset createdAt, int version)
    {
        _title = title;
        _createdAt = createdAt;
        _version = version;
    }

    // Необобщённый Clone()
    IDocument IDocument.Clone() => Clone();

    // Обобщённый Clone()
    public abstract T Clone();

    public virtual void Print()
    {
        Console.WriteLine($"[{GetType().Name}] {_title}");
        Console.WriteLine($"Created: {_createdAt:dd.MM.yyyy HH:mm:ss}");
        Console.WriteLine($"Version: {_version}");
    }

    public string GetTitle() => _title;

    public void IncrementVersion()
    {
        _version++;
        Console.WriteLine($"Version incremented to {_version}");
    }
}

// === Конкретные типы ===

public class TextDocument : Document<TextDocument>
{
    private string _content;
    private List<string> _tags;

    public TextDocument(string title, DateTimeOffset createdAt, int version, 
                       string content, List<string> tags)
        : base(title, createdAt, version)
    {
        _content = content;
        _tags = tags;
    }

    public override TextDocument Clone()
    {
        // Deep copy
        var clonedTags = _tags.Select(t => t).ToList();
        return new TextDocument(_title, _createdAt, _version, _content, clonedTags);
    }

    public override void Print()
    {
        base.Print();
        Console.WriteLine($"Content: {_content}");
        Console.WriteLine($"Tags: {string.Join(", ", _tags)}");
    }

    public void EditContent(string newContent)
    {
        _content = newContent;
        IncrementVersion();
        Console.WriteLine($"Content updated to: {_content}");
    }

    public void AddTag(string tag)
    {
        _tags.Add(tag);
        Console.WriteLine($"Tag '{tag}' added");
    }
}

public class SpreadsheetDocument : Document<SpreadsheetDocument>
{
    private int _rows;
    private int _columns;
    private Dictionary<string, double> _cells;

    public SpreadsheetDocument(string title, DateTimeOffset createdAt, int version,
                              int rows, int columns)
        : base(title, createdAt, version)
    {
        _rows = rows;
        _columns = columns;
        _cells = new Dictionary<string, double>();
    }

    public override SpreadsheetDocument Clone()
    {
        var clone = new SpreadsheetDocument(_title, _createdAt, _version, _rows, _columns);
        
        // Deep copy cells
        foreach (var kvp in _cells)
        {
            clone._cells[kvp.Key] = kvp.Value;
        }

        return clone;
    }

    public override void Print()
    {
        base.Print();
        Console.WriteLine($"Dimensions: {_rows}x{_columns}");
        Console.WriteLine($"Filled cells: {_cells.Count}");
    }

    public void SetCell(string address, double value)
    {
        _cells[address] = value;
        IncrementVersion();
        Console.WriteLine($"Cell {address} = {value}");
    }

    public double GetCell(string address)
    {
        return _cells.TryGetValue(address, out var value) ? value : 0;
    }
}

// === Менеджер документов (работает с необобщённым интерфейсом) ===

public class DocumentManager
{
    private readonly List<IDocument> _documents = [];

    public void AddDocument(IDocument doc)
    {
        _documents.Add(doc);
        Console.WriteLine($"Added document: {doc.GetTitle()}");
    }

    public void CloneAll()
    {
        Console.WriteLine("\n=== Cloning all documents ===");
        var clones = _documents.Select(d => d.Clone()).ToList();
        
        foreach (var clone in clones)
        {
            Console.WriteLine($"Cloned: {clone.GetTitle()}");
        }
    }

    public void PrintAll()
    {
        Console.WriteLine("\n=== All documents ===");
        foreach (var doc in _documents)
        {
            doc.Print();
            Console.WriteLine();
        }
    }

    public IDocument? GetDocument(string title)
    {
        return _documents.FirstOrDefault(d => d.GetTitle() == title);
    }
}

// === Типобезопасные операции (работают с обобщённым интерфейсом) ===

public class DocumentProcessor
{
    // Типобезопасное клонирование с модификацией
    public static T CloneAndModify<T>(T document, Action<T> modifier) 
        where T : IDocument<T>
    {
        var clone = document.Clone();
        modifier(clone);
        return clone;
    }

    // Создание версии документа
    public static T CreateVersion<T>(T document) 
        where T : IDocument<T>
    {
        var version = document.Clone();
        Console.WriteLine($"Created new version of: {version.GetTitle()}");
        return version;
    }
}

// === Visitor для типобезопасной работы с коллекциями ===

public interface IDocumentVisitor
{
    void Visit(TextDocument doc);
    void Visit(SpreadsheetDocument doc);
}

public class DocumentStatisticsVisitor : IDocumentVisitor
{
    public int TextDocumentCount { get; private set; }
    public int SpreadsheetCount { get; private set; }

    public void Visit(TextDocument doc)
    {
        TextDocumentCount++;
        Console.WriteLine($"Visited text document: {doc.GetTitle()}");
    }

    public void Visit(SpreadsheetDocument doc)
    {
        SpreadsheetCount++;
        Console.WriteLine($"Visited spreadsheet: {doc.GetTitle()}");
    }
}

// Добавляем Accept в интерфейс
public interface IDocumentVisitable
{
    void Accept(IDocumentVisitor visitor);
}

// Обновляем классы документов
public partial class TextDocument : IDocumentVisitable
{
    public void Accept(IDocumentVisitor visitor)
    {
        visitor.Visit(this);
    }
}

public partial class SpreadsheetDocument : IDocumentVisitable
{
    public void Accept(IDocumentVisitor visitor)
    {
        visitor.Visit(this);
    }
}

// === Главная программа ===

public class Program
{
    public static void Main()
    {
        Console.WriteLine("╔═══════════════════════════════════════════════════════╗");
        Console.WriteLine("║  Демонстрация Prototype с рекурсивными дженериками  ║");
        Console.WriteLine("╚═══════════════════════════════════════════════════════╝\n");

        // === Пример 1: Типобезопасное клонирование ===
        Console.WriteLine("▶ Пример 1: Типобезопасное клонирование\n");

        var textDoc = new TextDocument(
            "Report",
            DateTimeOffset.Now,
            1,
            "Initial content",
            new List<string> { "urgent", "finance" }
        );

        // Типобезопасное клонирование — тип сохраняется!
        TextDocument clonedText = DocumentProcessor.CloneAndModify(
            textDoc,
            doc => doc.EditContent("Modified content")
        );

        Console.WriteLine("\nOriginal:");
        textDoc.Print();

        Console.WriteLine("\nClone:");
        clonedText.Print();

        // === Пример 2: Работа через необобщённый интерфейс ===
        Console.WriteLine("\n▶ Пример 2: Коллекция разнородных документов\n");

        var manager = new DocumentManager();
        
        manager.AddDocument(textDoc);
        manager.AddDocument(new SpreadsheetDocument("Budget", DateTimeOffset.Now, 1, 10, 10));
        manager.AddDocument(new TextDocument("Notes", DateTimeOffset.Now, 1, "Some notes", new List<string>()));

        manager.PrintAll();
        manager.CloneAll();

        // === Пример 3: Получение конкретного типа из коллекции ===
        Console.WriteLine("\n▶ Пример 3: Работа с конкретным типом из коллекции\n");

        var doc = manager.GetDocument("Budget");
        if (doc is SpreadsheetDocument spreadsheet)
        {
            spreadsheet.SetCell("A1", 1000);
            spreadsheet.SetCell("B1", 2000);
            spreadsheet.Print();
        }

        // === Пример 4: Visitor для избежания cast'ов ===
        Console.WriteLine("\n▶ Пример 4: Visitor Pattern для типобезопасности\n");

        var statsVisitor = new DocumentStatisticsVisitor();
        
        foreach (var document in new IDocument[] { textDoc, clonedText })
        {
            if (document is IDocumentVisitable visitable)
            {
                visitable.Accept(statsVisitor);
            }
        }

        Console.WriteLine($"\nСтатистика:");
        Console.WriteLine($"  Текстовых документов: {statsVisitor.TextDocumentCount}");
        Console.WriteLine($"  Таблиц: {statsVisitor.SpreadsheetCount}");

        // === Пример 5: Создание версий ===
        Console.WriteLine("\n▶ Пример 5: Версионирование документов\n");

        var version1 = textDoc;
        var version2 = DocumentProcessor.CreateVersion(version1);
        version2.EditContent("Version 2 content");

        var version3 = DocumentProcessor.CreateVersion(version2);
        version3.AddTag("reviewed");
        version3.EditContent("Version 3 content");

        Console.WriteLine("\nВерсия 1:");
        version1.Print();

        Console.WriteLine("\nВерсия 2:");
        version2.Print();

        Console.WriteLine("\nВерсия 3:");
        version3.Print();

        Console.WriteLine("\n╔═══════════════════════════════════════════════════════╗");
        Console.WriteLine("║                 Демонстрация завершена                ║");
        Console.WriteLine("╚═══════════════════════════════════════════════════════╝");
    }
}
```

Выводы
Когда использовать какой подход:
1. Простое наследование (abstract class Prototype):

✅ Простая иерархия без сложной типизации

✅ Все методы общие для всех классов

❌ Теряете конкретный тип при возврате из методов

2. Интерфейсы (IPrototype):

✅ Множественное наследование

✅ Гибкая композиция

❌ Всё ещё теряете тип без дженериков

3. Рекурсивные дженерики (IPrototype<T> where T : IPrototype<T>):

✅ Типобезопасность на уровне компиляции

✅ Не нужны cast'ы при работе через обобщённые методы

✅ Компилятор помогает избежать ошибок

❌ Сложнее понять и реализовать

❌ Проблемы с разнородными коллекциями

4. Комбинация обобщённых и необобщённых интерфейсов:

✅ Типобезопасность когда нужно

✅ Возможность работать с коллекциями

✅ Best of both worlds

❌ Больше кода

❌ Требует понимания явной реализации интерфейсов

Золотое правило:
Используйте рекурсивные дженерики, когда:

- Работаете с типобезопасными операциями

- Нужно сохранять конкретный тип через цепочку вызовов

- Хотите, чтобы компилятор помогал избегать ошибок

Добавляйте необобщённый базовый интерфейс, когда:

- Нужно хранить разные типы в одной коллекции

- Работаете с плагинами/расширениями

- Используете Visitor или другие паттерны для типобезопасности
