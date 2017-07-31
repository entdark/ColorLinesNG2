//Copyright (c) 2014 Mike Clift

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Point = CLDataTypes.CLPoint;

namespace SimpleAStarExample
{
	/// <summary>
	/// Defines the parameters which will be used to find a path across a section of the map
	/// </summary>
	public class SearchParameters
	{
		public Point StartLocation { get; set; }

		public Point EndLocation { get; set; }
		
		public bool[,] Map { get; set; }

		public SearchParameters(Point startLocation, Point endLocation, bool[,] map)
		{
			this.StartLocation = startLocation;
			this.EndLocation = endLocation;
			this.Map = map;
		}
	}
}
