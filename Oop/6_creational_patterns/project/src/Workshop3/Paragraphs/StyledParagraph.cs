using Workshop3.RenderableModifiers;

namespace Workshop3.Paragraphs;

internal sealed class StyledParagraph : IParagraph
{
    private readonly IParagraph _paragraph;
    private readonly IRenderableModifier _modifier;

    public StyledParagraph(IParagraph paragraph, IRenderableModifier modifier)
    {
        _paragraph = paragraph;
        _modifier = modifier;
    }

    public string Render()
    {
        return _modifier.Apply(_paragraph.Render());
    }
}
