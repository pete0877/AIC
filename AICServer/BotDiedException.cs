using System;

namespace AIC
{
	/// <summary>
	/// Exception thrown when one of the bots dies. 
	/// </summary>
	public class BotDiedException : Exception
	{
		public Bot		DeadBot;
		public string	Reason;
		public BotDiedException(Bot deadBot, string reason)
		{
			DeadBot = deadBot;
			Reason = reason;
		}
	}
}
