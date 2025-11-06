using Workshop2.Expressions;
using Workshop2.Expressions.Expressions;

ExpressionEvaluationContext context = new ExpressionEvaluationContext()
    .AddVariable("x", 2)
    .AddVariable("y", 3);

IExpression expression = new VariableExpression("x")
    .Add(new ConstantExpression(1))
    .Multiply(new VariableExpression("y").Negate());

// c = (x * y) + a
// a = c + 2 

var c = new VariableExpression("x").Multiply(new VariableExpression("y"))
    .Add(new VariableExpression("a"))
    .Evaluate(context)
    .Expression;
var a = c.Add(new ConstantExpression(2));

Console.WriteLine(expression.Evaluate(context));
Console.WriteLine(expression.Evaluate(context));
