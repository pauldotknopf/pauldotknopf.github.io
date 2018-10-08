using System.Collections.Generic;

namespace Blog.Services
{
    public interface IPages
    {
        IReadOnlyCollection<Page> GetPages();
    }
}