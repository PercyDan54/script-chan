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
                switch (beatmap.PickType)
                {
                    case Scores.PickType.NoMod:
                        return ModsBrushes.NoModLight;
                    case Scores.PickType.HardRock:
                        return ModsBrushes.HardRockLight;
                    case Scores.PickType.Hidden:
                        return ModsBrushes.HiddenLight;
                    case Scores.PickType.DoubleTime:
                        return ModsBrushes.DoubleTimeLight;
                    case Scores.PickType.FreeMod:
                        return ModsBrushes.FreeModLight;
                    case Scores.PickType.TieBreaker:
                        return ModsBrushes.TieBreakerLight;
                    default:
                        return ModsBrushes.NoModLight;
                }
            }
        }

        /// <summary>
        /// Border property
        /// </summary>
        public Brush Border
        {
            get
            {
                switch (beatmap.PickType)
                {
                    case Scores.PickType.NoMod:
                        return ModsBrushes.NoMod;
                    case Scores.PickType.HardRock:
                        return ModsBrushes.HardRock;
                    case Scores.PickType.Hidden:
                        return ModsBrushes.Hidden;
                    case Scores.PickType.DoubleTime:
                        return ModsBrushes.DoubleTime;
                    case Scores.PickType.FreeMod:
                        return ModsBrushes.FreeMod;
                    case Scores.PickType.TieBreaker:
                        return ModsBrushes.TieBreaker;
                    default:
                        return ModsBrushes.NoMod;
                }
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
        /// Selected item property
        /// </summary>
        public PickType SelectedPickType
        {
            get
            {
                return beatmap.PickType;
            }
            set
            {
                if (value != beatmap.PickType)
                {
                    beatmap.PickType = value;
                    NotifyOfPropertyChange(() => SelectedPickType);
                    NotifyOfPropertyChange(() => Background);
                    NotifyOfPropertyChange(() => Border);
                    parent.Update();
                }
            }
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
        #endregion
    }
}
