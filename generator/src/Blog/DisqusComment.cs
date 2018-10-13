using System;

namespace Blog
{
    public class DisqusComment : Comment
    {
        public ulong Id { get; set; }
        
        public DateTimeOffset CreatedAt { get; set; }
        
        public string Body { get; set; }
        
        public string Name { get; set; }
        
        public string UserName { get; set; }
    }
}