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
			try
			{
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

				Task DiscordMessageReactionAdded(MessageReactionAddEventArgs e)
				{
					if (e.User.IsBot) { return Task.Delay(0); }
					if (e.User.Id != ctx.User.Id)
					{
						e.Message.DeleteReactionAsync(e.Emoji, e.User);
						return Task.Delay(0);
					}
					
					//im sure theres a better way to do this but for now its this
					if (e.Emoji == DiscordEmoji.FromUnicode("🇶"))
					{
						e.Message.ModifyAsync("", embed: Dota.MakeAbilityEmbed(hero, "q").GetAwaiter().GetResult());
					}
					if (e.Emoji == DiscordEmoji.FromUnicode("🇼"))
					{
						e.Message.ModifyAsync("", embed: Dota.MakeAbilityEmbed(hero, "w").GetAwaiter().GetResult());
					}
					if (e.Emoji == DiscordEmoji.FromUnicode("🇪"))
					{
						e.Message.ModifyAsync("", embed: Dota.MakeAbilityEmbed(hero, "e").GetAwaiter().GetResult());
					}
					if (e.Emoji == DiscordEmoji.FromUnicode("🇷"))
					{
						e.Message.ModifyAsync("", embed: Dota.MakeAbilityEmbed(hero, "r").GetAwaiter().GetResult());
					}
					if (e.Emoji == DiscordEmoji.FromUnicode("🇫"))
					{
						e.Message.ModifyAsync("", embed: Dota.MakeAbilityEmbed(hero, "f").GetAwaiter().GetResult());
					}
					if (e.Emoji == DiscordEmoji.FromUnicode("🇩"))
					{
						e.Message.ModifyAsync("", embed: Dota.MakeAbilityEmbed(hero, "d").GetAwaiter().GetResult());
					}

					return Task.Delay(0);
				}

				Task DiscordMessageReactionRemoved(MessageReactionRemoveEventArgs e)
				{
					if (e.User.IsBot) { return Task.Delay(0); }

					e.Message.ModifyAsync("", embed: Dota.MakeHeroEmbed(ctx, hero).GetAwaiter().GetResult());

					return Task.Delay(0);
				}
			}
			catch (HeroNotFoundException hnfex)
			{
				await ctx.Message.Channel.SendMessageAsync(hnfex.Message);
			}
		}
	}
}