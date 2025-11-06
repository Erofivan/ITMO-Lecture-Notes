namespace Workshop2.Expressions.Expressions;

public interface IExpressionEvaluationContext
{
    VariableResolutionResult ResolveVariable(string variableName);
}
