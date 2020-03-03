using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Melodic.Models
{
    public class Playlist
    {
        public Playlist()
        {
            descriptor = new PlaylistDescriptor();
            songs = new List<Song>();
        }
        public PlaylistDescriptor descriptor;
        public List<Song> songs;
    }

    public class PlaylistDescriptor
    {
        public int playlistId;
        public string name;
        public string description;

    }
}
