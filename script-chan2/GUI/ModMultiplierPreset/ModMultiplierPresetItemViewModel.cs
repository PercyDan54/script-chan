using System.Linq;
using System.Windows.Media;
using Caliburn.Micro;
using script_chan2.DataTypes;
using script_chan2.Enums;
using Serilog;

namespace script_chan2.GUI
{
    public class ModMultiplierPresetItemViewModel : Screen
    {
        private ILogger localLog = Log.ForContext<ModMultiplierPresetItemViewModel>();
        private readonly ModMultiplier modMultiplier;
        private readonly ModMultiplierPreset modMultiplierPreset;
        public static BindableCollection<GameMods> ModsListCache = new BindableCollection<GameMods>(typeof(GameMods).GetEnumValues().Cast<GameMods>());
        public BindableCollection<GameMods> ModsList { get; set; } = ModsListCache;

        #region Constructor
        public ModMultiplierPresetItemViewModel(ModMultiplierPreset modMultiplierPreset, ModMultiplier modMultiplier)
        {
            this.modMultiplierPreset = modMultiplierPreset;
            this.modMultiplier = modMultiplier;
        }
        #endregion

        #region Properties
        public GameMods Mods
        {
            get => modMultiplier.Mods;
            set
            {
                modMultiplier.Mods = value;
                UpdateMods();
            }
        }

        public double Multiplier
        {
            get => modMultiplier.Multiplier;
            set => modMultiplier.Multiplier = value;
        }

        public void UpdateMods()
        {
            NotifyOfPropertyChange(() => Background);
            Events.Aggregator.PublishOnUIThread("UpdateModMultiplier");
        }

        public Brush Background
        {
            get
            {
                Brush brushToAdd = Brushes.Gray;
                switch (Mods)
                {
                    case GameMods.Easy: brushToAdd = new SolidColorBrush(Settings.UserColors.First(x => x.Key == "EZ").Color); break;
                    case GameMods.Hidden: brushToAdd = new SolidColorBrush(Settings.UserColors.First(x => x.Key == "HD").Color); break;
                    case GameMods.HardRock: brushToAdd = new SolidColorBrush(Settings.UserColors.First(x => x.Key == "HR").Color); break;
                    case GameMods.DoubleTime: brushToAdd = new SolidColorBrush(Settings.UserColors.First(x => x.Key == "DT").Color); break;
                    case GameMods.Flashlight: brushToAdd = new SolidColorBrush(Settings.UserColors.First(x => x.Key == "FL").Color); break;
                    case GameMods.Freemod: brushToAdd = new SolidColorBrush(Settings.UserColors.First(x => x.Key == "Freemod").Color); break;
                    case GameMods.TieBreaker: brushToAdd = new SolidColorBrush(Settings.UserColors.First(x => x.Key == "Tiebreaker").Color); break;
                    case GameMods.NoFail: brushToAdd = new SolidColorBrush(Settings.UserColors.First(x => x.Key == "NoFail").Color); break;
                }
                return brushToAdd;
            }
        }
        #endregion

        #region Actions

        public void Delete()
        {
            modMultiplierPreset.Multipliers.Remove(modMultiplier);
            Events.Aggregator.PublishOnUIThread("DeleteModMultiplier");
        }
        #endregion
    }
}
