using System.Drawing;
using Workshop3.RenderableModifiers;

namespace Workshop3.Renderables;

public static class TextExtensions
{
    public static T Bold<T>(this T text)
        where T : IText<T>
    {
        T copy = text.Clone();
        copy.AddModifier(new BoldModifier());

        return copy;
    }

    public static T Colored<T>(this T text, Color color)
        where T : IText<T>
    {
        T copy = text.Clone();
        copy.AddModifier(new ColorModifer(color));

        return copy;
    }
}
