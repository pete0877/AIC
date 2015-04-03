using System;
using System.Text;

namespace AIC
{
	/// <summary>
	/// Summary description for ArenaCell.
	/// </summary>
	public class ArenaCell
	{
		/// <summary>
		/// Cell attribute
		/// </summary>		
		public int Height;

		/// <summary>
		/// Cell attribute
		/// </summary>	
		public int Color;

		/// <summary>
		/// Cell attribute
		/// </summary>	
		public int Heat;

		/// <summary>
		/// Cell attribute
		/// </summary>	
		public int Hardness;

		/// <summary>
		/// Representing character
		/// </summary>	
		public string RepChar;

		/// <summary>
		/// Constructor
		/// </summary>
		public ArenaCell()
		{
			Height = 0;
			Color = 0;
			Heat = 0;
			Hardness = 0;			
			RepChar = " ";
		}


		/// <summary>
		/// Returns self in string representation
		/// </summary>
		/// <returns>State string</returns>
		public string GetState()
		{
			string result = 
				Height.ToString() + "," +
				Color.ToString() + "," +
				Heat.ToString() + "," +
				Hardness.ToString() + "," +
				RepChar;

			return result;
		}


		/// <summary>
		/// Sets self parameters based on state string previously generated through GetState()
		/// </summary>
		/// <param name="stateString">State string</param>
		public void SetState(string stateString)
		{			
			string[] parameters = stateString.Split(',');
			Height = int.Parse(parameters[0]);
			Color = int.Parse(parameters[1]);
			Heat = int.Parse(parameters[2]);
			Hardness = int.Parse(parameters[3]);
			RepChar = parameters[4];
		}


		/// <summary>
		/// Copies given cell by value and returns it.
		/// </summary>
		/// <param name="cell">Cell to be copied</param>
		/// <returns>Copied cell</returns>
		public static ArenaCell Copy(ArenaCell cell)
		{
			ArenaCell result = new ArenaCell();
			result.Color = cell.Color;
			result.Height = cell.Height;
			result.Heat = cell.Heat;
			result.Hardness = cell.Hardness;
			result.RepChar = string.Copy(cell.RepChar);

			return result;
		}
	}
}
