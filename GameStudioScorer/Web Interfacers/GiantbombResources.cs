using System;

namespace GameStudioScorer.Giantbomb
{
	//A struct representing a response from the Giantbomb "Company" endpoint.
	public struct Company
	{
		public string abbreviation;
		public string[] aliases;
		public string api_detail_url;

		public string[] characters;
		public string[] concepts;

		public DateTime date_added;
		public DateTime date_founded;
		public DateTime date_last_updated;

		public string deck;
		public string description;

		public string[] developed_games;
		public string[] developer_releases;
		public string[] distributor_releases;

		public string guid;
		public string id;

		public string image;
		public string[] image_tags;

		public string location_address;
		public string location_city;
		public string location_country;
		public string location_state;
		public string[] locations;

		public string name;
		public string[] objects;

		public string[] people;

		public string phone;

		public string[] published_games;
		public string[] publisher_releases;

		public string site_detail_url;
		public string website;
	}

	//A struct representing a response from the Giantbomb "Companies" endpoint.
	public struct Companies
	{
		public string abbreviation;
		public string[] aliases;
		public string api_detail_url;

		public DateTime date_added;
		public DateTime date_founded;
		public DateTime date_last_updated;

		public string deck;
		public string description;

		public string guid;
		public string id;

		public string image;
		public string image_tags;

		public string location_address;
		public string location_city;
		public string location_country;
		public string location_state;

		public string name;
		public string phone;

		public string site_detail_url;
		public string website;
	}

	//A struct representing a response from the Giantbomb "Games" endpoint.
	public struct Games
	{
		public string[] aliases;
		public string api_detail_url;

		public DateTime date_added;
		public DateTime date_last_updated;

		public string deck;
		public string description;

		public int expected_release_month;
		public int expected_release_year;
		public int expected_release_quarter; //1, 2, 3, or 4

		public string guid;
		public string id;

		public string image;
		public string image_tags;

		public string name;

		public int number_of_user_reviews;

		public string original_game_rating;
		public DateTime original_release_date;

		public string[] platforms;
		public string site_detail_url;
	}
}
