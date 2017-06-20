using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.ApplicationModel;

using Newtonsoft.Json;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace GoodListener
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        List<string> recentlyPlayed = new List<string>();
        List<BookmarkCollection> bookmarkStorage = new List<BookmarkCollection>();
        BookmarkCollection currentCollection = null;
        const string storageFile = "bookmarks.json";
        const string recentTracks = "recentTracks.json";
        
        public MainPage()
        {
            this.InitializeComponent();
            Application.Current.Suspending += new SuspendingEventHandler(OnPageSuspension);
            Application.Current.EnteredBackground += new EnteredBackgroundEventHandler(OnPageBackground);
            loadBookmarkCollection();
            loadRecentTracks();
        }

        public void OnPageSuspension(object sender, SuspendingEventArgs e)
        {
            saveBookmarkCollection();
            saveRecentTracks();
        }

        public void OnPageBackground(object sender, EnteredBackgroundEventArgs e)
        {
            saveBookmarkCollection();
            saveRecentTracks();
        }

        public async void loadRecentTracks()
        {
            // load bookmark json from file
            Windows.Storage.StorageFolder storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;

            if (await storageFolder.TryGetItemAsync(recentTracks) != null)
            {
                Windows.Storage.StorageFile storage = await storageFolder.GetFileAsync(recentTracks);

                string json = await Windows.Storage.FileIO.ReadTextAsync(storage);

                recentlyPlayed = JsonConvert.DeserializeObject<List<string>>(json);

                foreach (var track in recentlyPlayed)
                {
                    Windows.Storage.StorageFile file = await Windows.Storage.StorageFile.GetFileFromPathAsync(track);

                    if (file != null)
                    {
                        // Application now has read/write access to the picked file
                        var t = new Track(file);
                        t.Tapped += track_Clicked;
                        trackList.Items.Add(t);
                    }
                }

            }
            else
            {
                recentlyPlayed = new List<string>();
            }
        }

        public async void saveRecentTracks()
        {
            // Serialize collection
            string json = JsonConvert.SerializeObject(recentlyPlayed);

            // save collection to file
            Windows.Storage.StorageFolder storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            Windows.Storage.StorageFile storage = null;

            if (await storageFolder.TryGetItemAsync(recentTracks) == null)
            {
                storage = await storageFolder.CreateFileAsync(recentTracks);
            }
            else
            {
                storage = await storageFolder.GetFileAsync(recentTracks);
            }

            await Windows.Storage.FileIO.WriteTextAsync(storage, json);
        }

        private async void getMusic_Click(object sender, RoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation =
                Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
            picker.FileTypeFilter.Add(".mp3");
            picker.FileTypeFilter.Add(".wav");

            Windows.Storage.StorageFile file = await picker.PickSingleFileAsync(); 

            if (file != null)
            {
                // Application now has read/write access to the picked file
                var t = new Track(file);
                t.Tapped += track_Clicked;
                trackList.Items.Add(t);
            }
        }

        public void createBookmark(string bookmarkName)
        {
            // grab current track's time position
            var currentTime = player.MediaPlayer.PlaybackSession.Position;

            var seconds = currentTime.Seconds;
            var minutes = currentTime.Minutes;
            var hours = currentTime.Hours;

            // create new bookmark
            currentCollection.addBookmark(new Bookmark(bookmarkName, seconds, minutes, hours));
        }

        public async void loadBookmarkCollection()
        {
            // load bookmark json from file
            Windows.Storage.StorageFolder storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;     

            if (await storageFolder.TryGetItemAsync(storageFile) != null)
            {
                Windows.Storage.StorageFile storage = await storageFolder.GetFileAsync(storageFile);

                string json = await Windows.Storage.FileIO.ReadTextAsync(storage);

                // deserialize it
                bookmarkStorage = JsonConvert.DeserializeObject<List<BookmarkCollection>>(json);
            }
            else
            {
                bookmarkStorage = new List<BookmarkCollection>();
            }     
        }

        public async void saveBookmarkCollection()
        {
            setPreviousPosition();

            // Serialize collection
            string json = JsonConvert.SerializeObject(bookmarkStorage);

            // save collection to file
            Windows.Storage.StorageFolder storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            Windows.Storage.StorageFile storage = null;

            if (await storageFolder.TryGetItemAsync(storageFile) == null)
            {
                storage = await storageFolder.CreateFileAsync(storageFile);
            }
            else
            {
                storage = await storageFolder.GetFileAsync(storageFile);
            }
             
            await Windows.Storage.FileIO.WriteTextAsync(storage, json);
        }

        public void setPreviousPosition()
        {
            if (currentCollection != null)
            {
                var previousPosition = currentCollection.findBookmark(b => b.name == Bookmark.previousPosition);

                if (previousPosition != null)
                {
                    var currentTime = player.MediaPlayer.PlaybackSession.Position;
                    previousPosition.seconds = currentTime.Seconds;
                    previousPosition.minutes = currentTime.Minutes;
                    previousPosition.hours = currentTime.Hours;
                }
            }
        }

        public void track_Clicked(object sender, RoutedEventArgs args)
        {
            var track = sender as Track;
            if (track != null)
            {
                // Save previous position for current track
                setPreviousPosition();

                // Change screen to media player
                mainPivot.SelectedIndex = 1;

                // Set the player to use the specified file
                player.Source = MediaSource.CreateFromStorageFile(track.track);

                if(!recentlyPlayed.Exists(t => t == track.track.Path))
                {
                    recentlyPlayed.Add(track.track.Path);
                }

                // Lookup bookmark collection for selected track
                currentCollection = bookmarkStorage.Find(bc => bc.trackPath == track.track.Path);

                // if one does not exist, create a new one
                if (currentCollection == null)
                {
                    // Ask user to name collection
                    string collectionName = track.track.DisplayName;

                    currentCollection = new BookmarkCollection(collectionName, track.track.Path);
                    bookmarkStorage.Add(currentCollection);
                }

                // Set current bookmark to keep track of current file's last play
                var previousPosition = currentCollection.findBookmark(b => b.name == Bookmark.previousPosition);

                // Load the 'Previous Position' bookmark by default
                if (previousPosition == null)
                {
                    previousPosition = new Bookmark("Previous Position", 0, 0, 0);
                    currentCollection.addBookmark(previousPosition);
                }

                var seconds = previousPosition.seconds;
                var minutes = previousPosition.minutes;
                var hours = previousPosition.hours;

                player.MediaPlayer.PlaybackSession.Position = new TimeSpan(hours, minutes, seconds);

                this.trackBookmarks.Items.Clear();

                foreach (var bookmark in currentCollection.bookmarks)
                {
                    var time = new TimeSpan(bookmark.hours, bookmark.minutes, bookmark.seconds);
                    this.trackBookmarks.Items.Add(new BookmarkCard(bookmark.name, time, this.player));
                }
            }
            else
            {
                track.toErrorState();
            }
        }

        private void AddBookmark_Click(object sender, RoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout(sender as FrameworkElement);
        }

        private void SaveBookmark_Click(object sender, RoutedEventArgs e)
        {
            var name = this.NewBookmarkName.Text;
            var time = this.player.MediaPlayer.PlaybackSession.Position;
            currentCollection.addBookmark(new Bookmark(name, time.Seconds, time.Minutes, time.Hours));
            this.trackBookmarks.Items.Add(new BookmarkCard(name, time, this.player));
        }
    }
}
