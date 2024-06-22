using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using System.Collections.Generic;

namespace MCU_server
{
	internal class main
	{
		private static TcpListener server = null;
		private static int clientCounter = 0; // 클라이언트 ID를 위한 카운터
		public static ClientHandler arduino = null, client = null, camera = null;
		public static object syncLock = new object(); // 동기화 객체

		private static List<ClientHandler> clientHandlers = new List<ClientHandler>(); // 클라이언트 핸들러 리스트
		static void Main(string[] args)
		{
			try
			{
				IPAddress localAddr = IPAddress.Parse("172.31.0.161");
				int port = 80;

				while (true)
				{
					try
					{
						server = new TcpListener(localAddr, port);
						server.Start();
						break;
					}
					catch (Exception e)
					{
						Console.WriteLine(e.ToString());
						Console.Write("IPAddress: ");
						localAddr = IPAddress.Parse(Console.ReadLine());
						Console.Write("port: ");
						port = int.Parse(Console.ReadLine());
					}
				}
				Console.WriteLine($"Server is listening on {localAddr}:{port}...");

				while (true)
				{
					TcpClient client = server.AcceptTcpClient();
					int clientId = ++clientCounter; // 클라이언트 ID 할당
					Console.WriteLine($"Client [{clientId}] connected!");

					// 클라이언트 처리를 위한 ClientHandler 인스턴스 생성
					ClientHandler clientHandler = new ClientHandler(client, clientId);

					lock (syncLock)
					{
						clientHandlers.Add(clientHandler);
					}

					// 클라이언트 처리를 위한 스레드 생성
					Thread clientThread = new Thread(new ThreadStart(clientHandler.HandleClient));
					clientThread.Start();
				}
			}
			catch (Exception e)
			{
				Console.WriteLine("Exception: " + e.Message);
			}
			finally
			{
				if (server != null)
					server.Stop();
			}
		}
		public static void RemoveClientHandler(int clientId)
		{
			lock (syncLock)
			{
				clientHandlers.RemoveAll(ch => ch.ClientId == clientId);
			}
		}
	}
}
