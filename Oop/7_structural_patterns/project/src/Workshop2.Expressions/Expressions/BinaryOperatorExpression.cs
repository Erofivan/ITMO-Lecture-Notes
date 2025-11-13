using System.Diagnostics;
using Workshop2.Expressions.BinaryOperators;

namespace Workshop2.Expressions.Expressions;

public sealed class BinaryOperatorExpression : IExpression
{
    private readonly IExpression _left;
    private readonly IExpression _right;
    private readonly IBinaryOperator _binaryOperator;

    public BinaryOperatorExpression(IExpression left, IExpression right, IBinaryOperator binaryOperator)
    {
        _left = left;
        _right = right;
        _binaryOperator = binaryOperator;
    }

    public ExpressionEvaluationResult Evaluate(IExpressionEvaluationContext context)
    {
        return (_left.Evaluate(context), _right.Evaluate(context)) switch
        {
            (ExpressionEvaluationResult.Full l, ExpressionEvaluationResult.Full r)
                => new ExpressionEvaluationResult.Full(new ConstantExpression(
                    _binaryOperator.Apply(l.Value.Value, r.Value.Value))),

            (ExpressionEvaluationResult.Full l, ExpressionEvaluationResult.Partial r)
                => new ExpressionEvaluationResult.Partial(new BinaryOperatorExpression(
                    l.Value,
                    r.Value,
                    _binaryOperator)),

            (ExpressionEvaluationResult.Partial l, ExpressionEvaluationResult.Full r)
                => new ExpressionEvaluationResult.Partial(new BinaryOperatorExpression(
                    l.Value,
                    r.Value,
                    _binaryOperator)),

            (ExpressionEvaluationResult.Partial l, ExpressionEvaluationResult.Partial r)
                => new ExpressionEvaluationResult.Partial(new BinaryOperatorExpression(
                    l.Value,
                    r.Value,
                    _binaryOperator)),

            _ => throw new UnreachableException(),
        };
    }

    public string Format()
    {
        return $"({_left.Format()} {_binaryOperator.Format()} {_right.Format()})";
    }
}
