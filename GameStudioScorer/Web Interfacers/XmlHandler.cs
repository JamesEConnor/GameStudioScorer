using System;
using System.Xml;
using System.Linq;
using GameStudioScorer.Extensions;
using GameStudioScorer.Giantbomb;

namespace GameStudioScorer.XML
{
	public static class XmlHandler
	{
		public static Company DeserializeCompany(string xml)
		{
			XmlDocument doc = new XmlDocument();
			doc.LoadXml(xml);
			XmlNode comp = doc.SelectSingleNode("response/results");

			string abbreviation = comp.SelectSingleNode("abbreviation").InnerText;
			string[] aliases = comp.SelectSingleNode("aliases").InnerText.Split('\n');
			string api_detail_url = comp.SelectSingleNode("api_detail_url").InnerText;

			string[] characters = Extensions.Extensions.ConvertToList(comp.SelectNodes("characters/character/id")).Select(x => x.InnerText).ToArray();
			string[] concepts = Extensions.Extensions.ConvertToList(comp.SelectNodes("concepts/concept/id")).Select(x => x.InnerText).ToArray();

			DateTime date_added = new DateTime();
			DateTime.TryParse(comp.SelectSingleNode("date_added").InnerText, out date_added);

			DateTime date_founded = new DateTime();
			DateTime.TryParse(comp.SelectSingleNode("date_founded").InnerText, out date_founded);

			DateTime date_last_updated = new DateTime();
			DateTime.TryParse(comp.SelectSingleNode("date_last_updated").InnerText, out date_last_updated);

			string deck = comp.SelectSingleNode("deck").InnerText;
			string description = comp.SelectSingleNode("description").InnerText;

			string[] developed_games = Extensions.Extensions.ConvertToList(comp.SelectNodes("developed_games/game/id")).Select(x => x.InnerText).ToArray();
			string[] developer_releases = null;
			string[] distributor_releases = null;

			string guid = comp.SelectSingleNode("guid").InnerText;
			string id = comp.SelectSingleNode("id").InnerText;

			string image = comp.SelectSingleNode("image").InnerText;
			string[] image_tags = Extensions.Extensions.ConvertToList(comp.SelectNodes("image_tags/image_tag/name")).Select(x => x.InnerText).ToArray();

			string location_address = comp.SelectSingleNode("location_address").InnerText;
			string location_city = comp.SelectSingleNode("location_city").InnerText;
			string location_country = comp.SelectSingleNode("location_country").InnerText;
			string location_state = comp.SelectSingleNode("location_state").InnerText;
			string[] locations = Extensions.Extensions.ConvertToList(comp.SelectNodes("locations/location/id")).Select(x => x.InnerText).ToArray();

			string name = comp.SelectSingleNode("name").InnerText;
			string[] objects = Extensions.Extensions.ConvertToList(comp.SelectNodes("objects/object/id")).Select(x => x.InnerText).ToArray();

			string[] people = Extensions.Extensions.ConvertToList(comp.SelectNodes("people/person/id")).Select(x => x.InnerText).ToArray();

			string phone = comp.SelectSingleNode("phone").InnerText;

			string[] published_games = Extensions.Extensions.ConvertToList(comp.SelectNodes("published_games/game/id")).Select(x => x.InnerText).ToArray();
			string[] publisher_releases = null;

			string site_detail_url = comp.SelectSingleNode("site_detail_url").InnerText;
			string website = comp.SelectSingleNode("website").InnerText;

			return new Company
			{
				abbreviation = abbreviation,
				aliases = aliases,
				api_detail_url = api_detail_url,
				characters = characters,
				concepts = concepts,
				date_added = date_added,
				date_founded = date_founded,
				date_last_updated = date_last_updated,
				deck = deck,
				description = description,
				developed_games = developed_games,
				developer_releases = developer_releases,
				distributor_releases = distributor_releases,
				guid = guid,
				id = id,
				image = image,
				image_tags = image_tags,
				location_address = location_address,
				location_city = location_city,
				location_country = location_country,
				location_state = location_state,
				locations = locations,
				name = name,
				objects = objects,
				people = people,
				phone = phone,
				published_games = published_games,
				publisher_releases = publisher_releases,
				site_detail_url = site_detail_url,
				website = website
			};
		}

		public static Companies[] DeserializeCompanies(string xml)
		{
			XmlDocument doc = new XmlDocument();
			doc.LoadXml(xml);
			XmlNodeList nodes = doc.SelectNodes("response/results/company");
			Companies[] result = new Companies[nodes.Count];

			for (int a = 0; a < result.Length; a++)
			{
				XmlNode comp = nodes[a];
				string abbreviation = comp.SelectSingleNode("abbreviation").InnerText;
				string[] aliases = comp.SelectSingleNode("abbreviation").InnerText.Split('\n');
				string api_detail_url = comp.SelectSingleNode("abbreviation").InnerText;

				DateTime date_added = new DateTime();
				DateTime.TryParse(comp.SelectSingleNode("date_added").InnerText, out date_added);


				DateTime date_founded = new DateTime();
				DateTime.TryParse(comp.SelectSingleNode("date_founded").InnerText, out date_founded);

				DateTime date_last_updated = new DateTime();
				DateTime.TryParse(comp.SelectSingleNode("date_last_updated").InnerText, out date_last_updated);

				string deck = comp.SelectSingleNode("deck").InnerText;
				string description = comp.SelectSingleNode("description").InnerText;

				string guid = comp.SelectSingleNode("guid").InnerText;
				string id = comp.SelectSingleNode("id").InnerText;

				string image = comp.SelectSingleNode("image").InnerText;
				string image_tags = comp.SelectSingleNode("image_tags").InnerText;

				string location_address = comp.SelectSingleNode("location_address").InnerText;
				string location_city = comp.SelectSingleNode("location_city").InnerText;
				string location_country = comp.SelectSingleNode("location_country").InnerText;
				string location_state = comp.SelectSingleNode("location_state").InnerText;

				string name = comp.SelectSingleNode("name").InnerText;
				string phone = comp.SelectSingleNode("phone").InnerText;

				string site_detail_url = comp.SelectSingleNode("site_detail_url").InnerText;
				string website = comp.SelectSingleNode("website").InnerText;

				result[a] = new Companies
				{
					abbreviation = abbreviation,
					aliases = aliases,
					api_detail_url = api_detail_url,
					date_added = date_added,
					date_founded = date_founded,
					date_last_updated = date_last_updated,
					deck = deck,
					description = description,
					guid = guid,
					id = id,
					image = image,
					image_tags = image_tags,
					location_address = location_address,
					location_city = location_city,
					location_country = location_country,
					location_state = location_state,
					name = name,
					phone = phone,
					site_detail_url = site_detail_url,
					website = website
				};
			}

			return result;
		}

		public static Games[] DeserializeGames(string xml)
		{
			XmlDocument doc = new XmlDocument();
			doc.LoadXml(xml);
			XmlNodeList nodes = doc.SelectNodes("response/results/game");
			Games[] result = new Games[nodes.Count];

			for (int a = 0; a < result.Length; a++)
			{
				XmlNode comp = nodes[a];
				string[] aliases = comp.SelectSingleNode("aliases").InnerText.Split('\n');
				string api_detail_url = comp.SelectSingleNode("api_detail_url").InnerText;

				DateTime date_added = new DateTime();
				DateTime date_last_updated = new DateTime();
				DateTime.TryParse(comp.SelectSingleNode("date_added").InnerText, out date_added);
				DateTime.TryParse(comp.SelectSingleNode("date_last_updated").InnerText, out date_last_updated);

				string deck = comp.SelectSingleNode("deck").InnerText;
				string description = comp.SelectSingleNode("description").InnerText;

				int expected_release_month = 1;
				int expected_release_year = 1;
				int.TryParse(comp.SelectSingleNode("expected_release_month").InnerText, out expected_release_month);
				int.TryParse(comp.SelectSingleNode("expected_release_year").InnerText, out expected_release_year);

				int expected_release_quarter = 0;
				int.TryParse(comp.SelectSingleNode("expected_release_quarter").InnerText, out expected_release_quarter);

				string guid = comp.SelectSingleNode("guid").InnerText;
				string id = comp.SelectSingleNode("id").InnerText;

				string image = comp.SelectSingleNode("image").InnerText;
				string image_tags = comp.SelectSingleNode("image_tags").InnerText;

				string name = comp.SelectSingleNode("name").InnerText;

				int number_of_user_reviews = 0;
				int.TryParse(comp.SelectSingleNode("number_of_user_reviews").InnerText, out number_of_user_reviews);

				string original_game_rating = comp.SelectSingleNode("original_game_rating/game_rating/name") == null ? "" : comp.SelectSingleNode("original_game_rating/game_rating/name").InnerText;

				DateTime original_release_date = new DateTime();
				DateTime.TryParse(comp.SelectSingleNode("original_release_date").InnerText, out original_release_date);

				string[] platforms = Extensions.Extensions.ConvertToList(comp.SelectNodes("platforms/platform/id")).Select(x => x.InnerText).ToArray();
				string site_detail_url = comp.SelectSingleNode("site_detail_url").InnerText;

				result[a] = new Games
				{
					aliases = aliases,
					api_detail_url = api_detail_url,
					date_added = date_added,
					date_last_updated = date_last_updated,
					deck = deck,
					description = description,
					expected_release_month = expected_release_month,
					expected_release_year = expected_release_year,
					expected_release_quarter = expected_release_quarter,
					guid = guid,
					id = id,
					image = image,
					image_tags = image_tags,
					name = name,
					number_of_user_reviews = number_of_user_reviews,
					original_game_rating = original_game_rating,
					original_release_date = original_release_date,
					platforms = platforms,
					site_detail_url = site_detail_url
				};
			}

			return result;
		}
	}
}
