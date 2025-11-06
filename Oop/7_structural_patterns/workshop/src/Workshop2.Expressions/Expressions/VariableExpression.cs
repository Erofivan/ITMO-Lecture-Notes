using System.Diagnostics;

namespace Workshop2.Expressions.Expressions;

public sealed class VariableExpression : IExpression
{
    private readonly string _variableName;

    public VariableExpression(string variableName)
    {
        _variableName = variableName;
    }

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

    public string Format()
        => _variableName;
}
