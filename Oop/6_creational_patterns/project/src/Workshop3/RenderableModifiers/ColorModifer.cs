using System.Drawing;

namespace Workshop3.RenderableModifiers;

public sealed class ColorModifer(Color color) : IRenderableModifier
{
    public string Apply(string value)
        => Crayon.Output.Rgb(color.R, color.G, color.B).Text(value);
}
