---
title: "Comments for static websites, using GitHub Issues."
date: 2018-10-13
comment_issue_id: 6
---

I have been using Disqus for my blog discussions. They began forcing ads on their free-tier users. I wouldn't normally care except for the fact that the ads were large, irrelevant and click-baity. This blog isn't highly trafficked so I shouldn't really care, but it just irritated me. So, I began looking for a different solution that was free with no-ads.

# My options

I've written off many options that I won't even bother to list. The reasons being I want:

* Ad-free
* No cost
* Privacy/security (no Facebook comments, Disqus, etc)

With that said, here were some options I considered.

## Staticman

<a href="https://staticman.net/" target="_blank" class="btn btn-light">Website</a>

Since I currently use GitHub to host my blog, this looked appealing. All you do is add a custom ```form``` element that posts comments to a third-party service, which will then automatically commit the comment to your repository (or open a pull request for moderation). It is up to your build scripts to properly render the comments that are committed into your repository.

I can see this as a powerful tool for many use-cases, but not for blog comments, due to the missing authentication and notifications.

## Utterances

<a href="https://utteranc.es/" target="_blank" class="btn btn-light">Website</a>

This looked even more appealing than Staticman. It uses GitHub Issues. Since I'm a developer, I'd love to be able to use [GitHub Flavored Markdown](https://github.github.com/gfm/). It also gives you authentication and notifications.

This products requires an external server to be up and online at all times. And while they do promise to be always free without ads, they don't promise to be always *available*. And while they are open-source and I could always decide host my own Utterances server/bot, I'd prefer to host nothing at all. This isn't a super deal breaker, because in the end, all content is stored in GitHub Issues.

## Gitment

<a href="https://github.com/imsun/gitment" target="_blank" class="btn btn-light">Website</a>

Gitment is similar to Utterances. It uses GitHub Issues as well. It doesn't seem to be under active development, but maybe it's "done"?

Honestly, Utterances seems a lot more polished. If I had to choose between Gitment or Utterances, I'd go with Utterances.

# My solution

Using the inspiration from these solutions, I decided to host my comments on GitHub Issues. But instead of using client-side scripting to render and post comments, I'd do it all on the server side when generating my static site.

Since I am using [Statik](https://github.com/pauldotknopf/statik) (a tool I wrote) to generate my static site, it is really easy to customize and perform the server-side rendering of the comments.

So, in my blog post's front-matter, I embedded a GitHub Issue ID that will host the discussion.

```yml
---
comment_issue_id: 1
---
```

Then I use [Octokit](https://github.com/octokit/octokit.net) to query the current comments to render them.

```csharp
foreach (var post in posts)
{
    if (post.CommentIssueID.HasValue)
    {
        post.Comments = new List<GitHubComment>();
        var comments = await _github.Issue.Comment.GetAllForIssue("pauldotknopf", "pauldotknopf.github.io", post.CommentIssueID.Value);
        foreach (var comment in comments)
        {
            var body = await _github.Miscellaneous.RenderArbitraryMarkdown(new NewArbitraryMarkdown(comment.Body, "gfm", "pauldotknopf/pauldotknopf.github.io"));
            post.Comments.Add(new GitHubComment
            {
                Id = comment.Id,
                CreatedAt = comment.CreatedAt,
                UpdatedAt = comment.UpdatedAt,
                User = comment.User.Login,
                UserAvatarUrl = comment.User.AvatarUrl,
                Body = body,
                Reactions = comment.Reactions
            });
        }
    }
}
```

Since I am querying the comments on the server, I can use authenticated GitHub requests, which give me a higher request rate-limit.

I can then render the content how I want (in Razor, see [here](https://github.com/pauldotknopf/pauldotknopf.github.io/blob/b5d615711a14f69c09f098f4a2373cecc2f4cf6f/generator/src/Blog/Resources/Views/Blog/Post.cshtml#L18)).

A few drawbacks:
* No ability to comment directly from the blog post. I don't really mind this, since I don't like asking users for OAuth permissions anyway. Instead, I guide them directly to GitHub for the comments.
* Out-of-date data. Since we render the comments on the server side, we wouldn't capture any new comments or new edits immediately. I use a cron-job in Travis CI to rebuild my site every 24-hours to help alleviate this issue. I wish there was some (easy) way to trigger a Travis CI build each time a comment is posted or updated on a GitHub Issue.

There is no harm in switching to Utterances in the future. It is a lot easier to implement, but I'm more partial too having smaller dependencies and fewer external services.

You can play around with [this](2018-10-10-test-github-comments.md) blog post.

In the end though, my blog doesn't get much traffic. Who cares? :-)

Titty sprinkles.