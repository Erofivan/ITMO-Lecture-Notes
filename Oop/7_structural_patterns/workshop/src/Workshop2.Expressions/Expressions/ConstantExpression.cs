using System.Globalization;

namespace Workshop2.Expressions.Expressions;

public sealed class ConstantExpression : IExpressionValue
{
    public ConstantExpression(double value)
    {
        Value = value;
    }

    public double Value { get; }

    public ExpressionEvaluationResult Evaluate(IExpressionEvaluationContext context)
        => new ExpressionEvaluationResult.Full(this);

    public string Format()
        => Value.ToString(CultureInfo.InvariantCulture);
}
