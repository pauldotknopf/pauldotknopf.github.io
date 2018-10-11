using System.IO;
using Blog.Misc;
using Blog.Models;
using Blog.Services;
using Blog.State;
using Microsoft.AspNetCore.Mvc;
using Statik;
using Statik.Markdown;
using Statik.Web;

namespace Blog.Controllers
{
    public class BlogController : Controller
    {
        private readonly IPosts _posts;
        private readonly IMarkdownRenderer _markdownRenderer;
        private readonly IPageRegistry _pageRegistry;

        public BlogController(IPosts posts,
            IMarkdownRenderer markdownRenderer,
            IPageRegistry pageRegistry)
        {
            _posts = posts;
            _markdownRenderer = markdownRenderer;
            _pageRegistry = pageRegistry;
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
            model.Body = _markdownRenderer.Render(post.Markdown, url =>
            {
                var possiblePath = StatikHelpers.ResolvePathPart(Path.GetDirectoryName(post.FilePath), url);

                var statikPage = _pageRegistry.FindOne(x =>
                {
                    var state = x.State as IFilePath;
                    if (state == null) return false;
                    return state.FilePath == possiblePath;
                });

                if (statikPage != null)
                {
                    return Request.PathBase + statikPage.Path;
                }
                
                return url;
            });
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