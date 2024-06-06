using System;
using System.IO;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using MCU_server;

namespace MCU_server
{
	class ClientHandler
	{
		private TcpClient client;
		private NetworkStream stream;
		private int clientId;
		private int mode = -1;  // -1:NULL, 0:arduino, 1:client, 2:camera
		public int ClientId => clientId;
		byte[] buffer = new byte[1024];
		int bytesRead;

		public ClientHandler(TcpClient client, int clientId)
		{
			this.client = client;
			this.stream = client.GetStream();
			this.clientId = clientId;
		}

		public void HandleClient()
		{
			try
			{
				while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) != 0)
				{
					string data = Encoding.ASCII.GetString(buffer, 0, bytesRead);
					Console.WriteLine($"클라이언트 [{clientId}] : {data}");
					if (data == "arduino")
					{
						lock (main.syncLock)
						{
							if (main.arduino == null)
							{
								main.arduino = client;
								SendMessage(client, "Arduino");
								mode = 0;
							}
							else
							{
								SendMessage(client, "fail");
								continue;
							}
						}
					}
					if (data == "client")
					{
						lock (main.syncLock)
						{
							if (main.client == null)
							{
								main.client = client;
								SendMessage(client, "Client");
								mode = 1;
							}
							else
							{
								SendMessage(client, "fail");
								continue;
							}
						}
					}
					if (data == "camera")
					{
							SendMessage(client, "camera");
							mode = 2;
							byte[] read = new byte[1];
							stream.Read(read, 0, 1);
					}
					break;
				}

				if (mode == 0)
				{
					ClientArduino ca = new ClientArduino();
					ca.socketArduino(client);
				}
				if (mode == 1)
				{
					ClientNormal cn = new ClientNormal();
					cn.socketNormal(client);
				}
				if (mode == 2)
				{
					ClientCamera cc = new ClientCamera();
					cc.socketCamera(client);
				}
			}
			catch (Exception e)
			{
				Console.WriteLine($"클라이언트 [{clientId}] 처리 중 예외 발생: {e.Message}");
			}
			finally
			{
				Console.WriteLine($"클라이언트 [{clientId}] : {(mode == 0 ? "Arduino" : (mode == 1 ? "Normal" : "Camera"))}");
			}
		}

		public static void SendMessage(TcpClient client, string message)
		{
			if (client.Connected)
			{
				byte[] msg = Encoding.ASCII.GetBytes(message);
				client.GetStream().Write(msg, 0, msg.Length);
			}
		}
	}
}
