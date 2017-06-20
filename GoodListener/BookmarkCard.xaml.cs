using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace GoodListener
{
    public sealed partial class BookmarkCard : UserControl
    {
        public TimeSpan time { get; }
        public MediaPlayerElement player = null;

        public BookmarkCard(string name, TimeSpan time, MediaPlayerElement player)
        {
            this.InitializeComponent();
            this.Bookmark.Text = name;
            this.time = time;
            this.player = player;
            this.Tapped += this.UseBookmark;
        }

        public void UseBookmark(object sender, TappedRoutedEventArgs e)
        {
            player.MediaPlayer.PlaybackSession.Position = time;
        }
    }
}
