namespace Blog.State
{
    public class PathState : IFilePath, ITitleHint
    {
        public PathState(string filePath, string title)
        {
            FilePath = filePath;
            Title = title;
        }
        
        public string FilePath { get; }
        
        public string Title { get; }
    }
}