using Caliburn.Micro;
using script_chan2.DataTypes;

namespace script_chan2.GUI
{
    public class ColorsViewModel : Screen
    {
        #region Color list
        public BindableCollection<ColorListItemViewModel> ColorsViews
        {
            get
            {
                var list = new BindableCollection<ColorListItemViewModel>();
                foreach (var userColor in Settings.UserColors)
                    list.Add(new ColorListItemViewModel(userColor));
                return list;
            }
        }
        #endregion
    }
}
