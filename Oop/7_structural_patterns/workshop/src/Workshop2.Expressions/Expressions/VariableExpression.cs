// Файл реализует выражение-переменную.
// Переменная — это именованный placeholder, значение которого
// определяется в контексте вычисления.

using System.Diagnostics;

namespace Workshop2.Expressions.Expressions;

// Класс VariableExpression представляет переменную в математическом выражении.
// Паттерн Composite: это "лист" в дереве выражений, аналогично ConstantExpression,
// но с отложенным вычислением — значение берётся из контекста.
//
// Примеры: x, y, myVariable
//
// Ключевое отличие от константы: значение переменной не известно заранее,
// оно определяется только в момент вычисления из контекста.
// Если переменная есть в контексте — вычисление успешно (Full),
// если нет — вычисление частичное (Partial).
public sealed class VariableExpression : IExpression
{
    // Имя переменной, хранится для поиска значения в контексте.
    private readonly string _variableName;

    // Конструктор создаёт выражение-переменную с заданным именем.
    // Параметры:
    //   variableName — имя переменной (например, "x", "y")
    public VariableExpression(string variableName)
    {
        _variableName = variableName;
    }

    // Вычисляет переменную, получая её значение из контекста.
    // Параметры:
    //   context — контекст, содержащий значения переменных
    // Возвращает: Full с константой, если переменная найдена,
    //             или Partial с самой собой, если не найдена
    //
    // Логика работы:
    // 1. Спрашиваем контекст: "Знаешь ли ты значение переменной с таким именем?"
    // 2. Если да (Found) — создаём константу с этим значением и возвращаем Full
    // 3. Если нет (NotFound) — возвращаем Partial с самой переменной,
    //    показывая, что это выражение не может быть вычислено в текущем контексте
    //
    // Pattern matching (switch expression) делает код выразительным:
    // явно видны все возможные варианты и их обработка.
    public ExpressionEvaluationResult Evaluate(IExpressionEvaluationContext context)
    {
        return context.ResolveVariable(_variableName) switch
        {
            VariableResolutionResult.Found found
                => new ExpressionEvaluationResult.Full(new ConstantExpression(found.Value)),

            VariableResolutionResult.NotFound
                => new ExpressionEvaluationResult.Partial(this),

            _ => throw new UnreachableException(),
        };
    }

    // Возвращает строковое представление переменной.
    // Возвращает: имя переменной
    //
    // Просто возвращаем имя переменной, чтобы при выводе выражения
    // было видно, что это переменная, а не константа.
    public string Format()
        => _variableName;
}
