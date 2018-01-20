using log4net;
using Osu.Api;
using Osu.Scores.Status;
using Osu.Utils;
using Osu.Utils.Info;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Osu.Scores
{
    /// <summary>
    /// Represents a room wrapper
    /// </summary>
    public class Room
    {
        #region Constants
        /// <summary>
        /// The dictionary of rooms
        /// </summary>
        protected static Dictionary<long, Room> rooms;

        /// <summary>
        /// The regex used to get the team name
        /// </summary>
        protected static readonly Regex TeamRegex = new Regex("^([\\w+ ]*): \\(([\\w+ ]*)\\)\\s\\w+\\s\\(([\\w+ ]*)\\)$");

        /// <summary>
        /// The logger
        /// </summary>
        protected static ILog log = LogManager.GetLogger("osu!scores");

        protected OsuMode wctype;
        #endregion

        #region Attribute
        /// <summary>
        /// The linked osu!room
        /// </summary>
        protected OsuRoom osu_room;

        /// <summary>
        /// The list of blacklisted games
        /// </summary>
        protected List<long> blacklist;

        /// <summary>
        /// The used ranking
        /// </summary>
        protected Ranking ranking;

        /// <summary>
        /// The player list
        /// </summary>
        protected Dictionary<long, Player> players;

        /// <summary>
        /// The list of irc targets
        /// </summary>
        protected List<string> irc_targets;

        /// <summary>
        /// The mappool of the room
        /// </summary>
        protected Mappool mappool;

        /// <summary>
        /// If the game blacklist is in manual mode or not
        /// </summary>
        protected bool manual;

        /// <summary>
        /// If the room allows players to use commands
        /// </summary>
        protected bool commands;

        /// <summary>
        /// The room's status
        /// </summary>
        protected RoomStatus status;

        /// <summary>
        /// If the players are currently playing a map
        /// </summary>
        protected bool playing;

        /// <summary>
        /// The BO of the room
        /// </summary>
        protected string mode;

        protected int countBans;

        protected bool isStreamed;

        protected List<string> roomMessages;

        protected RoomConfiguration roomConfiguration;
        #endregion

        #region Constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        public Room(OsuRoom osu_room)
        {
            this.osu_room = osu_room;
            blacklist = new List<long>();
            ranking = null;
            players = new Dictionary<long, Player>();
            irc_targets = new List<string>();
            manual = true;
            commands = false;
            status = RoomStatus.NotStarted;
            isStreamed = false;
            roomMessages = new List<string>();
            roomConfiguration = new RoomConfiguration() { TeamMode = (OsuTeamType)int.Parse(InfosHelper.TourneyInfos.TeamMode), ScoreMode = (OsuScoringType)int.Parse(InfosHelper.TourneyInfos.ScoreMode), RoomSize = InfosHelper.TourneyInfos.RoomSize };
            mode = Cache.GetCache("osu!options.db").Get("mode", "3");
            string t = Cache.GetCache("osu!options.db").Get("wctype", "Standard");
            string mp = Cache.GetCache("osu!options.db").Get("defaultmappool", "");

            switch (t)
            {
                case "Standard":
                    wctype = OsuMode.Standard;
                    break;
                case "Taiko":
                    wctype = OsuMode.Taiko;
                    break;
                case "CTB":
                    wctype = OsuMode.CTB;
                    break;
                case "Mania":
                    wctype = OsuMode.Mania;
                    break;
            }

            mappool = Mappool.Mappools.FirstOrDefault(x => x.Name == mp);
            if(mappool != null && mp != null)
            {
                manual = false;
            }

        }
        #endregion

        #region Properties
        /// <summary>
        /// wctype property
        /// </summary>
        public OsuMode Wctype
        {
            get
            {
                return wctype;
            }
            set
            {
                if (wctype != value)
                {
                    wctype = value;
                }
            }
        }

        /// <summary>
        /// Id property
        /// </summary>
        public long Id
        {
            get
            {
                return osu_room.Match.MatchId;
            }
        }

        /// <summary>
        /// Name property
        /// </summary>
        public string Name
        {
            get
            {
                return osu_room.Match.Name;
            }
        }

        /// <summary>
        /// osu!room property
        /// </summary>
        public OsuRoom OsuRoom
        {
            get
            {
                return osu_room;
            }
            set
            {
                osu_room = value;
            }
        }

        /// <summary>
        /// Blacklist property
        /// </summary>
        public List<long> Blacklist
        {
            get
            {
                return blacklist;
            }
        }

        /// <summary>
        /// Ranking property
        /// </summary>
        public Ranking Ranking
        {
            get
            {
                return ranking;
            }
        }

        /// <summary>
        /// Players property
        /// </summary>
        public Dictionary<long, Player> Players
        {
            get
            {
                return players;
            }
        }

        /// <summary>
        /// Player getter
        /// </summary>
        /// <param name="i">the player id</param>
        /// <returns>the player</returns>
        public Player this[long i]
        {
            get
            {
                if (players.ContainsKey(i))
                    return players[i];
                else
                    return null;
            }
            set
            {
                players[i] = value;
            }
        }

        /// <summary>
        /// IRC Targets properties
        /// </summary>
        public List<string> IrcTargets
        {
            get
            {
                return irc_targets;
            }
        }

        /// <summary>
        /// Mappool properties
        /// </summary>
        public Mappool Mappool
        {
            get
            {
                return mappool;
            }
            set
            {
                mappool = value;
            }
        }

        /// <summary>
        /// Manual property
        /// </summary>
        public bool Manual
        {
            get
            {
                return manual;
            }
            set
            {
                manual = value;
            }
        }

        /// <summary>
        /// Commands property
        /// </summary>
        public bool Commands
        {
            get
            {
                return commands;
            }
            set
            {
                commands = value;
            }
        }

        /// <summary>
        /// Commands property
        /// </summary>
        public RoomStatus Status
        {
            get
            {
                return status;
            }
            set
            {
                status = value;
            }
        }

        /// <summary>
        /// Playing property
        /// </summary>
        public bool Playing
        {
            get
            {
                return playing;
            }
            set
            {
                playing = value;
            }
        }

        /// <summary>
        /// Best of mode property
        /// </summary>
        public string Mode
        {
            get
            {
                return mode;
            }
            set
            {
                if (mode != value)
                {
                    mode = value;
                }
            }
        }

        public int CountBans
        {
            get
            {
                return countBans;
            }
            set
            {
                if(value >=0 && value <= 2)
                {
                    countBans = value;
                }
            }
        }

        public bool IsStreamed
        {
            get
            {
                return isStreamed;
            }
            set
            {
                if (value != isStreamed)
                    isStreamed = value;
            }
        }

        public List<string> RoomMessages
        {
            get
            {
                return roomMessages;
            }
        }

        public RoomConfiguration RoomConfiguration
        {
            get
            {
                return roomConfiguration;
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Initializes the ranking type based on the given type
        /// </summary>
        /// <param name="type"></param>
        private void InitializeRankingType(Ranking.Type type)
        {
            // Try to match the room name with the regex
            Match match = TeamRegex.Match(Name);

            HeadToHead head_to_head = null;
            TeamVs team_vs = null;

            switch (type)
            {
                case Ranking.Type.HeadToHead:
                    // Head to head, just create the thing
                    head_to_head = new HeadToHead(this);
                    break;
                case Ranking.Type.TeamVs:
                    // Team vs
                    team_vs = new TeamVs(this);

                    // Try to match the regex to get team names
                    if (match.Success)
                    {
                        // Get the team names
                        team_vs.Blue.Name = match.Groups[3].Value;
                        team_vs.Red.Name = match.Groups[2].Value;
                    }
                    break;
                case Ranking.Type.Auto:
                    // If the regex didn't match
                    if (!match.Success)
                        // Head to head
                        head_to_head = new HeadToHead(this);
                    // Else
                    else
                    {
                        // Create a new teamvs
                        team_vs = new TeamVs(this);

                        // Get the team names
                        team_vs.Blue.Name = match.Groups[3].Value;
                        team_vs.Red.Name = match.Groups[2].Value;
                    }
                    break;
                default:
                    break;
            }

            if (head_to_head != null)
                ranking = head_to_head;
            else if (team_vs != null)
                ranking = team_vs;
            else
            {
                log.Error("Ranking type not valid or not detected");
                ranking = null;
            }
        }

        /// <summary>
        /// Updates the players
        /// </summary>
        /// <returns>a task</returns>
        private async Task UpdatePlayers()
        {
            // For each game
            foreach (OsuGame game in osu_room.Games)
            {
                // For each score
                foreach (OsuScore score in game.Scores)
                {
                    // Get the player id
                    long id = score.UserId;

                    // If the room does not contain this player yet
                    if (!players.ContainsKey(id))
                    {
                        // Get the player from the api

                        OsuUser user = await OsuApi.GetUser(id, wctype, false);

                        // If we have an user
                        if (user != null)
                        {
                            // Create a new player
                            Player player = new Player();

                            // Set some values
                            player.OsuUser = user;
                            player.Playing = true;

                            // Store this player
                            players[id] = player;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Checks if a map is valid
        /// </summary>
        /// <param name="beatmap">the beatmap</param>
        /// <returns>true/false</returns>
        private bool IsValid(OsuGame game)
        {
            // Room is in manual mode
            if (manual)
                // Check if the game is in the blacklist
                return !blacklist.Contains(game.GameId);
            // Room is in mappool mode
            else
                // Check if mappool is set and the beatmap is in the mappool
                return mappool != null && mappool.Pool.ContainsKey(game.BeatmapId);
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Updates the room
        /// </summary>
        public async Task Update(bool isIrcTrigger)
        {
            log.Info("Updating room " + osu_room.Match.MatchId);

            // Update the osu!room
            if(isIrcTrigger)
                await Task.Delay(4000);

            osu_room = await OsuApi.GetRoom(osu_room.Match.MatchId);

            // Update the player list
            await UpdatePlayers();

            // If we don't have a ranking yet
            if (ranking == null)
                // Initialize it
                InitializeRankingType(Scores.Ranking.Type.Auto);

            // Match is not started yet
            status = RoomStatus.NotStarted;

            // If the ranking is not null
            if (ranking != null)
            {
                // Call on update
                ranking.OnUpdate();

                // For each game
                foreach (OsuGame game in osu_room.Games)
                {
                    // If the game is valid
                    if (IsValid(game))
                    {
                        // They're currently playing the match
                        status = RoomStatus.Playing;

                        // If the game doesn't have scores yet
                        if (game.Scores.Count == 0)
                            // They're playing a map
                            playing = true;
                        // Else
                        else
                            // They're waiting for a map to be picked
                            playing = false;

                        // Call on start game
                        ranking.OnStartGame(game);

                        // For each score
                        foreach (OsuScore score in game.Scores.OrderByDescending(entry => entry.Score))
                            // Call on score
                            ranking.OnScore(game, score);

                        // Call on end game
                        ranking.OnEndGame(game);
                    }
                    // Else
                    else
                        // The currently played game is a warmup
                        status = RoomStatus.Warmup;
                }
            }

            // If they're currently playing a map
            if (status == RoomStatus.Playing)
            {
                // Check if they're playing a tiebreaker or if the match is finished
                if (ranking.IsTiebreaker)
                    status = RoomStatus.Tiebreaker;
                else if (ranking.IsFinished)
                    status = RoomStatus.Finished;
            }
        }

        /// <summary>
        /// Changes the room ranking type
        /// </summary>
        public void ChangeRankingType()
        {
            if (ranking.type == Ranking.Type.HeadToHead)
                InitializeRankingType(Ranking.Type.TeamVs);
            else
                InitializeRankingType(Ranking.Type.HeadToHead);
        }
        #endregion

        #region Static Methods
        /// <summary>
        /// Initializes the rooms
        /// </summary>
        public static async Task Initialize()
        {
            rooms = new Dictionary<long, Room>();

            Cache cache = Cache.GetCache("osu!cache.db");
            Dictionary<long, OsuRoom> matches = cache.GetObject<Dictionary<long, OsuRoom>>("rooms", new Dictionary<long, OsuRoom>());
            Dictionary<long, Api.OsuTeam> teamvsfirst = cache.GetObject<Dictionary<long, Api.OsuTeam>>("teamvs_isbluefirstpicking", new Dictionary<long, Api.OsuTeam>());

            OsuTeam firstteam;

            foreach (var match in matches)
            {
                rooms[match.Key] = new Room(match.Value);
                await rooms[match.Key].Update(false);
                if(rooms[match.Key].Ranking.type == Ranking.Type.TeamVs && teamvsfirst.TryGetValue(match.Key, out firstteam))
                {
                    ((TeamVs)rooms[match.Key].Ranking).First = firstteam;
                }
            }
        }

        /// <summary>
        /// Saves the mappools list in the cache
        /// </summary>
        public static void Save()
        {
            Cache cache = Cache.GetCache("osu!cache.db");
            Dictionary<long, OsuRoom> matches = new Dictionary<long, OsuRoom>();
            Dictionary<long, Api.OsuTeam> teamfirstdic = new Dictionary<long, Api.OsuTeam>();
            foreach (var r in rooms)
            {
                matches.Add(r.Key, r.Value.OsuRoom);
                if(r.Value.Ranking.type == Ranking.Type.TeamVs)
                {
                    teamfirstdic.Add(r.Key, ((TeamVs)r.Value.Ranking).First);
                }
            }
            cache["rooms"] = matches;
            cache["teamvs_isbluefirstpicking"] = teamfirstdic;
        }

        /// <summary>
        /// Rooms property
        /// </summary>
        public static Dictionary<long, Room> Rooms
        {
            get
            {
                return rooms;
            }
        }

        /// <summary>
        /// Returns a room based on its id
        /// </summary>
        /// <param name="id">the room id</param>
        /// <returns>the room</returns>
        public static async Task<Room> Get(long id)
        {
            if (!rooms.ContainsKey(id))
            {
                OsuRoom osu_room = await OsuApi.GetRoom(id);

                if (osu_room == null)
                    return null;
                else
                    rooms[id] = new Room(osu_room);
            }

            return rooms[id];
        }

        /// <summary>
        /// Removes a room from the list
        /// </summary>
        /// <param name="id">the id of the room to remove</param>
        public static void Remove(long id)
        {
            if (rooms.ContainsKey(id))
                rooms.Remove(id);
        }
        #endregion
    }
}
