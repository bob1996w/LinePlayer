using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Media.Animation;
using LinePlayer.UserControls;
using LinePlayer.Modules;
using System.Text.RegularExpressions;
using static LinePlayer.Modules.LRCParser;

namespace LinePlayer
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary> 
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private PlayerModule playerModule = new PlayerModule();
        public string[] validExtension = { ".mp3", ".flac" };
        public double _position { get => playerModule.Position.TotalMilliseconds; set => playerModule.Position = TimeSpan.FromMilliseconds(value); }
        public double _length { get => playerModule.Length.TotalMilliseconds;}
        public string Label_time_get { get => (playerModule.PlaybackState == CSCore.SoundOut.PlaybackState.Stopped)? "" : TimeSpan.FromMilliseconds(_position).ToString(@"mm\:ss"); }
        public Thread updatePosition;
        public bool updatePositionPause = false;

        // volume
        public double _volume { get => playerModule.Volume; set => playerModule.Volume = value; }
        private double _volume_cache = 0d;
        public bool isMute = false;
        public bool volumeEnabled = false;

        // Slider effects
        public Storyboard slideInEffect;
        public bool slideInPlaying = false;
        public Storyboard slideOutEffect;
        public bool slideOutPlaying = false;

        // lyrics
        public LRCParser lrcParser;
        public int nowLyricIndex;
        public bool display_lyric = false;

        // current playing music tag
        public TagLib.Tag currentTag;
        public MainWindow()
        {
            var args = Environment.GetCommandLineArgs();
            bool hasFilePathInArgs = args.Length > 1 && args.Length < 3;
            InitializeComponent();
            NotifyAllProperties();
            playerModule.PlaybackStopped += PlayerModule_PlaybackStopped;
            updatePosition = new Thread(() =>
            {
                while(true)
                {
                    while (updatePositionPause) { Thread.Sleep(10); }
                    NotifyPropertyChanged(nameof(_position));
                    NotifyPropertyChanged(nameof(Label_time_get));
                    if (display_lyric)
                    {
                        
                    }
                    Thread.Sleep(10);
                }
            });
            updatePosition.SetApartmentState(ApartmentState.STA);
            updatePosition.Start();

            slideInEffect = new Storyboard();
            var slideIn = new ThicknessAnimation
            {
                From = (Thickness)Resources["Slider_Offset_Margin"],
                To = new Thickness(0, 0, 0, 0),
                Duration = TimeSpan.FromMilliseconds(250)
            };
            Storyboard.SetTargetName(slideIn, nameof(c_slider));
            Storyboard.SetTargetProperty(slideIn, new PropertyPath( PopSlider.MarginProperty));
            slideInEffect.Children.Add(slideIn);
            slideInEffect.Completed += SlideInEffect_Completed;

            slideOutEffect = new Storyboard();
            var slideOut = new ThicknessAnimation
            {
                From = new Thickness(0, 0, 0, 0),
                To = (Thickness)Resources["Slider_Offset_Margin"],
                Duration = TimeSpan.FromMilliseconds(250)
            };
            Storyboard.SetTargetName(slideOut, nameof(c_slider));
            Storyboard.SetTargetProperty(slideOut, new PropertyPath(PopSlider.MarginProperty));
            slideOutEffect.Children.Add(slideOut);
            slideOutEffect.Completed += SlideOutEffect_Completed;

            _volume_cache = _volume;

            // event binding from slider
            vol_slider.ValueChangedEvent += (object sender, double value) => { NotifyAllProperties(); };
            c_slider.HoverChangedEvent += (object sender, double value) => {
                c_slider.LabelText = TimeSpan.FromMilliseconds(value).ToString(@"mm\:ss") + " / " + TimeSpan.FromMilliseconds(_length).ToString(@"mm\:ss");
            };

            if (hasFilePathInArgs) LoadFileAndPlay(args[1]);
        }
        
        private void SlideInEffect_Completed(object sender, EventArgs e)
        {
            slideInPlaying = false;
        }

        private void SlideOutEffect_Completed(object sender, EventArgs e)
        {
            slideOutPlaying = false;
        }

        private void PlayerModule_PlaybackStopped(object sender, CSCore.SoundOut.PlaybackStoppedEventArgs e)
        {
            playerModule.Position = TimeSpan.Zero;
            NotifyAllProperties();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }

        private void btn_window_close_Click(object sender, RoutedEventArgs e)
        {
            updatePosition?.Abort();
            Close();
        }

        private void btn_open_file_Click(object sender, RoutedEventArgs e)
        {
            using (var ofd = new System.Windows.Forms.OpenFileDialog())
            {
                ofd.Filter = (string)App.Current.Resources["s_ofd_filter"];
                switch (ofd.ShowDialog())
                {
                    case System.Windows.Forms.DialogResult.None:
                        break;
                    case System.Windows.Forms.DialogResult.OK:
                        LoadFileAndPlay(ofd.FileName);
                        break;
                    case System.Windows.Forms.DialogResult.Cancel:
                        break;
                    case System.Windows.Forms.DialogResult.Abort:
                        break;
                    case System.Windows.Forms.DialogResult.Retry:
                        break;
                    case System.Windows.Forms.DialogResult.Ignore:
                        break;
                    case System.Windows.Forms.DialogResult.Yes:
                        break;
                    case System.Windows.Forms.DialogResult.No:
                        break;
                    default:
                        break;
                }
            }
            NotifyAllProperties();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            playerModule.Dispose();
        }

        public string Btn_play_icon =>
            (playerModule.PlaybackState == CSCore.SoundOut.PlaybackState.Playing) ? "pause" :
            (playerModule.PlaybackState == CSCore.SoundOut.PlaybackState.Paused) ? "play" : "play";

        public string WindowTitle =>
            (playerModule.PlaybackState == CSCore.SoundOut.PlaybackState.Stopped) ? "Line Player" :
            ((playerModule.PlaybackState == CSCore.SoundOut.PlaybackState.Playing) ? "> " : "|| ") + "Music Title - Line Player";

        private void LoadFileAndPlay(string filepath)
        {
            playerModule.Open(filepath);
            c_slider.MinValue = 0d;
            c_slider.MaxValue = _length;
            var currentFile = TagLib.File.Create(filepath);
            currentTag = currentFile.Tag;
            label_title.Content = (string.IsNullOrWhiteSpace(currentTag.Title)) ? System.IO.Path.GetFileName(filepath) : currentTag.Title;

            // lyrics
            Regex rgx = new Regex(@"\.[a-zA-Z0-9_]+$");
            var lrcPath = rgx.Replace(filepath, ".lrc");
            //Lyrics = LRCParser.ParseLyricInstant(lrcPath);
            lrcParser = new LRCParser(lrcPath);
            /*
            if(Lyrics.Count > 0)
            {
                display_lyric = true;
            }
            */
            Play();
        }

        private void Play()
        {
            playerModule.Play();
            NotifyAllProperties();
        }

        private void Pause()
        {
            playerModule.Pause();
            NotifyAllProperties();
        }

        private void Stop()
        {
            playerModule.Stop();
            NotifyAllProperties();
        }

        private void btn_play_Click(object sender, RoutedEventArgs e)
        {
            switch (playerModule.PlaybackState)
            {
                case CSCore.SoundOut.PlaybackState.Stopped:
                    Play();
                    break;
                case CSCore.SoundOut.PlaybackState.Playing:
                    Pause();
                    break;
                case CSCore.SoundOut.PlaybackState.Paused:
                    Play();
                    break;
                default:
                    break;
            }
        }

        private void btn_stop_Click(object sender, RoutedEventArgs e)
        {
            Stop();
        }

        // Change theme
        private void change_theme(object sender, RoutedEventArgs e)
        {
            var themeCode = (sender as MenuItem).Name.Replace("btn_theme_", "");
            Console.WriteLine("Theme = " + themeCode);
            Resources.Source = new Uri($"./Resources/Styles/{themeCode}.xaml", UriKind.Relative);
            c_slider.Resources.Source = new Uri($"../Resources/Styles/{themeCode}.xaml", UriKind.Relative);
            vol_slider.Resources.Source = new Uri($"../Resources/Styles/{themeCode}.xaml", UriKind.Relative);
            foreach (MenuItem btns in btn_themes.Items)
            {
                btns.IsChecked = false;
            }
            (sender as MenuItem).IsChecked = true;
        }

        // data binding
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName]string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private void NotifyAllProperties()
        {
            NotifyPropertyChanged(nameof(Btn_play_icon));
            NotifyPropertyChanged(nameof(WindowTitle));
            NotifyPropertyChanged(nameof(_volume));
            NotifyPropertyChanged(nameof(Btn_volume_icon));
            NotifyPropertyChanged(nameof(Vol_slider_label));
        }

        // slider

        private void slider_panel_MouseEnter(object sender, MouseEventArgs e)
        {
            c_slider_slideIn();
            c_slider.Margin = new Thickness(0, 0, 0, 0);
        }

        private void slider_panel_MouseLeave(object sender, MouseEventArgs e)
        {
            c_slider_slideOut();
            c_slider.Margin = (Thickness)Resources["Slider_Offset_Margin"];
        }

        private void c_slider_slideIn()
        {
            if (!slideInPlaying || slideOutPlaying)
            {
                slideInPlaying = true;
                slideInEffect.Begin(this);
            }
        }

        private void c_slider_slideOut()
        {
            if (!slideOutPlaying || slideInPlaying)
            {
                slideOutPlaying = true;
                slideOutEffect.Begin(this);
            }
        }

        // volume

        public string Btn_volume_icon => (isMute) ? "\xf026" : "\xf028";

        public string Vol_slider_label => "Volume: " + (int)_volume;

        private void btn_volume_Click(object sender, RoutedEventArgs e)
        {
            if (volumeEnabled)
            {
                vol_slider.Margin = (Thickness)Resources["Vol_Slider_Offset_Margin"];
                volumeEnabled = false;
            }
            else
            {
                vol_slider.Margin = new Thickness(0, 0, 0, 0);
                volumeEnabled = true;
            }
        }

        private void btn_volume_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (isMute)
            {
                isMute = false;
                _volume = _volume_cache;
                NotifyAllProperties();
            }
            else
            {
                _volume_cache = _volume;
                _volume = 0d;
                isMute = true;
                NotifyAllProperties();
            }
        }

        private void MainPlayerWindow_Drop(object sender, DragEventArgs e)
        {
            string[] files = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (files != null)
            {
                if(files.Length == 1 && validExtension.Contains(System.IO.Path.GetExtension(files[0])))
                {
                    LoadFileAndPlay(files[0]);
                }
            }
        }

        private void btn_menu_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            main_menu.PlacementTarget = this;
            main_menu.IsOpen = true;
        }

    }
}
