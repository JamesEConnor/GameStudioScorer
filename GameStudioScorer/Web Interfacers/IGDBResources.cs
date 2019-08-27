using System;
using System.Collections.Generic;

namespace GameStudioScorer.IGDB
{
	//A class representing a company, which allows for deserializing from JSON
	//when making an IGDB request to the Companies endpoint.
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

	//A class representing a game, which allows for deserializing from JSON
	//when making an IGDB request to the Game endpoint.
	public class Game
	{
		public int id { get; set; }
		public int[] genres { get; set; }
	}
}
