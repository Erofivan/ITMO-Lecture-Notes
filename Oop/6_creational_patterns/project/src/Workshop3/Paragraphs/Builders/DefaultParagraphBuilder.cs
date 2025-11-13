namespace Workshop3.Paragraphs.Builders;

internal sealed class DefaultParagraphBuilder : ParagraphBuilderBase
{
    public override IParagraph Build()
    {
        return new DefaultParagraph(
            Title ?? throw new ArgumentNullException(nameof(Title)),
            Content,
            Footer);
    }
}
