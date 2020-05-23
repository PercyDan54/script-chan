using Caliburn.Micro;
using script_chan2.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace script_chan2.GUI
{
    public class MatchBanchoEventViewModel : Screen
    {
        #region Constructor
        public MatchBanchoEventViewModel(MatchBanchoEvents type, DateTime eventTime, string user = "", TeamColors? team = null, string data = "")
        {
            this.type = type;
            this.eventTime = eventTime;
            this.user = user;
            this.team = team;
            this.data = data;
        }
        #endregion

        #region Properties
        private MatchBanchoEvents type;

        private DateTime eventTime;

        private string user;

        private TeamColors? team;

        private string data;

        public string Name
        {
            get
            {
                var name = $"[{eventTime.ToString("HH:mm")}]";
                if (type == MatchBanchoEvents.AllPlayersFinished)
                    name += " All players finished";
                if (type == MatchBanchoEvents.AllPlayersReady)
                    name += " All players ready";
                if (type == MatchBanchoEvents.PlayerRoll)
                    name += $" {user} rolled {data}";
                return name;
            }
        }

        public Brush FontColor
        {
            get
            {
                if (team == TeamColors.Red)
                    return Brushes.Red;
                if (team == TeamColors.Blue)
                    return Brushes.DeepSkyBlue;
                return Brushes.White;
            }
        }
        #endregion
    }
}
