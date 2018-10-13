using System;
using System.Collections.Generic;
using System.Xml.Linq;

// ReSharper disable PossibleNullReferenceException

namespace Blog.Disqus.Impl
{
    public class DisqusCommentParser : IDisqusCommentParser
    {
        public List<DisqusComment> ParseComments(string xml)
        {
            var result = new List<DisqusComment>();
            var document = XDocument.Parse(xml);
            foreach (var disqus in document.Descendants("{http://disqus.com}disqus"))
            {
                foreach (var post in disqus.Descendants("{http://disqus.com}post"))
                {
                    var deleted = bool.Parse(post.Element("{http://disqus.com}isDeleted").Value);
                    if (deleted)
                    {
                        continue;
                    }
                    
                    var comment = new DisqusComment();
                    comment.Id =  ulong.Parse(post.Attribute("{http://disqus.com/disqus-internals}id").Value);
                    comment.Body = post.Element("{http://disqus.com}message").Value;
                    comment.CreatedAt = DateTimeOffset.Parse(post.Element("{http://disqus.com}createdAt").Value);
                    
                    var author = post.Element("{http://disqus.com}author");
                    comment.Name = author.Element("{http://disqus.com}name").Value;
                    comment.UserName = author.Element("{http://disqus.com}username").Value;
                    
                    result.Add(comment);
                }
            }

            return result;
        }
    }
}