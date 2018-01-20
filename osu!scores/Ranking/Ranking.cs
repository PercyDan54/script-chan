using Osu.Api;
using osu_utils.DiscordModels;
using System.Collections.Generic;

namespace Osu.Scores
{
    /// <summary>
    /// Represents a ranking calculator
    /// </summary>
    public abstract class Ranking
    {
        #region Subclasses
        /// <summary>
        /// The different types of ranking
        /// </summary>
        public enum Type
        {
            TeamVs,
            HeadToHead,
            Auto
        }
        #endregion

        #region Attributes
        /// <summary>
        /// The ranking type
        /// </summary>
        public readonly Type type;
        #endregion

        #region Constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="type">the ranking type</param>
        public Ranking(Type type)
        {
            this.type = type;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Played property
        /// </summary>
        public abstract int Played { get; }

        /// <summary>
        /// Is tiebreaker property
        /// </summary>
        public abstract bool IsTiebreaker { get; }

        /// <summary>
        /// Is finished property
        /// </summary>
        public abstract bool IsFinished { get; }
        #endregion

        #region Public Methods
        /// <summary>
        /// Called on room update
        /// </summary>
        /// <returns>a task</returns>
        public abstract void OnUpdate();

        /// <summary>
        /// Called when a game is encountered
        /// </summary>
        /// <param name="match">the game</param>
        public abstract void OnStartGame(OsuGame game);

        /// <summary>
        /// Called when a score is encountered
        /// </summary
        /// <param name="game">the game</param>
        /// <param name="score">the score</param>
        /// <returns>a task</returns>
        public abstract void OnScore(OsuGame game, OsuScore score);

        /// <summary>
        /// Called when all the scores of a game are scanned
        /// </summary>
        /// <param name="game">the game</param>
        public abstract void OnEndGame(OsuGame game);

        /// <summary>
        /// Returns the ranking and informations as a string
        /// </summary>
        /// <returns>a list of string</returns>
        public abstract List<string> GetStatus();

        public abstract Embed GetDiscordStatus();

        #endregion
    }
}
