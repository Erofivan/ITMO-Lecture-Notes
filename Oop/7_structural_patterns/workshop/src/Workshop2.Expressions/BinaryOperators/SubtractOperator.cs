// Файл реализует оператор вычитания как конкретную стратегию.
// Аналогичен SumOperator, но выполняет операцию вычитания.

namespace Workshop2.Expressions.BinaryOperators;

// Класс SubtractOperator реализует операцию вычитания.
// Паттерн Strategy: конкретная стратегия для операции вычитания.
// sealed гарантирует, что это финальная реализация без возможности наследования.
public sealed class SubtractOperator : IBinaryOperator
{
    // Выполняет вычитание одного числа из другого.
    // Параметры:
    //   left — уменьшаемое
    //   right — вычитаемое
    // Возвращает: разность left - right
    public double Apply(double left, double right)
        => left - right;

    // Возвращает символ операции для отображения в выражениях.
    // Возвращает: строку "-"
    public string Format()
        => "-";
}
