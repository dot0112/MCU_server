using System;
using System.IO;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
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
							if (main.arduino != null)
							{
								main.arduino.Close();
							}
							main.arduino = this;
							SendMessage(this, "Arduino");
							mode = 0;
						}
					}
					if (data == "client")
					{
						lock (main.syncLock)
						{
							if (main.client != null)
							{
								main.client.Close();
							}
							main.client = this;
							SendMessage(this, "Client");
							mode = 1;
						}
					}
					if (data == "camera")
					{
						lock (main.syncLock)
						{
							if (main.camera != null)
							{
								main.camera.Close();
							}
							main.camera = this;
							SendMessage(this, "camera");
							mode = 2;
						}
						// camera 접속 전 마지막 확인 문자 받음
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
				if(stream!=null) stream.Close();
				if(client!=null) client.Close();
			}
		}

		public static void SendMessage(ClientHandler client, string message)
		{
			if (client.client != null)
			{
				byte[] msg = Encoding.ASCII.GetBytes(message);
				client.client.GetStream().Write(msg, 0, msg.Length);
			}
		}

		public void Close()
		{
			this.client.Close();
			this.stream.Close();
		}
	}
}
