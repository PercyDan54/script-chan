using Caliburn.Micro;
using Osu.Ircbot;
using Osu.Mvvm.Miscellaneous;
using Osu.Scores;
using System;
using System.Linq;
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

        private int position;
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
        /// <summary>
        /// The Position property
        /// </summary>
        public string Position
        {
            get { return position == 0 ? "" : position.ToString(); }
        }

        /// <summary>
        /// The Background property
        /// </summary>
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
                    var brush = new LinearGradientBrush();

                    for (var i = 0; i < beatmap.PickType.Count; i++)
                    {
                        var brushToAdd = ModsBrushes.GetModBrushLight(beatmap.PickType[i]);

                        brush.GradientStops.Add(new GradientStop(((SolidColorBrush)brushToAdd).Color, 1f / (beatmap.PickType.Count - 1) * i));
                    }

                    return brush;
                }
            }
        }

        /// <summary>
        /// The Border property
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
        /// The BeatmapCheckBox property to pick the map on the client side without sending the map on osu!
        /// </summary>
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
                        position = bh.AddPick(beatmap);
                        isAlreadyPicked = value;
                        NotifyOfPropertyChange(() => Position);
                    }
                    else
                    {
                        //If it has been successfully removed (last pick)
                        if (bh.RemoveLastPick(beatmap))
                        {
                            isAlreadyPicked = value;
                            position = 0;
                            NotifyOfPropertyChange(() => Position);
                        }
                        else
                        {
                            Dialog.ShowDialog(Tournament.Properties.Resources.Error_Title, Tournament.Properties.Resources.Error_UnpickOrder);
                        }
                    }
                    NotifyOfPropertyChange(() => Background);
                    NotifyOfPropertyChange(() => BeatmapCheckBox);
                }
            }
        }

        /// <summary>
        /// IsCheckboxEnabled property
        /// </summary>
        public bool IsCheckboxEnabled
        {
            get
            {
                //Enabled if not banned
                return !isBanned;
            }
        }

        /// <summary>
        /// The beatmap property
        /// </summary>
        public Beatmap Beatmap
        {
            get
            {
                return beatmap;
            }
        }

        /// <summary>
        /// The ButtonText property
        /// </summary>
        public string ButtonMapText
        {
            get
            {
                return string.Format("{0} - {1} ({2}) [{3}]", beatmap.OsuBeatmap.Artist, beatmap.OsuBeatmap.Title, beatmap.OsuBeatmap.Creator, beatmap.OsuBeatmap.Version);
            }
        }

        /// <summary>
        /// THe ToolTipInfos property
        /// </summary>
        public string ToolTipInfos
        {
            get
            {
                TimeSpan t = TimeSpan.FromSeconds(beatmap.OsuBeatmap.TotalLength);
                return string.Format("{0} - {1}✩ - {2} BPM - {3} CS - {4} AR", t.ToString(@"mm\:ss"), Math.Round(beatmap.OsuBeatmap.DifficultyRating, 2), beatmap.OsuBeatmap.BPM, beatmap.OsuBeatmap.CircleSize, beatmap.OsuBeatmap.ApproachRate);
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Function called to pick the map in game
        /// </summary>
        public void PickTheMap()
        {
            string modvalue = "";
            int worldcupId = 0;

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
                var bot = OsuIrcBot.GetInstancePrivate();
                bot.SendMessage("#mp_" + room.Id, string.Format("!mp mods {0}", beatmap.PickType.Aggregate(string.Empty, (a, b) => a + " " + (b == PickType.TieBreaker ? "Freemod" : Enum.GetName(typeof(PickType), b) ))));
                bot.SendMessage("#mp_" + room.Id, string.Format("!mp map {0} {1}", beatmap.OsuBeatmap.BeatmapID, worldcupId));
                BeatmapCheckBox = true;
            }
        }

        /// <summary>
        /// Function called to ban the map on the client
        /// </summary>
        public void MapBannerino()
        {
            RefereeMatchHelper bh = RefereeMatchHelper.GetInstance(room.Id);
            if (!isBanned)
            {
                isBanned = true;
                position = bh.ApplyBan(beatmap, room);
                NotifyOfPropertyChange(() => Background);
                NotifyOfPropertyChange(() => IsCheckboxEnabled);
                NotifyOfPropertyChange(() => Position);
            }
            else
            {
                Dialog.ShowDialog(Tournament.Properties.Resources.Error_Title, Tournament.Properties.Resources.Error_BanLimit);
            }
        }

        /// <summary>
        /// Function called to revert the ban on the client
        /// </summary>
        public void RevertBannerino()
        {
            RefereeMatchHelper bh = RefereeMatchHelper.GetInstance(room.Id);
            if (bh.CanUnban(beatmap))
            {
                isBanned = false;
                position = 0;
                bh.RemoveBan();
                NotifyOfPropertyChange(() => Background);
                NotifyOfPropertyChange(() => IsCheckboxEnabled);
                NotifyOfPropertyChange(() => Position);
            }
            else
            {
                Dialog.ShowDialog(Tournament.Properties.Resources.Error_Title, Tournament.Properties.Resources.Error_UnbanOrder);
            }
        }
        #endregion
    }
}
