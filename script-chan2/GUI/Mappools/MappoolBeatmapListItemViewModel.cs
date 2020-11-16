using Caliburn.Micro;
using script_chan2.DataTypes;
using Serilog;
using System.Linq;
using System.Windows.Media;

namespace script_chan2.GUI
{
    public class MappoolBeatmapListItemViewModel : Screen
    {
        private ILogger localLog = Log.ForContext<MappoolBeatmapListItemViewModel>();

        #region Constructor
        public MappoolBeatmapListItemViewModel(MappoolMap beatmap)
        {
            this.beatmap = beatmap;
        }
        #endregion

        #region Properties
        private MappoolMap beatmap;

        public string BeatmapName
        {
            get { return $"{beatmap.Beatmap.Artist} - {beatmap.Beatmap.Title} [{beatmap.Beatmap.Version}]"; }
        }

        public string ModTag
        {
            get { return beatmap.Tag; }
            set
            {
                if (value != beatmap.Tag)
                {
                    beatmap.Tag = value;
                    beatmap.Save();
                    NotifyOfPropertyChange(() => ModTag);
                }
            }
        }

        public void UpdateMods()
        {
            NotifyOfPropertyChange(() => HasModHD);
            NotifyOfPropertyChange(() => HasModHR);
            NotifyOfPropertyChange(() => HasModDT);
            NotifyOfPropertyChange(() => HasModFreemod);
            NotifyOfPropertyChange(() => HasModTiebreaker);
            NotifyOfPropertyChange(() => HasModNF);
            NotifyOfPropertyChange(() => Background);
            Events.Aggregator.PublishOnUIThread("UpdateMappoolMap");
        }

        public bool HasModHD
        {
            get { return beatmap.Mods.Contains(Enums.GameMods.Hidden); }
            set
            {
                if (value)
                    beatmap.AddMod(Enums.GameMods.Hidden);
                else
                    beatmap.RemoveMod(Enums.GameMods.Hidden);
                beatmap.Save();
                UpdateMods();
            }
        }

        public bool HasModHR
        {
            get { return beatmap.Mods.Contains(Enums.GameMods.HardRock); }
            set
            {
                if (value)
                    beatmap.AddMod(Enums.GameMods.HardRock);
                else
                    beatmap.RemoveMod(Enums.GameMods.HardRock);
                beatmap.Save();
                UpdateMods();
            }
        }

        public bool HasModDT
        {
            get { return beatmap.Mods.Contains(Enums.GameMods.DoubleTime); }
            set
            {
                if (value)
                    beatmap.AddMod(Enums.GameMods.DoubleTime);
                else
                    beatmap.RemoveMod(Enums.GameMods.DoubleTime);
                beatmap.Save();
                UpdateMods();
            }
        }

        public bool HasModFreemod
        {
            get { return beatmap.Mods.Contains(Enums.GameMods.Freemod); }
            set
            {
                if (value)
                    beatmap.AddMod(Enums.GameMods.Freemod);
                else
                    beatmap.RemoveMod(Enums.GameMods.Freemod);
                beatmap.Save();
                UpdateMods();
            }
        }

        public bool HasModTiebreaker
        {
            get { return beatmap.Mods.Contains(Enums.GameMods.TieBreaker); }
            set
            {
                if (value)
                    beatmap.AddMod(Enums.GameMods.TieBreaker);
                else
                    beatmap.RemoveMod(Enums.GameMods.TieBreaker);
                beatmap.Save();
                UpdateMods();
            }
        }

        public bool HasModNF
        {
            get { return beatmap.Mods.Contains(Enums.GameMods.NoFail); }
            set
            {
                if (value)
                    beatmap.AddMod(Enums.GameMods.NoFail);
                else
                    beatmap.RemoveMod(Enums.GameMods.NoFail);
                beatmap.Save();
                UpdateMods();
            }
        }

        public Brush Background
        {
            get
            {
                var brush = new LinearGradientBrush();
                for (var i = 0; i < beatmap.Mods.Count; i++)
                {
                    Brush brushToAdd = Brushes.White;
                    switch (beatmap.Mods[i])
                    {
                        case Enums.GameMods.Hidden: brushToAdd = new SolidColorBrush(Settings.UserColors.First(x => x.Key == "HD").Color); break;
                        case Enums.GameMods.HardRock: brushToAdd = new SolidColorBrush(Settings.UserColors.First(x => x.Key == "HR").Color); break;
                        case Enums.GameMods.DoubleTime: brushToAdd = new SolidColorBrush(Settings.UserColors.First(x => x.Key == "DT").Color); break;
                        case Enums.GameMods.Flashlight: brushToAdd = new SolidColorBrush(Settings.UserColors.First(x => x.Key == "FL").Color); break;
                        case Enums.GameMods.Freemod: brushToAdd = new SolidColorBrush(Settings.UserColors.First(x => x.Key == "Freemod").Color); break;
                        case Enums.GameMods.TieBreaker: brushToAdd = new SolidColorBrush(Settings.UserColors.First(x => x.Key == "Tiebreaker").Color); break;
                        case Enums.GameMods.NoFail: brushToAdd = new SolidColorBrush(Settings.UserColors.First(x => x.Key == "NoFail").Color); break;
                    }
                    brush.GradientStops.Add(new GradientStop(((SolidColorBrush)brushToAdd).Color, 1f / (beatmap.Mods.Count - 1) * i));
                }
                return brush;
            }
        }
        #endregion

        #region Actions
        public void MoveUp()
        {
            localLog.Information("mappool '{mappool}' move beatmap '{beatmap}' up", beatmap.Mappool.Name, beatmap.Beatmap.Id);
            beatmap.MoveUp();
            Events.Aggregator.PublishOnUIThread("MoveMappoolMap");
        }

        public void MoveDown()
        {
            localLog.Information("mappool '{mappool}' move beatmap '{beatmap}' down", beatmap.Mappool.Name, beatmap.Beatmap.Id);
            beatmap.MoveDown();
            Events.Aggregator.PublishOnUIThread("MoveMappoolMap");
        }

        public void Delete()
        {
            localLog.Information("mappool '{mappool}' delete beatmap '{beatmap}'", beatmap.Mappool.Name, beatmap.Beatmap.Id);
            beatmap.Delete();
            Events.Aggregator.PublishOnUIThread("DeleteMappoolMap");
        }
        #endregion
    }
}
