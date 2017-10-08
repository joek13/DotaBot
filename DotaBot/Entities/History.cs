using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace DotaBot.Entities
{
	public class History
	{
		[JsonProperty("hero_id")]
		public string HeroID { get; set; }

		[JsonProperty("against_win")]
		public int AgainstWin { get; set; }

		[JsonProperty("against_games")]
		public int AgainstGames { get; set; }

		[JsonProperty("games")]
		public int Games { get; set; }

		[JsonProperty("win")]
		public int Win { get; set; }

		[JsonProperty("last_played")]
		public long LastPlayed { get; set; }

		[JsonProperty("with_games")]
		public int WithGames { get; set; }

		[JsonProperty("with_win")]
		public int WithWin { get; set; }
	}
}
