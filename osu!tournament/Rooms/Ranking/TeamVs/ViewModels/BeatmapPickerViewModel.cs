using Caliburn.Micro;
using Osu.Ircbot;
using Osu.Mvvm.Miscellaneous;
using Osu.Scores;
using System.Windows.Media;

namespace Osu.Mvvm.Rooms.Ranking.TeamVs.ViewModels
{
    public class BeatmapPickerViewModel : Screen
    {
        #region Attributes
        private Beatmap beatmap;

        private Room room;

        private bool isAlreadyPicked;

        private bool isBanned;
        #endregion

        #region Constructor
        public BeatmapPickerViewModel(Room r, Beatmap bm)
        {
            room = r;
            beatmap = bm;
            if (RefereeMatchHelper.IsInstanceExisting(r.Id))
            {
                RefereeMatchHelper bh = RefereeMatchHelper.GetInstance(room.Id);
                if (bh.IsThisMapBanned(beatmap))
                {
                    isBanned = true;
                }
                else
                {
                    isBanned = false;
                }
                if(bh.IsThisMapPicked(beatmap))
                {
                    isAlreadyPicked = true;
                }
                else
                {
                    isAlreadyPicked = false;
                }
            }
            else
            {
                RefereeMatchHelper bh = RefereeMatchHelper.GetInstance(room.Id);
                isBanned = false;
            }
            
        }
        #endregion

        #region Properties
        public Brush Background
        {
            get
            {
                if (isAlreadyPicked)
                {
                    return ModsBrushes.DarkGray;
                }
                else if(isBanned)
                {
                    return ModsBrushes.BannedMap;
                }
                else
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
        }

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

        public bool BeatmapCheckBox
        {
            get
            {
                return isAlreadyPicked;
            }
            set
            {
                if (value != isAlreadyPicked)
                {
                    RefereeMatchHelper bh = RefereeMatchHelper.GetInstance(room.Id);
                    if (value)
                    {
                        bh.AddPick(beatmap);
                        isAlreadyPicked = value;
                    }
                    else
                    {
                        //If it has been successfully removed (last pick)
                        if (bh.RemoveLastPick(beatmap))
                        {
                            isAlreadyPicked = value;
                        }
                        else
                        {
                            Dialog.ShowDialog("Whoops!", "You have to remove the last pick, not removing things randomly! Don't break me, duh!");
                        }
                    }
                    NotifyOfPropertyChange("Background");
                    NotifyOfPropertyChange("BeatmapCheckBox");
                }
            }
        }

        public bool IsCheckboxEnabled
        {
            get
            {
                //Enabled if not banned
                return !isBanned;
            }
        }

        public Beatmap Beatmap
        {
            get
            {
                return beatmap;
            }
        }

        public string ButtonMapText
        {
            get
            {
                return string.Format("{0} - {1} ({2}) [{3}]", beatmap.OsuBeatmap.Artist, beatmap.OsuBeatmap.Title, beatmap.OsuBeatmap.Creator, beatmap.OsuBeatmap.Version);
            }
        }
        #endregion

        #region Public Methods
        public void PickTheMap()
        {
            string modvalue = "";
            int worldcupId = 0;

            switch(beatmap.PickType)
            {
                case PickType.NoMod:
                    modvalue = "none";
                    break;
                case PickType.Hidden:
                    modvalue = "hd";
                    break;
                case PickType.HardRock:
                    modvalue = "hr";
                    break;
                case PickType.DoubleTime:
                    modvalue = "dt";
                    break;
                case PickType.FreeMod:
                    modvalue = "freemod";
                    break;
                case PickType.TieBreaker:
                    modvalue = "freemod";
                    break;
            }

            switch(room.Wctype)
            {
                case Api.OsuMode.Standard:
                    worldcupId = 0;
                    break;
                case Api.OsuMode.Taiko:
                    worldcupId = 1;
                    break;
                case Api.OsuMode.CTB:
                    worldcupId = 2;
                    break;
                case Api.OsuMode.Mania:
                    worldcupId = 3;
                    break;
            }

            if(!isAlreadyPicked && !isBanned)
            {
                var bot = OsuIrcBot.GetInstance();
                bot.SendMessage("#mp_" + room.Id, string.Format("!mp mods {0}", modvalue));
                bot.SendMessage("#mp_" + room.Id, string.Format("!mp map {0} {1}", beatmap.OsuBeatmap.BeatmapID, worldcupId));
                BeatmapCheckBox = true;
            }
        }

        public void MapBannerino()
        {
            RefereeMatchHelper bh = RefereeMatchHelper.GetInstance(room.Id);
            if (bh.CanBan() && !isBanned)
            {
                var dbot = osu_discord.DiscordBot.GetInstance();
                isBanned = true;
                //dbot.SendMessage(bh.ApplyBan(beatmap));
                bh.ApplyBan(beatmap);
                NotifyOfPropertyChange("Background");
                NotifyOfPropertyChange("IsCheckboxEnabled");
            }
            else
            {
                Dialog.ShowDialog("Whoops!", "All bans have been selected or you can't ban a beatmap which is already banned!");
            }
        }

        public void RevertBannerino()
        {
            RefereeMatchHelper bh = RefereeMatchHelper.GetInstance(room.Id);
            if (bh.CanUnban(beatmap))
            {
                var dbot = osu_discord.DiscordBot.GetInstance();
                isBanned = false;
                //dbot.SendMessage(bh.RemoveBan());
                bh.RemoveBan();
                NotifyOfPropertyChange("Background");
                NotifyOfPropertyChange("IsCheckboxEnabled");
            }
            else
            {
                Dialog.ShowDialog("Whoops!", "You can't unban this beatmap! It is either because there is no map banned or you have to unban the second beatmap first!");
            }
        }
        #endregion
    }
}
