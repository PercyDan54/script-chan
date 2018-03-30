using Caliburn.Micro;
using Osu.Mvvm.Rooms.Ranking;
using Osu.Mvvm.Miscellaneous;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Osu.Ircbot;
using Osu.Scores;
using Osu.Utils;
using Osu.Utils.Info;

namespace Osu.Mvvm.Rooms.Ranking.TeamVs.ViewModels
{
    public class MultiplayerCommandsViewModel : Screen, IRankingViewModel
    {
        #region Attributes
        /// <summary>
        /// The room
        /// </summary>
        public Room room;

        /// <summary>
        /// The blue team name
        /// </summary>
        private string BlueTeamName { get; set; }

        /// <summary>
        /// The red team name
        /// </summary>
        private string RedTeamName { get; set; }

        /// <summary>
        /// The group name in case we are not in the TeamVs situation
        /// </summary>
        private string GroupName { get; set; }
        #endregion

        #region Constructors
        public MultiplayerCommandsViewModel(Room r, string bTeamname, string rTeamName) : this(r)
        {
            BlueTeamName = bTeamname;
            RedTeamName = rTeamName;
        }

        public MultiplayerCommandsViewModel(Room r)
        {
            room = r;

            // If it's a FFA room, set the groupname to the room name;
            if (room.Ranking.GetType() == typeof(Osu.Scores.HeadToHead))
                GroupName = room.Name;
        }
        #endregion

        #region Properties
        /// <summary>
        /// IsOsuAdmin property to know if we have to enable the 'switch server' button
        /// </summary>
        public bool IsOsuAdmin
        {
            get
            {
                return !string.IsNullOrEmpty(InfosHelper.UserDataInfos.IPPrivateBancho);
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Send mp start 5 to the osu room
        /// </summary>
        public void SendStart()
        {
            OsuIrcBot.GetInstancePrivate().SendMessage("#mp_" + room.Id, "!mp start 5");
        }

        /// <summary>
        /// Send mp settings to the osu room
        /// </summary>
        public void SendSettings()
        {
            OsuIrcBot.GetInstancePrivate().SendMessage("#mp_" + room.Id, "!mp settings");
        }

        /// <summary>
        /// Change the password of the osu room
        /// </summary>
        public async void SetPassword()
        {
            var pw = await Dialog.ShowInput("New password", "Enter the new password for the room");
            OsuIrcBot.GetInstancePrivate().SendMessage("#mp_" + room.Id, "!mp password " + pw);
        }

        /// <summary>
        /// Send the welcome message generated for the osu room
        /// </summary>
        public void SendWelcomeMessage()
        {
            OsuIrcBot.GetInstancePrivate().SendWelcomeMessage(room);
        }

        /// <summary>
        /// Send switch commands on osu to switch every players referenced in both teams or group
        /// </summary>
        public async void SendQuickSwitch()
        {
            if(string.IsNullOrEmpty(GroupName))
            {
                await Switch(BlueTeamName);
                await Switch(RedTeamName);
            }
            else
            {
                await Switch(GroupName);
            }
        }

        /// <summary>
        /// Send invite commands on osu to invite every players referenced in both teams or group
        /// </summary>
        public async void SendInvitations()
        {
            if (string.IsNullOrEmpty(GroupName))
            {
                await Invite(BlueTeamName);
                await Invite(RedTeamName);
            }
            else
            {
                await Invite(GroupName);
            }
        }

        /// <summary>
        /// Update the view model
        /// </summary>
        public void Update()
        {
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Switch a team to the private server by sending switch command
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private async Task Switch(string name)
        {
            SwitchHandler sh = new SwitchHandler();

            if (sh.FillPlayerList(name))
            {
                OsuIrcBot.GetInstancePublic().SwitchPlayers(sh);
            }
            else
            {
                var players = await Dialog.ShowInput(name + " not found", "Enter usernames or userids separated by semicolons");
                if(!string.IsNullOrEmpty(players))
                {
                    OsuIrcBot.GetInstancePublic().SwitchPlayers(players.Split(new char[] { ';' }).ToList());
                }
            }
        }

        /// <summary>
        /// Invite a team to the room by sending invite command
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private async Task Invite(string name)
        {
            SwitchHandler sh = new SwitchHandler();

            if (sh.FillPlayerList(name))
            {
                OsuIrcBot.GetInstancePrivate().InvitePlayers(room.Id, sh);
            }
            else
            {
                var players = await Dialog.ShowInput(name + " not found", "Enter usernames or userids separated by semicolons");
                if (!string.IsNullOrEmpty(players))
                {
                    OsuIrcBot.GetInstancePublic().InvitePlayers(room.Id, players.Split(new char[] { ';' }).ToList());
                }
            }
        }
        #endregion
    }
}
