using System;
using System.Collections.Generic;
using System.Linq;

namespace Blog.Services.Impl
{
    public class Posts : IPosts
    {
        private readonly List<Post> _posts;

        public Posts(List<Post> posts)
        {
            _posts = posts.OrderByDescending(x => x.Date).ToList();
        }

        public PagedList<Post> GetPosts(int pageIndex, int pageSize, bool onlyListed = true)
        {
            var posts = _posts
                .Where(x => !onlyListed || x.Listed)
                .Skip(pageIndex * pageSize)
                .Take(pageSize).ToList();

            var count = _posts.Count;
            
            return new PagedList<Post>(posts, pageIndex, (int)Math.Ceiling(count / (decimal)pageSize) - 1, pageSize, count);
        }
    }
}