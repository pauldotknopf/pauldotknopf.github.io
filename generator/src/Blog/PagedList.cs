using System.Collections.Generic;

namespace Blog
{
    public class PagedList<T> : List<T>
    {
        public PagedList(IEnumerable<T> collection,
            int pageIndex,
            int maxPageIndex,
            int pageSize,
            int totalCount)
            :base(collection)
        {
            PageIndex = pageIndex;
            MaxPageIndex = maxPageIndex;
            PageSize = pageSize;
            TotalCount = totalCount;
        }
        
        public int PageIndex { get; }
        
        public int MaxPageIndex { get; }
        
        public int PageSize { get; }
        
        public int TotalCount { get; }
        
        public bool HasNextPage => PageIndex < MaxPageIndex;

        public bool HasPreviousPage => PageIndex > 0;
    }
}