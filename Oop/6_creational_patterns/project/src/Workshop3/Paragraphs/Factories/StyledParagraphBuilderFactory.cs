using Workshop3.Paragraphs.Builders;
using Workshop3.RenderableModifiers;

namespace Workshop3.Paragraphs.Factories;

public sealed class StyledParagraphBuilderFactory(IRenderableModifier modifier)
    : IParagraphBuilderFactory
{
    public IParagraphTitleSelector CreateBuilder()
    {
        return new StyledParagraphBuilder(modifier);
    }
}
