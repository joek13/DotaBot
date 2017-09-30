using System;
using System.IO;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

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


			Discord.Ready += DiscordReady;
			Discord.MessageCreated += DiscordMessageCreated;
			Discord.ClientErrored += DiscordClientErrored;


			await Discord.ConnectAsync();
			await Task.Delay(-1);
		}

		private Task DiscordReady(ReadyEventArgs e)
		{
			Console.WriteLine("Ready to 322 the game");
			Discord.UpdateStatusAsync(new Game("DOTA 2"));

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