// Файл реализует контекст вычисления выражений.
// Контекст хранит словарь переменных и их значений,
// необходимых для вычисления математических выражений.

namespace Workshop2.Expressions.Expressions;

// Класс ExpressionEvaluationContext — конкретная реализация контекста вычисления.
// Хранит значения переменных и предоставляет доступ к ним при вычислении выражений.
//
// Паттерн Context Object: инкапсулирует состояние и данные, необходимые
// для выполнения операции (в данном случае — вычисления выражения).
//
// Использование:
// 1. Создаём контекст
// 2. Добавляем в него переменные с помощью AddVariable
// 3. Передаём контекст в метод Evaluate выражения
// 4. Выражение обращается к контексту для получения значений переменных
public sealed class ExpressionEvaluationContext : IExpressionEvaluationContext
{
    // Словарь для хранения переменных и их значений.
    // Ключ — имя переменной (строка), значение — числовое значение (double).
    private readonly Dictionary<string, double> _variableValues = [];

    // Разрешает переменную по имени, возвращая её значение или информацию об отсутствии.
    // Параметры:
    //   variableName — имя переменной для поиска
    // Возвращает: Found со значением, если переменная есть в контексте,
    //             или NotFound, если переменной нет
    //
    // Реализация использует тернарный оператор и TryGetValue для эффективного
    // и безопасного поиска в словаре без исключений.
    public VariableResolutionResult ResolveVariable(string variableName)
    {
        return _variableValues.TryGetValue(variableName, out double value)
            ? new VariableResolutionResult.Found(value)
            : new VariableResolutionResult.NotFound();
    }

    // Добавляет или обновляет переменную в контексте.
    // Параметры:
    //   variableName — имя переменной
    //   value — числовое значение переменной
    // Возвращает: this для поддержки fluent API (цепочки вызовов)
    //
    // Fluent API позволяет писать код в удобном стиле:
    // context.AddVariable("x", 1).AddVariable("y", 2).AddVariable("z", 3);
    //
    // Это делает код настройки контекста более читаемым и выразительным.
    public ExpressionEvaluationContext AddVariable(string variableName, double value)
    {
        _variableValues[variableName] = value;
        return this;
    }
}
