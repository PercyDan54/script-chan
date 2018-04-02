using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Caliburn.Micro;
using Osu.Mvvm.Miscellaneous;
using Osu.Scores;

namespace Osu.Tournament.Mappools.ViewModels
{
    public class BeatmapModViewModel : PropertyChangedBase
    {
        #region Attributes

        private Beatmap _beatmap;

        private PickType _mod;

        private string _modName;

        private Brush _backgroundBrush;

        private Brush _borderBrush;

        #endregion

        #region Properties

        public string ModName
        {
            get => _modName;
            set
            {
                if (_modName != null && _modName.Equals(value)) return;
                _modName = value;
                NotifyOfPropertyChange(() => ModName);
            }
        }

        public Brush BackgroundBrush
        {
            get => _backgroundBrush;
            set
            {
                if (_backgroundBrush != null && _backgroundBrush.Equals(value)) return;
                _backgroundBrush = value;
                NotifyOfPropertyChange(() => BackgroundBrush);
            }
        }

        public Brush BorderBrush
        {
            get => _borderBrush;
            set
            {
                if (_borderBrush != null && _borderBrush.Equals(value)) return;
                _borderBrush = value;
                NotifyOfPropertyChange(() => BorderBrush);
            }
        }

        #endregion

        #region Constructor

        public BeatmapModViewModel(Beatmap beatmap, PickType mod)
        {
            _beatmap = beatmap;
            _mod = mod;
            ModName = Enum.GetName(typeof(PickType), mod);
            BackgroundBrush = ModsBrushes.GetModBrush(mod);
            BorderBrush = ModsBrushes.GetModBrush(mod);
        }

        #endregion

        #region Public Methods

        public void RemoveMod()
        {
            _beatmap.RemoveMod(_mod);
        }

        #endregion
    }
}
