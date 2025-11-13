namespace Workshop3.Paragraphs.Builders;

internal abstract class ParagraphBuilderBase :
    IParagraphTitleSelector,
    IParagraphBuilder
{
    private readonly List<IRenderable> _content = [];

    protected IRenderable? Title { get; private set; }
    protected IEnumerable<IRenderable> Content => _content;
    protected IRenderable? Footer { get; private set; }

    public IParagraphBuilder WithTitle(IRenderable title)
    {
        Title = title;
        return this;
    }

    public IParagraphBuilder AddContent(IRenderable content)
    {
        _content.Add(content);
        return this;
    }

    public IParagraphBuilder WithFooter(IRenderable footer)
    {
        Footer = footer;
        return this;
    }

    public abstract IParagraph Build();
}
