#SyslogNet

.Net Syslog client. Supports both RFC 3164 and RFC 5424 Syslog standards as well as UDP and encrypted TCP transports.

##Installation

Available on NuGet:
```
Install-Package SyslogNet.Client
```

## Code Samples

### Getting started
Connecting to a SyslogNg server with the default properties (TCP server with no encryption, using the format RFC5424 with prepended length for each message):

```
public async Task SendMessages()
{
    using (var client = new SyslogClient("192.168.100.100", ""))
    {
        await client.ConnectAsync();
        await client.SendMessageAsync("Hello World!");
        await client.SendMessageAsync("See you later.");
    }
}
```

This is how you could configure SyslogNg to read messages sent by this sample program:
```
source s_test {
    syslog(
        port(6514)
    )
};

log {
    source(s_test);
    destination(d_syslog);
}
```

### More control

Here is how to explicitly set some stuff, and have more control over the connection (it works with the same SyslgNg snippet from before):
```
public async Task SendMessages()
{
    var client = new SyslogClient("192.168.100.100", "")
    {
            Transport = SyslogTransport.Tcp;
            Format = SyslogFormat.RFC5424;
            Framing = SyslogFraming.OctetCounting;
            SendTimeout = TimeSpan.FromSeconds(5);
            RecvTimeout = TimeSpan.FromSeconds(5);
    };
    
    await client.ConnectAsync();
    
    var message = new SyslogMessage
    {
        Facility = Facility.LogAlert,
        Severity = Severity.Emergency,
        Message = "Hello World!"
    };
    await client.SendMessageAsync(message);
    message.Message ="See you later.";
    message.Timestamp = DateTimeOffset.UtcNow;
    await client.SendMessageAsync(message);
    
    await client.Disconnect();
    
    // Some time later:
    client.Dispose();
}
```


### Using CancellationTokens
You can pass a CancellationToken to the asynchronous functions like this:

```
public async Task SendMessages()
{
    var cancelAfter = TimeSpan.FromSeconds(10);
    
    using (var tokenSource = new CancellationTokenSource(cancelAfter))
    using (var client = new SyslogClient("192.168.100.100", ""))
    {
        await client.ConnectAsync(tokenSource.Token);
        await client.SendMessageAsync("Hello World!", tokenSource.Token);
        await client.SendMessageAsync("See you later.", tokenSource.Token);
    }
}
```

