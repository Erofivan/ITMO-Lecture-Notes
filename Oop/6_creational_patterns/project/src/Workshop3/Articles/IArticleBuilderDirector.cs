namespace Workshop3.Articles;

public interface IArticleBuilderDirector
{
    IArticleBuilder Direct(IArticleBuilder builder);
}
