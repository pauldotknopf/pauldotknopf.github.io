using Blog.Misc;
using Blog.Models;
using Blog.Services;
using Microsoft.AspNetCore.Mvc;

namespace Blog.Controllers
{
    public class BlogController : Controller
    {
        private readonly IPosts _posts;

        public BlogController(IPosts posts)
        {
            _posts = posts;
        }
        
        public ActionResult Page([FromRouteData]int pageIndex)
        {
            var model = new PageModel();
            model.Posts = _posts.GetPosts(pageIndex, 1);
            return View("Page", model);
        }
    }
}