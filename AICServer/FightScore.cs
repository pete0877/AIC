using System;

namespace AIC
{
	/// <summary>
	/// Summary description for FightScore.
	/// </summary>
	public class FightScore
	{
		public int Rank;
		public Bot ScoredBot;
		public string Info;

		public FightScore()
		{
			Rank = 0;
			ScoredBot = null;
			Info = null;
		}

		override public string ToString()
		{
			if (ScoredBot == null || Rank == 0)
				return "No score";

			string result = "Bot '" + ScoredBot.Name + "' ranked on position number: " + Rank.ToString() + ".";
			if (Info != null)
				result += " " + Info;

			return result;
		}
	}
}
