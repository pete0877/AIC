using System;
using System.Collections;

namespace AIC
{
	/// <summary>
	/// Simple utility class which uses Bresenhams algorithm to describe line-drawing.
	/// </summary>
	public class Line
	{
		/// <summary>
		/// Hash table of point number to X positions.
		/// </summary>
		protected Hashtable m_pointsX;

		/// <summary>
		/// Hash table of point number to Y positions.
		/// </summary>
		protected Hashtable m_pointsY;


		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="x0">Starting X</param>
		/// <param name="y0">Starting Y</param>
		/// <param name="x1">End X</param>
		/// <param name="y1">End Y</param>
		public Line(int x0, int y0, int x1, int y1)
		{
			m_pointsX = new Hashtable();
			m_pointsY = new Hashtable();

			int dy = y1 - y0;
			int dx = x1 - x0;
			int stepx, stepy;

			if (dy < 0) { dy = -dy;  stepy = -1; } else { stepy = 1; }
			if (dx < 0) { dx = -dx;  stepx = -1; } else { stepx = 1; }
			dy <<= 1;
			dx <<= 1;

			m_pointsX[0] = x0;
			m_pointsY[0] = y0;
			
			int n = 0;

			if (dx > dy)
			{
				int fraction = dy - (dx >> 1);
				while (x0 != x1) 
				{
					n++;

					if (fraction >= 0) 
					{
						y0 += stepy;
						fraction -= dx;
					}
					x0 += stepx;
					fraction += dy;
					m_pointsX[n] = x0;
					m_pointsY[n] = y0;					
				}
			} 
			else 
			{
				int fraction = dx - (dy >> 1);
				while (y0 != y1) 
				{
					n++;

					if (fraction >= 0) 
					{
	                    x0 += stepx;
		                fraction -= dy;
			        }
				    y0 += stepy;
					fraction += dx;
					m_pointsX[n] = x0;
					m_pointsY[n] = y0;	
				}
			}
		}


		/// <summary>
		/// Function which is used to return n-th point (zero-based)
		/// </summary>
		/// <param name="n">Point index</param>
		/// <param name="x">X output param</param>
		/// <param name="y">Y output param</param>
		/// <returns>True if the point is found.</returns>
		public bool GetPoint(int n, out int x, out int y)
		{		
			x = -1;
			y = -1;
			if (!m_pointsX.Contains(n))
				return false;

            x = (int) m_pointsX[n];
			y = (int) m_pointsY[n];

			return true;
		}
	}
}
