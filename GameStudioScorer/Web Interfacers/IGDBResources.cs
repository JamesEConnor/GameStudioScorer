using System;
namespace GameStudioScorer.IGDB
{
	public struct Company
	{
		public string name;
		public int id;
		public Game[] developed;
		public Game[] published;
	}

	public struct Game
	{
		public int id;
		public int[] genres;
	}
}
