using script_chan2.DataTypes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace script_chan2.GUI
{
    public static class NotificationPlayer
    {
        private static MediaPlayer mediaPlayer;

        static NotificationPlayer()
        {
            Refresh();
        }

        public static void Refresh()
        {
            mediaPlayer = new MediaPlayer();
            if (string.IsNullOrEmpty(Settings.NotificationSoundFile))
                mediaPlayer.Open(new Uri(Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "Resources/notification.mp3")));
            else
                mediaPlayer.Open(new Uri(Settings.NotificationSoundFile));
        }

        public static void PlayNotification()
        {
            mediaPlayer.Stop();
            mediaPlayer.Play();
        }
    }
}
