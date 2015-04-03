using System;
using System.Net.Sockets;

namespace AIC
{
	/// <summary>
	/// Internal server class representing 
	/// </summary>
	public class Bot
	{
		/// <summary>
		/// Name of the bot as submitted by the client
		/// </summary>
		public string			Name;

		/// <summary>
		/// Demage percentage (0 - 100)
		/// </summary>
		protected int			m_demage;
		public int				Demage
		{	
			get { return m_demage; }
			set { 				
				if (m_demage != value) 
					DemageChanged = true; 

				m_demage = value; 
				if (m_demage >= 100)
					throw new BotDiedException(this, "Exesive demage");
			}
		}

		/// <summary>
		/// Armor percentage (0 - 100)
		/// </summary>
		protected int			m_armor;
		public int				Armor
		{	
			get { return m_armor; }
			set { if (m_armor != value) ArmorChanged = true; m_armor = value; }
		}

		/// <summary>
		/// Speed (cells per move) (0+)
		/// </summary>
		protected int			m_speed;
		public int				Speed
		{	
			get { return m_speed; }
			set { if (m_speed != value) SpeedChanged = true; m_speed = value; }
		}

		/// <summary>
		/// Firepower (one for each demage point assuming no armor) (0+)
		/// </summary>
		protected int			m_firePower;
		public int				FirePower
		{	
			get { return m_firePower; }
			set { if (m_firePower != value) FirePowerChanged = true; m_firePower = value; }
		}

		/// <summary>
		/// Firespeed (cells per move) (0+)
		/// </summary>
		protected int			m_fireSpeed;
		public int				FireSpeed
		{	
			get { return m_fireSpeed; }
			set { if (m_fireSpeed != value) FireSpeedChanged = true; m_fireSpeed = value; }
		}

		/// <summary>
		/// X position
		/// </summary>
		protected int			m_x;
		public int				X
		{	
			get { return m_x; }
			set { if (m_x != value) PositionChanged = true; m_x = value; }
		}

		/// <summary>
		/// Y position
		/// </summary>
		protected int			m_y;
		public int				Y
		{	
			get { return m_y; }
			set { if (m_y != value) PositionChanged = true; m_y = value; }
		}

		/// <summary>
		/// Parameters used for tracking changes to bot's state and position:
		/// </summary>
		public bool				PositionChanged;
		public bool				ArmorChanged;
		public bool				DemageChanged;
		public bool				SpeedChanged;
		public bool				FirePowerChanged;
		public bool				FireSpeedChanged;

		/// <summary>
		/// Wrapper on TCPClient used to simplify communication
		/// </summary>
		public NetworkClient	Client;

		public ArenaCell		CoveredCell;

		public static ArenaCell		RepresentedCell;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="tcpClient">TCP Client used to communicate with the bot program</param>
		/// <param name="name">Name of the bot</param>
		public Bot(TcpClient tcpClient, string name)
		{
			// Name and network client setup:
			Name = name;
			Client = new NetworkClient(tcpClient);

			// Set up of default state:
			Armor = 0;
			Demage = 0;
			Speed = 1;
			FirePower = 1;
			FireSpeed = 5;

			PositionChanged = false;
			ArmorChanged = false;
			DemageChanged = false;
			SpeedChanged = false;
			FirePowerChanged = false;
			FireSpeedChanged = false;

			CoveredCell = null;
		}


		/// <summary>
		/// Statuc Constructor
		/// </summary>
		static Bot()
		{
			RepresentedCell = new ArenaCell();
			RepresentedCell.Hardness = 100;
			RepresentedCell.Color = 50;
			RepresentedCell.Height = 90;			
			RepresentedCell.Heat = 10;
			RepresentedCell.RepChar = "O";
		}


		/// <summary>
		/// Method used to motify the network client which controls this bot with bot parameters and 
		/// </summary>
		/// <param name="arena">Arena</param>
		public void UpdateClient(Arena arena, bool fullArena, bool fullState)
		{
			// Check what has changed since we last talked to the bot and send potential updates:
			if (fullArena)
			{
				string mapCommand = "MAP:" + arena.GetState();
				Client.Send(mapCommand);
			}
			else
			{
				if (!arena.IsEmptyChangeTrack(Name))
				{
					string mapCommand = "MAP:" + arena.GetChangeTrack(Name);
					arena.StartChangeTrack(Name);
					Client.Send(mapCommand);
				}
			}

			if (PositionChanged || fullState)
			{
				string positionCommand = "POS:" + X.ToString() + "," + Y.ToString();
				Client.Send(positionCommand);
				PositionChanged = false;			
			}

			if (ArmorChanged || fullState)
			{
				string armorCommand = "ARMOR:" + Armor.ToString();
				Client.Send(armorCommand);
				ArmorChanged = false;				
			}

			if (DemageChanged || fullState)
			{
				string demageCommand = "DEMAGE:" + Demage.ToString();
				Client.Send(demageCommand);
				DemageChanged = false;				
			}

			if (SpeedChanged || fullState)
			{
				string speedCommand = "SPEED:" + Speed.ToString();
				Client.Send(speedCommand);
				SpeedChanged = false;				
			}

			if (FirePowerChanged || fullState)
			{
				string firepowerCommand = "FIREPOWER:" + FirePower.ToString();
				Client.Send(firepowerCommand);
				FirePowerChanged = false;				
			}

			if (FireSpeedChanged || fullState)
			{
				string firepowerCommand = "FIRESPEED:" + FireSpeed.ToString();
				Client.Send(firepowerCommand);
				FireSpeedChanged = false;				
			}		
		}


		/// <summary>
		/// Returns self state in form of a string.
		/// </summary>
		/// <returns></returns>
		override public string ToString()
		{
			string result = Name;
			result += ":  DEMAGE = " + Demage.ToString();
			result +=  "  ARMOR = " + Armor.ToString();
			result +=  "  SPEED = " + Speed.ToString();
			result +=  "  FIREPOWER = " + FirePower.ToString();
			result +=  "  FIRESPEED = " + FireSpeed.ToString();
			return result;
		}
	}
}
