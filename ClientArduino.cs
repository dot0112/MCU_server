using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MCU_server
{
	internal class ClientArduino
	{
		public void socketArduino(TcpClient c)
		{
			try
			{
				TcpClient client = c;
				NetworkStream stream = c.GetStream();
				MemoryStream ms = new MemoryStream();
				byte[] buffer = new byte[1024];
				int bytesRead;
				while (true)
				{
					/*while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) != 0)
					{
						DateTime startTime = DateTime.Now; // 선언 위치 확인 필요
						while ((DateTime.Now - startTime).TotalSeconds < 3) { }
						// 재요청 전달
						ClientHandler.SendMessage(main.arduino, "r");
					}*/
				}
			} catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}
			finally
			{
				lock(main.syncLock)
				{
					main.arduino.Close();
					main.arduino = null;
				}
			}
		}
	}
}
