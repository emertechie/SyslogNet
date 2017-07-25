namespace SyslogNet.Client.Transport
{
    internal static class StringExtensions
    {
        public static string EnsureMaxLength(this string s, int maxLength)
        {
            return string.IsNullOrWhiteSpace(s)
                ? s
                : s.Length > maxLength ? s.Substring(0, maxLength) : s;
        }
    }
}
