namespace Workshop2.Expressions.Expressions;

public abstract record ExpressionEvaluationResult
{
    private ExpressionEvaluationResult() { }

    public abstract IExpression Expression { get; }

    public sealed record Full(IExpressionValue Value) : ExpressionEvaluationResult
    {
        public override IExpression Expression => Value;

        public override string ToString()
            => Value.Format();
    }

    public sealed record Partial(IExpression Value) : ExpressionEvaluationResult
    {
        public override IExpression Expression => Value;

        public override string ToString()
            => Value.Format();
    }
}
