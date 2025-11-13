namespace Workshop3.RenderableModifiers;

public sealed class BoldModifier : IRenderableModifier
{
    public string Apply(string value) 
        => Crayon.Output.Bold(value);
}
