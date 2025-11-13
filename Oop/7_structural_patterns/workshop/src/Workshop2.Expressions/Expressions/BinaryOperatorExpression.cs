// Файл реализует бинарную операцию над двумя выражениями.
// Это ключевой класс для построения составных выражений.
// Демонстрирует паттерны Composite и Strategy в действии.

using System.Diagnostics;
using Workshop2.Expressions.BinaryOperators;

namespace Workshop2.Expressions.Expressions;

// Класс BinaryOperatorExpression представляет бинарную операцию над двумя выражениями.
// Паттерн Composite: это "узел" в дереве выражений, который содержит два дочерних
// выражения (левое и правое) и оператор для их комбинирования.
// Паттерн Strategy: использует IBinaryOperator для выбора конкретной операции.
//
// Примеры:
// - "(x + 1)" — сложение переменной и константы
// - "((2 * 3) - y)" — вложенная структура операций
//
// Ключевая особенность: рекурсивная структура. BinaryOperatorExpression может
// содержать другие BinaryOperatorExpression в качестве дочерних элементов,
// что позволяет строить выражения произвольной сложности.
public sealed class BinaryOperatorExpression : IExpression
{
    // Левое выражение (первый операнд).
    private readonly IExpression _left;
    
    // Правое выражение (второй операнд).
    private readonly IExpression _right;
    
    // Оператор, который будет применён к результатам вычисления левого и правого выражений.
    // Паттерн Strategy: оператор определяет, как именно комбинировать значения.
    private readonly IBinaryOperator _binaryOperator;

    // Конструктор создаёт бинарное выражение.
    // Параметры:
    //   left — левое выражение (первый операнд)
    //   right — правое выражение (второй операнд)
    //   binaryOperator — оператор для выполнения операции
    public BinaryOperatorExpression(IExpression left, IExpression right, IBinaryOperator binaryOperator)
    {
        _left = left;
        _right = right;
        _binaryOperator = binaryOperator;
    }

    // Вычисляет бинарное выражение, применяя оператор к результатам дочерних выражений.
    // Параметры:
    //   context — контекст с переменными
    // Возвращает: Full, если оба операнда вычислены полностью,
    //             или Partial, если хотя бы один операнд не может быть вычислен
    //
    // Логика работы (рекурсивное вычисление):
    // 1. Вычисляем левое выражение — получаем Full или Partial
    // 2. Вычисляем правое выражение — получаем Full или Partial
    // 3. Анализируем комбинацию результатов:
    //
    //    a) (Full, Full) — оба операнда вычислены полностью:
    //       Применяем оператор к их значениям и получаем константу.
    //       Результат: Full с новой константой.
    //       Пример: (2 + 3) => Full(5)
    //
    //    b) (Full, Partial) — левый вычислен, правый нет:
    //       Не можем получить конечное значение, но можем упростить выражение,
    //       заменив левую часть на вычисленную константу.
    //       Результат: Partial с упрощённым выражением.
    //       Пример: (2 + x) где x неизвестна => Partial(2 + x)
    //
    //    c) (Partial, Full) — правый вычислен, левый нет:
    //       Аналогично случаю b), но упрощаем правую часть.
    //       Пример: (x + 2) где x неизвестна => Partial(x + 2)
    //
    //    d) (Partial, Partial) — оба не вычислены полностью:
    //       Результат: Partial с обновлёнными операндами (они могли упроститься).
    //       Пример: (x + y) где обе неизвестны => Partial(x + y)
    //
    // Pattern matching через switch expression делает эту логику явной и читаемой.
    // Tuple deconstruction позволяет красиво работать с парой результатов.
    public ExpressionEvaluationResult Evaluate(IExpressionEvaluationContext context)
    {
        return (_left.Evaluate(context), _right.Evaluate(context)) switch
        {
            (ExpressionEvaluationResult.Full l, ExpressionEvaluationResult.Full r)
                => new ExpressionEvaluationResult.Full(new ConstantExpression(
                    _binaryOperator.Apply(l.Value.Value, r.Value.Value))),

            (ExpressionEvaluationResult.Full l, ExpressionEvaluationResult.Partial r)
                => new ExpressionEvaluationResult.Partial(new BinaryOperatorExpression(
                    l.Value,
                    r.Value,
                    _binaryOperator)),

            (ExpressionEvaluationResult.Partial l, ExpressionEvaluationResult.Full r)
                => new ExpressionEvaluationResult.Partial(new BinaryOperatorExpression(
                    l.Value,
                    r.Value,
                    _binaryOperator)),

            (ExpressionEvaluationResult.Partial l, ExpressionEvaluationResult.Partial r)
                => new ExpressionEvaluationResult.Partial(new BinaryOperatorExpression(
                    l.Value,
                    r.Value,
                    _binaryOperator)),

            _ => throw new UnreachableException(),
        };
    }

    // Возвращает строковое представление выражения.
    // Возвращает: строку вида "(left operator right)"
    //
    // Рекурсивно форматирует всё выражение:
    // - Вызывает Format() для левого выражения
    // - Добавляет символ оператора
    // - Вызывает Format() для правого выражения
    // - Оборачивает всё в скобки для явного указания приоритета операций
    //
    // Пример: "(x + 1)" или "((2 * 3) + y)"
    public string Format()
    {
        return $"({_left.Format()} {_binaryOperator.Format()} {_right.Format()})";
    }
}
