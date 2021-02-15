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
                    Title = "The match has been created!",
                    Description = $"You can join the match on IRC by typing ```/join #mp_{match.RoomId}```"
                };

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

                    using (var client = new DiscordWebhookClient(webhook.URL))
                    {
                        client.SendMessageAsync(embeds: new[] { embed.Build() }, username: webhook.Username, avatarUrl: webhook.Avatar).GetAwaiter().GetResult();
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
                        Url = "https://osu.ppy.sh/community/matches/" + match.RoomId,
                        Title = "Ban Recap " + (match.RollWinnerTeam != null ? "(Roll Winner: " + match.RollWinnerTeam.Name + ")" : ""),
                    };
                    var redTeam = "";
                    var blueTeam = "";
                    foreach (var ban in match.Bans)
                    {
                        var mod = ban.Map.Tag;
                        if (string.IsNullOrEmpty(mod))
                            mod = Utils.ConvertGameModsToString(ban.Map.Mods);
                        if (ban.Team == match.TeamRed)
                            redTeam += $"-{ban.ListIndex}- __{mod}__ **{ban.Map.Beatmap.Artist.Replace("_", "__").Replace("*", "\\*")} - {ban.Map.Beatmap.Title.Replace("_", "__").Replace("*", "\\*")} [{ban.Map.Beatmap.Version.Replace("_", "__").Replace("*", "\\*")}]**" + Environment.NewLine;
                        if (ban.Team == match.TeamBlue)
                            blueTeam += $"-{ban.ListIndex}- __{mod}__ **{ban.Map.Beatmap.Artist.Replace("_", "__").Replace("*", "\\*")} - {ban.Map.Beatmap.Title.Replace("_", "__").Replace("*", "\\*")} [{ban.Map.Beatmap.Version.Replace("_", "__").Replace("*", "\\*")}]**" + Environment.NewLine;
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
                        Url = "https://osu.ppy.sh/community/matches/" + match.RoomId,
                        Title = "Ban Recap " + (match.RollWinnerPlayer != null ? "(Roll Winner: " + match.RollWinnerPlayer.Name + ")" : ""),
                    };
                    foreach (var player in match.Players)
                    {
                        var bans = "";
                        foreach (var ban in match.Bans.Where(x => x.Player == player.Key))
                        {
                            var mod = ban.Map.Tag;
                            if (string.IsNullOrEmpty(mod))
                                mod = Utils.ConvertGameModsToString(ban.Map.Mods);
                            bans += $"__{mod}__ **{ban.Map.Beatmap.Artist.Replace("_", "__").Replace("*", "\\*")} - {ban.Map.Beatmap.Title.Replace("_", "__").Replace("*", "\\*")} [{ban.Map.Beatmap.Version.Replace("_", "__").Replace("*", "\\*")}]**" + Environment.NewLine;
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

                    using (var client = new DiscordWebhookClient(webhook.URL))
                    {
                        client.SendMessageAsync(embeds: new[] { embed.Build() }, username: webhook.Username, avatarUrl: webhook.Avatar).GetAwaiter().GetResult();
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
                        Url = "https://osu.ppy.sh/community/matches/" + match.RoomId,
                        Title = "Pick Recap " + (match.RollWinnerPlayer != null ? "(Roll Winner: " + match.RollWinnerPlayer.Name + ")" : ""),
                    };
                    var redTeam = "";
                    var blueTeam = "";
                    for (var i = 0; i < match.Picks.Count; i++)
                    {
                        var pick = match.Picks[i];
                        var mod = pick.Map.Tag;
                        if (string.IsNullOrEmpty(mod))
                            mod = Utils.ConvertGameModsToString(pick.Map.Mods);
                        if (pick.Team == match.TeamRed)
                            redTeam += $"-{pick.ListIndex}- __{mod}__ **{pick.Map.Beatmap.Artist.Replace("_", "__").Replace("*", "\\*")} - {pick.Map.Beatmap.Title.Replace("_", "__").Replace("*", "\\*")} [{pick.Map.Beatmap.Version.Replace("_", "__").Replace("*", "\\*")}]**" + Environment.NewLine;
                        if (pick.Team == match.TeamBlue)
                            blueTeam += $"-{pick.ListIndex}- __{mod}__ **{pick.Map.Beatmap.Artist.Replace("_", "__").Replace("*", "\\*")} - {pick.Map.Beatmap.Title.Replace("_", "__").Replace("*", "\\*")} [{pick.Map.Beatmap.Version.Replace("_", "__").Replace("*", "\\*")}]**" + Environment.NewLine;
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
                        Url = "https://osu.ppy.sh/community/matches/" + match.RoomId,
                        Title = "Pick Recap " + (match.RollWinnerTeam != null ? "(Roll Winner: " + match.RollWinnerTeam.Name + ")" : ""),
                    };
                    var picks = "";
                    foreach (var pick in match.Picks)
                    {
                        var mod = pick.Map.Tag;
                        if (string.IsNullOrEmpty(mod))
                            mod = Utils.ConvertGameModsToString(pick.Map.Mods);
                        picks += $"-{(pick.Player != null ? pick.Player.Name : "???")}- __{mod}__ **{pick.Map.Beatmap.Artist.Replace("_", "__").Replace("*", "\\*")} - {pick.Map.Beatmap.Title.Replace("_", "__").Replace("*", "\\*")} [{pick.Map.Beatmap.Version.Replace("_", "__").Replace("*", "\\*")}]**" + Environment.NewLine;
                    }
                    if (!string.IsNullOrEmpty(picks))
                        embed.Fields.Add(new EmbedFieldBuilder { Name = "Picks", Value = picks });
                }

                if (embed != null)
                {
                    embed.Footer = new EmbedFooterBuilder
                    {
                        Text = webhook.FooterText,
                        IconUrl = webhook.FooterIcon
                    };

                    using (var client = new DiscordWebhookClient(webhook.URL))
                    {
                        client.SendMessageAsync(embeds: new[] { embed.Build() }, username: webhook.Username, avatarUrl: webhook.Avatar).GetAwaiter().GetResult();
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

                    var mvp = match.Games.Last().Scores.OrderByDescending(x => x.Points).First();

                    embed.Title = $"{map.Team.Name} {(pickingTeamWon ? "won" : "lost")} their __{mod}__ pick by {string.Format("{0:n0}", Math.Abs(teamRedScore - teamBlueScore))}";
                    if (lastGame)
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
                    embed.Description = $"**{map.Map.Beatmap.Artist.Replace("_", "__").Replace("*", "\\*")} - {map.Map.Beatmap.Title.Replace("_", "__").Replace("*", "\\*")} [{map.Map.Beatmap.Version.Replace("_", "__").Replace("*", "\\*")}]**";
                    embed.Fields.Add(new EmbedFieldBuilder { Name = match.TeamRed.Name, Value = match.TeamRedPoints, IsInline = true });
                    embed.Fields.Add(new EmbedFieldBuilder { Name = match.TeamBlue.Name, Value = match.TeamBluePoints, IsInline = true });
                    embed.Fields.Add(new EmbedFieldBuilder { Name = "MVP", Value = $":flag_{mvp.Player.Country.ToLower()}: **{mvp.Player.Name.Replace("_", "__")}** with {string.Format("{0:n0}", mvp.Points)} points" });
                    if (lastGame)
                        embed.Fields.Add(new EmbedFieldBuilder { Name = "Status", Value = $"{(match.TeamRedPoints * 2 >= match.BO ? match.TeamRed.Name : match.TeamBlue.Name)} wins the match :clap:" });
                    else
                        embed.Fields.Add(new EmbedFieldBuilder { Name = "Status", Value = "Next team to pick: " + (map.Team == match.TeamRed ? match.TeamBlue.Name : match.TeamRed.Name) + " :loudspeaker:" });
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
                    embed.Title = $"{winner.Player.Name.Replace("_", "__").Replace("*", "\\*")} won the __{mod}__ pick with {winner.Points} points";
                    embed.ThumbnailUrl = "https://b.ppy.sh/thumb/" + map.SetId + "l.jpg";
                    embed.Description = $"**{map.Artist.Replace("_", "__").Replace("*", "\\*")} - {map.Title.Replace("_", "__").Replace("*", "\\*")} [{map.Version.Replace("_", "__").Replace("*", "\\*")}]**";

                    string players = "";
                    string points = "";
                    foreach (var score in match.Games.Last().Scores.OrderByDescending(x => x.Points))
                    {
                        players += score.Player.Name + Environment.NewLine;
                        points += score.Points + Environment.NewLine;
                    }
                    embed.Fields.Add(new EmbedFieldBuilder { Name = "Player", Value = players, IsInline = true });
                    embed.Fields.Add(new EmbedFieldBuilder { Name = "Points", Value = points, IsInline = true });
                }

                if (embed != null)
                {
                    embed.Footer = new EmbedFooterBuilder
                    {
                        Text = webhook.FooterText,
                        IconUrl = webhook.FooterIcon
                    };

                    using (var client = new DiscordWebhookClient(webhook.URL))
                    {
                        client.SendMessageAsync(embeds: new[] { embed.Build() }, username: webhook.Username, avatarUrl: webhook.Avatar).GetAwaiter().GetResult();
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
