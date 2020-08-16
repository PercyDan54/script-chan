using Caliburn.Micro;
using MaterialDesignThemes.Wpf;
using script_chan2.DataTypes;
using Serilog;
using System;
using System.Linq;
using System.Windows.Media;

namespace script_chan2.GUI
{
    public class ColorsViewModel : Screen
    {
        private ILogger localLog = Log.ForContext<ColorsViewModel>();

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

        #region Actions
        public async void SetDefaultValues()
        {
            localLog.Information("set color default values dialog open");
            var model = new DefaultColorValuesDialogViewModel();
            var view = ViewLocator.LocateForModel(model, null, null);
            ViewModelBinder.Bind(model, view, null);

            var result = Convert.ToBoolean(await DialogHost.Show(view, "MainDialogHost"));

            if (result)
            {
                localLog.Information("set color default values");
                Settings.UserColors.First(x => x.Key == "BanchoBot").Color = (Color)ColorConverter.ConvertFromString(DefaultColors.GetDefaultColor("BanchoBot"));
                Settings.UserColors.First(x => x.Key == "Self").Color = (Color)ColorConverter.ConvertFromString(DefaultColors.GetDefaultColor("Self"));
                Settings.UserColors.First(x => x.Key == "Default").Color = (Color)ColorConverter.ConvertFromString(DefaultColors.GetDefaultColor("Default"));
                Settings.UserColors.First(x => x.Key == "HD").Color = (Color)ColorConverter.ConvertFromString(DefaultColors.GetDefaultColor("HD"));
                Settings.UserColors.First(x => x.Key == "HR").Color = (Color)ColorConverter.ConvertFromString(DefaultColors.GetDefaultColor("HR"));
                Settings.UserColors.First(x => x.Key == "DT").Color = (Color)ColorConverter.ConvertFromString(DefaultColors.GetDefaultColor("DT"));
                Settings.UserColors.First(x => x.Key == "FL").Color = (Color)ColorConverter.ConvertFromString(DefaultColors.GetDefaultColor("FL"));
                Settings.UserColors.First(x => x.Key == "Freemod").Color = (Color)ColorConverter.ConvertFromString(DefaultColors.GetDefaultColor("Freemod"));
                Settings.UserColors.First(x => x.Key == "Tiebreaker").Color = (Color)ColorConverter.ConvertFromString(DefaultColors.GetDefaultColor("Tiebreaker"));
                Settings.UserColors.First(x => x.Key == "NoFail").Color = (Color)ColorConverter.ConvertFromString(DefaultColors.GetDefaultColor("NoFail"));
                Settings.SaveConfig();
                Events.Aggregator.PublishOnUIThread("UpdateColors");
                NotifyOfPropertyChange(() => ColorsViews);
            }
        }
        #endregion
    }
}
