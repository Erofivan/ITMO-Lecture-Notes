// See https://aka.ms/new-console-template for more information

using System.Drawing;
using Workshop3.Articles;
using Workshop3.Paragraphs;
using Workshop3.Paragraphs.Factories;
using Workshop3.RenderableModifiers;
using Workshop3.Renderables;


Console.Clear();

// var paragraphFactory = new StyledParagraphBuilderFactory(
// new ColorModifer(Color.Red));

var paragraphFactory = new DefaultParagraphBuilderFactory();

Text renderable = new Text("My article").Bold();

IArticle article = CreateArticle(
    new ArticleBuilder(),
    paragraphFactory);

IArticleBuilder newArticleBuilder = new ArticleBuilder();
newArticleBuilder.AddParagraph(BuildParagraphs(new StyledParagraphBuilderFactory(new ColorModifer(Color.Red))).First());
newArticleBuilder = article.Direct(newArticleBuilder);

// newArticleBuilder.WithTitle(new Text("My article (new)"));

renderable.Value = "1234";

Console.WriteLine(newArticleBuilder.Build().Render());
Console.WriteLine(article.Render());

IArticle CreateArticle(
    IArticleBuilder builder,
    IParagraphBuilderFactory paragraphFactory)
{
    builder.WithTitle(renderable);

    foreach (IParagraph paragraph in BuildParagraphs(paragraphFactory))
    {
        builder.AddParagraph(paragraph);
    }

    builder.WithAuthor(new Text("ronimizy"));

    return builder.Build();
}

static IEnumerable<IParagraph> BuildParagraphs(IParagraphBuilderFactory builderFactory)
{
    return Enumerable
        .Range(0, 2)
        .Select(i => builderFactory
            .CreateBuilder()
            .WithTitle(new Text($"Paragraph {i + 1}"))
            .AddContent(new Text("abc"))
            .Build());
}
