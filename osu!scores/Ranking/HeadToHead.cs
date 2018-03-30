using Osu.Api;
using Osu.Scores.Status;
using osu_utils.DiscordModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Osu.Scores
{
    /// <summary>
    /// Represents an head to head ranking
    /// </summary>
    public class HeadToHead : Ranking
    {
        #region Constants
        /// <summary>
        /// The maximum number of players in the room
        /// </summary>
        public const int MaxPlayers = 16;

        /// <summary>
        /// No player constant
        /// </summary>
        public const int NoPlayer = 0;
        #endregion

        #region Attributes
        /// <summary>
        /// The parent room
        /// </summary>
        protected Room room;

        /// <summary>
        /// The points dictionary
        /// </summary>
        protected Dictionary<long, int> points;

        /// <summary>
        /// The points settings
        /// </summary>
        protected Dictionary<int, int> settings;

        /// <summary>
        /// The player order
        /// </summary>
        protected Dictionary<int, long> order;

        /// <summary>
        /// The number of played games
        /// </summary>
        protected int played;

        ///<summary>
        /// The counter for the rank number
        /// </summary>
        protected int rank;

        /// <summary>
        /// The list of players that passed
        /// </summary>
        protected List<OsuScore> players_pass;

        /// <summary>
        /// The list of players that failed
        /// </summary>
        protected List<OsuScore> players_fail;

        /// <summary>
        /// The first player of the last map
        /// </summary>
        protected OsuScore firstPlayer;
        #endregion

        #region Constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        public HeadToHead(Room room)
            : base(Type.HeadToHead)
        {
            // Initialize fields
            this.room = room;
            points = new Dictionary<long, int>();
            settings = new Dictionary<int, int>();
            order = new Dictionary<int, long>();
            players_pass = new List<OsuScore>();
            players_fail = new List<OsuScore>();

            // Add all the orders we need
            for (int i = 0; i < MaxPlayers; i++)
            {
                order[i] = NoPlayer;
                settings[i] = NoPlayer;
            }

            // Initialize other fields
            played = 0;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Room property
        /// </summary>
        public Room Room
        {
            get
            {
                return room;
            }
        }

        /// <summary>
        /// Points property
        /// </summary>
        public Dictionary<long, int> Points
        {
            get
            {
                return points;
            }
        }

        /// <summary>
        /// Points property
        /// </summary>
        /// <param name="i">the id of the player</param>
        /// <returns>the player points</returns>
        public int this[long i]
        {
            get
            {
                if (points.ContainsKey(i))
                    return points[i];
                else
                    return NoPlayer;
            }
            set
            {
                points[i] = value;
            }
        }

        /// <summary>
        /// Settings property
        /// </summary>
        public Dictionary<int, int> Settings
        {
            get
            {
                return settings;
            }
        }

        /// <summary>
        /// Order property
        /// </summary>
        public Dictionary<int, long> Order
        {
            get
            {
                return order;
            }
        }

        /// <summary>
        /// Get the first player
        /// </summary>
        public long First
        {
            get
            {
                // Get the first player id that is not 0 and corresponding to a playing player
                foreach (long id in order.Values.ToList())
                    if (id != 0 && room.Players[id].Playing)
                        return id;

                // Get the first player
                return NoPlayer;
            }
        }

        /// <summary>
        /// Get the current player
        /// </summary>
        public long Current
        {
            get
            {
                // Store a list of players id
                List<long> players = new List<long>();

                // For each player id
                foreach (long id in order.Values.ToList())
                    // Add id that are not 0 and corresponding to a playing player
                    if (id != 0 && room.Players[id].Playing)
                        players.Add(id);

                // No players or no map played from the pool yet
                if (players.Count == 0 || played == 0)
                    // 0
                    return NoPlayer;

                // Return the right player
                return players[(played - 1) % players.Count];
            }
        }

        /// <summary>
        /// Get the next player
        /// </summary>
        public long Next
        {
            get
            {
                // Store a list of players id
                List<long> players = new List<long>();

                // For each player id
                foreach (long id in order.Values.ToList())
                    // Add id that are not 0 and corresponding to a playing player
                    if (id != 0 && room.Players[id].Playing)
                        players.Add(id);

                // No players
                if (players.Count == 0)
                    // 0
                    return NoPlayer;

                // Return the right player
                return players[played % players.Count];
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
                return false;
            }
        }

        /// <summary>
        /// Is finished property
        /// </summary>
        public override bool IsFinished
        {
            get
            {
                return false;
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Updates the players scores
        /// </summary>
        /// <param name="scores">the scores</param>
        private void UpdateScores(List<OsuScore> scores)
        {
            // For each score
            foreach (OsuScore score in scores)
            {
                // Get the player from the room
                Player player = room[score.UserId];

                // If the player is playing and we have a setting for this rank
                if (player != null && player.Playing && settings.ContainsKey(rank))
                {
                    // If the player already has some points
                    if (points.ContainsKey(score.UserId))
                        // Increment the player points, and increment the rank for this game
                        points[score.UserId] += settings[rank++];
                    // Else
                    else
                        // Initialize the player points
                        points[score.UserId] = settings[rank++];
                }
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

            // Reset all the player points
            foreach (long id in points.Keys.ToList())
                points[id] = 0;
        }

        /// <summary>
        /// Called on game scan
        /// </summary>
        /// <param name="game"></param>
        public override void OnStartGame(OsuGame game)
        {
            // Increment the playcount
            played++;

            // Reset the rank count
            rank = 0;
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
                // Add him to the passed list
                players_pass.Add(score);
            // Else
            else
                // Add him to the failed list
                players_fail.Add(score);
        }

        /// <summary>
        /// Called on end game scan
        /// </summary>
        /// <param name="game">the game</param>
        public override void OnEndGame(OsuGame game)
        {
            // Update the players that passed first
            UpdateScores(players_pass);

            // Then the players that failed
            UpdateScores(players_fail);

            if (players_pass.Count > 0)
                firstPlayer = players_pass[0];
            else
                firstPlayer = players_fail[0];

            // And finally clear the scores lists
            players_pass.Clear();
            players_fail.Clear();
        }

        /// <summary>
        /// Returns the ranking and informations as a string
        /// </summary>
        /// <returns>a list of string</returns>
        public override List<string> GetStatus()
        {
            List<string> sentences = new List<string>();

            // Counting the rank
            int i = 1;

            // start of the first string
            sentences.Add("Ranking of the room :");
            String res = "";
            foreach (KeyValuePair<long, int> userWithScore in points.OrderByDescending(entry => entry.Value))
            {
                res += String.Format("(" + i + ") {0}: {1} pt(s) | ",
                        room.Players[userWithScore.Key].OsuUser.Username,
                        userWithScore.Value);
                i++;
            }

            sentences.Add(res.Substring(0, res.Length - 2));
            if (this.Next != 0)
            {
                sentences.Add("Next player to pick: " + room.Players[this.Next].OsuUser.Username);
            }


            return sentences;
        }

        public override Embed GetDiscordStatus()
        {
            // Grabbing the osu beatmap
            OsuBeatmap obm = null;
            Beatmap bm = null;

            if (room.Mappool != null)
            {
                var game = room.OsuRoom.Games.LastOrDefault();
                if(game != null)
                {
                    room.Mappool?.Pool.TryGetValue(game.BeatmapId, out bm);
                    obm = bm.OsuBeatmap;
                }
            }
            else
            {
                var game = room.OsuRoom.Games.LastOrDefault();
                if (game != null)
                {
                    Task<OsuBeatmap> beatmap = OsuApi.GetBeatmap(game.BeatmapId, false);
                    obm = beatmap.Result;
                }
            }

            // If there is one, we build discord Embed
            if(obm != null)
            {
                Embed embed = new Embed();
                embed.Author = new Author { Name = this.Room.Name, Url = "https://osu.ppy.sh/community/matches/" + this.room.Id, IconUrl = "https://cdn0.iconfinder.com/data/icons/fighting-1/258/brawl003-512.png" };
                embed.Color = "6729778";
                embed.Title = string.Format("{0} won the{1}pick with {2} points", room.Players[firstPlayer.UserId].Username, (bm == null ? " " : " __" + bm.PickType.ToString() + "__ "), string.Format("{0:n0}", firstPlayer.Score));
                embed.Thumbnail = new Image { Url = "https://b.ppy.sh/thumb/" + obm.BeatmapSetID + "l.jpg" };
                embed.Description = string.Format("**{0} - {1} [{2}]**", obm.Artist, obm.Title, obm.Version);
                embed.Fields = new List<Field>();

                string players = string.Empty;
                string points = string.Empty;

                Points.OrderByDescending(x => x.Value).ToList().ForEach(y => { players += room.Players[y.Key].Username + Environment.NewLine; points += y.Value + " pts" + Environment.NewLine; });

                embed.Fields.Add(new Field() { Name = "User", Value = players, Inline = true });
                embed.Fields.Add(new Field() { Name = "Points", Value = points, Inline = true });

                return embed;
            }

            return null;
        }
        #endregion
    }
}
