using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace script_chan2.GUI
{
    public class TournamentPointListItemViewModel : Screen
    {
        #region Constructor
        public TournamentPointListItemViewModel(int place, int points)
        {
            Place = place;
            Points = points;
        }
        #endregion

        #region Properties
        public int Place;

        public int Points;

        public string Name
        {
            get { return $"{Place}. {Points}"; }
        }

        private bool hover = false;
        public SolidColorBrush Background
        {
            get
            {
                if (hover)
                    return Brushes.LightGray;
                return Brushes.Transparent;
            }
        }
        #endregion

        #region Actions
        public void MouseEnter()
        {
            hover = true;
            NotifyOfPropertyChange(() => Background);
        }

        public void MouseLeave()
        {
            hover = false;
            NotifyOfPropertyChange(() => Background);
        }
        #endregion
    }
}
