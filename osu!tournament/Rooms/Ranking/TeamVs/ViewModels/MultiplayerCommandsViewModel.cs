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

namespace Osu.Mvvm.Rooms.Ranking.TeamVs.ViewModels
{
    public class MultiplayerCommandsViewModel : Screen, IRankingViewModel
    {
        public Room room;

        private string BlueTeamName { get; set; }
        private string RedTeamName { get; set; }
        private string GroupName { get; set; }

        public MultiplayerCommandsViewModel(Room r, string bTeamname, string rTeamName) : this(r)
        {
            BlueTeamName = bTeamname;
            RedTeamName = rTeamName;
        }

        public MultiplayerCommandsViewModel(Room r, string gName) : this(r)
        {
            GroupName = gName;
        }

        public MultiplayerCommandsViewModel(Room r)
        {
            room = r;
        }

        public void SendStart()
        {
            OsuIrcBot.GetInstancePrivate().SendMessage("#mp_" + room.Id, "!mp start 5");
        }

        public void SendSettings()
        {
            OsuIrcBot.GetInstancePrivate().SendMessage("#mp_" + room.Id, "!mp settings");
        }

        public async void SetPassword()
        {
            var pw = await Dialog.ShowInput("New password", "Enter the new password for the room");
            OsuIrcBot.GetInstancePrivate().SendMessage("#mp_" + room.Id, "!mp password " + pw);
        }

        public void SendWelcomeMessage()
        {
            OsuIrcBot.GetInstancePrivate().SendWelcomeMessage(room);
        }

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

        public void Update()
        {
        }

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
    }
}
