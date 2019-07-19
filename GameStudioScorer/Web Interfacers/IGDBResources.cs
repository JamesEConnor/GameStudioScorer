using System;
namespace GameStudioScorer.IGDB
{
	public struct Company
	{
		public string name;
		public Game[] developed;
		public Game[] published;
	}

	public struct Game
	{
		public string id;
		public int[] genres;
	}
}
