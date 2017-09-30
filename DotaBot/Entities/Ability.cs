using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace DotaBot.Entities
{
	public class Ability
	{
		[JsonProperty("Cooldown")]
		public string Cooldown { get; set; }

		[JsonProperty("Aghanims")]
		public string Aghanims { get; set; }

		[JsonProperty("Key")]
		public string Key { get; set; }

		[JsonProperty("Name")]
		public string Name { get; set; }

		[JsonProperty("Hero")]
		public string Hero { get; set; }

		[JsonProperty("Manacost")]
		public string Manacost { get; set; }

		[JsonProperty("Description")]
		public string[] Description { get; set; }

		[JsonProperty("Effects")]
		public string[] Effects { get; set; }

		[JsonProperty("Stats")]
		public string[] Stats { get; set; }

		[JsonProperty("Notes")]
		public string[] Notes { get; set; }
	}

	[Serializable]
	public class AbilityNotFoundException : Exception
	{
		public AbilityNotFoundException() { }

		public AbilityNotFoundException(string message) : base(message) { }
	}
}
