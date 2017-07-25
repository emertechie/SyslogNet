
using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using SyslogNet.Client.Serialization;

namespace SyslogNet.Client.Transport
{
    public class SyslogClient : IDisposable
    {
        public SyslogTransport Transport { get; set; }
        public bool EnableTls { get; set; }
        public SyslogFormat Format { get; set; }
        public SyslogFraming Framing { get; set; }
        public bool AutoConnect { get; set; }
        public bool AutoFlush { get; set; }
        public TimeSpan SendTimeout { get; set; }
        public TimeSpan RecvTimeout { get; set; }

        private readonly string _hostname;
        private int _port;
        
        private TcpClient _tcpClient;
        private NetworkStream _tcpStream;
        private bool _disposed;
        
        // Default port is -1, which means to use the default syslog port depending on the transport chosen:
        // * TCP: 6514
        // * UDP:  514
        public SyslogClient(string hostname, int port = -1)
        {
            _hostname = hostname;
            _port = port;
            
            // Defaults to a totally functional Syslog Client
            Transport = SyslogTransport.Tcp;
            EnableTls = false;
            Format = SyslogFormat.RFC5424;
            Framing = SyslogFraming.OctetCounting;
            AutoConnect = true;
            AutoFlush = true;
            SendTimeout = TimeSpan.FromSeconds(5);
            RecvTimeout = TimeSpan.FromSeconds(5);

            _tcpClient.SendTimeout = (int)SendTimeout.TotalMilliseconds;
            _tcpClient.ReceiveTimeout = (int)RecvTimeout.TotalMilliseconds;
        }

        public void Dispose()
        {
            ThowIfDisposed();
            Disconnect();
            
            _disposed = true;
        }
        
        public async Task ConnectAsync()
        {
            ThowIfDisposed();
            ThowIfConnected();
            try
            {
                // If we want the default port, it's assigned the first time it's used
                if (_port == -1)
                {
                    _port = Transport == SyslogTransport.Tcp ? 6514 : 514;
                }
                
                _tcpClient = new TcpClient();
                await _tcpClient.ConnectAsync(_hostname, _port);
                _tcpStream = _tcpClient.GetStream();
            }
            catch
            {
                Disconnect();
                throw;
            }
        }
        
        public void Disconnect()
        {
            ThowIfDisposed();
            try
            {
                _tcpClient?.Close();
            }
            catch (Exception)
            {
                // Not interested in exceptions when trying to close the connection
            }
            
            _tcpClient = null;
            _tcpStream = null;
        }
        
        public async Task SendAsync(SyslogMessage message)
        {
            ThowIfDisposed();

            try
            {
                await EnsureConnected();

                using (var serializedMessage = new MemoryStream())
                {
                    if (Format == SyslogFormat.RFC5424)
                    {
                        await SyslogRfc5424MessageSerializer.SerializeAsync(message, serializedMessage);
                    }
                    else
                    {
                        await SyslogRfc3164MessageSerializer.SerializeAsync(message, serializedMessage);
                    }

                    // Prepended length
                    if (Framing == SyslogFraming.OctetCounting)
                    {
                        var messageLength = Encoding.ASCII.GetBytes(serializedMessage.Length.ToString());
                        await _tcpStream.WriteAsync(messageLength, 0, messageLength.Length);
                        await _tcpStream.WriteByteAsync(0x20); // Space
                    }

                    await _tcpStream.WriteAsync(serializedMessage.GetBuffer(), 0, (int) serializedMessage.Length);

                    // Appended Line-Feed
                    if (Framing == SyslogFraming.NonTransparentFraming)
                    {
                        await _tcpStream.WriteByteAsync(0x0A); // LF
                    }
                }

                if (AutoFlush)
                {
                    await _tcpStream.FlushAsync();
                }
            }
            catch (Exception e)
            {
                throw new SyslogException("Error sending message to server.", e);
            }
        }

        private async Task EnsureConnected()
        {
            if (AutoConnect)
            {
                if (_tcpClient == null)
                {
                    await ConnectAsync();
                }
                else if (!_tcpClient.Connected)
                {
                    Disconnect();
                    await ConnectAsync();
                }
            }
            else
            {
                ThowIfNotConnected();
            }
        }
        
        private void ThowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException("Object had already been disposed of.");
            }
        }

        private void ThowIfNotConnected()
        {
            if (_tcpClient == null)
            {
                throw new SyslogException("Client is not connected to server.");
            }
            if (!_tcpClient.Connected)
            {
                throw new SyslogException("Client has been disconnected.");
            }
        }
        
        private void ThowIfConnected()
        {
            if (_tcpClient != null && _tcpClient.Connected)
            {
                throw new SyslogException("Client is already connected.");
            }
        }
    }
}
