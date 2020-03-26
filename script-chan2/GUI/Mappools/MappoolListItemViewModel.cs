using Caliburn.Micro;
using MaterialDesignThemes.Wpf;
using script_chan2.DataTypes;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using WK.Libraries.SharpClipboardNS;

namespace script_chan2.GUI
{
    public class MappoolListItemViewModel : Screen
    {
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
        #endregion

        #region Actions
        public async void Edit()
        {
            Log.Information("MappoolListItemViewModel: edit dialog of mappool '{mappool}' open", mappool.Name);
            var model = new EditMappoolDialogViewModel(mappool.Id);
            var view = ViewLocator.LocateForModel(model, null, null);
            ViewModelBinder.Bind(model, view, null);

            var result = Convert.ToBoolean(await DialogHost.Show(view));

            if (result)
            {
                mappool.Name = model.Name;
                mappool.Save();
                NotifyOfPropertyChange(() => Name);
            }
        }

        public async void EditMaps()
        {
            Log.Information("TeamListItemViewModel: beatmap list dialog of mappool '{mappool}' open", mappool.Name);
            var model = new MappoolBeatmapsDialogViewModel(mappool);
            var view = ViewLocator.LocateForModel(model, null, null);
            ViewModelBinder.Bind(model, view, null);

            model.Activate();
            await DialogHost.Show(view);
            model.Deactivate();
        }

        public async void Delete()
        {
            Log.Information("MappoolListItemViewModel: delete mappool '{mappool}'", mappool.Name);
            var model = new DeleteMappoolDialogViewModel(mappool);
            var view = ViewLocator.LocateForModel(model, null, null);
            ViewModelBinder.Bind(model, view, null);

            var result = Convert.ToBoolean(await DialogHost.Show(view));

            if (result)
            {
                mappool.Delete();
                Events.Aggregator.PublishOnUIThread("DeleteMappool");
            }
        }

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
