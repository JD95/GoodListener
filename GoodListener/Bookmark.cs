using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodListener
{
    class BookmarkCollection
    {
        // Track Name       "Dune"
        public string track;

        // File path        "D:\Music\Audiobooks\Dune.mp3"
        public string trackPath { get; }

        public List<Bookmark> bookmarks = new List<Bookmark>();

        public BookmarkCollection(string track, string trackPath)
        {
            this.track = track;
            this.trackPath = trackPath;
        }

        public void addBookmark(Bookmark bookmark)
        {
            bookmarks.Add(bookmark);
        }

        public Bookmark findBookmark(Predicate<Bookmark> pred)
        {
            return bookmarks.Find(pred);
        }
    }

    class Bookmark
    {
        public const string previousPosition = "Previous Position";

        // Bookmark Name    "Chpt1"
        public string name { get; }

        // Bookmarked Time  00:10:34
        public int seconds { set; get; }
        public int minutes { set; get; }
        public int hours { set; get; }

        public Bookmark(string name, int seconds, int minutes, int hours)
        {
            this.name = name;
            this.seconds = seconds;
            this.minutes = minutes;
            this.hours = hours;

        }
    }
}
