using System;

namespace SyslogNet.Client.Transport
{
    public class SyslogException : Exception
    {
        public SyslogException()
        {
        }

        public SyslogException(string message) : base(message)
        {
        }

        public SyslogException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}