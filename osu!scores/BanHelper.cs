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
    [DataContract]
    public class RefereeMatchHelper
    {
        protected static Dictionary<long, RefereeMatchHelper> dicInstance;

        [DataMember]
        protected Team FirstTeamToBan { get; set; }
        [DataMember]
        protected Team SecondTeamToBan { get; set; }

        [DataMember]
        protected List<Beatmap> BeatmapBanned;

        [DataMember]
        protected List<Beatmap> picks;

        protected RefereeMatchHelper()
        {
            BeatmapBanned = new List<Beatmap>();
            picks = new List<Beatmap>();
        }
        /// <summary>
        /// 
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

        public bool IsThisMapPicked(Beatmap bm)
        {
            return picks.Exists(x => x.OsuBeatmap.BeatmapID == bm.OsuBeatmap.BeatmapID);
        }

        public bool IsThisMapBanned(Beatmap bm)
        {
            return BeatmapBanned.Exists(x => x.Id == bm.Id); 
        }

        private bool CanShowBan()
        {
            return BeatmapBanned.Count % 2 == 0;
        }

        public bool CanUnban(Beatmap bm)
        {
            if (BeatmapBanned.Count != 0)
                return BeatmapBanned.Last().Id == bm.Id;
            else
                return false;
        }

        public bool ApplyBan(Beatmap bm, Room room)
        {
            bool res = true;
            BeatmapBanned.Add(bm);

            return res;
        }

        public void RemoveBan()
        {
            BeatmapBanned.RemoveAt(BeatmapBanned.Count - 1);
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

        public Embed GenerateBanRecapMessage()
        {
            //Check if both bans have been made
            if(CanShowBan())
            {
                Embed e = new Embed();
                e.Title = "Ban Recap";
                e.Fields = new List<Field>();

                string first_team = "";
                string second_team = "";

                e.Fields.Add(new Field() { Name = "Roll Winner", Value = SecondTeamToBan.Name });

                for (int i = 0; i < BeatmapBanned.Count; i++)
                {
                    var res = string.Format("__{0}__ **{1} - {2} [{3}]**" + System.Environment.NewLine, BeatmapBanned[i].PickType, BeatmapBanned[i].OsuBeatmap.Artist, BeatmapBanned[i].OsuBeatmap.Title, BeatmapBanned[i].OsuBeatmap.Version);
                    if (i % 2 == 0)
                        first_team += res;
                    else
                        second_team += res;
                }

                e.Fields.Add(new Field() { Name = FirstTeamToBan.Name + " Bans", Value = first_team });
                e.Fields.Add(new Field() { Name = SecondTeamToBan.Name + " Bans", Value = second_team });

                return e;
            }
            return null;
        }

        public Embed GeneratePickRecapMessage()
        {
            Embed e = new Embed();
            e.Title = "Pick Recap";
            e.Fields = new List<Field>();
            Field firstteam = new Field() { Name = FirstTeamToBan.Name };
            Field secondteam = new Field() { Name = SecondTeamToBan.Name };

            if (picks.Count > 0)
            {
            }
            Beatmap bm = null;
            for(int counter = 0; counter < picks.Count; counter++)
            {
                bm = picks[counter];
                string sentence = string.Format("-{0}- __{1}__ **{2} - {3} [{4}]**" + Environment.NewLine, counter + 1, bm.PickType, bm.OsuBeatmap.Artist, bm.OsuBeatmap.Title, bm.OsuBeatmap.Version);
                if (counter % 2 == 0)
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

            if (string.IsNullOrEmpty(secondteam.Value))
            {
                secondteam.Value = "None";
            }

            e.Fields.Add(firstteam);
            e.Fields.Add(secondteam);

            return e;
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
