using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SyslogNet.Client.Extensions;
using SyslogNet.Client.Serialization;
using SyslogNet.Client.Transport;

namespace SyslogNet.Client
{
    public class SyslogClient : IDisposable
    {
        public SyslogTransport Transport { get; set; }
        public SyslogFormat Format { get; set; }
        public SyslogFraming Framing { get; set; }
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
            Format = SyslogFormat.RFC5424;
            Framing = SyslogFraming.OctetCounting;
            SendTimeout = TimeSpan.FromSeconds(5);
            RecvTimeout = TimeSpan.FromSeconds(5);
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

                _tcpClient = new TcpClient
                {
                    SendTimeout = (int) SendTimeout.TotalMilliseconds,
                    ReceiveTimeout = (int) RecvTimeout.TotalMilliseconds
                };
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

        public Task SendAsync(string message, CancellationToken token = default(CancellationToken))
        {
            return SendAsync(new SyslogMessage
                {
                    Message = message
                },
                token);
        }

        public async Task SendAsync(SyslogMessage message, CancellationToken token = default(CancellationToken))
        {
            ThowIfDisposed();

            try
            {
                ThowIfNotConnected();

                using (var serializedMessage = new MemoryStream())
                {
                    if (Format == SyslogFormat.RFC5424)
                    {
                        await SyslogRfc5424MessageSerializer.SerializeAsync(message, serializedMessage, token);
                    }
                    else
                    {
                        await SyslogRfc3164MessageSerializer.SerializeAsync(message, serializedMessage, token);
                    }

                    // Prepended length
                    if (Framing == SyslogFraming.OctetCounting)
                    {
                        var messageLength = Encoding.ASCII.GetBytes(serializedMessage.Length.ToString());
                        await _tcpStream.WriteAsync(messageLength, 0, messageLength.Length, token);
                        await _tcpStream.WriteByteAsync(' ', token); // Space
                    }

                    await _tcpStream.WriteAsync(serializedMessage.GetBuffer(), 0, (int) serializedMessage.Length, token);

                    // Appended Line-Feed
                    if (Framing == SyslogFraming.NonTransparentFraming)
                    {
                        await _tcpStream.WriteByteAsync('\n', token); // LF
                    }
                }

                await _tcpStream.FlushAsync(token);
            }
            catch (Exception e)
            {
                throw new SyslogException("Error sending message to server.", e);
            }
        }

        public bool IsConnected()
        {
            return _tcpClient != null && _tcpClient.Connected;
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
            if (!IsConnected())
            {
                throw new SyslogException("Client is not connected to server.");
            }
        }
        
        private void ThowIfConnected()
        {
            if (IsConnected())
            {
                throw new SyslogException("Client is already connected.");
            }
        }
    }
}
