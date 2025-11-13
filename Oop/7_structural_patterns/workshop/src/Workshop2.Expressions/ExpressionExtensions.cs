// Файл содержит extension methods для удобной работы с выражениями.
// Это реализация паттерна Fluent API, который позволяет писать
// выразительный и читаемый код построения выражений.

using Workshop2.Expressions.BinaryOperators;
using Workshop2.Expressions.Expressions;

namespace Workshop2.Expressions;

// Класс ExpressionExtensions предоставляет методы-расширения для IExpression.
// Паттерн Fluent API (текучий интерфейс): позволяет строить выражения
// через цепочку вызовов методов, что делает код похожим на естественный язык.
//
// Сравним два подхода:
//
// Без Fluent API:
// var expr = new BinaryOperatorExpression(
//     new BinaryOperatorExpression(
//         new VariableExpression("x"),
//         new ConstantExpression(1),
//         new SumOperator()),
//     new VariableExpression("y"),
//     new MultiplyOperator());
//
// С Fluent API:
// var expr = new VariableExpression("x")
//     .Add(new ConstantExpression(1))
//     .Multiply(new VariableExpression("y"));
//
// Видно, что второй вариант значительно более читаемый и понятный.
public static class ExpressionExtensions
{
    // Создаёт выражение сложения двух выражений.
    // Параметры:
    //   left — левое выражение (this параметр — расширяемый тип)
    //   right — правое выражение
    // Возвращает: новое выражение, представляющее (left + right)
    //
    // Extension method позволяет вызывать как: left.Add(right)
    // вместо: ExpressionExtensions.Add(left, right)
    //
    // Пример использования:
    // new VariableExpression("x").Add(new ConstantExpression(5))
    // эквивалентно: x + 5
    public static IExpression Add(this IExpression left, IExpression right)
    {
        return new BinaryOperatorExpression(
            left,
            right,
            new SumOperator());
    }

    // Создаёт выражение умножения двух выражений.
    // Параметры:
    //   left — левое выражение (this параметр)
    //   right — правое выражение
    // Возвращает: новое выражение, представляющее (left * right)
    //
    // Особенность: здесь преподаватель демонстрирует применение паттерна Proxy.
    // MultiplyOperator обёрнут в CachingBinaryOperatorProxy, что означает,
    // что результаты умножения будут кешироваться.
    //
    // Это сделано намеренно для демонстрации:
    // 1. Как легко добавить кеширование без изменения кода оператора
    // 2. Разницы в поведении: сложение не кешируется, умножение — кешируется
    // 3. Гибкости подхода: можно выбирать, какие операции кешировать
    //
    // Пример использования:
    // new VariableExpression("x").Multiply(new ConstantExpression(2))
    // эквивалентно: x * 2
    public static IExpression Multiply(this IExpression left, IExpression right)
    {
        return new BinaryOperatorExpression(
            left,
            right,
            new CachingBinaryOperatorProxy(new MultiplyOperator()));
    }

    // Создаёт выражение отрицания.
    // Параметры:
    //   expression — выражение для отрицания (this параметр)
    // Возвращает: новое выражение, представляющее (-expression)
    //
    // Применяет паттерн Decorator через удобный метод-расширение.
    //
    // Пример использования:
    // new VariableExpression("x").Negate()
    // эквивалентно: -x
    //
    // Можно комбинировать с другими операциями:
    // new VariableExpression("x").Add(new ConstantExpression(1)).Negate()
    // эквивалентно: -(x + 1)
    public static IExpression Negate(this IExpression expression)
    {
        return new NegativeExpressionDecorator(expression);
    }
}
