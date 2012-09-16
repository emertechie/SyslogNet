using System;
using System.IO;
using System.Net.Sockets;
using CommandLine;
using SyslogNet;

namespace TesterApp
{
	public static class Program
	{
		public static void Main(string[] args)
		{
			var options = new Options();
			if (new CommandLineParser().ParseArguments(args, options))
			{
				var syslogMessage = new SyslogMessage(
					DateTimeOffset.Now,
					Facility.UserLevelMessages,
					Severity.Informational,
					options.HostName ?? Environment.MachineName,
					options.AppName,
					options.ProcId,
					options.MsgType,
					options.Message);

				var client = new UdpClient(options.SyslogServerHostname, options.SyslogServerPort);

				try
				{
					using (var stream = new MemoryStream())
					{
						var serializer = new SyslogMessageSerializer();
						serializer.Serialize(syslogMessage, stream);

						stream.Position = 0;
						byte[] datagramBytes = stream.GetBuffer();

						client.Send(datagramBytes, (int)stream.Length);
					}
				}
				finally
				{
					client.Close();
				}
			}
		}
	}

	internal class Options
	{
		[Option("h", "hostName", Required = false, HelpText = "The host name. If not set, defaults to the NetBIOS name of the local machine")]
		public string HostName { get; set; }

		[Option("a", "appName", Required = false, HelpText = "The application name")]
		public string AppName { get; set; }

		[Option("p", "procId", Required = false, HelpText = "The process identifier")]
		public string ProcId { get; set; }

		[Option("t", "msgType", Required = false, HelpText = "The message type (called msgId in spec)")]
		public string MsgType { get; set; }

		[Option("m", "msg", Required = false, HelpText = "The message")]
		public string Message { get; set; }

		[Option("s", "syslogServer", Required = true, HelpText = "Host name of the syslog server")]
		public string SyslogServerHostname { get; set; }

		[Option("r", "syslogPort", Required = true, HelpText = "The syslog server port")]
		public int SyslogServerPort { get; set; }
	}
}