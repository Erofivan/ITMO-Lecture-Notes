using Workshop3.RenderableModifiers;

namespace Workshop3.Paragraphs.Builders;

internal sealed class StyledParagraphBuilder(IRenderableModifier modifier)
    : ParagraphBuilderBase
{
    public override IParagraph Build()
    {
        var paragraph = new DefaultParagraph(
            Title ?? throw new ArgumentNullException(nameof(Title)),
            Content,
            Footer);

        return new StyledParagraph(paragraph, modifier);
    }
}
