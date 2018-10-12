using System.Collections.Generic;
using System.Threading.Tasks;

namespace Blog.Comments
{
    public interface IGitHubComments
    {
        Task<List<GitHubComment>> GetCommentsForIssue(int issueId);
    }
}