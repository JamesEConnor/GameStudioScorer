using System;
using System.Collections.Generic;

namespace GameStudioScorer.IGDB
{
	public class Company
	{
		public string name { get; set; }
		public int id { get; set; }
		public List<Game> developed { get; set; }
		public List<Game> published { get; set; }

		public override string ToString()
		{
			return "{" + name + "," + id + "}";
		}
	}

	public class Game
	{
		public int id { get; set; }
		public int[] genres { get; set; }
	}
}
