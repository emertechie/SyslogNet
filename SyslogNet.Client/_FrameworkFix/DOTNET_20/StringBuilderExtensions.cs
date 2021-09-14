
#if DOTNET_20

namespace System.Linq
{


    internal static class StringBuilderExtensions 
    {


        internal static System.Collections.Generic.List<TSource> ToList<TSource>(
            this System.Collections.Generic.IEnumerable<TSource> source)
        {
            if (source == null)
            {
                throw new System.ArgumentNullException(nameof(source));
            }
            return new System.Collections.Generic.List<TSource>(source);
        }


        internal static bool Any<TSource>(
            this System.Collections.Generic.IEnumerable<TSource> source)
        {
            if (source == null)
            {
                throw new System.ArgumentNullException(nameof(source));
            }

            using (System.Collections.Generic.IEnumerator<TSource> enumerator = 
                source.GetEnumerator())
            {
                if (enumerator.MoveNext())
                {
                    return true;
                }
            }
            return false;
        }

        internal static System.Text.StringBuilder Clear(this System.Text.StringBuilder sb)
        {
            sb.Length = 0;
            return sb;
        }
    }
}

#endif
