using osu_utils.DiscordModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu_discord
{
    public static class DiscordHelper
    {
        public static void SendGame(Embed embed, List<DiscordChannelEnum> channels)
        {
            Payload p = GenerateBasePayload();
            p.Embeds.Add(GenerateFooter(embed));
            foreach(DiscordChannelEnum channel in channels)
            {
                DiscordClient.GetInstance().PostMessage(p, channel);
            }
        }

        public static void SendRecap(Embed embed, List<DiscordChannelEnum> channels, string name, string id)
        {
            Payload p = GenerateBasePayload();
            embed.Author = new Author() { IconUrl = "https://cdn.discordapp.com/attachments/130304896581763072/400744720772628481/more-info-button.png", Name = name, Url = "https://osu.ppy.sh/community/matches/" + id };
            embed.URL = "https://osu.ppy.sh/community/matches/" + id;
            embed.Color = "16574595";
            p.Embeds.Add(GenerateFooter(embed));
            foreach (DiscordChannelEnum channel in channels)
            {
                DiscordClient.GetInstance().PostMessage(p, channel);
            }
        }

        public static void SendNewMatch(string id, string redname, string bluename)
        {
            Embed e = new Embed();
            e.Author = new Author() { IconUrl = "https://cdn.discordapp.com/attachments/130304896581763072/400731693192839179/plus.png", Name = string.Format("{0} VS {1}", redname, bluename), Url = "https://osu.ppy.sh/community/matches/" + id };
            e.Color = "2061822";
            e.Title = "The match has been created!";
            e.Description = "You can join the match on IRC by typing ```/join #mp_" + id + "```";
            Payload p = GenerateBasePayload();
            p.Embeds.Add(GenerateFooter(e));
            DiscordClient.GetInstance().PostMessage(p, DiscordChannelEnum.Admins);
        }

        private static Payload GenerateBasePayload()
        {
            Payload payload = new Payload();
            payload.Username = "Script-chan";
            payload.AvatarUrl = "https://cdn.discordapp.com/attachments/130304896581763072/400723356283961354/d366ce5fdd90f4e4471da04db380c378.png";
            payload.Embeds = new List<Embed>();

            return payload;
        }

        private static Embed GenerateFooter(Embed embed)
        {
            embed.Footer = new Footer();
            embed.Footer.Text = "Woah! So cool! :smirk:";
            embed.Footer.IconUrl = "https://i.imgur.com/fKL31aD.jpg";

            return embed;
        }
    }
}
