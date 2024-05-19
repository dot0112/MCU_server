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
		public static int arduino = -1, client = -1;
		public static object syncLock = new object(); // 동기화 객체

		private static List<ClientHandler> clientHandlers = new List<ClientHandler>(); // 클라이언트 핸들러 리스트
		static void Main(string[] args)
		{
			try
			{
				IPAddress localAddr = IPAddress.Parse("127.0.0.1");
				int port = 12345;
				server = new TcpListener(localAddr, port);

				server.Start();
				Console.WriteLine($"서버가 {localAddr}:{port}에서 대기 중입니다...");

				while (true)
				{
					TcpClient client = server.AcceptTcpClient();
					int clientId = ++clientCounter; // 클라이언트 ID 할당
					Console.WriteLine($"클라이언트 [{clientId}]가 연결되었습니다!");

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
				Console.WriteLine("예외: " + e.Message);
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

		public static void SendCommandToClient(int clientId, string message)
		{
			lock (syncLock)
			{
				ClientHandler handler = clientHandlers.Find(ch => ch.ClientId == clientId);
				if (handler != null)
				{
					handler.SendMessage(message);
				}
				else
				{
					Console.WriteLine($"클라이언트 [{clientId}]를 찾을 수 없습니다.");
					return;
				}
			}
		}
	}
}
