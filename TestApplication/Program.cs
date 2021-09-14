
namespace TestApplication
{


	public class Program
    {


		private static SyslogNet.Client.SyslogMessage CreateSyslogMessage(
              SyslogNet.Client.SyslogOptions options
			, string message)
		{
            // https://www.syslog-ng.com/technical-documents/doc/syslog-ng-open-source-edition/3.16/administration-guide/option-description-log-msg-size
            // Description: Maximum length of a message in bytes. 
            // This length includes the entire message 
            // (the data structure and individual fields). 
            // The maximal value that can be set is 268435456 bytes(256MB). 
            // For messages using the IETF-syslog message format(RFC5424), 
            // the maximal size of the value of an SDATA field is 64kB.
            // In most cases, it is not recommended to set log-msg-size() 
            // higher than 10 MiB.

            // https://stackoverflow.com/questions/3310875/find-syslog-max-message-length
            // Keep in mind syslog is a protocol, 
            // which means it sets minimums and makes recommendations. 
            // I can't find a source to this, but I believe 
            // the minimum length that should be supported is 1k, 
            // with 64k being recommended.


            System.Collections.Generic.Dictionary<string, string> sd = new System.Collections.Generic.Dictionary<string, string>(System.StringComparer.InvariantCultureIgnoreCase);
			sd["Hello"] = "World";
			sd["Привет"] = "мир";
			sd["你好"] = "世界";


			SyslogNet.Client.StructuredDataElement sde = new SyslogNet.Client.StructuredDataElement("sdld", sd);


			// Each implementation is free to do what they want, 
			// i.e. if you wanted a 16MB maximum and were writing 
			// a syslog server, you're free to do that. 
			// I'm not sure why you would, but you could.
			// As far as I know, there is no standard programatic way 
			// of ascertaining this, so keeping messages at 
			// just under 1k would be ideal for portability.
			return new SyslogNet.Client.SyslogMessage(
				System.DateTimeOffset.Now,
				SyslogNet.Client.Facility.UserLevelMessages,
				SyslogNet.Client.Severity.Error,
				options.LocalHostName,
				options.AppName,
				options.ProcId,
				options.MsgType,
				message ?? (options.Message ?? 
				  "Test message at " 
				+ System.DateTime.UtcNow.ToString("dddd, dd.MM.yyyy HH:mm:ss.fff", System.Globalization.CultureInfo.InvariantCulture))
				, null
			);
		} // End Function CreateSyslogMessage 



		public static bool AllowAnything(
			  object sender
			, System.Security.Cryptography.X509Certificates.X509Certificate certificate
			, System.Security.Cryptography.X509Certificates.X509Chain chain
			, System.Net.Security.SslPolicyErrors sslPolicyErrors)
		{
			return true;
		} // End Function AllowAnything 


		public static void Main(string[] args)
        {
			// System.Net.ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(AllowAnything);

			// SyslogNet.Client.SyslogOptions options = null;
			// delegate (SyslogNet.Client.SyslogOptions a) { options = a; }
			// CommandLine.Parser.Default.ParseArguments<Options>(args).WithParsed(opt => options = opt);
			// CommandLine.ParserResultExtensions.WithParsed(CommandLine.Parser.Default.ParseArguments<Options>(args), opt => options = opt);
			
			SyslogNet.Client.SyslogOptions options = new SyslogNet.Client.SyslogOptions();
			// options.SyslogVersion = SyslogNet.Client.SyslogVersions.Rfc3164;
			options.SyslogVersion = SyslogNet.Client.SyslogVersions.Rfc5424;
			options.NetworkProtocol = SyslogNet.Client.NetworkProtocols.TCP;
			// options.NetworkProtocol = SyslogNet.Client.NetworkProtocols.UPD;
			// options.SyslogServerPort = 515; // Visual Syslog 
			options.InferDefaultPort();
			
			System.Console.WriteLine(options);

			string logMessage = "Test message 112 äöüÄÖÜß 你好世界 Привет мир";
			// logMessage = "test123";

			SyslogNet.Client.SyslogMessage msg1 = CreateSyslogMessage(options, logMessage);
			msg1.Send(options);

			System.Console.WriteLine("Sent " + msg1.Message);


			System.Console.WriteLine(" --- Press any key to continue --- ");
            System.Console.ReadKey();
		} // End Sub Main 


	} // End Class Program 


} // End Namespace TestApplication 
