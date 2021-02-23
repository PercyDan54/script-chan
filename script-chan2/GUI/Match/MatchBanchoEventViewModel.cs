using Caliburn.Micro;
using script_chan2.Enums;
using System;
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

        public string EventText
        {
            get
            {
                var text = $"[{eventTime.ToString(Properties.Resources.MatchBanchoEventViewModel_TimeFormat)}] ";
                if (type == MatchBanchoEvents.AllPlayersFinished)
                    text += Properties.Resources.MatchBanchoEventViewModel_AllPlayersFinishedText;
                if (type == MatchBanchoEvents.AllPlayersReady)
                    text += Properties.Resources.MatchBanchoEventViewModel_AllPlayersReadyText;
                if (type == MatchBanchoEvents.PlayerRoll)
                    text += string.Format(Properties.Resources.MatchBanchoEventViewModel_RollText, user, data);
                return text;
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
