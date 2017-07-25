namespace SyslogNet.Client.Transport
{
    // https://tools.ietf.org/html/rfc6587#section-3.4.1
    public enum SyslogFraming
    {
        Implicit,                    // only valid for UDP and TCP/TLS
        OctetCounting,               // only valid for TCP: a.k.a. prepended length
        NonTransparentFraming,       // only valid for TCP: a.k.a. separated by LF (0x0A)
    }
}
