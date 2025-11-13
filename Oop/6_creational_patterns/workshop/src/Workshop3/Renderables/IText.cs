using Workshop3.RenderableModifiers;

namespace Workshop3.Renderables;

public interface IText : IRenderable
{
    string Value { get; }

    void AddModifier(IRenderableModifier modifier);
}

public interface IText<TSelf> : IText
    where TSelf : IText<TSelf>
{
    TSelf Clone();
}
