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

        public ActionResult Archive()
        {
            var model = new ArchiveModel();
            var posts = _posts.GetPosts(0, int.MaxValue);
            ArchiveModel.YearPosts yearPosts = null;
            ArchiveModel.YearPosts.MonthPosts monthPosts = null;
            foreach (var post in posts)
            {
                if (yearPosts == null)
                {
                    yearPosts = new ArchiveModel.YearPosts();
                    yearPosts.Year = post.Date.Year;
                    model.Years.Add(yearPosts);
                }

                if (yearPosts.Year != post.Date.Year)
                {
                    yearPosts = new ArchiveModel.YearPosts();
                    yearPosts.Year = post.Date.Year;
                    model.Years.Add(yearPosts);
                }

                if (monthPosts == null)
                {
                    monthPosts = new ArchiveModel.YearPosts.MonthPosts();
                    monthPosts.Month = post.Date.Month;
                    yearPosts.Months.Add(monthPosts);
                }

                if (monthPosts.Month != post.Date.Month)
                {
                    monthPosts = new ArchiveModel.YearPosts.MonthPosts();
                    monthPosts.Month = post.Date.Month;
                    yearPosts.Months.Add(monthPosts);
                }
                
                monthPosts.Posts.Add(post);
            }

            return View("Archive", model);
        }
    }
}