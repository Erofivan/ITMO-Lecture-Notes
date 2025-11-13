using Workshop3.Paragraphs;

namespace Workshop3.Articles;

public sealed class ArticleBuilder : IArticleBuilder
{
    private IRenderable? _title;
    private readonly List<IParagraph> _paragraphs = [];
    private IRenderable? _author;

    public IArticleBuilder WithTitle(IRenderable title)
    {
        _title = title;
        return this;
    }

    public IArticleBuilder AddParagraph(IParagraph paragraph)
    {
        _paragraphs.Add(paragraph);
        return this;
    }

    public IArticleBuilder WithAuthor(IRenderable author)
    {
        _author = author;
        return this;
    }

    public IArticle Build()
    {
        return new Article(
            _title ?? throw new ArgumentNullException(nameof(_title)),
            _paragraphs.ToArray(),
            _author);
    }
}
