using Osu.Api;
using Osu.Scores.Status;
using osu_utils.DiscordModels;
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

                return (Blue.Points + Blue.PointAddition) >= limit + 1 || (Red.Points + Red.PointAddition) >= limit + 1;
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
                    // If this is a 1v1 match selection without teamVS mode
                    else
                    {
                        switch(score.Slot)
                        {
                            case 1:
                                red_score += score.Score;
                                break;
                            case 2:
                                blue_score += score.Score;
                                break;
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

        public override Embed GetDiscordStatus()
        {
            // Get the beatmap object
            Beatmap bm = null;
            room.Mappool?.Pool.TryGetValue(room.OsuRoom.Games.Last().BeatmapId, out bm);

            if(bm != null)
            {
                bool didCurrentTeamWon;

                // Did the team who picked the map won
                if((Blue == CurrentTeam && blue_score > red_score) || (Red == CurrentTeam && blue_score < red_score))
                {
                    didCurrentTeamWon = true;
                }
                else
                {
                    didCurrentTeamWon = false;
                }

                // Difference of points between the two teams
                var diffPoint = (blue_score > red_score) ? blue_score - red_score : red_score - blue_score;

                // ??
                if(abortHappened)
                {
                    didCurrentTeamWon = !didCurrentTeamWon;
                }

                // Retrieve the player with the best score on the map
                IOrderedEnumerable<OsuScore> scores = room.OsuRoom.Games.Last().Scores.OrderByDescending(x => x.Score);
                OsuScore mvp = scores.First();
                Player mvpPlayer;
                room.Players.TryGetValue(mvp.UserId, out mvpPlayer);

                // Generate the embed
                Embed embed = new Embed();
                embed.Author = new Author { Name = string.Format("{0} VS {1}", teams[OsuTeam.Red].Name, teams[OsuTeam.Blue].Name), Url = "https://osu.ppy.sh/community/matches/" + this.room.Id, IconUrl = "https://cdn0.iconfinder.com/data/icons/fighting-1/258/brawl003-512.png" };
                embed.Color = didCurrentTeamWon ? "6729778" : "14177041";
                embed.Title = string.Format("{0} " + (didCurrentTeamWon ? "won" : "lost") + " their __{1}__ pick by {2} points", (abortHappened ? NextTeam.Name : CurrentTeam.Name), bm.PickType, diffPoint);
                embed.Thumbnail = new Image { Url = didCurrentTeamWon ? "https://cdn.discordapp.com/attachments/130304896581763072/400388818127290369/section-pass.png" : "https://cdn.discordapp.com/attachments/130304896581763072/400388814213873666/section-fail.png" };
                embed.Description = string.Format("**{0} - {1} [{2}]**", bm.OsuBeatmap.Artist, bm.OsuBeatmap.Title, bm.OsuBeatmap.Version);
                embed.Fields = new List<Field>();
                embed.Fields.Add(new Field() { Name = teams[OsuTeam.Red].Name, Value = (teams[OsuTeam.Red].Points + teams[OsuTeam.Red].PointAddition).ToString(), Inline = true });
                embed.Fields.Add(new Field() { Name = teams[OsuTeam.Blue].Name, Value = (teams[OsuTeam.Blue].Points + teams[OsuTeam.Blue].PointAddition).ToString(), Inline = true });
                if (mvpPlayer != null)
                {
                    embed.Fields.Add(new Field() { Name = "MVP", Value = string.Format(":flag_{1}: **{0}** with {2} points", mvpPlayer.Username, mvpPlayer.OsuUser.Country.ToLower(), mvp.Score) });
                }

                embed.Fields.Add(new Field() { Name = "Status", Value = GetStatus().Last() + " " + (room.Status == RoomStatus.Finished ? ":clap:" : ":loudspeaker:") });

                if (room.Status == RoomStatus.Finished)
                {
                    embed.Image = new Image { Url = "https://78.media.tumblr.com/b94193615145d12bfb64aa77b677269e/tumblr_njzqukOpBP1ti1gm1o1_500.gif" };
                    embed.Thumbnail = new Image { Url = "https://cdn.discordapp.com/attachments/130304896581763072/411660079771811870/crown.png" };
                    embed.Color = "10494192";
                }
                
                return embed;
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
