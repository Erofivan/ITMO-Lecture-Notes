namespace Workshop2.Expressions.BinaryOperators;

public sealed class CachingBinaryOperatorProxy : IBinaryOperator
{
    private readonly IBinaryOperator _binaryOperator;
    private readonly Dictionary<Key, double> _cache;

    public CachingBinaryOperatorProxy(IBinaryOperator binaryOperator)
    {
        _binaryOperator = binaryOperator;
        _cache = [];
    }

    public double Apply(double left, double right)
    {
        var key = new Key(left, right);

        if (_cache.TryGetValue(key, out double value))
            return value;

        return _cache[key] = _binaryOperator.Apply(left, right);
    }

    public string Format()
        => _binaryOperator.Format();

    private readonly record struct Key(double Left, double Right);
}
