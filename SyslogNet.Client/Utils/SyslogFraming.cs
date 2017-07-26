namespace SyslogNet.Client.Transport
{
    // Ignored for SyslogTransport.Udp and SyslogTransport.TlsTcp
    // https://tools.ietf.org/html/rfc6587#section-3.4.1
    public enum SyslogFraming
    {
        OctetCounting,               // only valid for TCP: a.k.a. prepended length
        NonTransparentFraming,       // only valid for TCP: a.k.a. separated by LF (0x0A)
    }
}
