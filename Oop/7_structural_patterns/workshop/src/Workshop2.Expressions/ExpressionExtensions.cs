using Workshop2.Expressions.BinaryOperators;
using Workshop2.Expressions.Expressions;

namespace Workshop2.Expressions;

public static class ExpressionExtensions
{
    public static IExpression Add(this IExpression left, IExpression right)
    {
        return new BinaryOperatorExpression(
            left,
            right,
            new SumOperator());
    }

    public static IExpression Multiply(this IExpression left, IExpression right)
    {
        return new BinaryOperatorExpression(
            left,
            right,
            new CachingBinaryOperatorProxy(new MultiplyOperator()));
    }

    public static IExpression Negate(this IExpression expression)
    {
        return new NegativeExpressionDecorator(expression);
    }
}
