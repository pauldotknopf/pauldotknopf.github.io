using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Blog.Services.Impl
{
    public class Pages : IPages
    {
        private readonly IReadOnlyCollection<Page> _pages;

        public Pages(List<Page> pages)
        {
            _pages = new ReadOnlyCollection<Page>(pages);
        }
        
        public IReadOnlyCollection<Page> GetPages()
        {
            return _pages;
        }
    }
}