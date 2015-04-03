using System;

namespace AIC
{
	/// <summary>
	/// Summary description for Missile.
	/// </summary>
	public class Missile
	{
		/// <summary>
		/// Cell underneeth the missile
		/// </summary>
		public ArenaCell			CoveredCell;

		/// <summary>
		/// Cell template which represents the missile
		/// </summary>
		public static ArenaCell		RepresentedCell;

		/// <summary>
		/// Line of fire.
		/// </summary>
		public Line					LineOfFire;

		/// <summary>
		/// The bot which fired the shot.
		/// </summary>
		public Bot					SourceBot;

		/// <summary>
		/// Zero-based
		/// </summary>
		public int					CurrentPoint;


		/// <summary>
		/// Constructor
		/// </summary>
		public Missile()
		{
			CoveredCell = null;
			LineOfFire = null;
			SourceBot = null;
			CurrentPoint = -1;
		}

		/// <summary>
		/// Statuc Constructor
		/// </summary>
		static Missile()
		{
			RepresentedCell = new ArenaCell();
			RepresentedCell.Hardness = 80;
			RepresentedCell.Color = 20;
			RepresentedCell.Height = 50;			
			RepresentedCell.Heat = 10;
			RepresentedCell.RepChar = "*";
		}

	}
}
