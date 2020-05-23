using System;

namespace script_chan2.DataTypes
{
    public class IrcMessage
    {
        public Match Match { get; set; }
        public DateTime Timestamp { get; set; }
        public string Channel { get; set; }
        public string User { get; set; }
        public string Message { get; set; }
    }
}
