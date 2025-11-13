using Workshop3.Paragraphs.Builders;

namespace Workshop3.Paragraphs.Factories;

public sealed class DefaultParagraphBuilderFactory : IParagraphBuilderFactory
{
    public IParagraphTitleSelector CreateBuilder() 
        => new DefaultParagraphBuilder();
}
