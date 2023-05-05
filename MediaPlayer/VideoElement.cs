using NAudio.Wave;
using NAudio.MediaFoundation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MediaToolkit;
using MediaToolkit.Model;
using MediaToolkit.Options;
using System.Windows;
using Microsoft.Win32;
using System.Diagnostics;

namespace MediaPlayer
{
    internal class VideoElement
    {
        public string fileName;
        public string title;
        public Image image;

        public VideoElement(string fname, FileInfo fileInfo)
        {
            fileName = fname;
            title = fileInfo.Name;
            image = GetVideoFirstFrame(fileName, title);
        }

        public static Image GetVideoFirstFrame(string fileName, string title)
        {
            // Создаем экземпляр MediaToolkit
            using (var engine = new Engine())
            {
                // Получаем информацию о видео
                var inputFile = new MediaFile { Filename = fileName };
                engine.GetMetadata(inputFile);

                // Получаем первый кадр видео
                var outputPath = Path.Combine(Path.GetTempPath(), title + ".jpg");
                var outputFile = new MediaFile { Filename = outputPath };
                var options = new ConversionOptions { Seek = TimeSpan.FromSeconds(0) };
                engine.GetThumbnail(inputFile, outputFile, options);

                // Отображаем изображение в Image
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(outputPath, UriKind.RelativeOrAbsolute);
                bitmap.EndInit();
                Image image = new Image();
                image.Source = bitmap;
                image.Width = 160;
                image.Height = 100;
                image.Stretch = Stretch.Fill;
                image.HorizontalAlignment = HorizontalAlignment.Center;

                return image;
            }
        }
    }
}
