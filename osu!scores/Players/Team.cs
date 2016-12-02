using System.Collections.Generic;

namespace Osu.Scores
{
    /// <summary>
    /// Represents a player team
    /// </summary>
    public class Team
    {
        #region Attributes
        /// <summary>
        /// The team name
        /// </summary>
        protected string name;

        /// <summary>
        /// The team points
        /// </summary>
        protected int points;

        /// <summary>
        /// Points that have been added manually
        /// </summary>
        protected int added_points;
        #endregion

        #region Constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        public Team()
        {
            name = "No Name";
            points = 0;
            added_points = 0;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Name property
        /// </summary>
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
            }
        }

        /// <summary>
        /// Points property
        /// </summary>
        public int Points
        {
            get
            {
                return points;
            }
            set
            {
                points = value;
            }
        }

        public int PointAddition
        {
            get
            {
                return added_points;
            }
            set
            {
                if (value != added_points && value + points >= 0)
                {
                    added_points = value;
                }
            }
        }
        #endregion
    }
}
