using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Osu.Utils
{
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
            Cache cache = Cache.GetCache("osu!teams.db");
            var t = cache.GetArray(teamname, new string[1]);
            if(t.Length != 1)
            {
                foreach (var u in t)
                {
                    players.Add(new IRCPlayerInfo(teamname, u));
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
