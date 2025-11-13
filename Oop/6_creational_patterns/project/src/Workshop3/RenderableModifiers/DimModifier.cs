namespace Workshop3.RenderableModifiers;

public sealed class DimModifier : IRenderableModifier
{
    public string Apply(string value) 
        => Crayon.Output.Dim(value);
}
