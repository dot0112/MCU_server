using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MCU_server
{
	internal class ClientNormal
	{
		public void socketNormal(TcpClient c)
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
					while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) != 0)
					{
						string data = Encoding.ASCII.GetString(buffer, 0, bytesRead);
						if (main.arduino != null)
						{
							if (data == "w")
							{
								// 전진
								ClientHandler.SendMessage(main.arduino, "w");
								Console.WriteLine("Normal: w");
							}
							if (data == "s")
							{
								// 후진
								ClientHandler.SendMessage(main.arduino, "s");
								Console.WriteLine("Normal: s");
							}
							if (data == "a")
							{
								// 좌회전
								ClientHandler.SendMessage(main.arduino, "a");
								Console.WriteLine("Normal: a");
							}
							if (data == "d")
							{
								// 우회전
								ClientHandler.SendMessage(main.arduino, "d");
								Console.WriteLine("Normal: d");
							}
						}
						if (data == "p")
						{
							// 이미지 요청
							lock (ClientCamera.syncLock)
							{
								if (ClientCamera.imageData != null)
								{
									ClientCamera.imageData.Seek(0, SeekOrigin.Begin);
									ClientCamera.imageData.CopyTo(ms);
									ms.Position = 0; // 스트림 위치를 시작으로 되돌림

									byte[] dataBuffer = new byte[1024];
									int read;
									while ((read = ms.Read(dataBuffer, 0, dataBuffer.Length)) > 0)
									{
										stream.Write(dataBuffer, 0, read);
									}
									ms = new MemoryStream();
								}
								else
								{
									StanbyScreen.Send(stream);
								}
							}
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
				lock (main.syncLock)
				{
					main.client.Close();
					main.client = null;
				}
			}
		}
	}
}
