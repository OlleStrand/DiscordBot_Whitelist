using Discord.Commands;
using Discord.WebSocket;
using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace WhitelistDiscordBot.Services
{
    class CommandHandler
    {
        private DiscordSocketClient _client;
        private CommandService _commands;

        public async Task InstallCommandsAsync(DiscordSocketClient c)
        {
            _client = c;
            _commands = new CommandService();

            // Hook the MessageReceived event into our command handler
            _client.MessageReceived += HandleCommandAsync;
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), null);
        }

        private async Task HandleCommandAsync(SocketMessage s)
        {
            // Don't process the command if it was a system message
            var message = s as SocketUserMessage;
            if (message == null) return;

            int argPos = 0;
            char prefix = '!';

            // Determine if the message is a command based on the prefix and make sure no bots trigger commands
            if (!message.HasCharPrefix(prefix, ref argPos))
                return;

            // Create a WebSocket-based command context based on the message
            var context = new SocketCommandContext(_client, message);

            var result = await _commands.ExecuteAsync(
                context: context,
                argPos: argPos,
                services: null);

            if (!result.IsSuccess)
            {
                switch (result.ToString())
                {
                    default:

                        await s.Channel.SendMessageAsync($"An error occurred! Details: ```" + result.ToString() + "```");
                        break;
                    case "UnknownCommand: Unknown command.":

                        await message.DeleteAsync();

                        await s.Channel.SendMessageAsync($"Command not found! Use the command {prefix}help for a list of commands.");
                        break;
                }
            }
        }
    }
}
