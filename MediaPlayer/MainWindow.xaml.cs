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
        public DispatcherTimer timerForBlocks = new DispatcherTimer();
        public bool opened = false;
        public bool fullscreen = false;
        public int selectedIndex = -1;
        public MainWindow()
        {
            InitializeComponent();

            library = new Library("Библиотека");

            timerForBlocks.Interval = TimeSpan.FromSeconds(2);
            timerForBlocks.Tick += Hidden_Blocks;
        }

        /**
         * Функция которая открывает выбранные файлы и записывает их в библиотеку
         */
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

        /**
         * Кнопка проигрывания, работает через функцию класс Utils, которая проверяет смещение видео во времени
         */
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

            StackPanel stack = new StackPanel();
            stack.Width = 160;

            TextBlock title = new TextBlock();

            title.TextWrapping = TextWrapping.Wrap;
            title.Text = video.title;
            title.TextAlignment = TextAlignment.Center;
            title.Foreground = new SolidColorBrush(Colors.White);

            stack.Children.Add(video.image);
            stack.Children.Add(title);

            listBoxItem.Content = stack;
            listBoxItem.DataContext = video;
            listBoxItem.HorizontalAlignment = HorizontalAlignment.Center;

            libraryBox.Items.Add(listBoxItem);
        }

        private void libraryBox_MouseDoubleClick_1(object sender, MouseButtonEventArgs e)
        {
            /**
             * Предполагается что, загружаются только объекты типа VideoElement
             */
            selectedIndex = libraryBox.SelectedIndex;
            loadVideoInMediaElement();
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

            animButton.To = !opened ? 220 : 0;
            animLibraryBlock.To = !opened ? 0 : -220;

            RotateTransform rotateTransform = new RotateTransform();
            rotateTransform.Angle = !opened ? 180 : 0;
            rotateTransform.CenterX = openMenuImage.ActualWidth/ 2;
            rotateTransform.CenterY = openMenuImage.ActualHeight/ 2;

            openMenuImage.RenderTransform= rotateTransform;
        }

        private void openMenuButton_Click(object sender, RoutedEventArgs e)
        {
            BeginAnimation();
        }

        private void fullSizeButton_Click(object sender, RoutedEventArgs e)
        {
            fullscreen = !fullscreen;

            if(fullscreen)
            {
                WindowState = WindowState.Maximized;
            }
            else
            {
                WindowState = WindowState.Normal;
            }
            setSizePlayer();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                fullscreen = !fullscreen;

                WindowState = WindowState.Normal;
                setSizePlayer();
            }
        }

        private void setSizePlayer()
        { 
            if(fullscreen)
            {
                windowHeaderGrid.Visibility = Visibility.Collapsed;

                mainCanvas.Margin = new Thickness(0, 0, 0, 0);

                videoPlayer.Width = ActualWidth;
                videoPlayer.Height = ActualHeight;

                Canvas.SetTop(settingsPanelBlock, ActualHeight - 40);
            } 
            else
            {
                windowHeaderGrid.Visibility = Visibility.Visible;

                mainCanvas.Margin = new Thickness(0, 30, 0, 0);

                videoPlayer.Width = ActualWidth;
                videoPlayer.Height = ActualHeight - 30;

                Canvas.SetTop(settingsPanelBlock, ActualHeight - 70);
            }
            timelineVideo.Width = ActualWidth - 290;
        }

        private void volumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            videoPlayer.Volume = (double)(e.NewValue / 100.0);

            if(videoPlayer.Volume == 0)
            {
                setMutedIcon();
            } else { setUnmutedIcon(); }
        }

        private void timelineVideo_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            videoPlayer.Position = TimeSpan.FromSeconds(timelineVideo.Value);
        }
         
        private void videoPlayer_MouseEnter(object sender, MouseEventArgs e)
        {
            titleBlock.Visibility = Visibility.Visible;
            settingsPanelBlock.Visibility = Visibility.Visible;

            timerForBlocks.Start();
        }

        private void Hidden_Blocks(object sender, EventArgs e) 
        {
            titleBlock.Visibility = Visibility.Collapsed;
            settingsPanelBlock.Visibility = Visibility.Collapsed;

            timerForBlocks.Stop();
        }

        private void videoPlayer_MouseMove(object sender, MouseEventArgs e)
        {
            titleBlock.Visibility = Visibility.Visible;
            settingsPanelBlock.Visibility = Visibility.Visible;

            timerForBlocks.Start();
        }

        /**
         * Функция обрабатывающая также двойной клик по окну, если проходит то вызывается функция кнопки "На полный экран"
         */
        private DateTime lastClickTime = DateTime.MinValue;
        private void videoPlayer_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if ((DateTime.Now - lastClickTime).TotalMilliseconds < 300)
            {
                lastClickTime = DateTime.MinValue;
                fullSizeButton_Click(sender, e); 
            }
            else { lastClickTime = DateTime.Now; }
            playButton_Click(sender, e);
        }

        /**
         * Громкость при помощи колеса мыши
         */
        private void videoPlayer_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            Slider? slider = volumeButton.Template.FindName("volumeSlider", volumeButton) as Slider;
            if (slider != null) {
                if (e.Delta < 0 && videoPlayer.Volume > 0)
                {
                    slider.Value -= 10;
                }
                else if (e.Delta > 0 && videoPlayer.Volume < 100)
                {
                    slider.Value += 10;
                }
            }
        }

        private void settingsPanelBlock_MouseEnter(object sender, MouseEventArgs e)
        {
            settingsPanelBlock.Visibility = Visibility.Visible;
            titleBlock.Visibility = Visibility.Visible;
        }

        private void volumeButton_Click(object sender, RoutedEventArgs e)
        {
            if(videoPlayer.Volume > 0)
            {
                videoPlayer.Volume = 0.0;
                setMutedIcon();
            }
            else {
                videoPlayer.Volume = 1.0;
                setUnmutedIcon();
            }
        }

        private void setMutedIcon()
        {
            volumeImage.Source = new BitmapImage(new Uri("resources/images/mute-speaker.png", UriKind.Relative));
        }

        private void setUnmutedIcon()
        {
            volumeImage.Source = new BitmapImage(new Uri("resources/images/medium-volume.png", UriKind.Relative));
        }

        private void minusWindowButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void shutdownAppButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void windowHeaderGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void volumeSlider_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            Slider? slider = volumeButton.Template.FindName("volumeSlider", volumeButton) as Slider;
            if (slider.IsVisible)
            {
                timelineVideo.Width -= 101;
            }
            else
            {
                timelineVideo.Width += 101;
            }
        }

        private void prevVideoButton_Click(object sender, RoutedEventArgs e)
        {
            if(selectedIndex != -1) {
                selectedIndex -= 1;
                if(selectedIndex < 0)
                {
                    selectedIndex = 0;
                }
                libraryBox.SelectedItem = libraryBox.Items[selectedIndex];
                loadVideoInMediaElement();
            }
        }

        private void nextVideoButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedIndex != -1) { 
                selectedIndex += 1;
                if(selectedIndex > (libraryBox.Items.Count - 1)) {
                    selectedIndex = 0;
                }

                libraryBox.SelectedItem = libraryBox.Items[selectedIndex];

                loadVideoInMediaElement();
            }
        }

        private void loadVideoInMediaElement()
        {
            ListBoxItem selectedItem = libraryBox.SelectedItem as ListBoxItem;

            VideoElement video = selectedItem.DataContext as VideoElement;
            videoPlayer.Source = new Uri(video.fileName);
            videoPlayer.Play();

            mediaTitle.Text = video.title;

            if (opened)
            {
               BeginAnimation();
            }

            playButtonImage.Source = new BitmapImage(new Uri("resources/images/pause.png", UriKind.Relative));
        }

        private void videoPlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            nextVideoButton_Click(sender, e);
        }
    }
}
