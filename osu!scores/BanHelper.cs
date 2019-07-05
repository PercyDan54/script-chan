using Osu.Api;
using Osu.Utils;
using osu_utils.DiscordModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Osu.Scores
{
    /// <summary>
    /// The RefereeMatchHelper for picks and bans in the pool picker
    /// </summary>
    [DataContract]
    public class RefereeMatchHelper
    {
        #region Attributes
        /// <summary>
        /// The dictionary containing all matchhelper for all rooms
        /// </summary>
        protected static Dictionary<long, RefereeMatchHelper> dicInstance;

        /// <summary>
        /// The first team who is banning if TeamVs mode
        /// </summary>
        [DataMember]
        protected Team FirstTeamToBan { get; set; }
        /// <summary>
        /// The second team who is banning if TeamVs mode
        /// </summary>
        [DataMember]
        protected Team SecondTeamToBan { get; set; }

        /// <summary>
        /// The list of beatmaps banned by Red
        /// </summary>
        [DataMember]
        protected List<Beatmap> BeatmapBannedRed;

        /// <summary>
        /// The list of beatmaps banned by Blue
        /// </summary>
        [DataMember]
        protected List<Beatmap> BeatmapBannedBlue;

        /// <summary>
        /// The list of beatmaps picked
        /// </summary>
        [DataMember]
        protected List<Beatmap> picks;
        #endregion

        #region Constructors
        protected RefereeMatchHelper()
        {
            BeatmapBannedRed = new List<Beatmap>();
            BeatmapBannedBlue = new List<Beatmap>();
            picks = new List<Beatmap>();
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Function which is updating the ban order
        /// </summary>
        /// <param name="r">The room</param>
        /// <param name="firstTeam">THE FIRST TEAM TO PICK (not banning, which is the contrary)</param>
        public void UpdateTeamBanOrder(Room r, OsuTeam firstTeam)
        {
            if(r.Ranking.GetType() == typeof(Osu.Scores.TeamVs))
            {
                FirstTeamToBan = firstTeam == OsuTeam.Blue ? ((Osu.Scores.TeamVs)r.Ranking).Red : ((Osu.Scores.TeamVs)r.Ranking).Blue;
                SecondTeamToBan = firstTeam == OsuTeam.Blue ? ((Osu.Scores.TeamVs)r.Ranking).Blue : ((Osu.Scores.TeamVs)r.Ranking).Red;
            }
        }

        /// <summary>
        /// Check if a map has already been picked or not
        /// </summary>
        /// <param name="bm">the beatmap to pick</param>
        /// <returns></returns>
        public bool IsThisMapPicked(Beatmap bm)
        {
            return picks.Exists(x => x.OsuBeatmap.BeatmapID == bm.OsuBeatmap.BeatmapID);
        }

        /// <summary>
        /// Check if a map has already been banned or not
        /// </summary>
        /// <param name="bm">the beatmap to ban</param>
        /// <returns></returns>
        public bool IsThisMapBanned(Beatmap bm)
        {
            return BeatmapBannedBlue.Exists(x => x.Id == bm.Id) || BeatmapBannedRed.Exists(x => x.Id == bm.Id); 
        }

        /// <summary>
        /// Check if we can print bans on discord
        /// </summary>
        /// <param name="teamVsMode">true if teamvs, false if headtohead</param>
        /// <returns></returns>
        private bool CanShowBan(bool teamVsMode)
        {
            return BeatmapBannedRed.Count + BeatmapBannedBlue.Count != 0;
        }

        /// <summary>
        /// Function which is applying the ban to the beatmap
        /// </summary>
        /// <param name="bm">the beatmap to ban</param>
        /// <param name="room">The room</param>
        /// <returns></returns>
        public int ApplyBan(Beatmap bm, OsuTeam team)
        {
            if (team == OsuTeam.Red)
                BeatmapBannedRed.Add(bm);
            else
                BeatmapBannedBlue.Add(bm);

            return BeatmapBannedRed.Count + BeatmapBannedBlue.Count;
        }

        /// <summary>
        /// Function which is removing the last ban
        /// </summary>
        public void RemoveBan(Beatmap beatmap)
        {
            BeatmapBannedBlue.RemoveAll(x => x.OsuBeatmap.BeatmapID == beatmap.OsuBeatmap.BeatmapID);
            BeatmapBannedRed.RemoveAll(x => x.OsuBeatmap.BeatmapID == beatmap.OsuBeatmap.BeatmapID);
        }

        /// <summary>
        /// Function which is adding a map to the pick list
        /// </summary>
        /// <param name="beatmap">the beatmap to pick</param>
        public int AddPick(Beatmap beatmap)
        {
            picks.Add(beatmap);
            return picks.Count;
        }

        /// <summary>
        /// Function which removes the last pick if it is the beatmap selected
        /// </summary>
        /// <param name="beatmap">the beatmap to remove from the pick list</param>
        /// <returns></returns>
        public bool RemoveLastPick(Beatmap beatmap)
        {
            if(picks.Last().OsuBeatmap.BeatmapID == beatmap.OsuBeatmap.BeatmapID)
            {
                picks.RemoveAt(picks.Count-1);
                return true;
            }
            else
            {
                return false;
            }
        }

        /*
        private string GenerateActionMessage(Team team, Beatmap beatmap, bool isItBan)
        {
            return string.Format("[{3} vs {4}] {0} ({1} pick) has been {2}", beatmap.OsuBeatmap.Title, beatmap.PickType, (isItBan ? "banned" : "unbanned"), team.name, FirstTeamToBan.Name, SecondTeamToBan.Name);
        }
        */

        /// <summary>
        /// Function called to generate the ban recap on discord depending of the rankingtype
        /// </summary>
        /// <param name="rankingType">TeamVs or Headtohead</param>
        /// <returns></returns>
        public Embed GenerateBanRecapMessage(Type rankingType, Team blue, Team red)
        {
            bool printTeamVsMode = rankingType == typeof(TeamVs);

            //Check if both bans have been made or if there's a map banned for head to head
            if (CanShowBan(printTeamVsMode))
            {
                Embed e = new Embed();
                e.Title = "Ban Recap " + (!printTeamVsMode ? "" : "(Roll Winner : " + SecondTeamToBan.Name + ")");
                e.Fields = new List<Field>();

                if (printTeamVsMode)
                {
                    string blueTeam = "";
                    string redTeam = "";
                    foreach (Beatmap beatmap in BeatmapBannedBlue)
                    {
                        var bmMods = "";
                        beatmap.PickType.ForEach(m => bmMods += "__" + m.ToString() + "__ ");

                        blueTeam += string.Format("{0}**{1} - {2} [{3}]**" + System.Environment.NewLine, bmMods, beatmap.OsuBeatmap.Artist.Replace("_", "__").Replace("*", "\\*"), beatmap.OsuBeatmap.Title.Replace("_", "__").Replace("*", "\\*"), beatmap.OsuBeatmap.Version.Replace("_", "__").Replace("*", "\\*"));
                    }
                    foreach (Beatmap beatmap in BeatmapBannedRed)
                    {
                        var bmMods = "";
                        beatmap.PickType.ForEach(m => bmMods += "__" + m.ToString() + "__ ");

                        redTeam += string.Format("{0}**{1} - {2} [{3}]**" + System.Environment.NewLine, bmMods, beatmap.OsuBeatmap.Artist.Replace("_", "__").Replace("*", "\\*"), beatmap.OsuBeatmap.Title.Replace("_", "__").Replace("*", "\\*"), beatmap.OsuBeatmap.Version.Replace("_", "__").Replace("*", "\\*"));
                    }

                    if (BeatmapBannedBlue.Count > 0)
                        e.Fields.Add(new Field() { Name = blue.Name, Value = blueTeam });
                    if (BeatmapBannedRed.Count > 0)
                        e.Fields.Add(new Field() { Name = red.Name, Value = redTeam });
                }
                else
                {
                    string generalTeam = "";
                    for (int i = 0; i < BeatmapBannedBlue.Count; i++)
                    {
                        var beatmap = BeatmapBannedBlue[i];
                        var bmMods = "";
                        beatmap.PickType.ForEach(m => bmMods += "__" + m.ToString() + "__ ");

                        generalTeam += string.Format("-{0}- ", i + 1) + string.Format("{0}**{1} - {2} [{3}]**" + System.Environment.NewLine, bmMods, beatmap.OsuBeatmap.Artist.Replace("_", "__").Replace("*", "\\*"), beatmap.OsuBeatmap.Title.Replace("_", "__").Replace("*", "\\*"), beatmap.OsuBeatmap.Version.Replace("_", "__").Replace("*", "\\*"));
                    }
                    for (int i = 0; i < BeatmapBannedRed.Count; i++)
                    {
                        var beatmap = BeatmapBannedRed[i];
                        var bmMods = "";
                        beatmap.PickType.ForEach(m => bmMods += "__" + m.ToString() + "__ ");

                        generalTeam += string.Format("-{0}- ", i + BeatmapBannedBlue.Count + 1) + string.Format("{0}**{1} - {2} [{3}]**" + System.Environment.NewLine, bmMods, beatmap.OsuBeatmap.Artist.Replace("_", "__").Replace("*", "\\*"), beatmap.OsuBeatmap.Title.Replace("_", "__").Replace("*", "\\*"), beatmap.OsuBeatmap.Version.Replace("_", "__").Replace("*", "\\*"));
                    }

                    e.Fields.Add(new Field() { Name = "The list", Value = generalTeam });
                }

                return e;
            }
            return null;
        }

        /// <summary>
        /// Function called to generate the pick recap on discord depending of the rankingtype
        /// </summary>
        /// <param name="rankingType"></param>
        /// <returns></returns>
        public Embed GeneratePickRecapMessage(Type rankingType)
        {
            Embed e = new Embed();
            e.Title = "Pick Recap";
            e.Fields = new List<Field>();
            var printTeamVsMode = (rankingType == typeof(TeamVs));
            Field firstteam = null;
            Field secondteam = null;


            if (printTeamVsMode)
            {
                firstteam = new Field() { Name = FirstTeamToBan.Name };
                secondteam = new Field() { Name = SecondTeamToBan.Name };
            }
            else
            {
                firstteam = new Field() { Name = "The list" };
            }

            if (picks.Count > 0)
            {
            }
            Beatmap bm = null;
            for(int counter = 0; counter < picks.Count; counter++)
            {
                bm = picks[counter];

                var bmMods = "";
                bm.PickType.ForEach(m => bmMods += "__" + m.ToString() + "__ ");

                string sentence = string.Format("-{0}- {1}**{2} - {3} [{4}]**" + Environment.NewLine, counter + 1, bmMods, bm.OsuBeatmap.Artist.Replace("_", "__").Replace("*", "\\*"), bm.OsuBeatmap.Title.Replace("_", "__").Replace("*", "\\*"), bm.OsuBeatmap.Version.Replace("_", "__").Replace("*", "\\*"));
                if (counter % 2 == 0 && printTeamVsMode)
                {
                    secondteam.Value += sentence;
                }
                else
                {
                    firstteam.Value += sentence;
                }
            }
            
            if(string.IsNullOrEmpty(firstteam.Value))
            {
                firstteam.Value = "None";
            }

            if (printTeamVsMode && string.IsNullOrEmpty(secondteam.Value))
            {
                secondteam.Value = "None";
            }

            e.Fields.Add(firstteam);

            if (printTeamVsMode)
                e.Fields.Add(secondteam);

            return e;
        }
        #endregion

        #region Static methods
        /// <summary>
        /// Initialize the ban helpers saved in the cache
        /// </summary>
        public static void Initialize()
        {
            Cache cache = Cache.GetCache("osu!cache.db");
            dicInstance = cache.GetObject<Dictionary<long, RefereeMatchHelper>>("refereematchhelpers", new Dictionary<long, RefereeMatchHelper>());
        }

        /// <summary>
        /// Save the matchhelpers in the cache
        /// </summary>
        public static void Save()
        {
            Cache cache = Cache.GetCache("osu!cache.db");
            cache["refereematchhelpers"] = dicInstance;
        }

        /// <summary>
        /// Get the instance of the room
        /// </summary>
        /// <param name="roomId">the room id</param>
        /// <returns></returns>
        public static RefereeMatchHelper GetInstance(long roomId)
        {
            RefereeMatchHelper instance;

            if (!dicInstance.TryGetValue(roomId, out instance))
            {
                instance = new RefereeMatchHelper();
                dicInstance.Add(roomId, instance);
            }

            return instance;
        }

        /// <summary>
        /// Check if a ban helper exists for the room already
        /// </summary>
        /// <param name="roomId">the room id</param>
        /// <returns></returns>
        public static bool IsInstanceExisting(long roomId)
        {
            RefereeMatchHelper instance;
            return dicInstance.TryGetValue(roomId, out instance);
        }
        #endregion
    }
}
