using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace MediaPlayer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Library library;
        public bool opened = false;
        public bool fullscreen = false;
        public MainWindow()
        {
            InitializeComponent();

            library = new Library("Библиотека");
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = "Video files (*.mp4)|*.mp4";
            fileDialog.Multiselect = true;

            if(fileDialog.ShowDialog() == true)
            {
                string[] fileNames = fileDialog.FileNames;
                foreach(string fileName in fileNames)
                {
                    this.library.AddVideoToLibrary(fileName, new FileInfo(fileName));
                    loadVideosInListBox();
                }
            }
        }

        private void playButton_Click(object sender, RoutedEventArgs e)
        {
            if(Utils.isPlaying(videoPlayer))
            {
                videoPlayer.Pause();
                playButtonImage.Source = new BitmapImage(new Uri("resources/images/play-button.png", UriKind.Relative));
            }
            else
            {
                videoPlayer.Play();
                playButtonImage.Source = new BitmapImage(new Uri("resources/images/pause.png", UriKind.Relative));
            }
        }

        /**
         Добавление последнего видео List в ListBox WPF
         */
        private void loadVideosInListBox()
        {
            ListBoxItem listBoxItem = new ListBoxItem();
            VideoElement video = library.GetLibrary(library.VideoElements.Count - 1);
            
            listBoxItem.Content = video.image;
            listBoxItem.ToolTip = video;

            libraryBox.Items.Add(listBoxItem);
        }

        private void libraryBox_MouseDoubleClick_1(object sender, MouseButtonEventArgs e)
        {
            /**
             * Предполагается что, загружаются только объекты типа VideoElement
             */
            ListBoxItem selectedVideo = libraryBox.SelectedItem as ListBoxItem;
            if (selectedVideo != null)
            {
                VideoElement video = selectedVideo.ToolTip as VideoElement;
                videoPlayer.Source = new Uri(video.fileName);
                videoPlayer.Play();

                if (opened)
                {
                    BeginAnimation();
                }

                playButtonImage.Source = new BitmapImage(new Uri("resources/images/pause.png", UriKind.Relative));
            }
        }

        private void BeginAnimation()
        {
            UpdateAnimation();
            Storyboard storyboard = (Storyboard)this.Resources["libraryBlockAnim"];
            storyboard.Begin();

            opened = !opened;
        }

        private void videoPlayer_MediaOpened(object sender, RoutedEventArgs e)
        {
            timelineVideo.Maximum = videoPlayer.NaturalDuration.TimeSpan.TotalSeconds;
            timelineVideo.Value = 0;

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(100);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            timelineVideo.Value = videoPlayer.Position.TotalSeconds;
            timelineLabel.Content = TimeSpan.FromSeconds(timelineVideo.Value).ToString(@"mm\:ss");
        }

        private void UpdateAnimation()
        {
            Storyboard libraryAnim = (Storyboard)this.Resources["libraryBlockAnim"];
            DoubleAnimation animLibraryBlock = (DoubleAnimation)libraryAnim.Children[0];
            DoubleAnimation animButton = (DoubleAnimation)libraryAnim.Children[1];

            animButton.To = !opened ? 205 : 5;
            animLibraryBlock.To = !opened ? 0 : -200;
        }

        private void openMenuButton_Click(object sender, RoutedEventArgs e)
        {
            BeginAnimation();
        }

        private void fullSizeButton_Click(object sender, RoutedEventArgs e)
        {
            if(!fullscreen)
            {
                WindowState = WindowState.Maximized;
                WindowStyle = WindowStyle.None;
            }
            else
            {
                WindowState = WindowState.Normal;
                WindowStyle = WindowStyle.SingleBorderWindow;
            }
            setSizePlayer();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                WindowState = WindowState.Normal;
                WindowStyle = WindowStyle.SingleBorderWindow;
                setSizePlayer();
            }
        }

        private void setSizePlayer()
        {
            videoPlayer.Width = ActualWidth;
            videoPlayer.Height = ActualHeight;
            libraryBlock.Height = ActualHeight;

            fullscreen = !fullscreen;
            
            if(fullscreen)
            {
                Canvas.SetLeft(libraryBlock, -200);
                Canvas.SetTop(settingsPanelBlock, ActualHeight - 45);
                settingsPanelBlock.Width = ActualWidth;
            }
            else
            {
                Canvas.SetTop(settingsPanelBlock, 644);
                settingsPanelBlock.Width = ActualWidth;
            }
            timelineVideo.Width = ActualWidth - 230;
        }

        private void volumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            videoPlayer.Volume = (double)(e.NewValue / 100.0);
        }

        private void timelineVideo_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            videoPlayer.Position = TimeSpan.FromSeconds(timelineVideo.Value);
        }


    }
}
