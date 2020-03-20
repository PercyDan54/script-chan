using Caliburn.Micro;
using script_chan2.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace script_chan2.GUI
{
    public class MatchBeatmapViewModel : Screen
    {
        #region Constructor
        public MatchBeatmapViewModel(Match match, MappoolMap beatmap)
        {
            this.match = match;
            this.beatmap = beatmap;
        }
        #endregion

        #region Properties
        private Match match;

        private MappoolMap beatmap;

        public string Tag
        {
            get { return beatmap.Tag; }
        }

        public string Name
        {
            get { return $"{beatmap.Beatmap.Artist} - {beatmap.Beatmap.Title} [{beatmap.Beatmap.Version}] ({beatmap.Beatmap.Creator})  BPM{beatmap.Beatmap.BPM} AR{beatmap.Beatmap.AR} CS{beatmap.Beatmap.CS}"; }
        }

        public string ToolTip
        {
            get
            {
                return "Mapper: " + beatmap.Beatmap.Creator + Environment.NewLine
                    + "BPM: " + beatmap.Beatmap.BPM + Environment.NewLine
                    + "AR: " + beatmap.Beatmap.AR + Environment.NewLine
                    + "CS: " + beatmap.Beatmap.CS;
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
                        case Enums.GameMods.Hidden: brushToAdd = ModBrushes.HDLight; break;
                        case Enums.GameMods.HardRock: brushToAdd = ModBrushes.HRLight; break;
                        case Enums.GameMods.DoubleTime: brushToAdd = ModBrushes.DTLight; break;
                        case Enums.GameMods.Flashlight: brushToAdd = ModBrushes.FLLight; break;
                        case Enums.GameMods.Freemod: brushToAdd = ModBrushes.FreeModLight; break;
                        case Enums.GameMods.TieBreaker: brushToAdd = ModBrushes.TieBreakerLight; break;
                        case Enums.GameMods.NoFail: brushToAdd = ModBrushes.NoFailLight; break;
                    }
                    brush.GradientStops.Add(new GradientStop(((SolidColorBrush)brushToAdd).Color, 1f / (beatmap.Mods.Count - 1) * i));
                }
                return brush;
            }
        }
        #endregion
    }
}
