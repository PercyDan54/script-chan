using Caliburn.Micro;
using script_chan2.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace script_chan2.GUI
{
    public class MatchViewModel : Screen
    {
        #region Constructor
        public MatchViewModel(Match match)
        {
            this.match = match;
        }
        #endregion

        #region Events
        protected override void OnDeactivate(bool close)
        {
            MatchList.OpenedMatches.Remove(match);
        }
        #endregion

        #region Properties
        private Match match;

        public string WindowTitle
        {
            get { return match.Name; }
        }
        #endregion
    }
}
