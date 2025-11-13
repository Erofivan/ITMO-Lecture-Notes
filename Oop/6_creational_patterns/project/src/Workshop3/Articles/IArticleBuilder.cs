using Workshop3.Paragraphs;

namespace Workshop3.Articles;

public interface IArticleBuilder
{
    IArticleBuilder WithTitle(IRenderable title);

    IArticleBuilder AddParagraph(IParagraph paragraph);

    IArticleBuilder WithAuthor(IRenderable author);

    IArticle Build();
}
