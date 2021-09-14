
namespace SyslogNet.Client.Serialization
{


    public abstract class SyslogMessageSerializerBase
    {


        protected static int CalculatePriorityValue(Facility facility, Severity severity)
        {
            return ((int)facility * 8) + (int)severity;
        }


    }


}