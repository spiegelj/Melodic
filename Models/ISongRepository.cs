using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Melodic.Models
{
    interface ISongRepository
    {
        public List<Song> GetSongs();

        public int SaveSong(Song updateSong);

        public Song GetSong(int songId);
    }
}
