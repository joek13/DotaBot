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

			Hero hero = Dota.FindHero(heroSearched);
			var heroAbs = await Dota.GetAbilities(hero);

			Discord.MessageReactionAdded -= DiscordMessageReactionAdded;
			Discord.MessageReactionRemoved -= DiscordMessageReactionRemoved;

			bool hasD = false;
			bool hasF = false;
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

			ulong dotaResponseId = ctx.Message.RespondAsync("", embed: await Dota.MakeHeroEmbed(ctx, hero)).GetAwaiter().GetResult().Id;
			DiscordMessage dotaResponse = await ctx.Channel.GetMessageAsync(dotaResponseId);
			await dotaResponse.CreateReactionAsync(DiscordEmoji.FromUnicode("🇶"));
			await dotaResponse.CreateReactionAsync(DiscordEmoji.FromUnicode("🇼"));
			await dotaResponse.CreateReactionAsync(DiscordEmoji.FromUnicode("🇪"));
			if (hasD)
			{
				await dotaResponse.CreateReactionAsync(DiscordEmoji.FromUnicode("🇩"));
			}
			if (hasF)
			{
				await dotaResponse.CreateReactionAsync(DiscordEmoji.FromUnicode("🇫"));
			}
			await dotaResponse.CreateReactionAsync(DiscordEmoji.FromUnicode("🇷"));

			Discord.MessageReactionAdded += DiscordMessageReactionAdded;
			Discord.MessageReactionRemoved += DiscordMessageReactionRemoved;

			async Task DiscordMessageReactionAdded(MessageReactionAddEventArgs e)
			{
				if (e.User.IsBot) { return; }
				if (e.Channel.LastMessageId != e.Message.Id) { return; }

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
				if (e.Channel.LastMessageId != e.Message.Id) { return; }

				await e.Message.ModifyAsync("", embed: await Dota.MakeHeroEmbed(ctx, hero));
			}
		}
	}
}
