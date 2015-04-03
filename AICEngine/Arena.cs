using System;
using System.Threading;
using System.Collections;

namespace AIC
{
	/// <summary>
	/// Arena class - stores current state of the arena grid
	/// </summary>
	public class Arena
	{
		/// <summary>
		/// Lock used on all reading and writing:
		/// </summary>
		protected ReaderWriterLock		m_lock;

		/// <summary>
		/// Two-dimensional array of cells.
		/// </summary>
		protected ArenaCell[,]			m_cells;

		/// <summary>
		/// Arena width.
		/// </summary>
		protected int					m_width;

		/// <summary>
		/// Arena height.
		/// </summary>
		protected int					m_height;

		/// <summary>
		/// List
		/// </summary>
		Hashtable						m_changeTracks;
		 
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="width">Width of the arena</param>
		/// <param name="height">Height of the arena</param>
		public Arena(int width, int height)
		{
			m_cells = new ArenaCell[width, height];
			m_width = width;
			m_height = height;
			m_lock = new ReaderWriterLock();
			m_changeTracks = new Hashtable();

			for (int h = 0; h < m_height; h++)
				for (int w = 0; w < m_width; w++)				
				{
					ArenaCell cell = new ArenaCell();
					m_cells[w, h] = cell;
				}
		}

		/// <summary>
		/// Returns representation of the arana as list of text lines. Automatically performs read-lock on the object.
		/// </summary>
		/// <returns></returns>
		public string[] GetTextGrid()
		{
			m_lock.AcquireReaderLock(int.MaxValue);

			string[] result = new string[m_height];
			for (int h = 0; h < m_height; h++)
			{
				string line = "";
				for (int w = 0; w < m_width; w++)
				{				
					ArenaCell cell = m_cells[w, h];
					if (cell != null)
						line += cell.RepChar;
					else
						line += "?";
				}
				result[h] = line;
			}

			m_lock.ReleaseReaderLock();

			return result;
		}

		/// <summary>
		/// Returns full state of the arena;
		/// </summary>
		/// <returns>State of the arena encoded as series of 'x,y|height,color,heat,hardness,repchar;'  .. one for each cell</returns>
		public string GetState()
		{
			m_lock.AcquireReaderLock(int.MaxValue);

			string result = "";
			for (int h = 0; h < m_height; h++)
			{				
				for (int w = 0; w < m_width; w++)
				{
					string cellSpec = w.ToString() + "," + h.ToString();
					ArenaCell cell = m_cells[w, h];
					if (cell != null)
						cellSpec += "|" + cell.GetState() + ";";
					else
						cellSpec += "|;";

					result += cellSpec;
				}
			}

			m_lock.ReleaseReaderLock();

			return result;
		}

		/// <summary>
		/// Starts arena change-tracking routine on given channel. Changes to the arena can be later gathered by calling GetChangeTrack(). Performs write-lock before performing the operation.
		/// </summary>
		public void StartChangeTrack(string channel)
		{		
			m_lock.AcquireWriterLock(int.MaxValue);			

			m_changeTracks[channel] = new Hashtable();

			m_lock.ReleaseWriterLock();
		}

		/// <summary>
		/// Returns changes made to the arena as tracked on given channel. Performs read-lock.
		/// </summary>
		/// <param name="channel">Channel name</param>
		/// <returns></returns>
		public string GetChangeTrack(string channel)
		{
			m_lock.AcquireReaderLock(int.MaxValue);			

			Hashtable changeTrack = (Hashtable) m_changeTracks[channel];
			
			// Make sure track has been started on this channel:
			if (changeTrack == null)
			{
				m_lock.ReleaseReaderLock();
				return "";
			}

			string result = "";
			IDictionaryEnumerator itr = changeTrack.GetEnumerator();
			while(itr.MoveNext())
			{
				string coordEncoded = (string) itr.Key;
				string[] coords = coordEncoded.Split(',');
				int x = int.Parse(coords[0]);
				int y = int.Parse(coords[1]);

				string cellSpec = x.ToString() + "," + y.ToString();
				ArenaCell cell = m_cells[x, y];
				if (cell != null)
					cellSpec += "|" + cell.GetState() + ";";
				else
					cellSpec += "|;";

				result += cellSpec;
			}

			m_lock.ReleaseReaderLock();

			return result;
		}


		/// <summary>
		/// Returns flag indicating if change-track for given channel is empty or not
		/// </summary>
		/// <param name="channel">Channel name</param>
		/// <returns></returns>
		public bool IsEmptyChangeTrack(string channel)
		{
			m_lock.AcquireReaderLock(int.MaxValue);

			bool result = false;
			Hashtable changeTrack = (Hashtable) m_changeTracks[channel];
			if (changeTrack == null || changeTrack.Count == 0)
				result = true;

			m_lock.ReleaseReaderLock();

			return result;

		}


		/// <summary>
		/// Updates the arena state and tracks the changes on all known track-channels. Does not perform write-lock. The responsibility of doing this through Lock() is up to the client code.
		/// </summary>
		/// <param name="x">X position</param>
		/// <param name="y">Y position</param>
		/// <param name="newCell">New cell</param>
		public void SetCell(int x, int y, ArenaCell newCell)
		{
			// Make the change:
			if (newCell != null && x < m_width && y < m_height && x >= 0 && y >= 0)
			{
				m_cells[x, y] = newCell;

				// Record the change on all applicable tracks:
				IDictionaryEnumerator itr = m_changeTracks.GetEnumerator();
				while (itr.MoveNext())
				{
					Hashtable changeTrack = (Hashtable) itr.Value;
					string positionRef = x.ToString() + "," + y.ToString();
					changeTrack[positionRef] = true;
				}

			}
		}


		/// <summary>
		/// Returns (by reference) cell at given X, Y position.
		/// </summary>
		/// <param name="x">X position</param>
		/// <param name="y">Y position</param>
		/// <returns>Cell or null if position out of range</returns>
		public ArenaCell GetCell(int x, int y)
		{
			if (x < m_width && y < m_height && x >= 0 && y >= 0)
				return m_cells[x, y];
			else
				return null;
		}

		/// <summary>
		/// Returns size in width of the arena
		/// </summary>
		/// <returns>Width of the arena</returns>
		public int GetWidth()
		{
			return m_width;
		}

		/// <summary>
		/// Returns size in height of the arena
		/// </summary>
		/// <returns>Height of the arena</returns>
		public int GetHeight()
		{
			return m_height;
		}

		/// <summary>
		/// Locks the arena object for writing
		/// </summary>
		public void Lock()
		{
			m_lock.AcquireWriterLock(int.MaxValue);
		}

		/// <summary>
		/// Unlocks the arena object. Other threads can read and write it to.
		/// </summary>
		public void Unlock()
		{
			m_lock.ReleaseWriterLock();;
		}
	}
}
