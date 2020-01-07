using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using Discord.Commands;
using WhitelistDiscordBot.Services;

namespace WhitelistDiscordBot
{
    class Program
    {
        private DiscordSocketClient _client;
        private CommandHandler _commandHandler;

        static void Main(string[] args) => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
            _client = new DiscordSocketClient();
            _commandHandler = new CommandHandler();


            _client.Log += Log;

            await _client.LoginAsync(TokenType.Bot, ConfigurationManager.AppSettings["DiscordToken"].ToString());
            await _client.StartAsync();
            await _commandHandler.InstallCommandsAsync(_client);

            // Block this task until the program is closed.
            await Task.Delay(-1);
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
    }

    public class Commands : ModuleBase<SocketCommandContext>
    {
        [Command("whitelist")]
        [Summary("Add user to whitelist DB")]
        public async Task AddWhitelist(string hexID)
        {
            var user = Context.User as SocketGuildUser;
            if (!user.GuildPermissions.Administrator)
            {
                var msg = await ReplyAsync("You have to be an Admin to whitelist");
                await msg.DeleteAsync();

                return;
            }

            DBConnect dBConnect = new DBConnect();

            if (hexID != "" || hexID != null)
            {
                var msg = await ReplyAsync("Whitelisting User");

                //DB Stuff
                if (!dBConnect.InWhitelist(hexID))
                {
                    if (dBConnect.Whitelist(hexID))
                    {
                        Console.WriteLine($"{DateTime.Now.TimeOfDay}    Whitelisted user: steam:{hexID}");
                        await ReplyAsync($"User steam:{hexID} is now whitelisted");
                        await msg.DeleteAsync();
                    }
                    else
                    {
                        await ReplyAsync($"Could not connect to DB");
                        await msg.DeleteAsync();
                    }
                }
                else
                {
                    if (dBConnect.UpdateWhitelist(hexID, 1))
                    {
                        Console.WriteLine($"{DateTime.Now.TimeOfDay}    Whitelisted user: steam:{hexID}");
                        await ReplyAsync($"User steam:{hexID} is now whitelisted");
                        await msg.DeleteAsync();
                    }
                    else
                    {
                        await ReplyAsync($"Could not connect to DB");
                        await msg.DeleteAsync();
                    }
                }
            }
        }

        [Command("-whitelist")]
        [Summary("Remove user from whitelist DB")]
        public async Task RemoveWhitelist(string hexID)
        {
            var user = Context.User as SocketGuildUser;
            if (!user.GuildPermissions.Administrator)
            {
                var msg = await ReplyAsync("You have to be an Admin to remove whitelist");
                await msg.DeleteAsync();

                return;
            }

            DBConnect dBConnect = new DBConnect();

            if (hexID != "" || hexID != null)
            {
                var msg = await ReplyAsync("Removing Whitelist");

                //DB Stuff
                if (dBConnect.InWhitelist(hexID))
                {
                    if (dBConnect.UpdateWhitelist(hexID, 0))
                    {
                        Console.WriteLine($"{DateTime.Now.TimeOfDay}    Removed user: steam:{hexID}");
                        await ReplyAsync($"User steam:{hexID} is now removed from whitelist");
                        await msg.DeleteAsync();
                    }
                    else
                    {
                        await ReplyAsync($"Could not connect to DB");
                        await msg.DeleteAsync();
                    }
                }
                else
                {
                    await ReplyAsync($"User isn't whitelisted");
                    await msg.DeleteAsync();
                }
            }
        }
    }
}
