namespace Workshop2.Expressions.BinaryOperators;

public interface IBinaryOperator
{
    double Apply(double left, double right);

    string Format();
}
