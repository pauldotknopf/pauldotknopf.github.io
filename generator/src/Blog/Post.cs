using System;
using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace Blog
{
    public class Post
    {
        public Post()
        {
            Listed = true;
        }
        
        [YamlMember(Alias = "title")]
        public string Title { get; set; }
        
        [YamlMember(Alias = "date")]
        public DateTime Date { get; set; }
        
        [YamlMember(Alias = "summary")]
        public string Summary { get; set; }
        
        [YamlMember(Alias = "slug")]
        public string Slug { get; set; }
        
        [YamlMember(Alias = "redirect_from")]
        public List<string> RedirectFrom { get; set; }
        
        [YamlMember(Alias = "comment_issue_id")]
        public int? CommentIssueID { get; set; }
        
        [YamlMember(Alias = "listed")]
        public bool Listed { get; set; }
        
        public List<GitHubComment> Comments { get; set; }
        
        public string Path { get; set; }
        
        public string Markdown { get; set; }
        
        public string FilePath { get; set; }
    }
}