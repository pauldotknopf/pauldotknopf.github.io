using System.IO;
using Blog.Misc;
using Blog.Models;
using Blog.State;
using Microsoft.AspNetCore.Mvc;
using Statik;
using Statik.Markdown;
using Statik.Web;

namespace Blog.Controllers
{
    public class PageController : Controller
    {
        private readonly IMarkdownRenderer _markdownRenderer;
        private readonly IPageRegistry _pageRegistry;

        public PageController(IMarkdownRenderer markdownRenderer,
            IPageRegistry pageRegistry)
        {
            _markdownRenderer = markdownRenderer;
            _pageRegistry = pageRegistry;
        }
        
        public ActionResult Page([FromRouteData]Page page)
        {
            var model = new PageModel();
            model.Page = page;
            model.Body = _markdownRenderer.Render(page.Markdown, url =>
            {
                var possiblePath = StatikHelpers.ResolvePathPart(Path.GetDirectoryName(page.FilePath), url);

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
            return View("Page", model);
        }
    }
}