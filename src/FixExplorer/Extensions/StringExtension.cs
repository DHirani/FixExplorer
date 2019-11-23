namespace FixExplorer.Extensions
{
    public static class StringExtension
    {
        public static string To8CharsString(this int x)
        {
            return x.ToString("0#######");
        }

    }
}
