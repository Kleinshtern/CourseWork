using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Wave;

namespace MediaPlayer
{


    internal class Library
    {
        public List<VideoElement> VideoElements = new List<VideoElement>();
        private string libraryName;

        public Library(string lName) {
            libraryName = lName;
        }

        public void AddVideoToLibrary(string fname, FileInfo fileInfo)
        {
            VideoElement video = new VideoElement(fname, fileInfo);
            VideoElements.Add(video);
        }

        public dynamic GetLibrary(int index)
        {
            return (index >= 0) ? VideoElements[index] : VideoElements;
        }


    }
}
