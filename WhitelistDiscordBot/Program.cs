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
        [Command("help")]
        [Summary("Show all commands")]
        public async Task Help()
        {
            var user = Context.User as SocketGuildUser;
            if (!user.GuildPermissions.Administrator)
            {
                var msg = await ReplyAsync("Botten innehåller bara admin kommandon");
                await Task.Delay(3000);
                await msg.DeleteAsync();

                return;
            }

            var builder = new EmbedBuilder();
            builder.WithTitle("Alla kommandon");

            builder.AddField("Kommando", "whitelist\n-whitelist\nlist", true);
            builder.AddField("Förklaring", "!whitelist <hexId> | Lägg  till hexId till whitelist\n" +
                "!-whitelist <hexId> | Ta bort hexId från whitelist\n" +
                "!list <vapen> (<jobb>) | Lista på alla användare med vapenet, skriv *all* för alla vapen. Filtrera genom att skriva in ett jobb efter vapen", true);

            //builder.AddField("whitelist", "!whitelist <hexId> | Lägg  till hexId till whitelist", true);
            //builder.AddField("-whitelist", "!-whitelist <hexId> | Ta bort hexId från whitelist", true);
            //builder.AddField("list", "!list <vapen> (<jobb>) | Lista på alla användare med vapenet, skriv *all* för alla vapen. Filtrera genom att skriva in ett jobb efter vapen", true);
            //builder.AddField("listuser", "!listuser <hexId> | Lista alla vapen hos hexId", true);
            builder.WithColor(Color.DarkBlue);

            var m2 = await ReplyAsync("", false, builder.Build());
            await Task.Delay(25000);
            await m2.DeleteAsync();
            await Context.Message.DeleteAsync();
        }

        [Command("whitelist")]
        [Summary("Add user to whitelist DB")]
        public async Task AddWhitelist(string hexID)
        {
            var user = Context.User as SocketGuildUser;
            if (!user.GuildPermissions.Administrator)
            {
                var msg = await ReplyAsync("Du måste vara en admin för att whitelista");
                await Task.Delay(3000);
                await msg.DeleteAsync();

                return;
            }

            DBConnect dBConnect = new DBConnect();

            if (hexID != "" || hexID != null)
            {
                var msg = await ReplyAsync("Whitelistar användare");

                //DB Stuff
                if (!dBConnect.InWhitelist(hexID))
                {
                    if (dBConnect.Whitelist(hexID))
                    {
                        Console.WriteLine($"{DateTime.Now.TimeOfDay}    Whitelistade användare: steam:{hexID}");
                        await ReplyAsync($"Användare steam:{hexID} är nu whitelistad");
                        await msg.DeleteAsync();
                    }
                    else
                    {
                        await ReplyAsync($"Kunde inte koppla till databasen");
                        await msg.DeleteAsync();
                    }
                }
                else
                {
                    if (dBConnect.UpdateWhitelist(hexID, 1))
                    {
                        Console.WriteLine($"{DateTime.Now.TimeOfDay}    Whitelistade användare: steam:{hexID}");
                        await ReplyAsync($"Användare steam:{hexID} är nu whitelistad");
                        await msg.DeleteAsync();
                    }
                    else
                    {
                        await ReplyAsync($"Kunde inte koppla till databasen");
                        await msg.DeleteAsync();
                    }
                }
            }
        }

        [Command("-whitelist")]
        [Summary("Ta bort användare från databasen")]
        public async Task RemoveWhitelist(string hexID)
        {
            var user = Context.User as SocketGuildUser;
            if (!user.GuildPermissions.Administrator)
            {
                var msg = await ReplyAsync("Du måste vara en admin för att ta bort whitelist");
                await Task.Delay(3000);
                await msg.DeleteAsync();

                return;
            }

            DBConnect dBConnect = new DBConnect();

            if (hexID != "" || hexID != null)
            {
                var msg = await ReplyAsync("Tar bort whitelist...");

                //DB Stuff
                if (dBConnect.InWhitelist(hexID))
                {
                    if (dBConnect.UpdateWhitelist(hexID, 0))
                    {
                        Console.WriteLine($"{DateTime.Now.TimeOfDay}    Tog bort: steam:{hexID}");
                        await ReplyAsync($"Användare steam:{hexID} är nu borttagen");
                        await msg.DeleteAsync();
                    }
                    else
                    {
                        await ReplyAsync($"Kunde inte koppla till databasen");
                        await msg.DeleteAsync();
                    }
                }
                else
                {
                    await ReplyAsync($"Användaren är inte whitelistad");
                    await msg.DeleteAsync();
                }
            }
        }

        [Command("list")]
        [Summary("Lista alla anävndare med specifikt vapen")]
        public async Task List(string weapon, string filterJob = "")
        {
            var user = Context.User as SocketGuildUser;
            if (!user.GuildPermissions.Administrator)
            {
                var msg = await ReplyAsync("Du måste vara en admin för att lista användare");
                await Task.Delay(3000);
                await msg.DeleteAsync();

                return;
            }
            DBConnect dBConnect = new DBConnect();

            if (weapon != "" || weapon != null)
            {
                var msg = await ReplyAsync("Listing Users");

                if (weapon == "all")
                {
                    await msg.DeleteAsync();
                    string message = "```Markdown";
                    foreach (var usr in dBConnect.GetUsersWithWeapons(filterJob))
                    {
                        if (usr.Weapons.Count <= 0)
                            continue;

                        string weaponString = "[";
                        foreach (var vapen in usr.Weapons)
                        {
                            weaponString += $"{vapen.Label}, ";
                        }
                        weaponString = weaponString.Remove(weaponString.Length - 2);
                        weaponString += "]";

                        message +=
                            $"\n#{usr.SteamName} | {usr.SteamID64}\n" +
                            $"RP Karaktär: {usr.Name}, jobbar som {usr.Job}\n" +
                            $"Vapen: {weaponString}\n";
                        if (message.Length >= 1500)
                        {
                            message += "```";
                            await ReplyAsync(message);
                            message = "```Markdown";
                            await Task.Delay(500);
                        }
                    }
                    message += "```";
                    await ReplyAsync(message);
                    await Task.Delay(500);
                }
                else
                {
                    await msg.DeleteAsync();
                    string message = "```Markdown";
                    foreach (var usr in dBConnect.GetUsersWithWeapon(weapon, filterJob))
                    {
                        if (usr.Weapons.Count <= 0)
                            continue;

                        string weaponString = "[";
                        foreach (var vapen in usr.Weapons)
                        {
                            weaponString += $"{vapen.Label}, ";
                        }
                        weaponString = weaponString.Remove(weaponString.Length - 2);
                        weaponString += "]";

                        message +=
                            $"\n#{usr.SteamName} | {usr.SteamID64}\n" +
                            $"RP Karaktär: {usr.Name}, jobbar som {usr.Job}\n" +
                            $"Vapen: {weaponString}\n";
                        if (message.Length >= 1500)
                        {
                            message += "```";
                            await ReplyAsync(message);
                            message = "```Markdown";
                            await Task.Delay(500);
                        }
                    }
                    message += "```";
                    await ReplyAsync(message);
                    await Task.Delay(500);
                }
            }
        }
    }
}
