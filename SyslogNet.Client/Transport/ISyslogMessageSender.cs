
using SyslogNet.Client.Serialization;


namespace SyslogNet.Client.Transport
{


    public interface ISyslogMessageSender
        : System.IDisposable
    {
        void Reconnect();
        void Send(SyslogMessage message, ISyslogMessageSerializer serializer);
        void Send(System.Collections.Generic.IEnumerable<SyslogMessage> messages, ISyslogMessageSerializer serializer);
    }


}