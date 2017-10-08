using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace DotaBot.Entities
{
	public class Hero
	{
		[JsonProperty("TrueName")]
		public string TrueName { get; set; } //phantom_assassin

		[JsonProperty("GameName")]
		public string GameName { get; set; } //Phantom Assassin

		[JsonProperty("Damage")]
		public string Damage { get; set; }

		[JsonProperty("Armor")]
		public int Armor { get; set; }

		[JsonProperty("Health")]
		public int Health { get; set; }

		[JsonProperty("Mana")]
		public int Mana { get; set; }

		[JsonProperty("Strength")]
		public int Strength { get; set; }

		[JsonProperty("StrGain")]
		public double StrGain { get; set; }

		[JsonProperty("Agility")]
		public int Agility { get; set; }

		[JsonProperty("AgiGain")]
		public double AgiGain { get; set; }

		[JsonProperty("Intelligence")]
		public int Intelligence { get; set; }

		[JsonProperty("IntGain")]
		public double IntGain { get; set; }

		[JsonProperty("BaseSpeed")]
		public int BaseSpeed { get; set; }

		[JsonProperty("MainAttribute")]
		public string MainAttribute { get; set; } //heroes2.json

		[JsonProperty("AttackType")]
		public string AttackType { get; set; } //heroes2.json

		[JsonProperty("Complexity")]
		public int Complexity { get; set; } //heroes2.json

		[JsonProperty("Url")]
		public string Url { get; set; } //heroes2.json

		[JsonProperty("Roles")]
		public Dictionary<string, int> Roles { get; set; } //heroes2.json

		[JsonProperty("HeroID")]
		public string HeroID { get; set; }//heroes2.json

		[JsonProperty("Code")]
		public string Code { get; set; } //heroes2.json
	}

	[Serializable]
	public class HeroNotFoundException : Exception
	{
		public HeroNotFoundException() { }

		public HeroNotFoundException(string message) : base(message) { }
	}
}
