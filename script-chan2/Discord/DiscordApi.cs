using Discord;
using Discord.Webhook;
using script_chan2.DataTypes;
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

            SendEmbed(match, embed);
        }

        private static void SendEmbed(Match match, EmbedBuilder embed)
        {
            foreach (var webhook in match.Tournament.Webhooks)
            {
                using (var client = new DiscordWebhookClient(webhook.URL))
                {
                    client.SendMessageAsync(text: "", embeds: new[] { embed.Build() }, username: "Script-chan", avatarUrl: "https://cdn.discordapp.com/attachments/130304896581763072/400723356283961354/d366ce5fdd90f4e4471da04db380c378.png").GetAwaiter().GetResult();
                }
            }
        }
    }
}
