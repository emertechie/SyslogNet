using System;
using System.Threading;
using CommandLine;
using SyslogNet;
using SyslogNet.Client;
using SyslogNet.Client.Serialization;
using SyslogNet.Client.Transport;

namespace TesterApp
{
	internal class Options
	{
        [Option('h', "hostName", Required = false, HelpText = "The host name. If not set, defaults to the NetBIOS name of the local machine")]
        public string LocalHostName { get; set; }

        [Option('a', "appName", Required = true, HelpText = "The application name")]
        public string AppName { get; set; }

        [Option('p', "procId", Required = false, HelpText = "The process identifier")]
        public string ProcId { get; set; }

        [Option('t', "msgType", Required = false, HelpText = "The message type (called msgId in spec)")]
        public string MsgType { get; set; }

        [Option('m', "msg", Required = false, HelpText = "The message")]
        public string Message { get; set; }

        [Option('s', "syslogServer", Required = true, HelpText = "Host name of the syslog server")]
        public string SyslogServerHostname { get; set; }

        [Option('r', "syslogPort", Required = true, HelpText = "The syslog server port")]
        public int SyslogServerPort { get; set; }

        [Option('v', "version", Required = false, Default = "5424", HelpText = "The version of syslog protocol to use. Possible values are '3164' and '5424' (from corresponding RFC documents) or 'local' to send messages to a local syslog (only on Linux or OS X). Default is '5424'")]
        public string SyslogVersion { get; set; }

        [Option('o', "protocol", Required = false, Default = "tcp", HelpText = "The network protocol to use. Possible values are 'tcp' or 'udp' to send to a remote syslog server, or 'local' to send to a local syslog over Unix sockets (only on Linux or OS X). Default is 'tcp'. Note: TCP always uses SSL connection.")]
        public string NetworkProtocol { get; set; }

        [Option('c', "cert", Required = false, HelpText = "Optional path to a CA certificate used to verify Syslog server certificate when using TCP protocol")]
        public string CACertPath { get; set; }
    }

	public static class Program
	{
		public static void Main(string[] args)
		{
			try
			{
                Options options = null;
                CommandLine.Parser.Default.ParseArguments<Options>(args).WithParsed(opt => options = opt);

                if (options != null)
				{
					// string exceptionMessage = CreateExceptionMessageLevel1();

					ISyslogMessageSerializer serializer = options.SyslogVersion == "5424"
						? new SyslogRfc5424MessageSerializer() 
                        : options.SyslogVersion == "3164" 
                                ? new SyslogRfc3164MessageSerializer() 
                                : (ISyslogMessageSerializer)new SyslogLocalMessageSerializer();

					ISyslogMessageSender sender = options.NetworkProtocol == "tcp"
#if NET4_0
                        ? new SyslogEncryptedTcpSender(options.SyslogServerHostname, options.SyslogServerPort)
#else
                        ? new SyslogTcpSender(options.SyslogServerHostname, options.SyslogServerPort)
#endif
                        : options.NetworkProtocol == "udp"
							? new SyslogUdpSender(options.SyslogServerHostname, options.SyslogServerPort)
							: (ISyslogMessageSender)new SyslogLocalSender();

					SyslogMessage msg1 = CreateSyslogMessage(options);
					sender.Send(msg1, serializer);
					Console.WriteLine("Sent message 1");

					Thread.Sleep(5000);

					SyslogMessage msg2 = CreateSyslogMessage(options);
					sender.Send(msg2, serializer);
					Console.WriteLine("Sent message 2");
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("ERROR: " + ex);
			}
		}

		private static SyslogMessage CreateSyslogMessage(Options options)
		{
			return new SyslogMessage(
				DateTimeOffset.Now,
				Facility.UserLevelMessages,
				Severity.Error,
				options.LocalHostName ?? Environment.MachineName,
				options.AppName,
				options.ProcId,
				options.MsgType,
				options.Message ?? "Test message at " + DateTime.Now);
		}

		private static string CreateExceptionMessageLevel1()
		{
			try
			{
				return CreateExceptionMessageLevel2();
			}
			catch (Exception ex)
			{
				return ex.ToString();
			}
		}

		private static string CreateExceptionMessageLevel2()
		{
			return CreateExceptionMessageLevel3();
		}

		private static string CreateExceptionMessageLevel3()
		{
			return CreateExceptionMessageLevel4();
		}

		private static string CreateExceptionMessageLevel4()
		{
			throw new Exception("Foo bar");
		}
	}
}