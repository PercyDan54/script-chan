using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Osu.Tournament.Ov.ViewModels
{
    /// <summary>
    /// Event object when a match has been created, return the room id
    /// </summary>
    public class MatchCreatedArgs
    {
        private readonly long id;

        public MatchCreatedArgs(long id)
        {
            this.id = id;
        }

        public long Id
        {
            get { return this.id; }
        }
    }
}
