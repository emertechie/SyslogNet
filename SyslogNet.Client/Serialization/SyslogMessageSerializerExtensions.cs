
namespace SyslogNet.Client.Serialization
{


	public static class SyslogMessageSerializerExtensions
	{


		public static byte[] Serialize(this ISyslogMessageSerializer serializer, SyslogMessage message)
		{
			byte[] datagramBytes;
			using (System.IO.MemoryStream stream = new System.IO.MemoryStream())
			{
				serializer.Serialize(message, stream);

				stream.Position = 0;

				datagramBytes = new byte[stream.Length];
				stream.Read(datagramBytes, 0, (int)stream.Length);
			}

			return datagramBytes;
		}	
		

	}


}