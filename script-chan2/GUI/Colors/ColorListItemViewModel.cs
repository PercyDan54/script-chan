using Caliburn.Micro;
using script_chan2.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace script_chan2.GUI
{
    public class ColorListItemViewModel : Screen
    {
        #region Constructor
        public ColorListItemViewModel(UserColor userColor)
        {
            this.userColor = userColor;
        }
        #endregion

        #region Properties
        private UserColor userColor;

        public string Name
        {
            get { return userColor.Key; }
        }

        public SolidColorBrush Color
        {
            get { return new SolidColorBrush(userColor.Color); }
        }

        private Color editColor;
        public Color EditColor
        {
            get { return editColor; }
            set
            {
                if (value != editColor)
                {
                    editColor = value;
                    NotifyOfPropertyChange(() => EditColor);
                    NotifyOfPropertyChange(() => EditPreviewColor);
                }
            }
        }

        public SolidColorBrush EditPreviewColor
        {
            get { return new SolidColorBrush(EditColor); }
        }
        #endregion

        #region Actions
        public void Edit()
        {
            EditColor = userColor.Color;
        }

        public void Save()
        {
            userColor.Color = EditColor;
            NotifyOfPropertyChange(() => Color);
            Settings.SaveConfig();
            Events.Aggregator.PublishOnUIThread("UpdateColors");
        }
        #endregion
    }
}
