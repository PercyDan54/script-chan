using Discord;
using Discord.Webhook;
using DiscordRPC;
using Newtonsoft.Json;
using script_chan2.DataTypes;
using Serilog;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using Discord.Net.Rest;
using Discord.Rest;

namespace script_chan2.Discord
{
    public static class DiscordApi
    {
        private static ILogger localLog = Log.ForContext(typeof(DiscordApi));

        private static DiscordRpcClient rpcClient;

        private const string redSquareEmoji = ":red_square:";
        private const string blueSquareEmoji = ":blue_square:";
        private const string purpleSquareEmoji = ":purple_square:";
        private const string winnerEmoji = ":first_place:";
        private static readonly Color redColor = new Color(218, 53, 72);
        private static readonly Color blueColor = new Color(52, 152, 219);

        static DiscordApi()
        {
            rpcClient = new DiscordRpcClient("772143303550435369");
            rpcClient.Initialize();
        }

        public static void SendMatchCreated(Match match)
        {
            localLog.Information("match '{match}' send room created", match.Name);
            foreach (var webhook in match.Tournament.Webhooks.Where(x => x.MatchCreated))
            {
                var embed = new EmbedBuilder
                {
                    Author = new EmbedAuthorBuilder
                    {
                        IconUrl = "https://cdn.discordapp.com/attachments/130304896581763072/400731693192839179/plus.png",
                        Url = "https://osu.ppy.sh/community/matches/" + match.RoomId
                    },
                    Color = Color.Blue,
                    Title = Properties.Resources.DiscordApi_MatchCreatedTitle,
                    Description = string.Format(Properties.Resources.DiscordApi_MatchCreatedDescription, match.RoomId)
                };
                if (!string.IsNullOrEmpty(webhook.AuthorIcon))
                    embed.Author.IconUrl = webhook.AuthorIcon;

                if (match.TeamMode == Enums.TeamModes.TeamVS)
                    embed.Author.Name = $"{match.TeamRed.Name} VS {match.TeamBlue.Name}";
                else
                    embed.Author.Name = match.Name;

                if (embed != null)
                {
                    embed.Footer = new EmbedFooterBuilder
                    {
                        Text = webhook.FooterText,
                        IconUrl = webhook.FooterIcon
                    };

                    try
                    {
                        using (var client = new DiscordWebhookClient(webhook.URL))
                        {
                            client.SendMessageAsync(embeds: new[] { embed.Build() }, username: webhook.Username, avatarUrl: webhook.Avatar).GetAwaiter().GetResult();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error sending match created to webhook " + webhook.Name);
                        localLog.Error(ex, "error sending match created to webhook " + webhook.Name);
                    }
                }
            }
        }

        public static void SendMatchBanRecap(Match match)
        {
            localLog.Information("match '{match}' send ban recap", match.Name);
            foreach (var webhook in match.Tournament.Webhooks.Where(x => x.BanRecap))
            {
                EmbedBuilder embed = null;
                if (match.TeamMode == Enums.TeamModes.TeamVS)
                {
                    embed = new EmbedBuilder
                    {
                        Author = new EmbedAuthorBuilder
                        {
                            IconUrl = "https://cdn.discordapp.com/attachments/130304896581763072/400744720772628481/more-info-button.png",
                            Name = $"{match.TeamRed.Name} VS {match.TeamBlue.Name}",
                            Url = "https://osu.ppy.sh/community/matches/" + match.RoomId
                        },
                        Color = Color.Gold,
                        Url = "https://osu.ppy.sh/community/matches/" + match.RoomId
                    };
                    if (!string.IsNullOrEmpty(webhook.AuthorIcon))
                        embed.Author.IconUrl = webhook.AuthorIcon;

                    if (match.RollWinnerTeam != null)
                        embed.Title = string.Format(match.Protects.Count > 0 ? Properties.Resources.DiscordApi_BanProtectRecapWithRollWinnerTitle : Properties.Resources.DiscordApi_BanRecapWithRollWinnerTitle, match.RollWinnerTeam.Name);
                    else
                        embed.Title = match.Protects.Count > 0 ? Properties.Resources.DiscordApi_BanProtectRecapTitle : Properties.Resources.DiscordApi_BanRecapTitle;
                    var redTeam = string.Empty;
                    var blueTeam = string.Empty;
                    foreach (var protect in match.Protects)
                    {
                        var mod = protect.Map.Tag;
                        if (string.IsNullOrEmpty(mod))
                            mod = Utils.ConvertGameModsToString(protect.Map.Mods);
                        if (protect.Team == match.TeamRed)
                            redTeam += $"🛡 -{protect.ListIndex}- __{mod}__ **{protect.Map.Beatmap.Artist.Replace("_", "\\_").Replace("*", "\\*")} - {protect.Map.Beatmap.Title.Replace("_", "\\_").Replace("*", "\\*")} [{protect.Map.Beatmap.Version.Replace("_", "\\_").Replace("*", "\\*")}]**" + Environment.NewLine;
                        if (protect.Team == match.TeamBlue)
                            blueTeam += $"🛡 -{protect.ListIndex}- __{mod}__ **{protect.Map.Beatmap.Artist.Replace("_", "\\_").Replace("*", "\\*")} - {protect.Map.Beatmap.Title.Replace("_", "\\_").Replace("*", "\\*")} [{protect.Map.Beatmap.Version.Replace("_", "\\_").Replace("*", "\\*")}]**" + Environment.NewLine;
                    }
                    foreach (var ban in match.Bans)
                    {
                        var mod = ban.Map.Tag;
                        if (string.IsNullOrEmpty(mod))
                            mod = Utils.ConvertGameModsToString(ban.Map.Mods);
                        if (ban.Team == match.TeamRed)
                            redTeam += $"🚫 -{ban.ListIndex}- __{mod}__ **{ban.Map.Beatmap.Artist.Replace("_", "\\_").Replace("*", "\\*")} - {ban.Map.Beatmap.Title.Replace("_", "\\_").Replace("*", "\\*")} [{ban.Map.Beatmap.Version.Replace("_", "\\_").Replace("*", "\\*")}]**" + Environment.NewLine;
                        if (ban.Team == match.TeamBlue)
                            blueTeam += $"🚫 -{ban.ListIndex}- __{mod}__ **{ban.Map.Beatmap.Artist.Replace("_", "\\_").Replace("*", "\\*")} - {ban.Map.Beatmap.Title.Replace("_", "\\_").Replace("*", "\\*")} [{ban.Map.Beatmap.Version.Replace("_", "\\_").Replace("*", "\\*")}]**" + Environment.NewLine;
                    }
                    if (!string.IsNullOrEmpty(redTeam))
                        embed.Fields.Add(new EmbedFieldBuilder { Name = match.TeamRed.Name, Value = redTeam });
                    if (!string.IsNullOrEmpty(blueTeam))
                        embed.Fields.Add(new EmbedFieldBuilder { Name = match.TeamBlue.Name, Value = blueTeam });
                }
                else if (match.TeamMode == Enums.TeamModes.HeadToHead)
                {
                    embed = new EmbedBuilder
                    {
                        Author = new EmbedAuthorBuilder
                        {
                            IconUrl = "https://cdn.discordapp.com/attachments/130304896581763072/400744720772628481/more-info-button.png",
                            Name = match.Name,
                            Url = "https://osu.ppy.sh/community/matches/" + match.RoomId
                        },
                        Color = Color.Gold,
                        Url = "https://osu.ppy.sh/community/matches/" + match.RoomId
                    };
                    if (!string.IsNullOrEmpty(webhook.AuthorIcon))
                        embed.Author.IconUrl = webhook.AuthorIcon;

                    if (match.RollWinnerPlayer != null)
                        embed.Title = string.Format(Properties.Resources.DiscordApi_BanRecapWithRollWinnerTitle, match.RollWinnerPlayer.Name);
                    else
                        embed.Title = Properties.Resources.DiscordApi_BanRecapTitle;
                    foreach (var player in match.Players)
                    {
                        var bans = string.Empty;
                        foreach (var ban in match.Bans.Where(x => x.Player == player.Key))
                        {
                            var mod = ban.Map.Tag;
                            if (string.IsNullOrEmpty(mod))
                                mod = Utils.ConvertGameModsToString(ban.Map.Mods);
                            bans += $"__{mod}__ **{ban.Map.Beatmap.Artist.Replace("_", "\\_").Replace("*", "\\*")} - {ban.Map.Beatmap.Title.Replace("_", "\\_").Replace("*", "\\*")} [{ban.Map.Beatmap.Version.Replace("_", "\\_").Replace("*", "\\*")}]**" + Environment.NewLine;
                        }
                        if (!string.IsNullOrEmpty(bans))
                            embed.Fields.Add(new EmbedFieldBuilder { Name = player.Key.Name, Value = bans });
                    }
                }

                if (embed != null)
                {
                    embed.Footer = new EmbedFooterBuilder
                    {
                        Text = webhook.FooterText,
                        IconUrl = webhook.FooterIcon
                    };

                    try
                    {
                        using (var client = new DiscordWebhookClient(webhook.URL))
                        {
                            client.SendMessageAsync(embeds: new[] { embed.Build() }, username: webhook.Username, avatarUrl: webhook.Avatar).GetAwaiter().GetResult();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error sending ban recap to webhook " + webhook.Name);
                        localLog.Error(ex, "error sending ban recap to webhook " + webhook.Name);
                    }
                }
            }
        }

        public static void SendMatchPickRecap(Match match)
        {
            localLog.Information("match '{match}' send pick recap", match.Name);
            foreach (var webhook in match.Tournament.Webhooks.Where(x => x.PickRecap))
            {
                EmbedBuilder embed = null;
                if (match.TeamMode == Enums.TeamModes.TeamVS)
                {
                    embed = new EmbedBuilder
                    {
                        Author = new EmbedAuthorBuilder
                        {
                            IconUrl = "https://cdn.discordapp.com/attachments/130304896581763072/400744720772628481/more-info-button.png",
                            Name = $"{match.TeamRed.Name} VS {match.TeamBlue.Name}",
                            Url = "https://osu.ppy.sh/community/matches/" + match.RoomId
                        },
                        Color = Color.Gold,
                        Url = "https://osu.ppy.sh/community/matches/" + match.RoomId
                    };
                    if (!string.IsNullOrEmpty(webhook.AuthorIcon))
                        embed.Author.IconUrl = webhook.AuthorIcon;

                    if (match.RollWinnerTeam != null)
                        embed.Title = string.Format(Properties.Resources.DiscordApi_PickRecapWithRollWinnerTitle, match.RollWinnerTeam.Name);
                    else
                        embed.Title = Properties.Resources.DiscordApi_PickRecapTitle;
                    var redTeam = string.Empty;
                    var blueTeam = string.Empty;
                    for (var i = 0; i < match.Picks.Count; i++)
                    {
                        var pick = match.Picks[i];
                        var mod = pick.Map.Tag;
                        if (string.IsNullOrEmpty(mod))
                            mod = Utils.ConvertGameModsToString(pick.Map.Mods);
                        if (pick.Team == match.TeamRed)
                            redTeam += $"-{pick.ListIndex}- __{mod}__ **{pick.Map.Beatmap.Artist.Replace("_", "\\_").Replace("*", "\\*")} - {pick.Map.Beatmap.Title.Replace("_", "\\_").Replace("*", "\\*")} [{pick.Map.Beatmap.Version.Replace("_", "\\_").Replace("*", "\\*")}]**" + Environment.NewLine;
                        if (pick.Team == match.TeamBlue)
                            blueTeam += $"-{pick.ListIndex}- __{mod}__ **{pick.Map.Beatmap.Artist.Replace("_", "\\_").Replace("*", "\\*")} - {pick.Map.Beatmap.Title.Replace("_", "\\_").Replace("*", "\\*")} [{pick.Map.Beatmap.Version.Replace("_", "\\_").Replace("*", "\\*")}]**" + Environment.NewLine;
                    }
                    if (!string.IsNullOrEmpty(redTeam))
                        embed.Fields.Add(new EmbedFieldBuilder { Name = match.TeamRed.Name, Value = redTeam });
                    if (!string.IsNullOrEmpty(blueTeam))
                        embed.Fields.Add(new EmbedFieldBuilder { Name = match.TeamBlue.Name, Value = blueTeam });
                }
                else if (match.TeamMode == Enums.TeamModes.HeadToHead)
                {
                    embed = new EmbedBuilder
                    {
                        Author = new EmbedAuthorBuilder
                        {
                            IconUrl = "https://cdn.discordapp.com/attachments/130304896581763072/400744720772628481/more-info-button.png",
                            Name = match.Name,
                            Url = "https://osu.ppy.sh/community/matches/" + match.RoomId
                        },
                        Color = Color.Gold,
                        Url = "https://osu.ppy.sh/community/matches/" + match.RoomId
                    };
                    if (!string.IsNullOrEmpty(webhook.AuthorIcon))
                        embed.Author.IconUrl = webhook.AuthorIcon;

                    if (match.RollWinnerTeam != null)
                        embed.Title = string.Format(Properties.Resources.DiscordApi_PickRecapWithRollWinnerTitle, match.RollWinnerPlayer.Name);
                    else
                        embed.Title = Properties.Resources.DiscordApi_PickRecapTitle;
                    var picks = string.Empty;
                    foreach (var pick in match.Picks)
                    {
                        var mod = pick.Map.Tag;
                        if (string.IsNullOrEmpty(mod))
                            mod = Utils.ConvertGameModsToString(pick.Map.Mods);
                        picks += $"-{(pick.Player != null ? pick.Player.Name : "???")}- __{mod}__ **{pick.Map.Beatmap.Artist.Replace("_", "\\_").Replace("*", "\\*")} - {pick.Map.Beatmap.Title.Replace("_", "\\_").Replace("*", "\\*")} [{pick.Map.Beatmap.Version.Replace("_", "\\_").Replace("*", "\\*")}]**" + Environment.NewLine;
                    }
                    if (!string.IsNullOrEmpty(picks))
                        embed.Fields.Add(new EmbedFieldBuilder { Name = Properties.Resources.DiscordApi_PickRecapPicksFieldName, Value = picks });
                }

                if (embed != null)
                {
                    embed.Footer = new EmbedFooterBuilder
                    {
                        Text = webhook.FooterText,
                        IconUrl = webhook.FooterIcon
                    };

                    try
                    {
                        using (var client = new DiscordWebhookClient(webhook.URL))
                        {
                            client.SendMessageAsync(embeds: new[] { embed.Build() }, username: webhook.Username, avatarUrl: webhook.Avatar).GetAwaiter().GetResult();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error sending pick recap to webhook " + webhook.Name);
                        localLog.Error(ex, "error sending pick recap to webhook " + webhook.Name);
                    }
                }
            }
        }

        public static void SendGameRecap(Match match)
        {
            localLog.Information("match '{match}' send game recap", match.Name);

            string getTeamIcon(Team team) => team.Id == match.TeamRed.Id ? redSquareEmoji : blueSquareEmoji;

            foreach (var webhook in match.Tournament.Webhooks.Where(x => x.GameRecap))
            {
                EmbedBuilder embed = null;
                if (match.TeamMode == Enums.TeamModes.TeamVS)
                {
                    bool teamRedWon = match.TeamRedPoints > match.TeamBluePoints;
                    string teamRedName = $"{(teamRedWon ? winnerEmoji : string.Empty)} {redSquareEmoji} {match.TeamRed.Name}";
                    string teamBlueName = $"{match.TeamBlue.Name} {blueSquareEmoji} {(!teamRedWon ? winnerEmoji : string.Empty)}";

                    embed = new EmbedBuilder
                    {
                        Author = new EmbedAuthorBuilder
                        {
                            IconUrl = webhook.AuthorIcon,
                            Name = match.Name,
                            Url = "https://osu.ppy.sh/community/matches/" + match.RoomId
                        },
                        Description = $"** {teamRedName} | {match.TeamRedPoints} - {match.TeamBluePoints} | {teamBlueName}**",
                        Color = teamRedWon ? redColor : blueColor
                    };

                    string title = match.RollWinnerTeam == null
                        ? Properties.Resources.DiscordApi_BanRecapTitle
                        : string.Format(Properties.Resources.DiscordApi_BanRecapWithRollWinnerTitle, getTeamIcon(match.RollWinnerTeam));
                    string content = match.Bans.Count > 0 ? string.Join(Environment.NewLine, match.Bans.Select(b => $"{(b.Team.Id == b.Match.TeamRed.Id ? redSquareEmoji : blueSquareEmoji)} bans `{b.Map.Tag}`")) : string.Empty;

                    embed.Fields.Add(new EmbedFieldBuilder { Name = title, Value = content });

                    title = match.FirstPickerTeam == null
                        ? Properties.Resources.DiscordApi_PickRecapTitle
                        : string.Format(Properties.Resources.DiscordApi_PickRecapWithRollWinnerTitle, getTeamIcon(match.FirstPickerTeam));
                    content = match.Bans.Count > 0 ? string.Join(Environment.NewLine, match.Picks.Select(p =>
                    {
                        int index = p.ListIndex - 1;
                        var games = match.Games.Where(m => !m.Warmup).ToArray();
                        string winner = index < games.Length ? $" > {(games[index].TeamRedWon ? redSquareEmoji : blueSquareEmoji)} wins" : string.Empty;
                        return p.Map.Mods.Contains(Enums.GameMods.TieBreaker)
                            ? $"{purpleSquareEmoji} `{p.Map.Tag}`"
                            : $"{getTeamIcon(p.Team)} picks `{p.Map.Tag}`{winner}";
                    })) : string.Empty;

                    embed.Fields.Add(new EmbedFieldBuilder { Name = title, Value = content });
                }
                else if (match.TeamMode == Enums.TeamModes.HeadToHead)
                {
                    embed = new EmbedBuilder
                    {
                        Author = new EmbedAuthorBuilder
                        {
                            IconUrl = "https://cdn0.iconfinder.com/data/icons/fighting-1/258/brawl003-512.png",
                            Name = match.Name,
                            Url = "https://osu.ppy.sh/community/matches/" + match.RoomId
                        },
                        Color = Color.Green
                    };

                    string mod = string.Empty;
                    Beatmap map;

                    if (match.Mappool != null && match.Mappool.Beatmaps.Any(x => x.Beatmap.Id == match.Games.Last().Beatmap.Id))
                    {
                        var mappoolMap = match.Mappool.Beatmaps.First(x => x.Beatmap.Id == match.Games.Last().Beatmap.Id);

                        mod = mappoolMap.Tag;
                        if (string.IsNullOrEmpty(mod))
                            mod = Utils.ConvertGameModsToString(mappoolMap.Mods);

                        map = mappoolMap.Beatmap;
                    }
                    else
                    {
                        var game = match.Games.Last();
                        mod = Utils.ConvertGameModsToString(game.Mods);
                        map = game.Beatmap;
                    }

                    var winner = match.Games.Last().Scores.OrderByDescending(x => x.Points).First();
                    embed.Title = string.Format(Properties.Resources.DiscordApi_GameRecapPlayerWinTitle, winner.Player.Name.Replace("_", "\\_").Replace("*", "\\*"), mod, winner.Points);
                    embed.ThumbnailUrl = "https://b.ppy.sh/thumb/" + map.SetId + "l.jpg";
                    embed.Description = $"**{map.Artist.Replace("_", "\\_").Replace("*", "\\*")} - {map.Title.Replace("_", "\\_").Replace("*", "\\*")} [{map.Version.Replace("_", "\\_").Replace("*", "\\*")}]**";

                    string players = string.Empty;
                    string points = string.Empty;
                    foreach (var score in match.Games.Last().Scores.OrderByDescending(x => x.Points))
                    {
                        players += score.Player.Name + Environment.NewLine;
                        points += score.Points + Environment.NewLine;
                    }
                    embed.Fields.Add(new EmbedFieldBuilder { Name = Properties.Resources.DiscordApi_GameRecapPlayerFieldName, Value = players, IsInline = true });
                    embed.Fields.Add(new EmbedFieldBuilder { Name = Properties.Resources.DiscordApi_GameRecapPointsFieldName, Value = points, IsInline = true });
                }

                if (embed != null)
                {
                    embed.Footer = new EmbedFooterBuilder
                    {
                        Text = webhook.FooterText,
                        IconUrl = webhook.FooterIcon
                    };

                    try
                    {
                        using (var client = new DiscordWebhookClient(webhook.URL, new DiscordRestConfig
                               {
                                   RestClientProvider = DefaultRestClientProvider.Create(true)
                               }))
                        {
                            client.SendMessageAsync(embeds: new[] { embed.Build() }, username: webhook.Username, avatarUrl: webhook.Avatar).GetAwaiter().GetResult();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error sending game recap to webhook " + webhook.Name);
                        localLog.Error(ex, "error sending game recap to webhook " + webhook.Name);
                    }
                }
            }
        }

        public static void SetRichPresence(string details, string state = "")
        {
            rpcClient.SetPresence(new RichPresence
            {
                Details = details,
                State = state,
                Assets = new Assets
                {
                    LargeImageKey = "logo",
                    LargeImageText = "Script-chan"
                }
            });
        }

        public static void StopRichPresence()
        {
            rpcClient.Dispose();
        }

        public static async Task SetWebhookChannel(Webhook webhook)
        {
            localLog.Information("get channel info for webhook {name}", webhook.Name);
            using (var webClient = new WebClient())
            {
                try
                {
                    var response = await webClient.DownloadStringTaskAsync(webhook.URL);
                    var data = JsonConvert.DeserializeObject<ApiWebhook>(response);
                    webhook.Guild = data.guild_id;
                    webhook.Channel = data.channel_id;
                }
                catch (Exception ex)
                {
                    localLog.Error(ex, "error retrieving webhook info");
                }
            }
        }
    }
}
