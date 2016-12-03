using Osu.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Osu.Scores
{
    public class RefereeMatchHelper
    {
        private static Dictionary<long, RefereeMatchHelper> dicInstance;
        private const int MAX_BAN = 2;

        private Team FirstTeamToBan { get; set; }
        private Team SecondTeamToBan { get; set; }

        private Beatmap FirstBeatmapBanned { get; set; }
        private Beatmap SecondBeatmapBanned { get; set; }

        public bool hasFirstTeamBanned;
        public bool hasSecondTeamBanned;

        private List<Beatmap> picks;

        private RefereeMatchHelper()
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

        public string ApplyBan(Beatmap bm)
        {
            string text = null;

            if (!hasFirstTeamBanned)
            {
                hasFirstTeamBanned = true;
                FirstBeatmapBanned = bm;
                //text = GenerateActionMessage(FirstTeamToBan, FirstBeatmapBanned, true);
            }
            else if(!hasSecondTeamBanned)
            {
                hasSecondTeamBanned = true;
                SecondBeatmapBanned = bm;
                //text = GenerateActionMessage(SecondTeamToBan, SecondBeatmapBanned, true);
            }

            return text;
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
            dicInstance = new Dictionary<long, RefereeMatchHelper>();
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
