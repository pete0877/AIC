using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections;

namespace AIC
{
	/// <summary>
	/// Summary description for TestClientBot.
	/// </summary>
	public class TestClientBot
	{
		protected NetworkClient m_client;
		protected Arena			m_arena;
		protected int			m_x;
		protected int			m_y;
		protected int			m_demage;
		protected int			m_armor;
		protected int			m_speed;
		protected int			m_firePower;
		protected int			m_fireSpeed;
		protected int			m_direction;
		protected int			m_countMoves;

		/// <summary>
		/// Constructor
		/// </summary>
		public TestClientBot()
		{	
			Reset();
		}


		public void Reset()
		{
			m_arena = null;
			m_client = null;
			m_x = -1;
			m_y = -1;
			m_demage = -1;
			m_armor = -1;
			m_speed = -1;
			m_firePower = -1;
			m_fireSpeed = -1;

			m_direction = 1;
			m_countMoves = 0;
		}


		/// <summary>
		/// Runs under given name
		/// </summary>
		/// <param name="myName"></param>
		public void Run(string myName)
		{
			Reset();

			try
			{
				TcpClient tcpClient = new TcpClient();
				tcpClient.Connect("localhost", 888);			
				m_client = new NetworkClient(tcpClient);

				string returndata;
				
				returndata = m_client.Receive();
				m_client.Send("NAME:" + myName);
				m_client.Send("COMMAND:FIGHT");

				while(true)
				{

					// Receive state:
					returndata = "";
					while (returndata != "BEGIN")
					{
						returndata = m_client.Receive();

						try
						{
							string[] actionAttribs = returndata.Split(':');
							string actionCommand = actionAttribs[0];

							if (actionCommand == "JOINING")
							{
							}
							else if (actionCommand == "SIZE")
							{	
								int x = 0;
								int y = 0;
								string[] parameters = actionAttribs[1].Split(',');
								x = int.Parse(parameters[0]);
								y = int.Parse(parameters[1]); 
								m_arena = new Arena(x, y);
							}
							else if (actionCommand == "MAP")
							{				
								string parameters = actionAttribs[1];
								string[] cells = parameters.Split(';');
								IEnumerator itr = cells.GetEnumerator();
								while (itr.MoveNext())
								{
									string cellSpec = (string) itr.Current;
									cellSpec = cellSpec.Trim();
									if (cellSpec.Length > 0)
									{
										string[] cellSpecList = cellSpec.Split('|');
										string[] positionList = cellSpecList[0].Split(',');
										int x = int.Parse(positionList[0]);
										int y = int.Parse(positionList[1]);

										string cellState = cellSpecList[1];
										ArenaCell cell = new ArenaCell();
										cell.SetState(cellState);

										m_arena.SetCell(x, y, cell);
									}
								}
							}
							else if (actionCommand == "POS")
							{
								string[] parameters = actionAttribs[1].Split(',');
								m_x = int.Parse(parameters[0]);
								m_y = int.Parse(parameters[1]); 
							}
							else if (actionCommand == "ARMOR")
							{
								m_armor = int.Parse(actionAttribs[1]); 
							}
							else if (actionCommand == "DEMAGE")
							{
								m_demage = int.Parse(actionAttribs[1]); 
							}
							else if (actionCommand == "SPEED")
							{				
								m_speed = int.Parse(actionAttribs[1]); 
							}
							else if (actionCommand == "FIREPOWER")
							{				
								m_firePower = int.Parse(actionAttribs[1]); 
							}
							else if (actionCommand == "FIRESPEED")
							{				
								m_fireSpeed = int.Parse(actionAttribs[1]); 
							}
						}
						catch(Exception e)
						{
							throw new Exception("Invalid action: '" + returndata + "'");
						}
					}


					// Simulate thinking for 1 second:
					Thread.Sleep(300);

					// Try to move north & be done
					if (m_y < 5)
						m_direction = 1;
					if (m_y > 28)
						m_direction = -1;

					if (m_direction == 1)
						m_client.Send("MOVE:down");
					else
						m_client.Send("MOVE:up");

					m_countMoves++;
					if (m_countMoves == 3)
					{
						Random r = new Random();
						int fireX = r.Next(2, 62);
						int fireY = r.Next(2, 30);
						m_client.Send("FIRE:" + fireX.ToString() + "," + fireY.ToString());
						m_countMoves = 0;
					}
					
					m_client.Send("DONE");
				}

			}	
			catch(Exception e)
			{
			}
		}
	}
}


