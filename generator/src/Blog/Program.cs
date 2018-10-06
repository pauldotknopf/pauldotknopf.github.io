using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Blog.Services;
using Blog.Services.Impl;
using Markdig;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Pek.Markdig.HighlightJs;
using PowerArgs;
using Statik.Embedded;
using Statik.Files;
using Statik.Markdown;
using Statik.Markdown.Impl;
using Statik.Mvc;
using Statik.Web;

namespace Blog
{
    class Program
    {
        private static IWebBuilder _webBuilder;
        private static string _contentDirectory;
        private static IMarkdownParser _markdownParser;
        private static IMarkdownRenderer _markdownRenderer;
        private static IPosts _posts;
        
        static int Main(string[] args)
        {
            try
            {
                _contentDirectory = Directory.GetCurrentDirectory();
                _markdownParser = new MarkdownParser();
                _markdownRenderer = new MarkdownRenderer(new MarkdownPipelineBuilder()
                    .UseAdvancedExtensions()
                    .UseHighlightJs()
                    .Build());
                _webBuilder = Statik.Statik.GetWebBuilder();
                _webBuilder.RegisterMvcServices();
                _webBuilder.RegisterServices(services =>
                {
                    services.AddSingleton(_markdownParser);
                    services.AddSingleton(_markdownRenderer);
                    services.Configure<RazorViewEngineOptions>(options =>
                    {
                        //options.FileProviders.Add(new EmbeddedFileProvider(typeof(Program).Assembly, "Blog.Resources"));
                        options.FileProviders.Add(new PhysicalFileProvider("/Users/pknopf/git/pauldotknopf.github.io/generator/src/Blog/Resources"));
                    });
                });
                
                LoadPosts();
                RegisterPages();
                RegisterResources();
                
                try
                {
                    Args.InvokeAction<Program>(args);
                }
                catch (ArgException ex)
                {
                    Console.WriteLine(ex.Message);
                    ArgUsage.GenerateUsageFromTemplate<Program>().WriteLine();
                    return -1;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return -1;
            }

            return 0;
        }

        private static void RegisterResources()
        {
            //_webBuilder.RegisterFileProvider(new EmbeddedFileProvider(typeof(Program).Assembly, "Blog.Resources.wwwroot"));
            _webBuilder.RegisterFileProvider(new PhysicalFileProvider("/Users/pknopf/git/pauldotknopf.github.io/generator/src/Blog/Resources/wwwroot"));
            var staticDirectory = Path.Combine(_contentDirectory, "static");
            if (Directory.Exists(staticDirectory))
            {
                _webBuilder.RegisterDirectory(staticDirectory);
            }
        }

        private static void LoadPosts()
        {
            var posts = new List<Post>();
            foreach (var post in Directory.GetFiles(Path.Combine(_contentDirectory, "posts"), "*.md"))
            {
                var result = _markdownParser.Parse<Post>(File.ReadAllText(post));
                if (result.Yaml == null)
                {
                    throw new InvalidOperationException($"no yaml provided for {post}");
                }

                result.Yaml.Markdown = result.Markdown;
                posts.Add(result.Yaml);
            }
            _posts = new Posts(posts);
            _webBuilder.RegisterServices(services => { services.AddSingleton(_posts); });
        }
        
        private static void RegisterPages()
        {
            var pageIndex = 0;
            PagedList<Post> posts;
            do
            {
                posts = _posts.GetPosts(pageIndex, 2);
                _webBuilder.RegisterMvc($"/blog/{pageIndex + 1}", new
                {
                    controller = "Blog",
                    action = "Page",
                    pageIndex
                });
                pageIndex++;
            } while (posts.HasNextPage);
            
            _webBuilder.RegisterMvc("/", new
            {
                controller = "Blog",
                action = "Page",
                pageIndex = 0
            });
        }
        
        [ArgActionMethod, ArgIgnoreCase]
        public void Serve()
        {
            Console.WriteLine("serve");
            using (var host = _webBuilder.BuildWebHost(port: 8000))
            {
                host.Listen();
                Console.WriteLine("Listening on port 8000...");
                Console.ReadLine();
            }
        }
        
        public class BuildArgs
        {
            [ArgDefaultValue("output"), ArgShortcut("o")]
            public string Output { get; set; }
        }
        
        [ArgActionMethod, ArgIgnoreCase]
        public async Task Build(BuildArgs args)
        {
            using (var host = _webBuilder.BuildVirtualHost())
            {
                await Statik.Statik.ExportHost(host, args.Output);
            }
        }
    }
}
