using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SyslogNet.Client.Tests.Serialization
{
    public class RegexComparer : IEqualityComparer<string>
    {
        public bool Equals(string regex, string value)
        {
            return new Regex(regex).Match(value).Success;
        }

        public int GetHashCode(string obj)
        {
            return obj.GetHashCode();
        }
    }
}