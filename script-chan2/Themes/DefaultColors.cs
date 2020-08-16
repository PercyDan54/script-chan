using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace script_chan2
{
    public static class DefaultColors
    {
        public static string GetDefaultColor(string key)
        {
            switch (key)
            {
                case "BanchoBot": return Colors.Pink.ToString();
                case "Self": return Colors.Green.ToString();
                case "Default": return Colors.White.ToString();
                case "HD": return "#AA8000";
                case "HR": return "#8C1E1E";
                case "DT": return "#134197";
                case "FL": return "#BDBDBD";
                case "Freemod": return "#493872";
                case "Tiebreaker": return "#467337";
                case "NoFail": return "#F97AE4";
            }
            return Colors.Black.ToString();
        }
    }
}
