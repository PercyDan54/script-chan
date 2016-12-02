using System;
using System.Diagnostics;
using System.Windows.Media.Animation;

namespace Osu.Utils
{
    /// <summary>
    /// Represents a timed counter
    /// </summary>
    public class Counter
    {
        /// <summary>
        /// The clock
        /// </summary>
        private static Stopwatch clock;

        /// <summary>
        /// The last action time
        /// </summary>
        private long last;

        /// <summary>
        /// The interval between actions
        /// </summary>
        private long interval;

        /// <summary>
        /// Constructor
        /// </summary>
        public Counter()
        {
            last = 0;
            interval = 0;
        }

        /// <summary>
        /// Is possible property
        /// </summary>
        public bool IsPossible
        {
            get
            {
                // Get the current time
                long current = clock.ElapsedMilliseconds;

                // Get the elapsed time
                double elapsed = current - last;

                // If the elapsed time is greater than the set interval
                if (elapsed > interval)
                {
                    // Change the last action time
                    last = current;

                    // The action is possible
                    return true;
                }
                // Else
                else
                    // The action is not possible
                    return false;
            }
        }

        /// <summary>
        /// Interval property
        /// </summary>
        public long Interval
        {
            get
            {
                return interval;
            }
            set
            {
                interval = value;
            }
        }

        /// <summary>
        /// Initializes the counters
        /// </summary>
        public static void Initialize()
        {
            clock = new Stopwatch();
            clock.Start();
        }
    }
}
