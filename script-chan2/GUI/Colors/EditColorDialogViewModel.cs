using Caliburn.Micro;
using MaterialDesignThemes.Wpf;
using System.Windows.Media;

namespace script_chan2.GUI
{
    public class EditColorDialogViewModel : Screen
    {
        #region Constructor
        public EditColorDialogViewModel(Color color, string name)
        {
            this.color = color;
            this.name = name;
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

        private string name;
        public string Name
        {
            get { return name; }
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
