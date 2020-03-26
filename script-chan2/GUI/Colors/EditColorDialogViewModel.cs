using Caliburn.Micro;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace script_chan2.GUI
{
    public class EditColorDialogViewModel : Screen
    {
        #region Constructor
        public EditColorDialogViewModel(Color color)
        {
            this.color = color;
        }
        #endregion

        #region Properties
        private Color color;
        public Color Color
        {
            get { return color; }
            set
            {
                if (value != color)
                {
                    color = value;
                    NotifyOfPropertyChange(() => Color);
                    NotifyOfPropertyChange(() => PreviewColor);
                }
            }
        }

        public SolidColorBrush PreviewColor
        {
            get { return new SolidColorBrush(Color); }
        }
        #endregion

        #region Actions
        public void DialogEscape()
        {
            DialogHost.CloseDialogCommand.Execute(false, null);
        }
        #endregion
    }
}
