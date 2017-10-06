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
		public CommandsNextModule CommandsModule;

		static void Main(string[] args)
			=> new Program().RunMrBot(args).ConfigureAwait(false).GetAwaiter().GetResult();

		private async Task RunMrBot(string[] args)
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
			Discord.MessageCreated += DiscordMessageCreated;
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
				stackTrace = (!String.IsNullOrWhiteSpace(stackTrace)) ? ((stackTrace.Length > 1000) ? stackTrace.Substring(0, 1000) : stackTrace) : "No stacktrace available";

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

		private Task DiscordReady(ReadyEventArgs e)
		{
			Console.WriteLine("Ready to 322 the game");
			Discord.UpdateStatusAsync(new DiscordGame("DOTA 2"));

			return Task.Delay(0);
		}

		private Task DiscordMessageCreated(MessageCreateEventArgs e)
		{
			if (e.Message.Author.IsBot) return Task.Delay(0);

			return Task.Delay(0);
		}

		private Task DiscordClientErrored(ClientErrorEventArgs e)
		{
			Console.WriteLine(e.Exception.Message + "\n" + e.Exception.StackTrace + "\n" + e.Exception.InnerException);

			return Task.Delay(0);
		}
	}
}