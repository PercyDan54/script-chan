using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace script_chan2.OsuIrc
{
    public class IrcMessage
    {
        public string User { get; set; }
        public string Message { get; set; }
    }

    public class RoomCreatedData
    {
        public string Name { get; set; }
        public int Id { get; set; }
    }

    public class ChannelMessageData
    {
        public string Channel { get; set; }
        public string User { get; set; }
        public string Message { get; set; }
    }
}
