using System.Diagnostics;

namespace Workshop2.Expressions.Expressions;

public sealed class NegativeExpressionDecorator : IExpression
{
    private readonly IExpression _expression;

    public NegativeExpressionDecorator(IExpression expression)
    {
        _expression = expression;
    }

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

    public string Format()
    {
        return $"-{_expression.Format()}";
    }
}
