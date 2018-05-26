using osu_utils.DiscordModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu_discord
{
    /// <summary>
    /// Static class helping to generate Payloads and send messages for different events
    /// </summary>
    public static class DiscordHelper
    {
        #region Public static methods

        /// <summary>
        /// Verify if discord client is enabled
        /// </summary>
        /// <returns></returns>
        public static bool IsEnabled()
        {
            return DiscordClient.GetInstance().IsEnabled;
        }

        /// <summary>
        /// Generate payload and post message when a game has been played
        /// </summary>
        /// <param name="embed">The embed generated with some infos from ranking classes</param>
        /// <param name="channels">The list of channels we need to send the payload</param>
        public static void SendGame(Embed embed, List<DiscordChannelEnum> channels)
        {
            // Generate the base of the payload
            Payload p = GenerateBasePayload();

            // Add the footer
            p.Embeds.Add(GenerateFooter(embed));

            // Send messages
            foreach(DiscordChannelEnum channel in channels)
            {
                DiscordClient.GetInstance().PostMessage(p, channel);
            }
        }

        /// <summary>
        /// Generate payload and post messages when the pick recap needs to be sent
        /// </summary>
        /// <param name="embed">The embed generated with some infos from the map picker class</param>
        /// <param name="channels">The list of channels we need to send the payload</param>
        /// <param name="name">The room name</param>
        /// <param name="id">The room id</param>
        public static void SendRecap(Embed embed, List<DiscordChannelEnum> channels, string name, string id)
        {
            // Generate the base of the payload
            Payload p = GenerateBasePayload();
            
            // Fill some informations
            embed.Author = new Author() { IconUrl = "https://cdn.discordapp.com/attachments/130304896581763072/400744720772628481/more-info-button.png", Name = name, Url = "https://osu.ppy.sh/community/matches/" + id };
            embed.URL = "https://osu.ppy.sh/community/matches/" + id;
            embed.Color = "16574595";

            // Generate the footer
            p.Embeds.Add(GenerateFooter(embed));

            // Send messages
            foreach (DiscordChannelEnum channel in channels)
            {
                DiscordClient.GetInstance().PostMessage(p, channel);
            }
        }

        /// <summary>
        /// Generate payload and post messages when the match has been created through the application on osu!
        /// </summary>
        /// <param name="id">the room id</param>
        /// <param name="redname">the name of the read team</param>
        /// <param name="bluename">the name of the blue team</param>
        public static void SendNewMatch(string id, string redname, string bluename)
        {
            // Create the embed
            Embed e = new Embed();
            e.Author = new Author() { IconUrl = "https://cdn.discordapp.com/attachments/130304896581763072/400731693192839179/plus.png", Name = $"{redname} VS {bluename}", Url = "https://osu.ppy.sh/community/matches/" + id };
            e.Color = "2061822";
            e.Title = Osu.Utils.Properties.Resources.Discord_NewMatchTitle;
            e.Description = string.Format(Osu.Utils.Properties.Resources.Discord_NewMatchDescription, id);

            // Generate the base of the payload
            Payload p = GenerateBasePayload();

            // Generate the footer
            p.Embeds.Add(GenerateFooter(e));

            // Send message for the admin webhook
            DiscordClient.GetInstance().PostMessage(p, DiscordChannelEnum.Admins);
        }
        #endregion

        #region Private static methods
        /// <summary>
        /// Generating the base of the payload with the username, avatar...
        /// </summary>
        /// <returns></returns>
        private static Payload GenerateBasePayload()
        {
            // Create the payload by filling informations
            Payload payload = new Payload();
            payload.Username = "Script-chan";
            payload.AvatarUrl = "https://cdn.discordapp.com/attachments/130304896581763072/400723356283961354/d366ce5fdd90f4e4471da04db380c378.png";
            payload.Embeds = new List<Embed>();

            return payload;
        }

        /// <summary>
        /// Generate the generic footer of the embed
        /// </summary>
        /// <param name="embed">the embed</param>
        /// <returns>the embed containing the generic footer</returns>
        private static Embed GenerateFooter(Embed embed)
        {
            // Create the footer
            embed.Footer = new Footer();
            embed.Footer.Text = Osu.Utils.Properties.Resources.Discord_Footer;
            embed.Footer.IconUrl = "https://i.imgur.com/fKL31aD.jpg";

            return embed;
        }
        #endregion
    }
}
