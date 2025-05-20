namespace Blog
{
    public static class Extensions
    {
        public static string Slugify(this string str)
        {
            return str.ToLowerInvariant()
                .Replace(" ", "-")
                .Replace("'", "")
                .Replace("(", "")
                .Replace(")", "")
                .Replace("&", "and")
                .Replace(",", "")
                .Replace(".", "")
                .Replace("!", "");
        }
    }
}