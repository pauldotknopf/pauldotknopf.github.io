using System;
using Octokit;

namespace Blog
{
    public class GitHubComment : Comment
    {
        public int Id { get; set; }
        
        public string User { get; set; }
        
        public string UserAvatarUrl { get; set; }
        
        public DateTimeOffset CreatedAt { get; set; }
        
        public DateTimeOffset? UpdatedAt { get; set; }
        
        public string Body { get; set; }
        
        public ReactionSummary Reactions { get; set; }
    }
}