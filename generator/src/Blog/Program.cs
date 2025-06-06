using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Blog.Models;
using Blog.Services;
using Blog.Services.Impl;
using Markdig;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Newtonsoft.Json;
using Octokit;
using Octokit.Internal;
using Pek.Markdig.HighlightJs;
using PowerArgs;
using Statik;
using Statik.Embedded;
using Statik.Files;
using Statik.Markdown;
using Statik.Markdown.Impl;
using Statik.Mvc;
using Statik.Web;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Blog
{
    class Program
    {
        private static IWebBuilder _webBuilder;
        private static string _contentDirectory;
        private static IMarkdownParser _markdownParser;
        private static IMarkdownRenderer _markdownRenderer;
        private static IPosts _posts;
        private static IPages _pages;
        private static IMusicTracks _musicTracks;
        private static GitHubClient _github;
        
        static async Task<int> Main(string[] args)
        {
            try
            {
                var githubUsername = Environment.GetEnvironmentVariable("GITHUB_USERNAME");
                var githubPassword = Environment.GetEnvironmentVariable("GITHUB_PASSWORD");
                if (string.IsNullOrEmpty(githubUsername) || string.IsNullOrEmpty(githubPassword))
                {
                    Console.WriteLine("Not using comments...");
                }
                else
                {
                    _github = new GitHubClient(new ProductHeaderValue("pauldotknopf.github.io"), new InMemoryCredentialStore(new Credentials(githubUsername, githubPassword)));
                }
                
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
                    services.AddRazorPages()
                        .AddRazorRuntimeCompilation();
                    services.Configure<MvcRazorRuntimeCompilationOptions>(options =>
                    {
                        options.FileProviders.Add(new Statik.Embedded.EmbeddedFileProvider(typeof(Program).Assembly, "Blog.Resources"));
                        //options.FileProviders.Add(new PhysicalFileProvider("/Users/paul.knopf/git/pauldotknopf/pauldotknopf.github.io/generator/src/Blog/Resources"));
                    });
                });
                
                LoadMusic();
                LoadPages();
                await LoadPosts();
                
                RegisterMusic();
                RegisterPages();
                RegisterPosts();
                RegisterResources();

                _webBuilder.RegisterMvc("/lit", new
                {
                    controller = "Lit",
                    action = "Index"
                },
                new State.PathState(string.Empty, "Lit"));

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
                Console.WriteLine(ex.StackTrace);
                return -1;
            }

            return 0;
        }

        private static void RegisterResources()
        {
            _webBuilder.RegisterFileProvider(new Statik.Embedded.EmbeddedFileProvider(typeof(Program).Assembly, "Blog.Resources.wwwroot"));
            //_webBuilder.RegisterFileProvider(new PhysicalFileProvider("/Users/paul.knopf/git/pauldotknopf/pauldotknopf.github.io/generator/src/Blog/Resources/wwwroot"));
            var staticDirectory = Path.Combine(_contentDirectory, "static");
            if (Directory.Exists(staticDirectory))
            {
                _webBuilder.RegisterDirectory(staticDirectory, new RegisterOptions
                {
                    State = delegate(string prefix, string path, string filePath, IFileInfo file,
                        IFileProvider provider)
                    {
                        return new State.PathState($"/static{filePath}", "");
                    }
                });
            }
        }

        private static void LoadMusic()
        {
            var musicYamlPath = Path.Combine(_contentDirectory, "music.yaml");
            var musicYaml = File.ReadAllText(musicYamlPath);
            
            
            var deserializer = new DeserializerBuilder().Build();
            var yamlObject = deserializer.Deserialize(new StringReader(musicYaml));
            var serializer = new SerializerBuilder()
                .JsonCompatible()
                .Build();
            var musicJson = serializer.Serialize(yamlObject);
            
            var musicTrackModels = JsonConvert.DeserializeObject<List<MusicTrack>>(musicJson);
            _musicTracks = new MusicTracks(musicTrackModels);
            _webBuilder.RegisterServices(services =>
            {
                services.AddSingleton(_musicTracks);
            });
        }
        
        private static void LoadPages()
        {
            var pagesDirectory = Path.Combine(_contentDirectory, "pages");
            if (!Directory.Exists(pagesDirectory))
            {
                return;
            }

            var pages = new List<Page>();
            foreach (var page in Directory.GetFiles(pagesDirectory, "*.md"))
            {
                var result = _markdownParser.Parse<Page>(File.ReadAllText(page));
                if (result.Yaml == null)
                {
                    throw new InvalidOperationException($"no yaml provided for {page}");
                }

                result.Yaml.Markdown = result.Markdown;
                if (string.IsNullOrEmpty(result.Yaml.Slug))
                {
                    result.Yaml.Slug = Statik.StatikHelpers.ConvertStringToSlug(result.Yaml.Title);
                }
                result.Yaml.Path = $"/{result.Yaml.Slug.ToLower()}";
                result.Yaml.FilePath = $"/pages/{Path.GetFileName(page)}";
                pages.Add(result.Yaml);
            }
            _pages = new Pages(pages);
            _webBuilder.RegisterServices(services => services.AddSingleton(_pages));
        }
        
        private static async Task LoadPosts()
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
                if (string.IsNullOrEmpty(result.Yaml.Slug))
                {
                    result.Yaml.Slug = Path.GetFileNameWithoutExtension(post);
                }
                result.Yaml.Path = $"/post/{result.Yaml.Slug}";
                result.Yaml.FilePath = $"/posts/{Path.GetFileName(post)}";
                
                posts.Add(result.Yaml);
            }
            
            // Load the comments from GitHub.
            if (_github != null)
            {
                foreach (var post in posts)
                {
                    if (post.CommentIssueID.HasValue)
                    {
                        post.Comments = new List<GitHubComment>();
                        var comments = await _github.Issue.Comment.GetAllForIssue("pauldotknopf",
                            "pauldotknopf.github.io", post.CommentIssueID.Value);
                        foreach (var comment in comments)
                        {
                            var body = await _github.Miscellaneous.RenderArbitraryMarkdown(
                                new NewArbitraryMarkdown(comment.Body, "gfm", "pauldotknopf/pauldotknopf.github.io"));
                            // The API returns an "UpdatedAt", even if it isn't updated.
                            post.Comments.Add(new GitHubComment
                            {
                                Id = comment.Id,
                                CreatedAt = comment.CreatedAt,
                                // The API returns UpdatedAt, even if the comment wasn't updated.
                                UpdatedAt = comment.UpdatedAt.HasValue
                                    ? (comment.UpdatedAt.Value == comment.CreatedAt
                                        ? (DateTimeOffset?) null
                                        : comment.UpdatedAt.Value)
                                    : null,
                                User = comment.User.Login,
                                UserAvatarUrl = comment.User.AvatarUrl,
                                Body = body,
                                Reactions = comment.Reactions
                            });
                        }
                    }
                }
            }

            _posts = new Posts(posts);
            _webBuilder.RegisterServices(services => { services.AddSingleton(_posts); });
        }

        private static void RegisterPages()
        {
            foreach (var page in _pages.GetPages())
            {
                _webBuilder.RegisterMvc(page.Path, new
                    {
                        controller = "Page",
                        action = "Page",
                        page
                    },
                    new State.PathState(page.FilePath, page.Title));
            }
        }

        private static void RegisterMusic()
        {
            _webBuilder.RegisterMvc("/music", new
                {
                    controller = "Music",
                    action = "Index"
                },
                new State.PathState(string.Empty, "Music"));
            _webBuilder.RegisterMvc("/music/guitar", new
                {
                    controller = "Music",
                    action = "Guitar"
                },
                new State.PathState(string.Empty, "Guitar"));
            _webBuilder.RegisterMvc("/music/drums", new
                {
                    controller = "Music",
                    action = "Drums"
                },
                new State.PathState(string.Empty, "Drums"));
            foreach (var track in _musicTracks.GetMusicTracks())
            {
                _webBuilder.RegisterMvc($"/music/track/{track.Artist.Slugify()}/{track.Song.Slugify()}", new
                    {
                        controller = "Music",
                        action = "Track",
                        track
                    },
                    new State.PathState(string.Empty, $"{track.Artist} - {track.Song}"));
            }
            foreach (var artist in _musicTracks.GetMusicTracks().Select(x => x.Artist).Distinct())
            {
                _webBuilder.RegisterMvc($"/music/artist/{artist.Slugify()}", new
                    {
                        controller = "Music",
                        action = "Artist",
                        artistName = artist
                    },
                    new State.PathState(string.Empty, artist));
            }
            foreach (var tuning in _musicTracks.GetMusicTracks().Where(x => x.Guitar != null).Select(x => x.Guitar.Tuning).Distinct())
            {
                _webBuilder.RegisterMvc($"/music/guitar/tuning/{tuning.Slugify()}", new
                    {
                        controller = "Music",
                        action = "Tuning",
                        tuning
                    },
                    new State.PathState(string.Empty, $"Guitar Tuning {tuning}"));
            }
        }
        
        private static void RegisterPosts()
        {
            _webBuilder.RegisterMvc("/", new
                {
                    controller = "Blog",
                    action = "Page",
                    pageIndex = 0
                },
                new State.PathState(string.Empty, "Blog"));
            
            var pageIndex = 0;
            PagedList<Post> posts;
            do
            {
                posts = _posts.GetPosts(pageIndex, 5);
                _webBuilder.RegisterMvc($"/blog/{pageIndex}", new
                    {
                        controller = "Blog",
                        action = "Page",
                        pageIndex
                    },
                    new State.PathState(string.Empty, "Blog"));
                pageIndex++;
            } while (posts.HasNextPage);

            posts = _posts.GetPosts(0, int.MaxValue, false);
            foreach (var post in posts)
            {
                _webBuilder.RegisterMvc(post.Path, new
                    {
                        controller = "Blog",
                        action = "Post",
                        post
                    },
                    new State.PathState(post.FilePath, post.Title));
                if (post.RedirectFrom != null && post.RedirectFrom.Count > 0)
                {
                    foreach(var redirectFrom in post.RedirectFrom)
                    {
                        _webBuilder.Redirect(redirectFrom, post.Path);
                    }
                }
            }
            
            _webBuilder.RegisterMvc("/archive", new
                {
                    controller = "Blog",
                    action = "Archive"
                },
                new State.PathState(string.Empty, "Archive"));
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
