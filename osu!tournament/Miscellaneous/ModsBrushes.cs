using System.Windows.Media;
using Osu.Scores;

namespace Osu.Mvvm.Miscellaneous
{
    /// <summary>
    /// Contains some cool brushes
    /// </summary>
    public static class ModsBrushes
    {
        /// <summary>
        /// The osu! pink
        /// </summary>
        public static Brush OsuPink = new SolidColorBrush(Color.FromRgb(204, 46, 138));

        /// <summary>
        /// Dark gray
        /// </summary>
        public static Brush DarkGray = new SolidColorBrush(Color.FromRgb(100, 100, 100));

        /// <summary>
        /// Light gray
        /// </summary>
        public static Brush LightGray = new SolidColorBrush(Color.FromRgb(160, 160, 160));

        /// <summary>
        /// None
        /// </summary>
        public static Brush None = new SolidColorBrush(Color.FromRgb(239, 239, 239));

        /// <summary>
        /// Light None
        /// </summary>
        public static Brush NoneLight = new SolidColorBrush(Color.FromRgb(255, 255, 255));

        /// <summary>
        /// HD
        /// </summary>
        public static Brush HD = new SolidColorBrush(Color.FromRgb(255, 229, 153));

        /// <summary>
        /// Light HD
        /// </summary>
        public static Brush HDLight = new SolidColorBrush(Color.FromRgb(255, 242, 204));

        /// <summary>
        /// HR
        /// </summary>
        public static Brush HR = new SolidColorBrush(Color.FromRgb(234, 153, 153));

        /// <summary>
        /// Light HR
        /// </summary>
        public static Brush HRLight = new SolidColorBrush(Color.FromRgb(244, 204, 204));

        /// <summary>
        /// DT
        /// </summary>
        public static Brush DT = new SolidColorBrush(Color.FromRgb(159, 197, 232));

        /// <summary>
        /// Light DT
        /// </summary>
        public static Brush DTLight = new SolidColorBrush(Color.FromRgb(207, 226, 243));

        /// <summary>
        /// FL
        /// </summary>
        public static Brush FL = new SolidColorBrush(Color.FromRgb(117, 117, 117));

        /// <summary>
        /// Light FL
        /// </summary>
        public static Brush FLLight = new SolidColorBrush(Color.FromRgb(189, 189, 189));

        /// <summary>
        /// Freemod
        /// </summary>
        public static Brush FreeMod = new SolidColorBrush(Color.FromRgb(180, 167, 214));

        /// <summary>
        /// Light Freemod
        /// </summary>
        public static Brush FreeModLight = new SolidColorBrush(Color.FromRgb(217, 210, 233));

        /// <summary>
        /// TieBreaker
        /// </summary>
        public static Brush TieBreaker = new SolidColorBrush(Color.FromRgb(182, 215, 168));

        /// <summary>
        /// Light TieBreaker
        /// </summary>
        public static Brush TieBreakerLight = new SolidColorBrush(Color.FromRgb(217, 234, 211));

        public static Brush BannedMap = new SolidColorBrush(Color.FromRgb(255, 0, 0));

        public static Brush BlueTeam = new SolidColorBrush(Color.FromRgb(20, 141, 175));

        public static Brush RedTeam = new SolidColorBrush(Color.FromRgb(252, 69, 73));

        public static Brush GetModBrush(PickType mod)
        {
            switch (mod)
            {
                case PickType.NoMod:
                    return None;
                case PickType.HD:
                    return HD;
                case PickType.HR:
                    return HR;
                case PickType.DT:
                    return DT;
                case PickType.FL:
                    return FL;
                case PickType.Freemod:
                    return FreeMod;
                case PickType.TieBreaker:
                    return TieBreaker;
                default:
                    return None;
            }
        }

        public static Brush GetModBrushLight(PickType mod)
        {
            switch (mod)
            {
                case PickType.NoMod:
                    return NoneLight;
                case PickType.HD:
                    return HDLight;
                case PickType.HR:
                    return HRLight;
                case PickType.DT:
                    return DTLight;
                case PickType.FL:
                    return FLLight;
                case PickType.Freemod:
                    return FreeModLight;
                case PickType.TieBreaker:
                    return TieBreakerLight;
                default:
                    return NoneLight;
            }
        }
    }
}