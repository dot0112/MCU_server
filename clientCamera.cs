using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace MCU_server
{
	internal class ClientCamera
	{
		public static MemoryStream imageData = null;

		public void socketCamera(TcpClient c)
		{
			try
			{

				TcpClient client = c;
				NetworkStream stream = client.GetStream();
				MemoryStream ms = new MemoryStream();
				byte[] buffer = new byte[4096];
				int bytesRead = 0;
				while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
				{
					ms.Write(buffer, 0, bytesRead);
					int start = -1;
					int end = -1;
					byte[] data = ms.ToArray();
					for (int i = 0; i < data.Length - 1; i++)
					{
						if (data[i] == 0xFF && data[i + 1] == 0xD8) { start = i; }
						if (data[i] == 0xFF && data[i + 1] == 0xD9) { end = i + 2; }
						if (start != -1 && end != -1 && end > start)
						{
							byte[] frameData = new byte[end - start];
							Array.Copy(data, start, frameData, 0, frameData.Length);
							using (MemoryStream mest = new MemoryStream(frameData))
							{
								imageData = mest;

							}
							ms = new MemoryStream();
							ms.Write(data, end, data.Length - end);
							break;
						}
					}
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e.ToString());
			}
			finally
			{
				c.Close();
			}
		}
	}
}

