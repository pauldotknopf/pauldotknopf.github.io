using Blog.Misc;
using Blog.Models;
using Blog.Services;
using Microsoft.AspNetCore.Mvc;
using Statik.Markdown;

namespace Blog.Controllers
{
    public class BlogController : Controller
    {
        private readonly IPosts _posts;
        private readonly IMarkdownRenderer _markdownRenderer;

        public BlogController(IPosts posts, IMarkdownRenderer markdownRenderer)
        {
            _posts = posts;
            _markdownRenderer = markdownRenderer;
        }
        
        public ActionResult Page([FromRouteData]int pageIndex)
        {
            var model = new PostsModel();
            model.Posts = _posts.GetPosts(pageIndex, 2);
            return View("Page", model);
        }

        public ActionResult Post([FromRouteData]Post post)
        {
            var model = new PostModel();
            model.Post = post;
            model.Body = _markdownRenderer.Render(post.Markdown);
            return View("Post", model);
        }
    }
}