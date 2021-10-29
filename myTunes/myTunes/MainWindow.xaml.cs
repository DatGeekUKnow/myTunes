using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
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

namespace myTunes
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MusicRepo musicRepo;
        private readonly ObservableCollection<string> playlists;
        private readonly MediaPlayer mediaPlayer;

        private Song s;
        private Point startPoint;
        private string playlistName;
        private string currenctPlaylist;
        private bool isPlaylistSelected = false;

        public MainWindow()
        {
            InitializeComponent();
            mediaPlayer = new MediaPlayer();

            try
            {
                musicRepo = new MusicRepo();
            }
            catch (Exception e)
            {
                MessageBox.Show("Error loading file: " + e.Message, "myTunes", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // make playlists an observablecollection to allow easy gui updates
            playlists = new ObservableCollection<string>(musicRepo.Playlists);
            playlists.Insert(0, "All Music");


            // TODO: update playlists list dynamically, look into McCown's Observable Collection example
            // initialize data grid to music located in music.xml
            musicDataGrid.ItemsSource = musicRepo.Songs.DefaultView;
            playlistListBox.ItemsSource = playlists;
        }

        // https://stackoverflow.com/questions/4662428/how-to-hide-arrow-on-right-side-of-a-toolbar/4662570
        // code used to hide pesky drop down arrow on right side of toolbar
        private void myToolBar_Loaded(object sender, RoutedEventArgs e)
        {
            ToolBar toolBar = sender as ToolBar;
            var overflowGrid = toolBar.Template.FindName("OverflowGrid", toolBar) as FrameworkElement;
            if (overflowGrid != null)
            {
                overflowGrid.Visibility = Visibility.Collapsed;
            }

            var mainPanelBorder = toolBar.Template.FindName("MainPanelBorder", toolBar) as FrameworkElement;
            if (mainPanelBorder != null)
            {
                mainPanelBorder.Margin = new Thickness(0);
            }
        }

        private void DeleteCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            DataRowView rowView = musicDataGrid.SelectedItem as DataRowView;
            if (rowView != null)
            {
                if(isPlaylistSelected)
                {
                    // Extract the song ID from the selected song
                    musicRepo.DeleteSong(Convert.ToInt32(rowView.Row.ItemArray[0]));
                    musicRepo.Save();

                    musicDataGrid.ItemsSource = musicRepo.SongsForPlaylist(currenctPlaylist).DefaultView;
                }
                else
                {
                    musicDataGrid.ItemsSource = musicRepo.Songs.DefaultView;
                }

            }

        }

        private void PlayCommand_Executed(object sender, RoutedEventArgs e)
        {
            mediaPlayer.Open(new Uri(s.Filename));
            mediaPlayer.Play();

        }

        private void stopButton_Click(object sender, RoutedEventArgs e)
        {
            mediaPlayer.Stop();
        }

        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (musicDataGrid.SelectedItem != null) e.CanExecute = true;
        }

        private void musicDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataRowView rowView = musicDataGrid.SelectedItem as DataRowView;
            if (rowView != null)
            {
                s = musicRepo.GetSong(Convert.ToInt32(rowView.Row.ItemArray[0]));
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            musicRepo.Save();
        }

        private void Label_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.None;

            Label playlist = sender as Label;

            if(playlist != null)
            {
                e.Effects = DragDropEffects.Copy;
                playlistName = playlist.Content.ToString();
            }
        }

        private void Label_Drop(object sender, DragEventArgs e)
        {
            if(e.Data.GetDataPresent(DataFormats.StringFormat))
            {
                int dataString = Convert.ToInt32(e.Data.GetData(DataFormats.StringFormat));

                musicRepo.AddSongToPlaylist(dataString, playlistName);
            }
        }

        private void musicDataGrid_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && 
                musicDataGrid.SelectedItems.Count > 0)
            {
                // Get the song ID from the currently selected row
                DataRowView rowView = musicDataGrid.SelectedItem as DataRowView;
                string songId = rowView.Row.ItemArray[0].ToString();

                DragDrop.DoDragDrop(musicDataGrid, songId, DragDropEffects.Copy);
            }

        }

        private void musicDataGrid_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            // Store the mouse position
            startPoint = e.GetPosition(null);
        }

        private void playlistListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string playlist = playlistListBox.SelectedItem.ToString();
            if(playlist == "All Music")
            {
                isPlaylistSelected = false;
                removeButton.Header = "Remove";
                musicDataGrid.ItemsSource = musicRepo.Songs.DefaultView;
            }
            else
            {
                isPlaylistSelected = true;
                removeButton.Header = "Remove from Playlist";
                currenctPlaylist = playlistListBox.SelectedItem.ToString();
                musicDataGrid.ItemsSource = musicRepo.SongsForPlaylist(playlist).DefaultView;
            }
        }
    }
}
