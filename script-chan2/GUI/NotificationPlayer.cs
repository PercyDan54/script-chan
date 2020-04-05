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
        private static MediaPlayer mediaPlayer = new MediaPlayer();

        static NotificationPlayer()
        {
            mediaPlayer.Open(new Uri(Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "Resources/notification.mp3")));
        }

        public static void PlayNotification()
        {
            mediaPlayer.Stop();
            mediaPlayer.Play();
        }
    }
}
