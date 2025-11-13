using System.Text;
using Workshop3.Paragraphs;

namespace Workshop3.Articles;

public sealed class Article : IArticle
{
    private readonly IRenderable _title;
    private readonly IEnumerable<IParagraph> _paragraphs;
    private readonly IRenderable? _author;

    public Article(
        IRenderable title,
        IEnumerable<IParagraph> paragraphs,
        IRenderable? author)
    {
        _title = title;
        _paragraphs = paragraphs;
        _author = author;
    }

    public IArticleBuilder Direct(IArticleBuilder builder)
    {
        builder = builder.WithTitle(_title);

        foreach (IParagraph paragraph in _paragraphs)
        {
            builder = builder.AddParagraph(paragraph);
        }

        if (_author is not null)
        {
            builder = builder.WithAuthor(_author);
        }

        return builder;
    }

    public string Render()
    {
        var builder = new StringBuilder();

        builder.AppendLine(_title.Render());

        foreach (IParagraph paragraph in _paragraphs)
        {
            builder.AppendLine(paragraph.Render());
        }

        if (_author is not null)
        {
            builder.AppendLine(_author.Render());
        }

        return builder.ToString();
    }
}
