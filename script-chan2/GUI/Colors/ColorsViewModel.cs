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
    enum IrcColors
    {
        BanchoBot,
        Self
    }

    public class ColorsViewModel : Screen
    {
        #region Properties
        private IrcColors? selectedIrcColor = null;

        private Color selectedColor;
        public Color SelectedColor
        {
            get { return selectedColor; }
            set
            {
                if (value != selectedColor)
                {
                    selectedColor = value;
                    NotifyOfPropertyChange(() => SelectedColor);
                    switch (selectedIrcColor)
                    {
                        case IrcColors.BanchoBot: Settings.BanchoBotColor = value; NotifyOfPropertyChange(() => BanchoBotColor); break;
                        case IrcColors.Self: Settings.SelfColor = value; NotifyOfPropertyChange(() => SelfColor); break;
                    }
                }
            }
        }

        public SolidColorBrush BanchoBotColor
        {
            get { return new SolidColorBrush(Settings.BanchoBotColor); }
        }

        public SolidColorBrush SelfColor
        {
            get { return new SolidColorBrush(Settings.SelfColor); }
        }
        #endregion

        #region Actions
        public void ChangeBanchoBotColor()
        {
            selectedColor = Settings.BanchoBotColor;
            selectedIrcColor = IrcColors.BanchoBot;
            NotifyOfPropertyChange(() => SelectedColor);
        }

        public void ChangeSelfColor()
        {
            selectedColor = Settings.SelfColor;
            selectedIrcColor = IrcColors.Self;
            NotifyOfPropertyChange(() => SelectedColor);
        }
        #endregion
    }
}
