using System.Collections.Generic;

namespace Blog.Services
{
    public interface IPosts
    {
        PagedList<Post> GetPosts(int pageIndex, int pageSize);
    }
}