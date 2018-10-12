using YamlDotNet.Serialization;

namespace Blog
{
    public class Page
    {
        [YamlMember(Alias = "title")]
        public string Title { get; set; }
        
        [YamlMember(Alias = "show_title")]
        public bool ShowTitle { get; set; }
        
        [YamlMember(Alias = "slug")]
        public string Slug { get; set; }
        
        public string Path { get; set; }
        
        public string Markdown { get; set; }
        
        public string FilePath { get; set; }
    }
}