using System.Text;

namespace Workshop3.Paragraphs;

internal sealed class DefaultParagraph : IParagraph
{
    private readonly IRenderable _title;
    private readonly IEnumerable<IRenderable> _content;
    private readonly IRenderable? _footer;

    public DefaultParagraph(
        IRenderable title,
        IEnumerable<IRenderable> content,
        IRenderable? footer)
    {
        _title = title;
        _content = content;
        _footer = footer;
    }

    public string Render()
    {
        var builder = new StringBuilder();

        builder.AppendLine(_title.Render());

        foreach (IRenderable renderable in _content)
        {
            builder.AppendLine(renderable.Render());
        }

        if (_footer is not null)
        {
            builder.AppendLine(_footer.Render());
        }

        return builder.ToString();
    }
}
