
namespace System
{


    internal static class SyslogClientStringExtensions 
    {


        internal static bool IsNullOrWhiteSpace(this string s)
        {
            if (s == null)
                return true;

            return (s.Trim() == string.Empty);
        }


    }


}
