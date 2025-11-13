// Файл реализует отрицание выражения через паттерн Decorator.
// Это классический пример применения Decorator для добавления функциональности
// (математическое отрицание) к существующему выражению без изменения его кода.

using System.Diagnostics;

namespace Workshop2.Expressions.Expressions;

// Класс NegativeExpressionDecorator реализует унарную операцию отрицания.
// Паттерн Decorator: обёртывает существующее выражение, добавляя функциональность
// отрицания без изменения оригинального выражения.
//
// Примеры:
// - "-x" — отрицание переменной
// - "-(2 + 3)" — отрицание составного выражения
// - "-(-x)" — двойное отрицание
//
// Ключевая идея Decorator: вместо создания отдельных классов для каждой
// комбинации (NegativeConstant, NegativeVariable, NegativeBinaryExpression...),
// создаём один класс-обёртку, который работает с любым IExpression.
// Это позволяет гибко комбинировать функциональность.
public sealed class NegativeExpressionDecorator : IExpression
{
    // Обёрнутое выражение, которое будет отрицаться.
    // Decorator держит ссылку на декорируемый объект.
    private readonly IExpression _expression;

    // Конструктор создаёт декоратор для заданного выражения.
    // Параметры:
    //   expression — выражение, которое нужно отрицать
    public NegativeExpressionDecorator(IExpression expression)
    {
        _expression = expression;
    }

    // Вычисляет отрицание выражения.
    // Параметры:
    //   context — контекст с переменными
    // Возвращает: Full с отрицательным значением, если выражение вычислено полностью,
    //             или Partial с декоратором отрицания, если выражение не вычислено
    //
    // Логика работы:
    // 1. Вычисляем обёрнутое выражение
    // 2. Анализируем результат:
    //
    //    a) Full — выражение вычислено полностью:
    //       Берём его значение, применяем унарный минус и создаём новую константу.
    //       Результат: Full с отрицательным значением.
    //       Пример: -(2 + 3) => -5
    //
    //    b) Partial — выражение не вычислено полностью:
    //       Не можем вычислить конечное значение, но можем упростить
    //       внутреннее выражение. Создаём новый декоратор с упрощённым выражением.
    //       Результат: Partial с обновлённым декоратором.
    //       Пример: -(x + 2) где x неизвестна => Partial(-(x + 2))
    //
    // Паттерн Decorator в действии: делегируем вычисление обёрнутому объекту,
    // а затем модифицируем результат (применяем отрицание).
    public ExpressionEvaluationResult Evaluate(IExpressionEvaluationContext context)
    {
        return _expression.Evaluate(context) switch
        {
            ExpressionEvaluationResult.Full full => new ExpressionEvaluationResult.Full(
                new ConstantExpression(-full.Value.Value)),

            ExpressionEvaluationResult.Partial partial => new ExpressionEvaluationResult.Partial(
                new NegativeExpressionDecorator(partial.Value)),

            _ => throw new UnreachableException(),
        };
    }

    // Возвращает строковое представление отрицания.
    // Возвращает: строку вида "-expression"
    //
    // Делегируем форматирование обёрнутому выражению и добавляем знак минус.
    // Пример: "-x" или "-(2 + 3)"
    public string Format()
    {
        return $"-{_expression.Format()}";
    }
}
