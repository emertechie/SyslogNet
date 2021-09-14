
namespace SyslogNet.Client.Serialization
{


    public class SyslogRfc3164MessageSerializer
        : SyslogMessageSerializerBase, ISyslogMessageSerializer
    {

        protected bool m_useUtf8;


        public SyslogRfc3164MessageSerializer(bool useUtf8)
        {
            m_useUtf8 = useUtf8;
        }


        public SyslogRfc3164MessageSerializer()
            : this(false)
        { }


        public void Serialize(SyslogMessage message, System.IO.Stream stream)
        {
            int priorityValue = CalculatePriorityValue(message.Facility, message.Severity);

            string timestamp = null;
            if (message.DateTimeOffset.HasValue)
            {
                System.DateTimeOffset dt = message.DateTimeOffset.Value;
                string day = dt.Day < 10 ? " " + dt.Day.ToString(System.Globalization.CultureInfo.InvariantCulture) : dt.Day.ToString(System.Globalization.CultureInfo.InvariantCulture); // Yes, this is stupid but it's in the spec
                timestamp = string.Concat(dt.ToString("MMM' '", System.Globalization.CultureInfo.InvariantCulture), day, dt.ToString("' 'HH':'mm':'ss", System.Globalization.CultureInfo.InvariantCulture));
            }

            System.Text.StringBuilder headerBuilder = new System.Text.StringBuilder();
            headerBuilder.Append("<").Append(priorityValue).Append(">");
            headerBuilder.Append(timestamp).Append(" ");
            headerBuilder.Append(message.HostName).Append(" ");
            headerBuilder.Append(message.AppName.IfNotNullOrWhitespace(x => x.EnsureMaxLength(32) + ":"));


            if (!this.m_useUtf8)
                headerBuilder.Append(message.Message ?? "");

            byte[] asciiBytes = System.Text.Encoding.ASCII.GetBytes(headerBuilder.ToString());
            stream.Write(asciiBytes, 0, asciiBytes.Length);

            if (this.m_useUtf8)
            {
                stream.Write(System.Text.Encoding.UTF8.GetPreamble(), 0, System.Text.Encoding.UTF8.GetPreamble().Length);
                asciiBytes = System.Text.Encoding.UTF8.GetBytes(message.Message ?? "");
                stream.Write(asciiBytes, 0, asciiBytes.Length);
            }

        }


    }


}