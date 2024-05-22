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
		byte[] buffer_image = new byte[4096];
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
					Console.WriteLine(data);
					if (data == "arduino")
					{
						lock (main.syncLock)
						{
							if (main.arduino == -1)
							{
								main.arduino = clientId;
								main.SendCommandToClient(clientId, "Arduino");
								mode = 0;
							}
							else
							{
								main.SendCommandToClient(clientId, "fail");
								continue;
							}
						}
					}
					if (data == "client")
					{
						lock (main.syncLock)
						{
							if (main.client == -1)
							{
								main.client = clientId;
								main.SendCommandToClient(clientId, "Client");
								mode = 1;
							}
							else
							{
								main.SendCommandToClient(clientId, "fail");
								continue;
							}
						}
					}
					if (data == "camera")
					{
								main.SendCommandToClient(clientId, "camera");
								mode = 2;
						byte[] read = new byte[1];
						stream.Read(read, 0, 1);
					}
					break;
				}

				if (mode == 0)
				{
					socket_arduino();
				}
				if (mode == 1)
				{
					socket_client();
				}
				if (mode == 2)
				{
					clientCamera cc = new clientCamera();
					cc.socketCamera_T(client);
				}
			}
			catch (Exception e)
			{
				Console.WriteLine($"클라이언트 [{clientId}] 처리 중 예외 발생: {e.Message}");
			}
			finally
			{
				/*// 클라이언트 연결 종료
				if (stream != null)
				{
					stream.Close();
				}
				if (client != null)
				{
					client.Close();
				}
				Console.WriteLine($"클라이언트 [{clientId}] 연결 종료");*/
				lock (main.syncLock)
				{
					if (clientId == main.arduino) main.arduino = -1;
					if (clientId == main.client) main.client = -1;
				}
			}
		}

		public void SendMessage(string message)
		{
			if (client.Connected)
			{
				byte[] msg = Encoding.ASCII.GetBytes(message);
				stream.Write(msg, 0, msg.Length);
			}
		}

		public void socket_arduino()
		{
			while (true)
			{
				while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) != 0)
				{
					DateTime startTime = DateTime.Now; // 선언 위치 확인 필요
					while ((DateTime.Now - startTime).TotalSeconds < 3) { }
					// 재요청 전달
					main.SendCommandToClient(main.arduino, "r");
				}
			}
		}

		public void socket_client()
		{
			while (true)
			{
				while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) != 0)
				{
					string data = Encoding.ASCII.GetString(buffer, 0, bytesRead);

					if (data == "w")
					{
						main.SendCommandToClient(main.arduino, "w");
					}
					if (data == "s")
					{
						main.SendCommandToClient(main.arduino, "s");
					}
					if (data == "a")
					{
						main.SendCommandToClient(main.arduino, "a");
					}
					if (data == "d")
					{
						main.SendCommandToClient(main.arduino, "d");
					}
					main.SendCommandToClient(clientId, " ");
				}
			}
		}
	}
}
