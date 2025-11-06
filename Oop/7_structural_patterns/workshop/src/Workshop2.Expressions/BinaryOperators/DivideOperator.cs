namespace Workshop2.Expressions.BinaryOperators;

public sealed class DivideOperator : IBinaryOperator
{
    public double Apply(double left, double right)
        => left / right;

    public string Format()
        => "/";
}
