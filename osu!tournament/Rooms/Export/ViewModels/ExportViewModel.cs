using Caliburn.Micro;
using Osu.Api;
using Osu.Mvvm.Miscellaneous;
using Osu.Scores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Osu.Mvvm.Rooms.Export.ViewModels
{
    /// <summary>
    /// Represents the export view model
    /// </summary>
    public class ExportViewModel : PropertyChangedBase
    {
        #region Subclasses
        /// <summary>
        /// Type enum
        /// </summary>
        public enum Type
        {
            BBCode,
            Json
        }
        #endregion

        #region Attributes
        /// <summary>
        /// The room
        /// </summary>
        private Room room;

        /// <summary>
        /// Selected type
        /// </summary>
        private Type selected;

        /// <summary>
        /// The export text
        /// </summary>
        private string export;
        #endregion

        #region Constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="room">the room</param>
        public ExportViewModel(Room room)
        {
            this.room = room;
            selected = Type.BBCode;
            export = "";
        }
        #endregion

        #region Properties
        /// <summary>
        /// Types property
        /// </summary>
        public IEnumerable<Type> Types
        {
            get
            {
                return Enum.GetValues(typeof(Type)).OfType<Type>();
            }
        }

        /// <summary>
        /// Selected type property
        /// </summary>
        public Type SelectedType
        {
            get
            {
                return selected;
            }
            set
            {
                if (value != selected)
                {
                    selected = value;
                    NotifyOfPropertyChange(() => SelectedType);
                }
            }
        }

        /// <summary>
        /// Export property
        /// </summary>
        public string ExportText
        {
            get
            {
                return export;
            }
            set
            {
                if (value != export)
                {
                    export = value;
                    NotifyOfPropertyChange(() => ExportText);
                }
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Clears the export text
        /// </summary>
        public void Clear()
        {
            ExportText = "";
        }

        /// <summary>
        /// Copies the export text to the clipboard
        /// </summary>
        public void Copy()
        {
            Clipboard.SetText(export);
        }

        /// <summary>
        /// Exports the current room
        /// </summary>
        public async void Export()
        {
            await Dialog.ShowProgress(Utils.Properties.Resources.Wait_Title, Utils.Properties.Resources.Wait_Export);

            // No selected value
            switch (selected)
            {
                case Type.BBCode:
                    ExportBBCode();
                    break;
                case Type.Json:
                    ExportJson();
                    break;
                default:
                    break;
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Exports the current room as bbcode
        /// </summary>
        private async void ExportBBCode()
        {
            // Get the players and the games of the room
            Dictionary<long, Player> players = room.Players;
            List<OsuGame> games = room.OsuRoom.Games;
            Osu.Scores.Ranking ranking = room.Ranking;

            // No player
            if (players.Count == 0)
            {
                // Hide progress
                await Dialog.HideProgress();

                // Error
                Dialog.ShowDialog(Utils.Properties.Resources.Error_Title, Utils.Properties.Resources.Error_RoomHasNoPlayers);
                return;
            }

            // No games
            if (games.Count == 0)
            {
                // Hide progress
                await Dialog.HideProgress();

                // Error
                Dialog.ShowDialog(Utils.Properties.Resources.Error_Title, Utils.Properties.Resources.Error_RoomHasNoGames);
                return;
            }

            // Head to head
            if (ranking.type == Osu.Scores.Ranking.Type.HeadToHead)
            {
                // Create a new string
                string buffer = "[centre][heading]" + room.OsuRoom.Match.Name + "[/heading][/centre]\n\n";

                // Cast the ranking
                HeadToHead headtohead = (HeadToHead)ranking;

                // Sort the players
                IOrderedEnumerable<Player> ordered = players.Values.OrderByDescending(player => headtohead[player.OsuUser.Id]);

                // Add the scores
                buffer += "[list][*][heading]Scores:[/heading]\n\n";

                // We list and reset the players
                foreach (Player player in ordered)
                {
                    int points = headtohead[player.OsuUser.Id];

                    buffer += "[b][url=http://osu.ppy.sh/u/" + player.OsuUser.Id + "]" + player.OsuUser.Username + "[/url]: " + points + "[/b]\n";
                    headtohead[player.OsuUser.Id] = 0;
                }
                buffer += "[/list]\n\n";

                // Storing the link of the match
                buffer += "[list][*][heading][url=http://osu.ppy.sh/mp/" + room.OsuRoom.Match.MatchId + "]Lien Multi[/url][/heading][/list]\n\n";

                // We list the games played
                for (int i = 0; i < games.Count; i++)
                {
                    // Get the game
                    OsuGame game = games[i];

                    // Get the beatmap
                    OsuBeatmap beatmap = await OsuApi.GetBeatmap(game.BeatmapId, false);

                    // Open a box
                    buffer += "[centre][heading]Map n°" + (i + 1) + "[/heading][/centre]\n";
                    buffer += "[box=---]\n\n";

                    // Add the played beatmap
                    buffer += "[list][*][heading]Map jouée:[/heading]" + "[b][url=http://osu.ppy.sh/b/" + beatmap.BeatmapID + "]";
                    buffer += beatmap.Artist + " - " + beatmap.Title + " [" + beatmap.Version + "][/url][/b][/list]\n\n";

                    // Add the scores
                    buffer += "[list][*][heading]Scores:[/heading]\n";

                    // Get the scores
                    List<OsuScore> scores = games[i].Scores;

                    // Initialize the rank to 1
                    int rank = 1;

                    // For each score
                    foreach (OsuScore score in scores.OrderByDescending(entry => entry.Score))
                    {
                        Player player;

                        if (players.TryGetValue(score.UserId, out player))
                        {
                            // If the player passed the map
                            if (score.Pass == 1)
                            {
                                headtohead[player.OsuUser.Id] += headtohead.Settings[rank];

                                buffer += "[b][url=http://osu.ppy.sh/u/" + player.OsuUser.Id + "]" + player.OsuUser.Username + "[/url]: ";
                                buffer += score.Score + "[/b]. [color=green]+" + headtohead.Settings[rank] + "[/color]. Total: " + headtohead[player.OsuUser.Id] + "\n";

                                rank++;
                            }
                            // Else
                            else
                            {
                                // Just strike through and add a "Fail" at the end
                                buffer += "[s][b][url=http://osu.ppy.sh/u/" + player.OsuUser.Id + "]" + player.OsuUser.Username + "[/url]: ";
                                buffer += score.Score + "[/b]. Total: " + headtohead[player.OsuUser.Id] + "[/s] - Fail\n";
                            }
                        }
                    }

                    buffer += "[/list][/box]\n\n";
                }

                ExportText = buffer;
            }
            // Team vs
            else if (ranking.type == Osu.Scores.Ranking.Type.TeamVs)
            {
                // Hide progress
                await Dialog.HideProgress();

                Dialog.ShowDialog(Utils.Properties.Resources.Error_Title, Utils.Properties.Resources.Error_NotImplemented);
            }

            // Hide progress
            await Dialog.HideProgress();
        }

        /// <summary>
        /// Exports the room as a json string
        /// </summary>
        private async void ExportJson()
        {
            // Hide progress
            await Dialog.HideProgress();

            Dialog.ShowDialog(Utils.Properties.Resources.Error_Title, Utils.Properties.Resources.Error_NotImplemented);
        }
        #endregion
    }
}
