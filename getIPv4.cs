using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace MCU_server
{
	class getIPv4
	{
		public static string GetIPv4()
		{
			string ret = null;
			while (ret == null)
			{
				try
				{
					// 모든 네트워크 인터페이스 정보를 가져옵니다.
					NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

					// 각 네트워크 인터페이스의 정보를 확인합니다.
					foreach (NetworkInterface netInterface in networkInterfaces)
					{
						// 인터페이스가 활성화되어 있고 이더넷 인터페이스인 경우에만 처리합니다.
						if (netInterface.OperationalStatus == OperationalStatus.Up && netInterface.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
						{
							// 네트워크 인터페이스의 IPv4 주소 정보를 가져옵니다.
							foreach (UnicastIPAddressInformation ip in netInterface.GetIPProperties().UnicastAddresses)
							{
								// IPv4 주소인 경우에만 출력합니다.
								if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
								{
									ret = ip.Address.ToString();
								}
							}
						}
					}
				}
				catch (Exception ex)
				{
					// 오류 발생 시 오류 메시지 출력
					Console.WriteLine("Error: " + ex.Message);
				}
			}
			return ret;
		}
	}
}
