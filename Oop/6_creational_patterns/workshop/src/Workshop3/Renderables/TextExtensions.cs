// Extension-методы для удобного создания стилизованных текстовых объектов
//
// Роль в проекте:
// Предоставляет fluent API для применения стилей к тексту. Вместо явного создания
// модификаторов и вызова AddModifier(), можно использовать цепочку вызовов:
//   new Text("Hello").Bold().Colored(Color.Red)
//
// Применяемые паттерны и концепции:
// 1. Prototype (Прототип) - каждый метод клонирует объект перед модификацией
// 2. Fluent Interface - методы возвращают модифицированный объект, позволяя цепочки вызовов
// 3. Extension Methods - расширение функциональности без изменения исходного класса
//
// Почему клонирование:
// Методы создают копию исходного объекта, чтобы не изменять оригинал. Это важно для
// предотвращения побочных эффектов и обеспечивает immutability API. Когда мы пишем
// text.Bold(), мы ожидаем получить новый жирный текст, а не изменить существующий.
//
// Связь с другими компонентами:
// - Работает с любым типом, реализующим IText<T>
// - Использует конкретные реализации модификаторов (BoldModifier, ColorModifier)
// - Активно применяется в Program.cs для создания стилизованного контента
using System.Drawing;
using Workshop3.RenderableModifiers;

namespace Workshop3.Renderables;

public static class TextExtensions
{
    // Создает копию текста с примененным жирным начертанием
    //
    // Параметры:
    //   text - исходный текстовый объект
    //
    // Возвращает: новый объект того же типа с добавленным BoldModifier
    //
    // Generic-параметр T с ограничением IText<T> обеспечивает type-safety:
    // метод работает с любым текстовым типом и возвращает тот же тип.
    // Например, если передать Text, вернется Text, а не просто IText.
    //
    // Процесс:
    // 1. Клонируем исходный объект (паттерн Prototype)
    // 2. Добавляем BoldModifier к копии
    // 3. Возвращаем модифицированную копию
    //
    // Пример использования:
    //   Text boldText = new Text("Hello").Bold();
    public static T Bold<T>(this T text)
        where T : IText<T>
    {
        T copy = text.Clone();
        copy.AddModifier(new BoldModifier());

        return copy;
    }

    // Создает копию текста с примененным цветом
    //
    // Параметры:
    //   text - исходный текстовый объект
    //   color - цвет для применения (System.Drawing.Color)
    //
    // Возвращает: новый объект того же типа с добавленным ColorModifier
    //
    // Использование System.Drawing.Color предоставляет удобный API с предопределенными
    // цветами (Color.Red, Color.Blue) и возможностью создания пользовательских цветов
    // через RGB-значения.
    //
    // Процесс аналогичен Bold():
    // 1. Клонируем объект
    // 2. Добавляем ColorModifier с указанным цветом
    // 3. Возвращаем модифицированную копию
    //
    // Пример использования:
    //   Text redText = new Text("Error").Colored(Color.Red);
    //   Text customText = new Text("Info").Colored(Color.FromArgb(100, 150, 200));
    //
    // Комбинирование с другими модификаторами:
    //   Text styledText = new Text("Important").Bold().Colored(Color.Red);
    public static T Colored<T>(this T text, Color color)
        where T : IText<T>
    {
        T copy = text.Clone();
        copy.AddModifier(new ColorModifer(color));

        return copy;
    }
}
