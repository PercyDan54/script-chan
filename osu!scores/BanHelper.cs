using Osu.Api;
using Osu.Utils;
using Osu.Utils.Bans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Osu.Scores
{
    [DataContract]
    public class RefereeMatchHelper
    {
        protected static Dictionary<long, RefereeMatchHelper> dicInstance;
        private const int MAX_BAN = 2;

        [DataMember]
        protected Team FirstTeamToBan { get; set; }
        [DataMember]
        protected Team SecondTeamToBan { get; set; }

        [DataMember]
        protected Beatmap FirstBeatmapBanned { get; set; }
        [DataMember]
        protected Beatmap SecondBeatmapBanned { get; set; }

        [DataMember]
        protected bool hasFirstTeamBanned;
        [DataMember]
        protected bool hasSecondTeamBanned;

        [DataMember]
        protected List<Beatmap> picks;

        protected RefereeMatchHelper()
        {
            hasFirstTeamBanned = false;
            hasSecondTeamBanned = false;
            picks = new List<Beatmap>();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="r"></param>
        /// <param name="firstTeam">THE FIRST TEAM TO PICK (not banning, which is the contrary)</param>
        public void UpdateTeamBanOrder(Room r, OsuTeam firstTeam)
        {
            FirstTeamToBan = firstTeam == OsuTeam.Blue ? ((Osu.Scores.TeamVs)r.Ranking).Red : ((Osu.Scores.TeamVs)r.Ranking).Blue;
            SecondTeamToBan = firstTeam == OsuTeam.Blue ? ((Osu.Scores.TeamVs)r.Ranking).Blue : ((Osu.Scores.TeamVs)r.Ranking).Red;
        }

        public bool IsThisMapPicked(Beatmap bm)
        {
            return picks.Exists(x => x.OsuBeatmap.BeatmapID == bm.OsuBeatmap.BeatmapID);
        }

        public bool IsThisMapBanned(Beatmap bm)
        {
            return (FirstBeatmapBanned?.OsuBeatmap.BeatmapID == bm.OsuBeatmap.BeatmapID || SecondBeatmapBanned?.OsuBeatmap.BeatmapID == bm.OsuBeatmap.BeatmapID);
        }

        public bool CanBan()
        {
            if (hasSecondTeamBanned && hasFirstTeamBanned)
                return false;
            else
                return true;
        }

        public bool CanUnban(Beatmap bm)
        {
            if (!hasFirstTeamBanned && !hasSecondTeamBanned)
            {
                return false;
            }
            
            if(hasSecondTeamBanned)
            {
                if(SecondBeatmapBanned.OsuBeatmap.BeatmapID == bm.OsuBeatmap.BeatmapID)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                if(FirstBeatmapBanned.OsuBeatmap.BeatmapID == bm.OsuBeatmap.BeatmapID)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public bool ApplyBan(Beatmap bm, Room room)
        {
            bool res = true;

            if (!hasFirstTeamBanned)
            {
                hasFirstTeamBanned = true;
                FirstBeatmapBanned = bm;
                if(room.IsStreamed && ObsBanHelper.IsValid)
                {
                    if (!ObsBanHelper.GetInstance().SetBannedMap(FirstTeamToBan.Name, bm.OsuBeatmap.BeatmapID.ToString(), 1))
                        res = false;
                }
                //text = GenerateActionMessage(FirstTeamToBan, FirstBeatmapBanned, true);
            }
            else if(!hasSecondTeamBanned)
            {
                hasSecondTeamBanned = true;
                SecondBeatmapBanned = bm;
                if (room.IsStreamed && ObsBanHelper.IsValid)
                {
                    if (!ObsBanHelper.GetInstance().SetBannedMap(SecondTeamToBan.Name, bm.OsuBeatmap.BeatmapID.ToString(), 1))
                        res = false;
                }
                //text = GenerateActionMessage(SecondTeamToBan, SecondBeatmapBanned, true);
            }

            return res;
        }

        public string RemoveBan()
        {
            string text = null;

            if (hasSecondTeamBanned)
            {
                //text = GenerateActionMessage(SecondTeamToBan, SecondBeatmapBanned, false);
                hasSecondTeamBanned = false;
                SecondBeatmapBanned = null;
            }
            else if (hasFirstTeamBanned)
            {
                //text = GenerateActionMessage(FirstTeamToBan, FirstBeatmapBanned, false);
                hasFirstTeamBanned = false;
                FirstBeatmapBanned = null;
            }

            return text;
        }

        public void AddPick(Beatmap beatmap)
        {
            picks.Add(beatmap);
        }

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

        public string GenerateBanRecapMessage()
        {
            //Check if both bans have been made
            if(!CanBan())
            {
                return string.Format("Ban recap for {0} vs **{1}** : " + Environment.NewLine + "{0} : `{4}` {2} - {3} [{8}]" + Environment.NewLine + "**{1}** : `{7}` {5} - {6} [{9}]",
                    FirstTeamToBan.Name, SecondTeamToBan.Name,
                    FirstBeatmapBanned.OsuBeatmap.Artist, FirstBeatmapBanned.OsuBeatmap.Title, FirstBeatmapBanned.PickType,
                    SecondBeatmapBanned.OsuBeatmap.Artist, SecondBeatmapBanned.OsuBeatmap.Title, SecondBeatmapBanned.PickType,
                    FirstBeatmapBanned.OsuBeatmap.Version, SecondBeatmapBanned.OsuBeatmap.Version);
            }
            return null;
        }

        public string GeneratePickRecapMessage()
        {
            string result = "";

            if(picks.Count > 0)
            {
                result += string.Format("Pick recap for {0} vs {1}" + Environment.NewLine, FirstTeamToBan.Name, SecondTeamToBan.Name);
            }
            Beatmap bm = null;
            for(int counter = 0; counter < picks.Count; counter++)
            {
                bm = picks[counter];
                result += string.Format("-{0}- **{1}** : `{4}` {2} - {3} [{5}]" + Environment.NewLine, counter + 1, (counter % 2 == 0 ? SecondTeamToBan.Name : FirstTeamToBan.Name), bm.OsuBeatmap.Artist, bm.OsuBeatmap.Title, bm.PickType, bm.OsuBeatmap.Version);
            }

            return result;
        }

        #region Static methods
        public static void Initialize()
        {
            Cache cache = Cache.GetCache("osu!cache.db");
            dicInstance = cache.GetObject<Dictionary<long, RefereeMatchHelper>>("refereematchhelpers", new Dictionary<long, RefereeMatchHelper>());
        }

        public static void Save()
        {
            Cache cache = Cache.GetCache("osu!cache.db");
            cache["refereematchhelpers"] = dicInstance;
        }

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

        public static bool IsInstanceExisting(long roomId)
        {
            RefereeMatchHelper instance;
            return dicInstance.TryGetValue(roomId, out instance);
        }
        #endregion
    }
}
