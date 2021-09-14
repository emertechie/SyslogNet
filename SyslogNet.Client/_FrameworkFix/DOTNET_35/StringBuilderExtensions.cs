
#if DOTNET_35

namespace System.Linq
{


    internal static class StringBuilderExtensions 
    {


        internal static System.Text.StringBuilder Clear(this System.Text.StringBuilder sb)
        {
            sb.Length = 0;
            return sb;
        }
    }
}

#endif
