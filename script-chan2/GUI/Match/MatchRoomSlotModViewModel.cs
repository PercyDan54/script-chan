using Caliburn.Micro;
using script_chan2.Enums;
using System;
using System.Windows.Media.Imaging;

namespace script_chan2.GUI
{
    public class MatchRoomSlotModViewModel : Screen
    {
        #region Constructor
        public MatchRoomSlotModViewModel(GameMods mod)
        {
            this.mod = mod;
        }
        #endregion

        #region Properties
        private GameMods mod;

        public BitmapImage ModImage
        {
            get
            {
                switch (mod)
                {
                    case GameMods.DoubleTime: return new BitmapImage(new Uri($"https://osu.ppy.sh/images/badges/mods/mod_double-time.png"));
                    case GameMods.Easy: return new BitmapImage(new Uri($"https://osu.ppy.sh/images/badges/mods/mod_easy.png"));
                    case GameMods.Flashlight: return new BitmapImage(new Uri($"https://osu.ppy.sh/images/badges/mods/mod_flashlight.png"));
                    case GameMods.HardRock: return new BitmapImage(new Uri($"https://osu.ppy.sh/images/badges/mods/mod_hard-rock.png"));
                    case GameMods.Hidden: return new BitmapImage(new Uri($"https://osu.ppy.sh/images/badges/mods/mod_hidden.png"));
                    case GameMods.NoFail: return new BitmapImage(new Uri($"https://osu.ppy.sh/images/badges/mods/mod_no-fail.png"));
                    case GameMods.Mirror: return new BitmapImage(new Uri($"https://osu.ppy.sh/images/badges/mods/mod_mirror.png"));
                }
                return null;
            }
        }

        public string ToolTip
        {
            get
            {
                return mod.ToString();
            }
        }
        #endregion
    }
}
