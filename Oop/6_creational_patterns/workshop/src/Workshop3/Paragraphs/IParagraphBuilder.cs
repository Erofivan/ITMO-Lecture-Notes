namespace Workshop3.Paragraphs;

public interface IParagraphTitleSelector
{
    IParagraphBuilder WithTitle(IRenderable title);
}

public interface IParagraphBuilder
{
    IParagraphBuilder AddContent(IRenderable content);

    IParagraphBuilder WithFooter(IRenderable footer);

    IParagraph Build();
}
