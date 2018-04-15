using Osu.Api;
using Osu.Utils;
using Osu.Utils.Bans;
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
        /// The list of beatmaps banned
        /// </summary>
        [DataMember]
        protected List<Beatmap> BeatmapBanned;

        /// <summary>
        /// The list of beatmaps picked
        /// </summary>
        [DataMember]
        protected List<Beatmap> picks;
        #endregion

        #region Constructors
        protected RefereeMatchHelper()
        {
            BeatmapBanned = new List<Beatmap>();
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
            return BeatmapBanned.Exists(x => x.Id == bm.Id); 
        }

        /// <summary>
        /// Check if we can print bans on discord
        /// </summary>
        /// <param name="teamVsMode">true if teamvs, false if headtohead</param>
        /// <returns></returns>
        private bool CanShowBan(bool teamVsMode)
        {
            if (teamVsMode)
                return BeatmapBanned.Count % 2 == 0 && BeatmapBanned.Count != 0;
            else
                return BeatmapBanned.Count != 0;
        }

        /// <summary>
        /// Check if a map can be unbanned or not
        /// </summary>
        /// <param name="bm">the beatmap to unban</param>
        /// <returns></returns>
        public bool CanUnban(Beatmap bm)
        {
            if (BeatmapBanned.Count != 0)
                return BeatmapBanned.Last().Id == bm.Id;
            else
                return false;
        }

        /// <summary>
        /// Function which is applying the ban to the beatmap
        /// </summary>
        /// <param name="bm">the beatmap to ban</param>
        /// <param name="room">The room</param>
        /// <returns></returns>
        public int ApplyBan(Beatmap bm, Room room)
        {
            BeatmapBanned.Add(bm);

            return BeatmapBanned.Count;
        }

        /// <summary>
        /// Function which is removing the last ban
        /// </summary>
        public void RemoveBan()
        {
            BeatmapBanned.RemoveAt(BeatmapBanned.Count - 1);
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
        public Embed GenerateBanRecapMessage(Type rankingType)
        {
            var printTeamVsMode = false;

            if (rankingType == typeof(TeamVs))
                printTeamVsMode = true;

            //Check if both bans have been made or if there's a map banned for head to head
            if (CanShowBan(printTeamVsMode))
            {
                Embed e = new Embed();
                e.Title = "Ban Recap " + (!printTeamVsMode ? "" : "(Roll Winner : " + SecondTeamToBan.Name + ")");
                e.Fields = new List<Field>();

                string first_team = "";
                string second_team = "";

                for (int i = 0; i < BeatmapBanned.Count; i++)
                {
                    var res = string.Format("__{0}__ **{1} - {2} [{3}]**" + System.Environment.NewLine, BeatmapBanned[i].PickType, BeatmapBanned[i].OsuBeatmap.Artist, BeatmapBanned[i].OsuBeatmap.Title, BeatmapBanned[i].OsuBeatmap.Version);
                    if(printTeamVsMode)
                    {
                        if (i % 2 == 0)
                            first_team += res;
                        else
                            second_team += res;
                    }
                    else
                    {
                        first_team += (string.Format("-{0}- ", i+1) + res);
                    }
                    
                }

                if (printTeamVsMode)
                {
                    e.Fields.Add(new Field() { Name = FirstTeamToBan.Name, Value = first_team });
                    e.Fields.Add(new Field() { Name = SecondTeamToBan.Name, Value = second_team });
                }
                else
                {
                    e.Fields.Add(new Field() { Name = "The list", Value = first_team });
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
                string sentence = string.Format("-{0}- __{1}__ **{2} - {3} [{4}]**" + Environment.NewLine, counter + 1, bm.PickType, bm.OsuBeatmap.Artist, bm.OsuBeatmap.Title, bm.OsuBeatmap.Version);
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
