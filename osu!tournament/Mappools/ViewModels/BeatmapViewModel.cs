using Caliburn.Micro;
using Osu.Scores;
using System;
using System.Collections.Generic;
using System.Linq;
using Osu.Mvvm.Miscellaneous;
using System.Windows.Media;

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

                for (var i = 0; i < beatmap.PickType.Count; i++) {
                    Brush brushToAdd = null;
                    switch (beatmap.PickType[i])
                    {
                        case Scores.PickType.None:
                            brushToAdd = ModsBrushes.NoModLight;
                            break;
                        case Scores.PickType.HD:
                            brushToAdd = ModsBrushes.HDLight;
                            break;
                        case Scores.PickType.HR:
                            brushToAdd = ModsBrushes.HRLight;
                            break;
                        case Scores.PickType.DT:
                            brushToAdd = ModsBrushes.DTLight;
                            break;
                        case Scores.PickType.FL:
                            brushToAdd = ModsBrushes.FLLight;
                            break;
                        case Scores.PickType.Freemod:
                            brushToAdd = ModsBrushes.FreeModLight;
                            break;
                        case Scores.PickType.TieBreaker:
                            brushToAdd = ModsBrushes.TieBreakerLight;
                            break;
                        default:
                            brushToAdd = ModsBrushes.NoModLight;
                            break;
                    }

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
                    Brush brushToAdd = null;
                    switch (beatmap.PickType[i])
                    {
                        case Scores.PickType.None:
                            brushToAdd = ModsBrushes.NoMod;
                            break;
                        case Scores.PickType.HD:
                            brushToAdd = ModsBrushes.HD;
                            break;
                        case Scores.PickType.HR:
                            brushToAdd = ModsBrushes.HR;
                            break;
                        case Scores.PickType.DT:
                            brushToAdd = ModsBrushes.DT;
                            break;
                        case Scores.PickType.FL:
                            brushToAdd = ModsBrushes.FL;
                            break;
                        case Scores.PickType.Freemod:
                            brushToAdd = ModsBrushes.FreeMod;
                            break;
                        case Scores.PickType.TieBreaker:
                            brushToAdd = ModsBrushes.TieBreaker;
                            break;
                        default:
                            brushToAdd = ModsBrushes.NoMod;
                            break;
                    }

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
        public List<PickType> Mods
        {
            get => beatmap.PickType.ToList();
        }

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
            foreach (var mod in Mods)
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
            NotifyOfPropertyChange(() => Mods);
            NotifyOfPropertyChange(() => Background);
            NotifyOfPropertyChange(() => Border);
        }

        /// <summary>
        /// Removes the chosen mod from the beatmap
        /// </summary>
        /// <param name="context">The mod name</param>
        public void RemoveMod(string context)
        {
            foreach (var mod in Mods)
            {
                if (Enum.GetName(typeof(PickType), mod) != context) continue;
                beatmap.RemoveMod(mod);
                break;
            }

            NotifyOfPropertyChange(() => Mods);
            NotifyOfPropertyChange(() => Background);
            NotifyOfPropertyChange(() => Border);
        }
        #endregion
    }
}
