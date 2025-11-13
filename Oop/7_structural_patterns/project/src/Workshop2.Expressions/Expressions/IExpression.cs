namespace Workshop2.Expressions.Expressions;

public interface IExpression
{
    string Format();

    ExpressionEvaluationResult Evaluate(IExpressionEvaluationContext context);
}
