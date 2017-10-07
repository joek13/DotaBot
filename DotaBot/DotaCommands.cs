using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DotaBot.Util;
using DotaBot.Entities;
using static DotaBot.Dict;
using static DotaBot.Program;

namespace DotaBot
{
	public class DotaCommands
	{
		[Command("Hero"), Description("Get's information about a Dota hero you ask for")]
		public async Task Hero(CommandContext ctx, params string[] heroSearched)
		{
			Dictionary<string, DiscordEmoji> emojiMap = new Dictionary<string, DiscordEmoji>()
			{
				{ "q", DiscordEmoji.FromUnicode("🇶") },
				{ "w", DiscordEmoji.FromUnicode("🇼") },
				{ "e", DiscordEmoji.FromUnicode("🇪") },
				{ "d", DiscordEmoji.FromUnicode("🇩") },
				{ "f", DiscordEmoji.FromUnicode("🇫") },
				{ "r", DiscordEmoji.FromUnicode("🇷") }
			};

			bool hasD = false;
			bool hasF = false;

			Hero hero = Dota.FindHero(heroSearched);
			var heroAbs = await Dota.GetAbilities(hero);

			foreach (Ability ab in heroAbs)
			{
				if (ab.Key == "d")
				{
					hasD = true;
				}
				if (ab.Key == "f")
				{
					hasF = true;
				}
			}

			Discord.MessageReactionAdded -= DiscordMessageReactionAdded;
			Discord.MessageReactionRemoved -= DiscordMessageReactionRemoved;

			var id = ctx.Message.RespondAsync("", embed: await Dota.MakeHeroEmbed(ctx, hero)).GetAwaiter().GetResult().Id;
			var initialMessage = await ctx.Channel.GetMessageAsync(id);

			await initialMessage.CreateReactionAsync(DiscordEmoji.FromUnicode("🇶"));
			await initialMessage.CreateReactionAsync(DiscordEmoji.FromUnicode("🇼"));
			await initialMessage.CreateReactionAsync(DiscordEmoji.FromUnicode("🇪"));
			if (hasD)
			{
				await initialMessage.CreateReactionAsync(DiscordEmoji.FromUnicode("🇩"));
			}
			if (hasF)
			{
				await initialMessage.CreateReactionAsync(DiscordEmoji.FromUnicode("🇫"));

			}
			await initialMessage.CreateReactionAsync(DiscordEmoji.FromUnicode("🇷"));

			Discord.MessageReactionAdded += DiscordMessageReactionAdded;
			Discord.MessageReactionRemoved += DiscordMessageReactionRemoved;

			async Task DiscordMessageReactionAdded(MessageReactionAddEventArgs e)
			{
				if (e.User.IsBot) { return; }
				if (e.User.Id != ctx.User.Id)
				{
					await e.Message.DeleteReactionAsync(e.Emoji, e.User);
					return;
				}

				foreach (var kvp in emojiMap)
				{
					if (kvp.Value == e.Emoji)
					{
						await e.Message.ModifyAsync("", embed: await Dota.MakeAbilityEmbed(hero, kvp.Key));
					}
				}
			}

			async Task DiscordMessageReactionRemoved(MessageReactionRemoveEventArgs e)
			{
				if (e.User.IsBot) { return; }

				await e.Message.ModifyAsync("", embed: await Dota.MakeHeroEmbed(ctx, hero));
			}
		}
	}
}
