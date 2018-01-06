using Osu.Api;
using Osu.Scores.Status;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Osu.Scores
{
    /// <summary>
    /// Represents a team vs ranking
    /// </summary>
    public class TeamVs : Ranking
    {
        #region Attributes
        /// <summary>
        /// The room
        /// </summary>
        protected Room room;

        /// <summary>
        /// The teams
        /// </summary>
        protected Dictionary<OsuTeam, Team> teams;

        /// <summary>
        /// The current team
        /// </summary>
        protected OsuTeam first;

        /// <summary>
        /// The number of games played
        /// </summary>
        protected int played;

        /// <summary>
        /// The current score of the red team
        /// </summary>
        protected long red_score;

        /// <summary>
        /// The current score of the blue team
        /// </summary>
        protected long blue_score;
        private bool abortHappened;
        #endregion

        #region Constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="room">the room</param>
        public TeamVs(Room room)
            : base(Type.TeamVs)
        {
            // Get the room
            this.room = room;

            // Initialize the team dictionary
            teams = new Dictionary<OsuTeam, Team>();

            // Initialize the teams
            teams[OsuTeam.Blue] = new Team();
            teams[OsuTeam.Red] = new Team();

            // Initialize other fields
            first = OsuTeam.Blue;
            played = 0;
            red_score = 0;
            blue_score = 0;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Red team property
        /// </summary>
        public Team Red
        {
            get
            {
                return teams[OsuTeam.Red];
            }
        }

        /// <summary>
        /// Blue team property
        /// </summary>
        public Team Blue
        {
            get
            {
                return teams[OsuTeam.Blue];
            }
        }

        /// <summary>
        /// First property
        /// </summary>
        public OsuTeam First
        {
            get
            {
                return first;
            }
            set
            {
                first = value;
            }
        }

        /// <summary>
        /// First team property
        /// </summary>
        public Team FirstTeam
        {
            get
            {
                if (teams.ContainsKey(first))
                    return teams[first];
                else
                    return null;
            }
        }

        /// <summary>
        /// Current team property
        /// </summary>
        public Team CurrentTeam
        {
            get
            {
                int count = (played - 1) % 2;

                if (count == 0)
                {
                    return teams[first];
                }
                else
                {
                    if (first == OsuTeam.Red)
                        return teams[OsuTeam.Blue];
                    else
                        return teams[OsuTeam.Red];
                }
            }
        }

        /// <summary>
        /// Next team property
        /// </summary>
        public Team NextTeam
        {
            get
            {
                int count = played % 2;

                if (count == 0)
                {
                    return teams[first];
                }
                else
                {
                    if (first == OsuTeam.Red)
                        return teams[OsuTeam.Blue];
                    else
                        return teams[OsuTeam.Red];
                }
            }
        }

        /// <summary>
        /// Played property
        /// </summary>
        public override int Played
        {
            get
            {
                return played;
            }
        }

        /// <summary>
        /// Is tiebreaker property
        /// </summary>
        public override bool IsTiebreaker
        {
            get
            {
                int limit = int.Parse(room.Mode) / 2;

                return (Blue.Points + Blue.PointAddition) == limit && (Red.Points + Red.PointAddition) == limit;
            }
        }

        /// <summary>
        /// Is finished property
        /// </summary>
        public override bool IsFinished
        {
            get
            {
                int limit = int.Parse(room.Mode) / 2;

                return (Blue.Points + Blue.PointAddition) == limit + 1 || (Red.Points + Red.PointAddition) == limit + 1;
            }
        }

        public bool DidAbortHappened
        {
            set
            {
                abortHappened = value;
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Called on room update
        /// </summary>
        public override void OnUpdate()
        {
            // Reset the playcount
            played = 0;

            // Reset team points
            Red.Points = 0;
            Blue.Points = 0;
        }

        /// <summary>
        /// Called on game scan
        /// </summary>
        /// <param name="game">the game</param>
        public override void OnStartGame(OsuGame game)
        {
            // Increment the playcount
            played++;

            // Scores holders
            red_score = 0;
            blue_score = 0;
        }

        /// <summary>
        /// Called on score scan
        /// </summary>
        /// <param name="game">the game</param>
        /// <param name="score">the score</param>
        public override void OnScore(OsuGame game, OsuScore score)
        {
            // If the player passed the map
            if (score.Pass == 1)
            {
                // Get the player from the room
                Player player = room[score.UserId];

                // If the player exists and is playing
                if (player != null && player.Playing)
                {
                    // If we have a team and the team contains the player
                    if (teams.ContainsKey(score.Team))
                    {
                        switch (score.Team)
                        {
                            case OsuTeam.Blue:
                                // Increment blue team score
                                blue_score += score.Score;
                                break;
                            case OsuTeam.Red:
                                // Increment red team score
                                red_score += score.Score;
                                break;
                            default:
                                // Shouldn't happen
                                throw new SystemException("TeamVS - Update: Match type is not TeamVS :(");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Called on end game scan
        /// </summary>
        /// <param name="game">the game</param>
        public override void OnEndGame(OsuGame game)
        {
            // If red score is greater
            if (red_score > blue_score)
            {
                // +1 point to the red
                Red.Points += 1;
            }
            // If blue score is greater
            else if (red_score < blue_score)
            {
                // +1 point to the blue
                Blue.Points += 1;
            }
        }

        /// <summary>
        /// Returns the ranking and informations as a string
        /// </summary>
        /// <returns>a list of string</returns>
        public override List<string> GetStatus()
        {
            List<string> sentences = new List<string>();
            switch(room.Status)
            {
                case RoomStatus.NotStarted:
                    sentences.Add("Waiting for the match to start...");
                    break;

                case RoomStatus.Warmup:
                    sentences.Add("Playing Warmup...");
                    break;

                case RoomStatus.Playing:
                    sentences.Add(GetScoreFormatted());
                    sentences.Add("Next team to pick: " + (abortHappened ? CurrentTeam.Name : NextTeam.Name) + ".");
                    break;
                case RoomStatus.Tiebreaker:
                    sentences.Add(GetScoreFormatted());
                    sentences.Add("The result is a tie. We have to play the tiebreaker.");
                    break;
                case RoomStatus.Finished:
                    sentences.Add(GetScoreFormatted());
                    sentences.Add("**" + (DidBlueTeamWin() ? teams[OsuTeam.Blue].Name : teams[OsuTeam.Red].Name) + " wins the match.**");
                    break;
            }
            return sentences;
        }

        public override List<String> GetDiscordStatus()
        {
            List<string> sentences = new List<string>();

            Beatmap bm = null;
            room.Mappool?.Pool.TryGetValue(room.OsuRoom.Games.Last().BeatmapId, out bm);

            if(bm != null)
            {
                IEnumerable<IGrouping<OsuTeam, OsuScore>> test = room.OsuRoom.Games.Last().Scores.OrderByDescending(x => x.Score).GroupBy(x => x.Team);
                var bpb = test.Single(g => g.Key == OsuTeam.Blue);
                var bluescore = bpb.Where(x => x.Pass == 1).Sum(x => x.Score);

                var bpb2 = test.Single(g => g.Key == OsuTeam.Red);
                var redscore = bpb2.Where(x => x.Pass == 1).Sum(x => x.Score);
                bool didCurrentTeamWon;

                if(Blue == CurrentTeam)
                {
                    if (bluescore > redscore)
                    {
                        didCurrentTeamWon = true;
                    }
                    else
                    {
                        didCurrentTeamWon = false;
                    }
                }
                else
                {
                    if (bluescore < redscore)
                    {
                        didCurrentTeamWon = true;

                    }
                    else
                    {
                        didCurrentTeamWon = false;
                    }
                }

                if(abortHappened)
                {
                    didCurrentTeamWon = !didCurrentTeamWon;
                }

                sentences.Add(String.Format("{0} " + (didCurrentTeamWon ? "won" : "lost") + " their pick `{1}` {2} - {3} [{4}]", (abortHappened ? NextTeam.Name : CurrentTeam.Name), bm.PickType, bm.OsuBeatmap.Artist, bm.OsuBeatmap.Title, bm.OsuBeatmap.Version));

                sentences.AddRange(GetStatus());
                IOrderedEnumerable<OsuScore> scores = room.OsuRoom.Games.Last().Scores.OrderByDescending(x => x.Score);
                OsuScore mvp = scores.First();

                Player mvpPlayer;

                room.Players.TryGetValue(mvp.UserId, out mvpPlayer);

                if(mvpPlayer != null)
                {
                    sentences.Add(String.Format("MVP : {0} from {1} with {2} points", mvpPlayer.Username, mvpPlayer.OsuUser.Country, mvp.Score));
                }

                return sentences;
            }
            return null;
        }

        public string GetScoreFormatted()
        {
            String res = String.Format("{0} : {1} | {2} : {3}",
                        teams[OsuTeam.Red].Name,
                        teams[OsuTeam.Red].Points + teams[OsuTeam.Red].PointAddition,
                        teams[OsuTeam.Blue].Points + teams[OsuTeam.Blue].PointAddition,
                        teams[OsuTeam.Blue].Name);
            return res;
        }

        public bool DidBlueTeamWin()
        {
            return (teams[OsuTeam.Blue].Points + teams[OsuTeam.Blue].PointAddition) > (teams[OsuTeam.Red].Points + teams[OsuTeam.Red].PointAddition);
        }
        #endregion
    }
}
