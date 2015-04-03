using System;
using System.Collections;
using System.Threading;
using System.Net.Sockets;
using System.Text;

namespace AIC
{
	/// <summary>
	/// Arena server
	/// </summary>
	public class ArenaServer
	{
		/// <summary>
		/// Flag indicating if server is currently running
		/// </summary>
		protected bool					m_started;

		/// <summary>
		/// Lock used when service control functions are called (Start, Stop, GetState)
		/// </summary>
		protected ReaderWriterLock		m_serviceStatusLock;

		/// <summary>
		/// Lock used when service control functions are called (Start, Stop, GetState)
		/// </summary>
		protected ReaderWriterLock		m_logLock;

		/// <summary>
		/// Lock used to protect the bot queue
		/// </summary>
		protected ReaderWriterLock		m_queueLock;

		/// <summary>
		/// Arena on which battles are played
		/// </summary>
		protected Arena					m_arena;

		/// <summary>
		/// One big string containing log entries since the server was created
		/// </summary>
		protected ArrayList				m_log;

		/// <summary>
		/// Socket listener - receives all incoming requests for fights and other commands
		/// </summary>
		protected TcpListener			m_tcpListener;

		/// <summary>
		/// Thread used to receive incoming connection and either place bots in fight queue or process other independent commands (e.g. returning statistics on bots, etc)
		/// </summary>
		protected Thread				m_directorThread;

		/// <summary>
		/// Thread which controls fights. Gets two bots from the waiting queue, starts and conducts the fight. When game is done, it repeats the process.
		/// </summary>
		protected Thread				m_fightRefereeThread;

		/// <summary>
		/// List of bots waiting to fight
		/// </summary>
		protected ArrayList				m_botQueue;

		/// <summary>
		/// Fight controller
		/// </summary>
		protected FightController		m_controller;
       

		/// <summary>
		/// Constructor - prepares the server to be started by configuring default parameters
		/// </summary>
		public ArenaServer()
		{
			m_serviceStatusLock = new ReaderWriterLock();
			m_queueLock = new ReaderWriterLock();
			m_logLock = new ReaderWriterLock();

			m_started = false;
			m_log = new ArrayList();
			m_botQueue = new ArrayList();

			m_controller = null;
			
			Log("Server created");			
		}


		/// <summary>
		/// Log-processing function
		/// </summary>
		/// <param name="entry"></param>
		public void Log(string entry)
		{
			m_logLock.AcquireWriterLock(int.MaxValue);

			DateTime now = DateTime.Now;
			m_log.Add(now.ToShortDateString() + " " + now.ToShortTimeString() + " | " + entry);

			m_logLock.ReleaseWriterLock();
		}


		/// <summary>
		/// Service control function used to start the arena server
		/// </summary>
		public void Start()
		{
			m_serviceStatusLock.AcquireWriterLock(int.MaxValue);

			if (!m_started)
			{
				m_directorThread = new Thread(new ThreadStart(StartDirector));
				m_directorThread.Start();

				m_fightRefereeThread = new Thread(new ThreadStart(StartFightReferee));
				m_fightRefereeThread.Start();

				m_started = true;
				Log("Server started");
			}

			m_serviceStatusLock.ReleaseWriterLock();
		}


		/// <summary>
		/// Service control function used to stop the arena server
		/// </summary>
		public void Stop()
		{
			m_serviceStatusLock.AcquireWriterLock(int.MaxValue);

			if (m_started)
			{
				if (m_tcpListener != null)
					m_tcpListener.Stop();

				if (m_directorThread != null)
					m_directorThread.Abort();

				if (m_fightRefereeThread != null)
					m_fightRefereeThread.Abort();

				m_started = false;
				m_tcpListener = null;
				m_directorThread = null;
				m_fightRefereeThread = null;

				Log("Server stopped");
			}

			m_serviceStatusLock.ReleaseWriterLock();
		}


		/// <summary>
		/// Arena state reading function 
		/// </summary>
		/// <returns></returns>
		public string[] GetStateDisplay()
		{
			if (m_started && m_controller != null)
				return m_controller.GetStateDisplay();
			else 
				return null;
		}


		/// <summary>
		/// Service log reading function
		/// </summary>
		/// <returns></returns>
		public string[] GetLog()
		{
			m_logLock.AcquireReaderLock(int.MaxValue);

			string[] result = new string[m_log.Count];			

			IEnumerator itr = m_log.GetEnumerator();
			int position = 0;
			while (itr.MoveNext())
			{
				result[m_log.Count - position - 1] = (string) itr.Current;
				position++;
			}

			m_logLock.ReleaseReaderLock();

			return result;
		}


		/// <summary>
		/// Function used by the welcoming thread - starts up TCP server and waits for incoming bots.
		/// </summary>
		protected void StartDirector()
		{
			// Create the new arena:
			m_arena = new Arena(64, 32);	

			// Start listening for incoming bots:
			m_tcpListener = new TcpListener(888);
			m_tcpListener.Start();	

			while(true)
			{
				TcpClient botClient = null;
				try
				{
					botClient = m_tcpListener.AcceptTcpClient();
					NetworkClient client = new NetworkClient(botClient);

					// Send welcome message
					client.Send("Welcome to AIC VR Server 1.0");
					string command = client.Receive();

					// Receive name of the bot
					// NAME:bot_name
					int colanPos = command.IndexOf(':');
					if (colanPos < 0)
					{
						client.Send("ERROR: 1: Invalid syntax or response; Missing comma in name declaration");
						Log("Client disconnected due to invalid syntax or response");
						throw new Exception();
					}
					
					// Format and validate the name:
					string botName = command.Substring(colanPos + 1, command.Length - (colanPos + 1));					
					botName = botName.Replace("'", "").Trim(); // Replace special characters
					if (botName == "")
					{
						client.Send("ERROR: 1: Invalid syntax or response; Empty bot name in NAME declaration");
						Log("Client disconnected due to invalid syntax or response");					
						throw new Exception();
					}

					// Receive processing command:
					// COMMAND:FIGHT|GETSTATS
					command = client.Receive();
					colanPos = command.IndexOf(':');
					if (colanPos < 0)
					{
						client.Send("ERROR: 1: Invalid syntax or response; Missing comma in COMMAND declaration");
						Log("Client disconnected due to invalid syntax or response");
						throw new Exception();
					}
					
					// Format and validate the command:
					string mode = command.Substring(colanPos + 1, command.Length - (colanPos + 1));					
					mode = mode.Trim(); // Replace special characters
					if (mode == "FIGHT")
					{
						string logEntry = "Bot '";
						logEntry += botName;
						logEntry += "' entered waiting queue";

						Log(logEntry);

						// Create the bot instance and enter the bot into the waiting queue:
						Bot bot = new Bot(botClient, botName);
						m_queueLock.AcquireWriterLock(int.MaxValue);
						m_botQueue.Add(bot);
						m_queueLock.ReleaseWriterLock();					
					}
					else if (mode == "GETSTATS")
					{
						client.Send(GetStats());
						throw new Exception();
					}						
					else
					{
						client.Send("ERROR: 1: Invalid syntax or response; Uknown command.");
						Log("Client disconnected due to invalid syntax or response");					
						throw new Exception();
					}
				}
				catch (Exception e)
				{
					Log("ERROR: " + e.Message);					
					if (botClient != null)
					{
						botClient.GetStream().Flush();
						botClient.Close();
					}
				}
			}
		}


		/// <summary>
		/// Method used to get player statistics from the server
		/// </summary>
		/// <returns></returns>
		public string GetStats()
		{
			return "Everybody is a winner :)";
		}


		/// <summary>
		/// Function responsible for running games with clients which have been placed in the queue.
		/// </summary>
		protected void StartFightReferee()
		{
			while (true)
			{
				// See if at least two bots are in the queue:
				ArrayList bots = new ArrayList();

				m_queueLock.AcquireWriterLock(int.MaxValue);
				if (m_botQueue.Count >= 2)
				{
					bots.Add(m_botQueue[0]);
					bots.Add(m_botQueue[1]);

					Log("Bot '" + ((Bot) m_botQueue[0]).Name + "' entered the arena");
					Log("Bot '" + ((Bot) m_botQueue[1]).Name + "' entered the arena");

					// Remove first two bots from the queue:
					m_botQueue.RemoveAt(0);
					m_botQueue.RemoveAt(0);
				}
				m_queueLock.ReleaseWriterLock();			

				// If at least 1 bot was selected by the logic above, we will start the match:
				if (bots.Count > 0)
				{					
					m_controller = new FightController(this, m_arena, bots); // Note: Controller manages all exceptions.
					ArrayList scores = m_controller.RunFight(); 
					if (scores == null)
						Log("Fight was score-inconclusive");
					else
					{
						IEnumerator itr = scores.GetEnumerator();
						while (itr.MoveNext())
						{
							FightScore score = (FightScore) itr.Current;
							Log(score.ToString());
						}
					}
				}
				else
					Thread.Sleep(1000);
			}
		}

	}
}
