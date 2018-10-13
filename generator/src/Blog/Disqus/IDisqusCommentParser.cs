using System.Collections.Generic;

namespace Blog.Disqus
{
    public interface IDisqusCommentParser
    {
        List<DisqusComment> ParseComments(string xml);
    }
}