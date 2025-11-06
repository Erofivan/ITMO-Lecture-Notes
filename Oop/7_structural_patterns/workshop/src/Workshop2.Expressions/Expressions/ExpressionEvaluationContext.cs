namespace Workshop2.Expressions.Expressions;

public sealed class ExpressionEvaluationContext : IExpressionEvaluationContext
{
    private readonly Dictionary<string, double> _variableValues = [];

    public VariableResolutionResult ResolveVariable(string variableName)
    {
        return _variableValues.TryGetValue(variableName, out double value)
            ? new VariableResolutionResult.Found(value)
            : new VariableResolutionResult.NotFound();
    }

    public ExpressionEvaluationContext AddVariable(string variableName, double value)
    {
        _variableValues[variableName] = value;
        return this;
    }
}
