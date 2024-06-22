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
				while (true)
				{
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
