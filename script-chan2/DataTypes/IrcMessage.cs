using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace script_chan2.DataTypes
{
    public class IrcMessage
    {
        public Match Match { get; set; }
        public DateTime Timestamp { get; set; }
        public string User { get; set; }
        public string Message { get; set; }
    }
}
