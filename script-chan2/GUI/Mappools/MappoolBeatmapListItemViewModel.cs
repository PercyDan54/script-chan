using Caliburn.Micro;
using script_chan2.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace script_chan2.GUI
{
    public class MappoolBeatmapListItemViewModel : Screen
    {
        public MappoolBeatmapListItemViewModel(MappoolMap beatmap)
        {
            this.beatmap = beatmap;
        }

        private MappoolMap beatmap;

        public string Title
        {
            get { return beatmap.Beatmap.Title; }
        }

        public string Artist
        {
            get { return beatmap.Beatmap.Artist; }
        }

        public string Version
        {
            get { return beatmap.Beatmap.Version; }
        }

        public void UpdateMods()
        {
            NotifyOfPropertyChange(() => HasModHD);
            NotifyOfPropertyChange(() => HasModHR);
            NotifyOfPropertyChange(() => HasModDT);
            NotifyOfPropertyChange(() => HasModFreemod);
            NotifyOfPropertyChange(() => HasModTiebreaker);
            NotifyOfPropertyChange(() => HasModNF);
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

        public void MoveUp()
        {
            beatmap.MoveUp();
            Events.Aggregator.PublishOnUIThread("MoveMappoolMap");
        }

        public void MoveDown()
        {
            beatmap.MoveDown();
            Events.Aggregator.PublishOnUIThread("MoveMappoolMap");
        }

        public void Delete()
        {
            beatmap.Delete();
            Events.Aggregator.PublishOnUIThread("DeleteMappoolMap");
        }
    }
}
