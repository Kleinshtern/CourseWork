using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MediaPlayer
{
    internal class Utils
    {
        public static bool isPlaying(MediaElement media)
        {
           var pos1 = media.Position;
           System.Threading.Thread.Sleep(1);
           var pos2 = media.Position;

           return pos2 != pos1;
        }
    }
}
