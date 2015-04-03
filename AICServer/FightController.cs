using System;
using System.Collections;

namespace AIC
{
	/// <summary>
	/// Class which controls fights within the arena.
	/// </summary>
	public class FightController
	{
		/// <summary>
		/// Arena on which all the fights are done
		/// </summary>
		protected Arena				m_arena;

		/// <summary>
		/// List of bots participating in the fight (usually 2)
		/// </summary>
		protected ArrayList			m_bots;

		/// <summary>
		/// List of all currently active (flying) missiles:
		/// </summary>
		protected ArrayList			m_activeMissiles;


		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="server">Arena Server</param>
		/// <param name="arena">Arena</param>
		/// <param name="bots">List of participating</param>
		public FightController(ArenaServer server, Arena arena, ArrayList bots)
		{
			m_arena = arena;
			m_bots = bots;
			m_activeMissiles = new ArrayList();
		}


		/// <summary>
		/// Returns display of the current state of the arena and bot states
		/// </summary>
		/// <returns>Text-display state of the arena and bot attributes.</returns>
		public string[] GetStateDisplay()
		{
			string[] arenaState = m_arena.GetTextGrid();
			string[] result = new string[arenaState.Length + m_bots.Count];

			for (int n = 0; n < arenaState.Length; n++)
				result[n] = arenaState[n];

			int m = arenaState.Length;
			IEnumerator itr = m_bots.GetEnumerator();
			while (itr.MoveNext())
			{
				Bot bot = (Bot) itr.Current;
				string botState = bot.ToString();
				result[m] = botState;
				m++;
			}

			return result;
		}


		/// <summary>
		/// Main method used on the controler to start the fight
		/// </summary>
		/// <returns></returns>
		public ArrayList RunFight()
		{
			try
			{
				InitializeArena();
				InitializeBots();

				while (true)
				{
					// Initialize the bots and send them the map plus their status:
					for (int n = 0; n < m_bots.Count; n++)
					{
						Bot bot = (Bot) m_bots[n];
						ProcessBotActions(bot);
						ProcessMissiles();
					}
				}
			} 
			catch (BotDiedException e) 
			{ 
				ArrayList scoreList = new ArrayList();
				
				for (int n = 0; n < m_bots.Count; n++)
				{
					Bot bot = (Bot) m_bots[n];					
					if (bot.Name == e.DeadBot.Name)
					{
						FightScore botScore = new FightScore();
						botScore.Rank = 2;
						botScore.ScoredBot = bot;
						botScore.Info = e.Reason;
						scoreList.Add(botScore);
					}					
					else
					{
						FightScore botScore = new FightScore();
						botScore.Rank = 1;
						botScore.ScoredBot = bot;						
						scoreList.Add(botScore);
					}
				}

				return scoreList; 
			}
			catch (Exception e) { return null; }			
		}


		protected void ProcessMissiles()
		{
			for (int speed = 0; speed < 3; speed++)
			{
				try
				{
					ArrayList deadMissisles = new ArrayList();
					IEnumerator itr = m_activeMissiles.GetEnumerator();
					while (itr.MoveNext())
					{
						Missile missile = (Missile) itr.Current;
						int currentX, currentY;
						if (!missile.LineOfFire.GetPoint(missile.CurrentPoint, out currentX, out currentY))
						{
							deadMissisles.Add(missile);
							continue;
						}

						missile.CurrentPoint++;
						int x, y;
						if (missile.LineOfFire.GetPoint(missile.CurrentPoint, out x, out y))
						{
							if (missile.CoveredCell != null)
								m_arena.SetCell(currentX, currentY, missile.CoveredCell);
					
							// Find out if there is a wall where the missile would go:
							ArenaCell nextCell = m_arena.GetCell(x, y);
							if (nextCell.RepChar == "#")
							{
								// Next cell would be the wall so we stop							
								deadMissisles.Add(missile);
								continue;						
							}

							// Find out if there is a bot there:
							IEnumerator itrBots = m_bots.GetEnumerator();
							while (itrBots.MoveNext())
							{
								Bot otherBot = (Bot) itrBots.Current;
								if (otherBot.Name != missile.SourceBot.Name && otherBot.X == x && otherBot.Y == y)
								{
									otherBot.Demage += 10;
									deadMissisles.Add(missile);
									continue;
								}
							}

							// Else push the missile forward:
							missile.CoveredCell = nextCell;
							m_arena.SetCell(x, y, Missile.RepresentedCell);

							// else missile just started the travel
						}
						else	// Missile reached the destination:
						{
							deadMissisles.Add(missile);
							if (missile.CoveredCell != null)
								m_arena.SetCell(currentX, currentY, missile.CoveredCell);
						}
					}

					IEnumerator itrRemovedMissiles = deadMissisles.GetEnumerator();
					while (itrRemovedMissiles.MoveNext())
						m_activeMissiles.Remove(itrRemovedMissiles.Current);

				} 
				catch (Exception e)
				{
					e.ToString();
				}
			}
				
		}


		/// <summary>
		/// Method used for communicating with the bots and processing actions they take.
		/// </summary>
		/// <param name="bot"></param>
		protected void ProcessBotActions(Bot bot)
		{
			try
			{
				// Send the new state back to the client:
				bot.UpdateClient(m_arena, false, false);

				// Ask the bot to send actions:
				bot.Client.Send("BEGIN");

				string action = "";
				int moveCount = 0;
				int fireCount = 0;
				while (action != "DONE")
				{
					action = bot.Client.Receive();
					if (action != "DONE")
					{
						// Analyze and process the bot action:
						string[] actionAttribs = action.Split(':');
						if (actionAttribs.Length < 1)
							throw new Exception("Invalid action: '" + action + "'");

						string actionCommand = actionAttribs[0];
						if (actionCommand == "MOVE")
						{
							moveCount++;

							// If bot is trying to move more times per turn then allowed, we just ignore the command:
							if (moveCount <= bot.Speed)
							{
								if (actionAttribs.Length < 2)
									throw new Exception("Invalid action: '" + action + "'");
							
								string actionParameters = actionAttribs[1];
								int xDiff = 0;
								int yDiff = 0;
								if (actionParameters == "left")
									xDiff = -1;
								else if (actionParameters == "right")
									xDiff = 1;
								else if (actionParameters == "up")
									yDiff = -1;
								else if (actionParameters == "down")
									yDiff = 1;
								else
									throw new Exception("Invalid action: '" + action + "'");

								ProcessBotMove(bot, xDiff, yDiff);
							}					
						}
						else if (actionCommand == "FIRE")
						{
							fireCount++;

							// If bot is trying to fire more times per turn then allowed, we just ignore the command:
							if (fireCount <= bot.FireSpeed)
							{
								if (actionAttribs.Length < 2)
									throw new Exception("Invalid action: '" + action + "'");
							
								string actionParameters = actionAttribs[1];
								string[] coordinates = actionParameters.Split(',');
								if (coordinates.Length != 2)
									throw new Exception("Invalid action: '" + action + "'");

								int xPos = 0;
								int yPos = 0;
								try
								{
									xPos = int.Parse(coordinates[0]);
									yPos = int.Parse(coordinates[1]);
								}
								catch (Exception e)
								{
									throw new Exception("Invalid action: '" + action + "'");
								}

								ProcessBotFire(bot, xPos, yPos);
							}						
						}
						else 
							throw new Exception("Invalid action: '" + action + "'");
					}
				}
			}
			catch (BotDiedException e)
			{
				throw e;
			}
			catch (Exception e)
			{
				// When exception is caught during processing of action for given bot, 
				// the bot is considered the looser:
				throw new BotDiedException(bot, e.Message);
			}
		}


		/// <summary>
		/// Method which processes bot request to move in given direction.
		/// </summary>
		/// <param name="bot">The bot</param>
		/// <param name="xDiff">Difference in location on the X axle</param>
		/// <param name="yDiff">Difference in location on the Y axle</param>
		protected void ProcessBotMove(Bot bot, int xDiff, int yDiff)
		{			
			int newX = bot.X + xDiff;
			int newY = bot.Y + yDiff;

			// Make sure the bot can move 
			ArenaCell cell = m_arena.GetCell(newX, newY);

			if (cell.Hardness < 100)
			{
				// Restore covered cell:
				m_arena.SetCell(bot.X, bot.Y, bot.CoveredCell);

				bot.X = newX;
				bot.Y = newY;

				bot.CoveredCell = cell;
				m_arena.SetCell(bot.X, bot.Y, Bot.RepresentedCell);
			}
			else
				bot.Demage++;		// If the bot ran into a wall, increase the demage by 1%
		}


		/// <summary>
		/// Method which processes bot request to shoot at given position.
		/// </summary>
		/// <param name="bot">The bot</param>
		/// <param name="xDiff">X position</param>
		/// <param name="yDiff">Y position</param>
		protected void ProcessBotFire(Bot bot, int xPos, int yPos)
		{	
			// Make sure shooting is done within the area of the arena. If it isn't we will just ignore the shot action:
			if (xPos >= 0 && xPos < m_arena.GetWidth() && yPos >= 0 && yPos < m_arena.GetHeight())
			{
				Line lineOfFire = new Line(bot.X, bot.Y, xPos, yPos);
				Missile missile = new Missile();
				missile.SourceBot = bot;
				missile.LineOfFire = lineOfFire;
				missile.CurrentPoint = 0;

				m_activeMissiles.Add(missile);
			}
		}


		/// <summary>
		/// Initializes the bots. For each bot the current bot state is sent as well as the state of the arena.
		/// </summary>
		protected void InitializeBots()
		{
			string joinCommand = "JOINING:Standard Arena";			
			string sizeCommand = "SIZE:" + m_arena.GetWidth().ToString() + "," + m_arena.GetHeight().ToString();
			string mapCommand = "MAP:" + m_arena.GetState();

			// Initialize the bots and send them the map plus their status:
			for (int n = 0; n < m_bots.Count; n++)
			{
				Bot bot = (Bot) m_bots[n];

				bot.Client.Send(joinCommand);
				bot.Client.Send(sizeCommand);

				int x = 0;
				int y = 0;

				// TODO: find better way to randomly place the bots in the arena
				if (n == 0) { x = 16; y = 5; }
				if (n == 1) { x = 48; y = 26; }

				bot.X = x;
				bot.Y = y;

				// Place to bot on the arena:
				bot.CoveredCell = m_arena.GetCell(bot.X, bot.Y);
				m_arena.SetCell(bot.X, bot.Y, ArenaCell.Copy(Bot.RepresentedCell));

				bot.UpdateClient(m_arena, true, true);
			}		
		}


		/// <summary>
		/// Initializes the state of the arena:
		/// </summary>
		protected void InitializeArena()
		{
			for (int h = 0; h < m_arena.GetHeight(); h++)
				for (int w = 0; w < m_arena.GetWidth(); w++)				
				{
					ArenaCell cell = new ArenaCell();
				
					if (h == 0 || w == 0 || (w == m_arena.GetWidth() - 1) || (h == m_arena.GetHeight() - 1))
					{
						cell.Hardness = 100;
						cell.Color = 100;
						cell.Height = 100;
						cell.Heat = 0;
						cell.RepChar = "#";						
					}
					else
					{
						cell.Hardness = 0;
						cell.Color = 0;
						cell.Height = 0;
						cell.Heat = 0;
						cell.RepChar = " ";
					}

					m_arena.SetCell(w, h, cell);
				}	

			BuildWall(32, 0, 32, 4);
			BuildWall(32, 32, 32, 28);
	


			// For each bot initialize the change-track:
			for (int b = 0; b < m_bots.Count; b++)
			{
				Bot bot = (Bot) m_bots[b];
				m_arena.StartChangeTrack(bot.Name);
			}			
		}

		protected void BuildWall(int startX, int startY, int endX, int endY)
		{
			Line l1 = new Line(startX, startY, endX, endY);
			int n = 0;
			int x = 0;
			int y = 0;
			while (l1.GetPoint(n, out x, out y))
			{
				ArenaCell cell = new ArenaCell();
				cell.Hardness = 100;
				cell.Color = 100;
				cell.Height = 100;
				cell.Heat = 0;
				cell.RepChar = "#";		
				m_arena.SetCell(x, y, cell);
				n++;
			}
		}
	}
}

