using Caliburn.Micro;
using MaterialDesignThemes.Wpf;
using script_chan2.DataTypes;
using Serilog;
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
        #endregion

        #region Actions
        public async void Edit()
        {
            Log.Information("ColorListItemViewModel: edit color '{name}'", userColor.Key);
            var model = new EditColorDialogViewModel(userColor.Color);
            var view = ViewLocator.LocateForModel(model, null, null);
            ViewModelBinder.Bind(model, view, null);

            var result = Convert.ToBoolean(await DialogHost.Show(view));

            if (result)
            {
                userColor.Color = model.Color;
                NotifyOfPropertyChange(() => Color);
                Settings.SaveConfig();
                Events.Aggregator.PublishOnUIThread("UpdateColors");
            }
        }
        #endregion
    }
}
