
namespace SyslogNet.Client.Serialization
{


    public interface ISyslogMessageSerializer
    {
        void Serialize(SyslogMessage message, System.IO.Stream stream);
    }


}