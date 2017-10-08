using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using static DotaBot.Dict;
using DotaBot.Entities;
using DSharpPlus.Entities;
using DSharpPlus.CommandsNext;
using System.Text.RegularExpressions;

namespace DotaBot.Util
{
	public static class Dota
	{
		public static Hero FindHero(string[] heroToFindArgs)
		{
			Hero[] heroList = JsonConvert.DeserializeObject<Hero[]>(File.ReadAllText("./Json/heroes.json"));
			Hero[] heroList2 = JsonConvert.DeserializeObject<Hero[]>(File.ReadAllText("./Json/heroes2.json"));
			Dictionary<string, string[]> shortHeroList = JsonConvert.DeserializeObject<Dictionary<string, string[]>>(File.ReadAllText("./Json/shortHeroes.json"));
			string heroToFind = string.Join(" ", heroToFindArgs).ToLower();

			Hero heroObj = null;
			Hero heroObj2 = null;
			Hero masterHeroObj = null;
			bool heroFound = false;

			//look for hero name through nicknames
			foreach (KeyValuePair<string, string[]> shortHero in shortHeroList)
			{
				foreach (string hero in shortHero.Value)
				{
					if (heroToFind.ToLower() == hero.ToLower())
					{
						heroToFind = shortHero.Key;
						heroFound = true;
					}
				}
			}
			//if the hero's name wasnt found, throws exception
			if (!heroFound) { throw new HeroNotFoundException("That hero doesn't exist or wasn't found. Check your spelling and/or stop being a fucking retard"); }

			//find hero object 1 (this one has most of the stats)
			foreach (Hero hero in heroList)
			{
				if (hero.TrueName == heroToFind)
				{
					heroObj = hero;
					masterHeroObj = heroObj;
				}
			}

			//find hero object 2 (less of the stats)
			foreach (Hero hero2 in heroList2)
			{
				if (hero2.Code.Substring(14) == masterHeroObj.TrueName && heroToFind != null)
				{
					heroObj2 = hero2;

					//adding the heroes stats to master object
					masterHeroObj.HeroID = heroObj2.HeroID;
					masterHeroObj.MainAttribute = heroObj2.MainAttribute;
					masterHeroObj.AttackType = heroObj2.AttackType;
					masterHeroObj.Complexity = heroObj2.Complexity;
					masterHeroObj.Url = heroObj2.Url;
					masterHeroObj.Roles = heroObj2.Roles;
				}
			}

			return masterHeroObj;
		}

		public static async Task<List<Ability>> GetAbilities(Hero hero)
		{
			Ability[] abilities = JsonConvert.DeserializeObject<Ability[]>(File.ReadAllText("./Json/abilities.json"));
			List<Ability> masterAbilities = new List<Ability>();
			string[] order = { "q", "w", "e", "d", "f", "r" };

			foreach (Ability ability in abilities)
			{
				if (ability.Hero == hero.TrueName)
				{
					masterAbilities.Add(ability);
				}
			}

			await Task.Delay(1);

			masterAbilities.Sort((a, b) => Array.IndexOf(order, a.Key).CompareTo(Array.IndexOf(order, b.Key)));

			return masterAbilities;
		}

		///<param name="steamID">Steam ID of the user.</param>
		///<param name="amountOfHeroes">Amount of heroes' histories you want to return. Defaults to all if empty.</param>
		public static async Task<History[]> GetHistory(long steamID, int amountOfHeroes = 112)
		{
			History[] masterHistory = null;

			using (HttpClient client = new HttpClient())
			{
				HttpResponseMessage response = await client.GetAsync($"https://api.opendota.com/api/players/{steamID}/heroes");
				HttpContent content = response.Content;
				string historyJson = await content.ReadAsStringAsync();
				History[] histories = JsonConvert.DeserializeObject<History[]>(historyJson);

				masterHistory = histories;

				client.Dispose();
			}

			return masterHistory.SubArray(0, amountOfHeroes);
		}

		public static async Task<DiscordEmbed> MakeHeroEmbed(CommandContext ctx, Hero hero)
		{
			long steamID = new long();
			string strMark = "", agiMark = "", intMark = "", roleString = "", abilityString = "", historyString = "";

			List<Ability> heroAbilities = await GetAbilities(hero);

			//find corresponding steamID using ID Dictionary
			foreach (var id in IdDict)
			{
				if (id.Key == ctx.Message.Author.Id)
				{
					steamID = id.Value;
				}
			}

			//make string of hero abilities
			foreach (Ability ability in heroAbilities)
			{
				if (ability.Key == "q" || ability.Key == "w" || ability.Key == "e" || ability.Key == "d" || ability.Key == "f")
				{
					abilityString += $"{ability.Key.ToUpper()} - {ability.Name}, ";
				}
				else if (ability.Key == "r")
				{
					abilityString += $"{ability.Key.ToUpper()} - {ability.Name} ";
				}
			}

			//setup string with roles for hero
			foreach (var role in hero.Roles)
			{
				roleString += $"{role.Key} {SetRoleEmoji(role.Value)}\n";
			}

			History[] heroHistory = await GetHistory(steamID);
			foreach (History history in heroHistory)
			{
				if (history.HeroID == hero.HeroID && history.Games == 0)
				{
					historyString = $"You haven't played {hero.GameName} yet";
				}
				else if (history.HeroID == hero.HeroID)
				{
					double winrate = Math.Round((((double)history.Win / history.Games) * 100), 2, MidpointRounding.AwayFromZero);
					historyString = $"{history.Games} Games - {history.Win} Win(s) / {(history.Games - history.Win)} Losses  ({winrate}% Winrate)";
				}
			}

			if (hero.MainAttribute == "strength") { strMark = "**"; }
			if (hero.MainAttribute == "agility") { agiMark = "**"; }
			if (hero.MainAttribute == "intelligence") { intMark = "**"; }
			hero.AttackType = (hero.AttackType == "ranged") ? "Ranged" : "Melee";

			var heroEmbed = new DiscordEmbedBuilder()
			{
				Color = new DiscordColor(53, 152, 219),
				ThumbnailUrl = $"https://api.opendota.com/apps/dota2/images/heroes/{hero.TrueName}_full.png"
			};
			heroEmbed.WithAuthor(hero.GameName, hero.Url, $"https://api.opendota.com/apps/dota2/images/heroes/{hero.TrueName}_icon.png")
				.AddField("Base Stats", $"HP: {hero.Health}\nMana: {hero.Mana}\nDamage: {hero.Damage}\nArmor: {hero.Armor}\n<:strength:298134111908790274>{hero.Strength} {strMark}Strength{strMark} + {hero.StrGain}\n<:agility:298134111988482048>{hero.Agility} " +
					$"{agiMark}Agility{agiMark} + {hero.AgiGain}\n<:intelligence:298134111610994690>{hero.Intelligence} {intMark}Intelligence{intMark} + {hero.IntGain}\n<:RUN:299695228233842689>{hero.BaseSpeed} MS", true)
				.AddField("Roles", $"**{hero.AttackType}**\n{roleString}", true)
				.AddField("Abilities", abilityString, false)
				.AddField("Your History", historyString, false);

			return heroEmbed.Build();
		}

		public static async Task<DiscordEmbed> MakeAbilityEmbed(Hero hero, string key)
		{
			var abilities = await Dota.GetAbilities(hero);
			var regex = new Regex(@"/ /g", RegexOptions.IgnorePatternWhitespace);
			DiscordEmbedBuilder abilityEmbed = null;

			foreach (Ability ab in abilities)
			{
				if (ab.Key == key)
				{
					if (ab.Manacost == null)
					{
						ab.Manacost = "Passive";
					}
					if (ab.Cooldown == null)
					{
						ab.Cooldown = "None";
					}

					abilityEmbed = new DiscordEmbedBuilder()
					{
						Color = new DiscordColor(53, 152, 219),
						Description = string.Join("\n", ab.Description)
					};
					abilityEmbed.WithAuthor($"{key.ToUpper()} - {ab.Name}", null, $"https://api.opendota.com/apps/dota2/images/abilities/{hero.TrueName}_{ab.Name.ToLower().Replace(' ', '_').Replace("'", "")}_md.png")
						.AddField($"<:manacost:298144629377990656> {ab.Manacost.Replace(" ", "/")}", String.Join("\n", ab.Stats).Replace(" / ", "/"), true)
						.AddField($"<:cooldown:298144629369470976> {ab.Cooldown.Replace(" ", "/")}", String.Join("\n", ab.Effects).Replace(" / ", "/"), true);
				}
			}

			return abilityEmbed.Build();
		}

		public static string SetRoleEmoji(int value)
		{
			string finalString = "";

			for (int i = 0; i < value; i++)
			{
				finalString += "<:role:298144629226733569>";
			}

			return finalString;
		}

		public static T[] SubArray<T>(this T[] data, int index, int length)
		{
			T[] result = new T[length];
			Array.Copy(data, index, result, 0, length);

			return result;
		}
	}
}
