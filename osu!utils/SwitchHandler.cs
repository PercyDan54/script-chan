using Osu.Utils.TeamsOv;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Osu.Utils
{
    /// <summary>
    /// Switch handler which was used to check if the user has been successfully switched to the private server.
    /// </summary>
    public class SwitchHandler
    {
        #region Attributes
        List<IRCPlayerInfo> players;
        #endregion

        public SwitchHandler()
        {
            players = new List<IRCPlayerInfo>();
        }

        public bool FillPlayerList(string teamname)
        {
            var team = TeamManager.Teams.FirstOrDefault(x => x.Name == teamname);
            if(team != null)
            {
                foreach (var u in team.Players)
                {
                    players.Add(new IRCPlayerInfo(team.Name, u.Name));
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        public void FoundPlayer(string player)
        {
            if(players.Exists(x => x.Username == player))
            {
                players.First(x => x.Username == player).IsSwitched = true;
            }
        }

        public void UpdateWithNewSwitch(SwitchHandler switchhandler)
        {
            foreach (var player in switchhandler.Players)
            {
                var psave = this.Players.Find(x => x.Username == player.Username);
                if (player.IsSwitched && psave != null && !psave.IsSwitched)
                {
                    this.Players.Find(x => x.Username == player.Username).IsSwitched = true;
                }
            }
        }

        #region Properties
        public List<IRCPlayerInfo> Players { get { return players; } }
        #endregion
    }
}
