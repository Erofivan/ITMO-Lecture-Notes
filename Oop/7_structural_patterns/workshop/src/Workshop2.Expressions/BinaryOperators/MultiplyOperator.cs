namespace Workshop2.Expressions.BinaryOperators;

public sealed class MultiplyOperator : IBinaryOperator
{
    public double Apply(double left, double right)
    {
        Console.WriteLine("Multiplying...");

        return left * right;
    }

    public string Format()
        => "*";
}
