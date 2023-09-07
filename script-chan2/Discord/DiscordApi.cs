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

namespace script_chan2.Discord
{
    public static class DiscordApi
    {
        private static ILogger localLog = Log.ForContext(typeof(DiscordApi));

        private static DiscordRpcClient rpcClient;

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
                        embed.Title = string.Format(Properties.Resources.DiscordApi_BanRecapWithRollWinnerTitle, match.RollWinnerTeam.Name);
                    else
                        embed.Title = Properties.Resources.DiscordApi_BanRecapTitle;
                    var redTeam = "";
                    var blueTeam = "";
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
                        var bans = "";
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
                    var redTeam = "";
                    var blueTeam = "";
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
                    var picks = "";
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
            foreach (var webhook in match.Tournament.Webhooks.Where(x => x.GameRecap))
            {
                EmbedBuilder embed = null;
                if (match.TeamMode == Enums.TeamModes.TeamVS)
                {
                    embed = new EmbedBuilder
                    {
                        Author = new EmbedAuthorBuilder
                        {
                            IconUrl = "https://cdn0.iconfinder.com/data/icons/fighting-1/258/brawl003-512.png",
                            Name = $"{match.TeamRed.Name} VS {match.TeamBlue.Name}",
                            Url = "https://osu.ppy.sh/community/matches/" + match.RoomId
                        }
                    };
                    if (!string.IsNullOrEmpty(webhook.AuthorIcon))
                        embed.Author.IconUrl = webhook.AuthorIcon;

                    var map = match.Picks.Last();

                    bool pickingTeamWon = false;
                    var teamRedScore = 0;
                    var teamBlueScore = 0;
                    foreach (var score in match.Games.Last().Scores)
                    {
                        if (score.Passed)
                        {
                            if (match.TeamRed.Players.Any(x => x.Id == score.Player.Id))
                                teamRedScore += score.Points;
                            if (match.TeamBlue.Players.Any(x => x.Id == score.Player.Id))
                                teamBlueScore += score.Points;
                        }
                    }
                    if (teamRedScore > teamBlueScore && map.Team == match.TeamRed)
                        pickingTeamWon = true;
                    if (teamBlueScore > teamRedScore && map.Team == match.TeamBlue)
                        pickingTeamWon = true;

                    var lastGame = match.TeamRedPoints * 2 >= match.BO || match.TeamBluePoints * 2 >= match.BO;

                    var mod = map.Map.Tag;
                    if (string.IsNullOrEmpty(mod))
                        mod = Utils.ConvertGameModsToString(map.Map.Mods);

                    var orderedScores = match.Games.Last().Scores.OrderByDescending(x => x.Points);
                    var mvpScores = orderedScores.Where(x => x.Points == orderedScores.First().Points);

                    if (teamRedScore == teamBlueScore)
                    {
                        embed.Title = Properties.Resources.DiscordApi_GameRecapTeamDrawTitle;
                    }
                    else
                    {
                        if (pickingTeamWon)
                            embed.Title = string.Format(Properties.Resources.DiscordApi_GameRecapTeamWinTitle, map.Team.Name, mod, string.Format("{0:n0}", Math.Abs(teamRedScore - teamBlueScore)));
                        else
                            embed.Title = string.Format(Properties.Resources.DiscordApi_GameRecapLostTitle, map.Team.Name, mod, string.Format("{0:n0}", Math.Abs(teamRedScore - teamBlueScore)));
                    }
                    
                    if (teamRedScore == teamBlueScore)
                    {
                        embed.ThumbnailUrl = "https://cdn.discordapp.com/attachments/696350776750243840/957382088733110282/equals_PNG19.png";
                        embed.Color = Color.Orange;
                    }
                    else if (lastGame)
                    {
                        embed.ThumbnailUrl = "https://cdn.discordapp.com/attachments/130304896581763072/411660079771811870/crown.png";
                        embed.Color = Color.Purple;
                        embed.ImageUrl = webhook.WinImage;
                    }
                    else if (pickingTeamWon)
                    {
                        embed.ThumbnailUrl = "https://cdn.discordapp.com/attachments/130304896581763072/400388818127290369/section-pass.png";
                        embed.Color = Color.Green;
                    }
                    else
                    {
                        embed.ThumbnailUrl = "https://cdn.discordapp.com/attachments/130304896581763072/400388814213873666/section-fail.png";
                        embed.Color = Color.Red;
                    }
                    embed.Description = $"**{map.Map.Beatmap.Artist.Replace("_", "\\_").Replace("*", "\\*")} - {map.Map.Beatmap.Title.Replace("_", "\\_").Replace("*", "\\*")} [{map.Map.Beatmap.Version.Replace("_", "\\_").Replace("*", "\\*")}]**";
                    embed.Fields.Add(new EmbedFieldBuilder { Name = match.TeamRed.Name, Value = match.TeamRedPoints, IsInline = true });
                    embed.Fields.Add(new EmbedFieldBuilder { Name = match.TeamBlue.Name, Value = match.TeamBluePoints, IsInline = true });
                    var mvps = "";
                    foreach (var score in mvpScores)
                    {
                        mvps += string.Format(Properties.Resources.DiscordApi_GameRecapMVPFieldValue, score.Player.Country.ToLower(), score.Player.Name.Replace("_", "\\_"), string.Format("{0:n0}", score.Points)) + Environment.NewLine;
                    }
                    embed.Fields.Add(new EmbedFieldBuilder { Name = Properties.Resources.DiscordApi_GameRecapMVPFieldName, Value = mvps });
                    if (teamRedScore == teamBlueScore)
                        embed.Fields.Add(new EmbedFieldBuilder { Name = Properties.Resources.DiscordApi_GameRecapStatusFieldTitle, Value = Properties.Resources.MatchViewModel_MapDrawMessage });
                    else if (lastGame)
                        embed.Fields.Add(new EmbedFieldBuilder { Name = Properties.Resources.DiscordApi_GameRecapStatusFieldTitle, Value = string.Format(Properties.Resources.DiscordApi_GameRecapStatusFieldTeamMatchWin, match.TeamRedPoints * 2 >= match.BO ? match.TeamRed.Name : match.TeamBlue.Name) });
                    else
                        embed.Fields.Add(new EmbedFieldBuilder { Name = Properties.Resources.DiscordApi_GameRecapStatusFieldTitle, Value = string.Format(Properties.Resources.DiscordApi_GameRecapStatusFieldTeamNextPick, map.Team == match.TeamRed ? match.TeamBlue.Name : match.TeamRed.Name) });
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

                    string mod = "";
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

                    string players = "";
                    string points = "";
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
                        using (var client = new DiscordWebhookClient(webhook.URL))
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
            rpcClient.SetPresence(new RichPresence()
            {
                Details = details,
                State = state,
                Assets = new Assets()
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
