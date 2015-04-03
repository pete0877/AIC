using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace AIC
{
	/// <summary>
	/// Wrapper class around TcpClient that is dedicated to sending and receiving lines of characeters (each communication token ends with \n)
	/// </summary>
	public class NetworkClient
	{
		protected int m_bufferSize = 16384;		
		protected byte[] m_buffer;
		protected int m_leftOverStart = 1;
		protected int m_leftOverEnd = 0;
		protected TcpClient m_tcpClient;

		public NetworkClient(TcpClient tcpClient)
		{
			m_buffer = new byte[m_bufferSize];
			m_tcpClient = tcpClient;

			m_tcpClient.ReceiveTimeout = 60000;
			m_tcpClient.SendTimeout = 60000;
		}


		/// <summary>
		/// Sends given string to the network client. New line feed (\n) will be appended to the sent string.
		/// </summary>
		/// <param name="client">Client</param>
		/// <param name="content">String to be sent</param>
		public void Send(string content)
		{
			Byte[] sendBytes = Encoding.ASCII.GetBytes(content + "\n");
			m_tcpClient.GetStream().Write(sendBytes, 0, sendBytes.Length);		
		}


		/// <summary>
		/// Receives response from client. When client sends response, it must end with \n
		/// </summary>
		/// <param name="client">Client</param>
		/// <returns>Response string</returns>
		public string Receive()
		{
			string result = "";
			// int bufferSize = client.ReceiveBufferSize;
			
			bool foundEnd = false;

			while (!foundEnd)
			{		
				// Go through characters remaining from last read:
				while (m_leftOverStart <= m_leftOverEnd)
				{					
					byte dataChar = m_buffer[m_leftOverStart];
					if (dataChar == 0 || dataChar == 10 || dataChar == 13)
					{
						m_leftOverStart++;
						foundEnd = true;
						break;
					}
					result += Encoding.ASCII.GetString(m_buffer, m_leftOverStart, 1);
					m_leftOverStart++;
				}

				if (foundEnd)
					break;			

				int bytesReceived = 0;
				while (bytesReceived == 0)
				{					
					bytesReceived = m_tcpClient.GetStream().Read(m_buffer, 0, m_bufferSize);
					if (bytesReceived == 0)
						Thread.Sleep(100);
				}


				// Remove special characters:
				int stringLenghth = 0;
				for (int n = 0; n < bytesReceived; n++)
				{
					byte dataChar = m_buffer[n];
					if (dataChar == 0 || dataChar == 10 || dataChar == 13)
					{
						foundEnd = true;
						break;
					}
					else 
						stringLenghth++;
				}

				result += Encoding.ASCII.GetString(m_buffer, 0, stringLenghth);				

				// Find out how much is remaining in the buffer:
				m_leftOverStart = stringLenghth + 1;
				m_leftOverEnd = bytesReceived - 1;
			}

			return result;
		}
	}
}
