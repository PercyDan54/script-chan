using Caliburn.Micro;
using MaterialDesignThemes.Wpf;
using script_chan2.DataTypes;
using Serilog;
using System;
using System.Windows.Media;

namespace script_chan2.GUI
{
    public class MappoolListItemViewModel : Screen
    {
        private ILogger localLog = Log.ForContext<MappoolListItemViewModel>();

        #region Constructor
        public MappoolListItemViewModel(Mappool mappool)
        {
            this.mappool = mappool;
            Events.Aggregator.Subscribe(this);
        }
        #endregion

        #region Properties
        private Mappool mappool;

        public string Name
        {
            get { return $"{mappool.Name} ({mappool.Tournament.Name})"; }
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

        public SolidColorBrush Foreground
        {
            get
            {
                if (hover)
                    return Brushes.Black;
                return Brushes.White;
            }
        }
        #endregion

        #region Actions
        public async void Edit()
        {
            localLog.Information("edit dialog of mappool '{mappool}' open", mappool.Name);
            var model = new EditMappoolDialogViewModel(mappool.Id);
            var view = ViewLocator.LocateForModel(model, null, null);
            ViewModelBinder.Bind(model, view, null);

            var result = Convert.ToBoolean(await DialogHost.Show(view, "MainDialogHost"));

            if (result)
            {
                localLog.Information("save mappool '{mappool}'", mappool.Name);
                mappool.Name = model.Name;
                mappool.Save();
                NotifyOfPropertyChange(() => Name);
            }
        }

        public async void EditMaps()
        {
            localLog.Information("beatmap list dialog of mappool '{mappool}' open", mappool.Name);
            var model = new MappoolBeatmapsDialogViewModel(mappool);
            var view = ViewLocator.LocateForModel(model, null, null);
            ViewModelBinder.Bind(model, view, null);

            model.Activate();
            await DialogHost.Show(view, "MainDialogHost");
            model.Deactivate();
        }

        public async void Delete()
        {
            localLog.Information("delete dialog of mappool '{mappool}' open", mappool.Name);
            var model = new DeleteMappoolDialogViewModel(mappool);
            var view = ViewLocator.LocateForModel(model, null, null);
            ViewModelBinder.Bind(model, view, null);

            var result = Convert.ToBoolean(await DialogHost.Show(view, "MainDialogHost"));

            if (result)
            {
                localLog.Information("delete mappool '{mappool}'", mappool.Name);
                mappool.Delete();
                Events.Aggregator.PublishOnUIThread("DeleteMappool");
            }
        }

        public void MouseEnter()
        {
            hover = true;
            NotifyOfPropertyChange(() => Background);
            NotifyOfPropertyChange(() => Foreground);
        }

        public void MouseLeave()
        {
            hover = false;
            NotifyOfPropertyChange(() => Background);
            NotifyOfPropertyChange(() => Foreground);
        }
        #endregion
    }
}
