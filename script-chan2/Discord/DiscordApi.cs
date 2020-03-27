using Discord;
using Discord.Webhook;
using script_chan2.DataTypes;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace script_chan2.Discord
{
    public static class DiscordApi
    {
        public static void SendMatchCreated(Match match)
        {
            Log.Information("Discord: match '{match}' send room created", match.Name);
            var embed = new EmbedBuilder
            {
                Author = new EmbedAuthorBuilder
                {
                    IconUrl = "https://cdn.discordapp.com/attachments/130304896581763072/400731693192839179/plus.png",
                    Name = match.Name,
                    Url = "https://osu.ppy.sh/community/matches/" + match.RoomId
                },
                Color = Color.Blue,
                Title = "The match has been created!",
                Description = $"You can join the match on IRC by typing ```/join #mp_{match.RoomId}```",
                Footer = new EmbedFooterBuilder
                {
                    Text = "Woah! So cool! :smirk:",
                    IconUrl = "https://i.imgur.com/fKL31aD.jpg"
                }
            };

            if (embed != null)
            {
                foreach (var webhook in match.Tournament.Webhooks.Where(x => x.MatchCreated))
                {
                    using (var client = new DiscordWebhookClient(webhook.URL))
                    {
                        client.SendMessageAsync(embeds: new[] { embed.Build() }, username: "Script-chan", avatarUrl: "https://cdn.discordapp.com/attachments/130304896581763072/400723356283961354/d366ce5fdd90f4e4471da04db380c378.png").GetAwaiter().GetResult();
                    }
                }
            }
        }

        public static void SendMatchBanRecap(Match match)
        {
            Log.Information("Discord: match '{match}' send ban recap", match.Name);
            EmbedBuilder embed = null;
            if (match.TeamMode == Enums.TeamModes.TeamVS)
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
                        redTeam += $"__{mod}__ **{ban.Map.Beatmap.Artist.Replace("_", "__").Replace("*", "\\*")} - {ban.Map.Beatmap.Title.Replace("_", "__").Replace("*", "\\*")} [{ban.Map.Beatmap.Version.Replace("_", "__").Replace("*", "\\*")}]**" + Environment.NewLine;
                    if (ban.Team == match.TeamBlue)
                        blueTeam += $"__{mod}__ **{ban.Map.Beatmap.Artist.Replace("_", "__").Replace("*", "\\*")} - {ban.Map.Beatmap.Title.Replace("_", "__").Replace("*", "\\*")} [{ban.Map.Beatmap.Version.Replace("_", "__").Replace("*", "\\*")}]**" + Environment.NewLine;
                }
                if (!string.IsNullOrEmpty(redTeam))
                    embed.Fields.Add(new EmbedFieldBuilder { Name = match.TeamRed.Name, Value = redTeam });
                if (!string.IsNullOrEmpty(blueTeam))
                    embed.Fields.Add(new EmbedFieldBuilder { Name = match.TeamBlue.Name, Value = blueTeam });

                embed.Footer = new EmbedFooterBuilder
                {
                    Text = "Woah! So cool! :smirk:",
                    IconUrl = "https://i.imgur.com/fKL31aD.jpg"
                };
            }
            else
            {
                //TODO!
            }

            if (embed != null)
            {
                foreach (var webhook in match.Tournament.Webhooks.Where(x => x.BanRecap))
                {
                    using (var client = new DiscordWebhookClient(webhook.URL))
                    {
                        client.SendMessageAsync(embeds: new[] { embed.Build() }, username: "Script-chan", avatarUrl: "https://cdn.discordapp.com/attachments/130304896581763072/400723356283961354/d366ce5fdd90f4e4471da04db380c378.png").GetAwaiter().GetResult();
                    }
                }
            }
        }

        public static void SendMatchPickRecap(Match match)
        {
            Log.Information("Discord: match '{match}' send pick recap", match.Name);
            EmbedBuilder embed = null;
            if (match.TeamMode == Enums.TeamModes.TeamVS)
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
                var redTeam = "";
                var blueTeam = "";
                for (var i = 0; i < match.Picks.Count; i++)
                {
                    var pick = match.Picks[i];
                    var mod = pick.Map.Tag;
                    if (string.IsNullOrEmpty(mod))
                        mod = Utils.ConvertGameModsToString(pick.Map.Mods);
                    if (pick.Team == match.TeamRed)
                        redTeam += $"-{i + 1}- __{mod}__ **{pick.Map.Beatmap.Artist.Replace("_", "__").Replace("*", "\\*")} - {pick.Map.Beatmap.Title.Replace("_", "__").Replace("*", "\\*")} [{pick.Map.Beatmap.Version.Replace("_", "__").Replace("*", "\\*")}]**" + Environment.NewLine;
                    if (pick.Team == match.TeamBlue)
                        blueTeam += $"-{i + 1}- __{mod}__ **{pick.Map.Beatmap.Artist.Replace("_", "__").Replace("*", "\\*")} - {pick.Map.Beatmap.Title.Replace("_", "__").Replace("*", "\\*")} [{pick.Map.Beatmap.Version.Replace("_", "__").Replace("*", "\\*")}]**" + Environment.NewLine;
                }
                if (!string.IsNullOrEmpty(redTeam))
                    embed.Fields.Add(new EmbedFieldBuilder { Name = match.TeamRed.Name, Value = redTeam });
                if (!string.IsNullOrEmpty(blueTeam))
                    embed.Fields.Add(new EmbedFieldBuilder { Name = match.TeamBlue.Name, Value = blueTeam });

                embed.Footer = new EmbedFooterBuilder
                {
                    Text = "Woah! So cool! :smirk:",
                    IconUrl = "https://i.imgur.com/fKL31aD.jpg"
                };
            }
            else
            {
                //TODO!
            }

            if (embed != null)
            {
                foreach (var webhook in match.Tournament.Webhooks.Where(x => x.PickRecap))
                {
                    using (var client = new DiscordWebhookClient(webhook.URL))
                    {
                        client.SendMessageAsync(embeds: new[] { embed.Build() }, username: "Script-chan", avatarUrl: "https://cdn.discordapp.com/attachments/130304896581763072/400723356283961354/d366ce5fdd90f4e4471da04db380c378.png").GetAwaiter().GetResult();
                    }
                }
            }
        }

        public static void SendGameRecap(Match match)
        {
            Log.Information("Discord: match '{match}' send game recap", match.Name);
            EmbedBuilder embed = null;
            if (match.TeamMode == Enums.TeamModes.TeamVS)
            {
                embed = new EmbedBuilder
                {
                    Author = new EmbedAuthorBuilder
                    {
                        IconUrl = "https://cdn0.iconfinder.com/data/icons/fighting-1/258/brawl003-512.png",
                        Name = match.Name,
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
                    embed.ImageUrl = "https://78.media.tumblr.com/b94193615145d12bfb64aa77b677269e/tumblr_njzqukOpBP1ti1gm1o1_500.gif";
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

                embed.Footer = new EmbedFooterBuilder
                {
                    Text = "Woah! So cool! :smirk:",
                    IconUrl = "https://i.imgur.com/fKL31aD.jpg"
                };
            }
            else
            {
                //TODO!
            }

            if (embed != null)
            {
                foreach (var webhook in match.Tournament.Webhooks.Where(x => x.GameRecap))
                {
                    using (var client = new DiscordWebhookClient(webhook.URL))
                    {
                        client.SendMessageAsync(embeds: new[] { embed.Build() }, username: "Script-chan", avatarUrl: "https://cdn.discordapp.com/attachments/130304896581763072/400723356283961354/d366ce5fdd90f4e4471da04db380c378.png").GetAwaiter().GetResult();
                    }
                }
            }
        }
    }
}
