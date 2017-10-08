using System;
using System.IO;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.CommandsNext.Exceptions;
using System.Collections.Generic;
using DotaBot.Entities;

namespace DotaBot
{
	public class Program
	{
		public static DiscordClient Discord;
		public static CommandsNextModule CommandsModule;

		static void Main(string[] args)
		{
			RunMrBot(args).ConfigureAwait(false).GetAwaiter().GetResult();
		}

		private static async Task RunMrBot(string[] args)
		{
			DiscordConfiguration discordConfig = new DiscordConfiguration
			{
				Token = File.ReadAllText("Token.txt"),
				TokenType = TokenType.Bot,
				UseInternalLogHandler = true,
				LogLevel = LogLevel.Debug
			};
			CommandsNextConfiguration cNextConfig = new CommandsNextConfiguration
			{
				StringPrefix = "--",
				CaseSensitive = false
			};
			Discord = new DiscordClient(discordConfig);
			CommandsModule = Discord.UseCommandsNext(cNextConfig);
			CommandsModule.RegisterCommands<DotaCommands>();
			CommandsModule.CommandErrored += CommandsModule_CommandErrored;

			Discord.Ready += DiscordReady;
			Discord.ClientErrored += DiscordClientErrored;


			await Discord.ConnectAsync();
			await Task.Delay(-1);
		}

		static async Task CommandsModule_CommandErrored(CommandErrorEventArgs e)
		{
			if (e.Exception is CommandNotFoundException) { return; }

			if (e.Exception is HeroNotFoundException)
			{
				await e.Context.RespondAsync(e.Exception.Message);
				return;
			}

			List<Exception> exceptions = new List<Exception>();
			if (e.Exception is AggregateException ag)
			{
				exceptions.AddRange(ag.InnerExceptions);
			}

			exceptions.Add(e.Exception);


			foreach (var ex in exceptions)
			{
				if (ex is CommandNotFoundException) { return; }

				var message = ex.Message;
				var stackTrace = ex.StackTrace;

				message = (message.Length > 1000) ? message.Substring(0, 1000) : message;
				stackTrace = (!string.IsNullOrWhiteSpace(stackTrace)) ? ((stackTrace.Length > 1000) ? stackTrace.Substring(0, 1000) : stackTrace) : "No stacktrace available";

				var exEmbed = new DiscordEmbedBuilder
				{
					Color = new DiscordColor(53, 152, 219),
					Title = "An exception occured when executing a command",
					Description = $"`{e.Exception.GetType()}` occcured when executing `{e.Command.QualifiedName}`.",
					Timestamp = DateTime.UtcNow
				};
				exEmbed.WithFooter(Discord.CurrentUser.Username, Discord.CurrentUser.AvatarUrl)
					.AddField("Message", message, false)
					.AddField("Stack Trace", $"```cs\n{stackTrace}\n```", false);
				await e.Context.Channel.SendMessageAsync("", embed: exEmbed.Build());
			}
		}

		static async Task DiscordReady(ReadyEventArgs e)
		{
			Console.WriteLine("Ready to 322 the game");
			await Discord.UpdateStatusAsync(new DiscordGame("DOTA 2"));
		}

		static Task DiscordClientErrored(ClientErrorEventArgs e)
		{
			List<Exception> exceptions = new List<Exception>();
			if (e.Exception is AggregateException ag)
			{
				exceptions.AddRange(ag.InnerExceptions);
			}

			exceptions.Add(e.Exception);
			foreach (var exception in exceptions)
			{
				Console.WriteLine(exception.Message + "\n" + exception.StackTrace);
			}

			return Task.Delay(0);
		}
	}
}