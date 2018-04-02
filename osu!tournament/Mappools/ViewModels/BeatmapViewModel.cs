using Caliburn.Micro;
using Osu.Scores;
using System;
using System.Collections.Generic;
using System.Linq;
using Osu.Mvvm.Miscellaneous;
using System.Windows.Media;
using Osu.Tournament.Mappools.ViewModels;
using Osu.Tournament.Mappools.Views;

namespace Osu.Mvvm.Mappools.ViewModels
{
    /// <summary>
    /// Represents a beatmap view model
    /// </summary>
    public class BeatmapViewModel : PropertyChangedBase, IHaveDisplayName
    {
        #region Attributes
        /// <summary>
        /// The parent mappool view model
        /// </summary>
        private MappoolViewModel parent;

        /// <summary>
        /// The beatmap
        /// </summary>
        private Beatmap beatmap;

        /// <summary>
        /// If add mod popup is open
        /// </summary>
        private bool _modPickerIsOpen;

        private List<BeatmapModViewModel> _mods;
        #endregion

        #region Constructor
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="beatmap">the beatmap</param>
        public BeatmapViewModel(MappoolViewModel parent, Beatmap beatmap)
        {
            this.parent = parent;
            this.beatmap = beatmap;

            beatmap.ModsChanged += UpdateMods;

            if(beatmap.PickType.Count == 0)
                beatmap.PickType.Add(Scores.PickType.None);

            UpdateMods();
        }
        #endregion

        #region Properties
        /// <summary>
        /// Background property
        /// </summary>
        public Brush Background
        {
            get
            {
                var brush = new LinearGradientBrush();

                for (var i = 0; i < beatmap.PickType.Count; i++)
                {
                    var brushToAdd = ModsBrushes.GetModBrushLight(beatmap.PickType[i]);

                    brush.GradientStops.Add(new GradientStop(((SolidColorBrush)brushToAdd).Color, 1f / (beatmap.PickType.Count - 1) * i));
                }

                return brush;
            }
        }

        /// <summary>
        /// Border property
        /// </summary>
        public Brush Border
        {
            get
            {
                var brush = new LinearGradientBrush();

                for (var i = 0; i < beatmap.PickType.Count; i++)
                {
                    var brushToAdd = ModsBrushes.GetModBrush(beatmap.PickType[i]);

                    brush.GradientStops.Add(new GradientStop(((SolidColorBrush)brushToAdd).Color, 1f / (beatmap.PickType.Count - 1) * i));
                }

                return brush;
            }
        }

        /// <summary>
        /// Beatmap property
        /// </summary>
        public Beatmap Beatmap
        {
            get
            {
                return beatmap;
            }
        }

        /// <summary>
        /// Display member property
        /// </summary>
        public string DisplayName
        {
            get
            {
                return beatmap.OsuBeatmap.Artist + " - " + beatmap.OsuBeatmap.Title + " [" + beatmap.OsuBeatmap.Version + "]";
            }
            set
            {
                return;
            }
        }

        /// <summary>
        /// Pick types property
        /// </summary>
        public IEnumerable<PickType> PickType
        {
            get
            {
                return Enum.GetValues(typeof(PickType)).Cast<PickType>();
            }
        }

        /// <summary>
        /// If add mod popup is open
        /// </summary>
        public bool ModPickerIsOpen
        {
            get => _modPickerIsOpen;
            set
            {
                if (_modPickerIsOpen != value)
                {
                    _modPickerIsOpen = value;
                    NotifyOfPropertyChange(() => ModPickerIsOpen);
                }
            }
        }

        /// <summary>
        /// The current mods for the beatmap
        /// </summary>
        public IObservableCollection<BeatmapModViewModel> Mods => new BindableCollection<BeatmapModViewModel>(_mods);

        /// <summary>
        /// List of unpicked mods for the add mod popup
        /// </summary>
        public List<PickType> PickableMods
        {
            get;
            set;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Deletes the beatmap
        /// </summary>
        public void Delete()
        {
            parent.Delete(this);
        }

        /// <summary>
        /// Opens the add mod popup
        /// </summary>
        public void ShowModPicker()
        {
            PickableMods = Enum.GetValues(typeof(PickType)).Cast<PickType>().ToList();
            foreach (var mod in beatmap.PickType)
            {
                PickableMods.RemoveAll(x => x == mod);
            }

            ModPickerIsOpen = true;
            NotifyOfPropertyChange(() => PickableMods);
        }

        /// <summary>
        /// Adds the chosen mod to the beatmap
        /// </summary>
        /// <param name="context">The mod name</param>
        public void AddMod(string context)
        {
            foreach (var mod in PickableMods)
            {
                if (Enum.GetName(typeof(PickType), mod) != context) continue;
                beatmap.AddMod(mod);
                break;
            }

            ModPickerIsOpen = false;
        }

        public void UpdateMods()
        {
            _mods = new List<BeatmapModViewModel>();
            foreach (var mod in beatmap.PickType)
                _mods.Add(new BeatmapModViewModel(beatmap, mod));

            NotifyOfPropertyChange(() => Mods);
            NotifyOfPropertyChange(() => Background);
            NotifyOfPropertyChange(() => Border);
        }
        #endregion
    }
}
