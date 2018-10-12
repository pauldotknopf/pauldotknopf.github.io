using System.Collections.Generic;

namespace Blog.Models
{
    public class ArchiveModel
    {
        public ArchiveModel()
        {
            Years = new List<YearPosts>();
        }
        
        public List<YearPosts> Years { get; set; }
        
        public class YearPosts
        {
            public YearPosts()
            {
                Months = new List<MonthPosts>();
            }
            
            public int Year { get; set; }

            public List<MonthPosts> Months { get; set; }
            
            public class MonthPosts
            {
                public MonthPosts()
                {
                    Posts = new List<Post>();
                }
                
                public int Month { get; set; }
                
                public List<Post> Posts { get; set; }
            }
        }
    }
}