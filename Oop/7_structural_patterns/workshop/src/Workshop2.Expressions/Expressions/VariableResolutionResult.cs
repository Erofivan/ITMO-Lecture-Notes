namespace Workshop2.Expressions.Expressions;

public abstract record VariableResolutionResult
{
    private VariableResolutionResult() { }

    public sealed record Found(double Value) : VariableResolutionResult;

    public sealed record NotFound : VariableResolutionResult;
}
