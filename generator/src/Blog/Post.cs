using System;
using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace Blog
{
    public class Post
    {
        [YamlMember(Alias = "title")]
        public string Title { get; set; }
        
        [YamlMember(Alias = "date")]
        public DateTime Date { get; set; }
        
        [YamlMember(Alias = "summary")]
        public string Summary { get; set; }
        
        [YamlMember(Alias = "disqus_identifier")]
        public string DisqusIdentifier { get; set; }
        
        [YamlMember(Alias = "slug")]
        public string Slug { get; set; }
        
        [YamlMember(Alias = "redirect_from")]
        public List<string> RedirectFrom { get; set; }
        
        [YamlMember(Alias = "comment_issue_id")]
        public int? CommentIssueID { get; set; }
        
        public List<Comment> Comments { get; set; }
        
        public string Path { get; set; }
        
        public string Markdown { get; set; }
        
        public string FilePath { get; set; }
    }
}