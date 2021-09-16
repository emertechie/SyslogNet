
using SyslogNet.Client.Serialization;


namespace SyslogNet.Client.Transport
{


    public enum MessageTransfer
    {
        OctetCounting = 0,
        NonTransparentFraming = 1
    }


    public class SyslogTcpSender
        : ISyslogMessageSender, System.IDisposable
    {
        protected string hostname;
        protected int port;

        protected System.Net.Sockets.TcpClient tcpClient = null;
        protected System.IO.Stream transportStream = null;

        public virtual MessageTransfer messageTransfer { get; set; }
        public byte trailer { get; set; }


        public SyslogTcpSender(string hostname, int port, bool shouldAutoConnect = true)
        {
            this.hostname = hostname;
            this.port = port;

            if (shouldAutoConnect)
            {
                Connect();
            }

            //messageTransfer = MessageTransfer.OctetCounting;
            messageTransfer = MessageTransfer.NonTransparentFraming;
            trailer = 10; // LF
        }


        public void Connect()
        {
            try
            {
                tcpClient = new System.Net.Sockets.TcpClient(hostname, port);
                transportStream = tcpClient.GetStream();
            }
            catch
            {
                Disconnect();
                throw;
            }
        }


        public virtual void Reconnect()
        {
            Disconnect();
            Connect();
        }


        public void Disconnect()
        {
            if (transportStream != null)
            {
                transportStream.Close();
                transportStream = null;
            }

            if (tcpClient != null)
            {
                tcpClient.Close();
                tcpClient = null;
            }
        }


        public void Send(SyslogMessage message, ISyslogMessageSerializer serializer)
        {
            Send(message, serializer, true);
        }


        protected void Send(SyslogMessage message, ISyslogMessageSerializer serializer, bool flush = true)
        {
            if (transportStream == null)
            {
                throw new System.IO.IOException("No transport stream exists");
            }

            using (System.IO.MemoryStream memoryStream = new System.IO.MemoryStream())
            {
                byte[] datagramBytes = serializer.Serialize(message);

                if (messageTransfer.Equals(MessageTransfer.OctetCounting))
                {
                    byte[] messageLength = System.Text.Encoding.ASCII.GetBytes(datagramBytes.Length.ToString(System.Globalization.CultureInfo.InvariantCulture));
                    memoryStream.Write(messageLength, 0, messageLength.Length);
                    memoryStream.WriteByte(32); // Space
                }

                memoryStream.Write(datagramBytes, 0, datagramBytes.Length);

                if (messageTransfer.Equals(MessageTransfer.NonTransparentFraming))
                {
                    memoryStream.WriteByte(trailer); // LF
                }

                transportStream.Write(memoryStream.GetBuffer(), 0, (int)memoryStream.Length);
            }

            if (flush && !(transportStream is System.Net.Sockets.NetworkStream))
                transportStream.Flush();
        }


        public void Send(System.Collections.Generic.IEnumerable<SyslogMessage> messages, ISyslogMessageSerializer serializer)
        {
            foreach (SyslogMessage message in messages)
            {
                Send(message, serializer, false);
            }

            if (!(transportStream is System.Net.Sockets.NetworkStream))
                transportStream.Flush();
        }


        public void Dispose()
        {
            Disconnect();
        }


    }


}
