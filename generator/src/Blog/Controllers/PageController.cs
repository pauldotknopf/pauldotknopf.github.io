using Blog.Misc;
using Blog.Models;
using Microsoft.AspNetCore.Mvc;
using Statik.Markdown;

namespace Blog.Controllers
{
    public class PageController : Controller
    {
        private readonly IMarkdownRenderer _markdownRenderer;

        public PageController(IMarkdownRenderer markdownRenderer)
        {
            _markdownRenderer = markdownRenderer;
        }
        
        public ActionResult Page([FromRouteData]Page page)
        {
            var model = new PageModel();
            model.Page = page;
            model.Body = _markdownRenderer.Render(page.Markdown);
            return View("Page", model);
        }
    }
}