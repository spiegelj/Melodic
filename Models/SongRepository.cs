using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Melodic.Models
{
    public class SongRepository : ISongRepository
    {
        public List<Song> GetSongs()
        {
            List<Song> songs = new List<Song>();
            Song song;
            try
            {
                DataTable songTable = DataAccess.DataAccess.GetTable("GetLibrary", new Dictionary<string, object>());
                foreach (DataRow r in songTable.Rows)
                {
                    song = new Song();
                    song.songId = Int32.Parse(r["SongId"].ToString());
                    song.title = r["Title"].ToString();
                    song.artist = r["Artist"].ToString();
                    song.lyrics = r["Lyrics"].ToString();
                    songs.Add(song);
                }
            }
            catch (Exception ex)
            {
                Logging.Log.ToDB($"Error attempting to retrieve song library.  Details:\n{ex.Message}");
                songs = null;
            }
            return songs;
        }

        public Song GetSong(int songId)
        {
            Song requestedSong;

            try
            {
                DataRow song = DataAccess.DataAccess.GetTable("GetSong", new Dictionary<string, object> { { "SongId", songId } }).Rows[0];
                requestedSong = new Song();
                requestedSong.songId = Int32.Parse(song["SongId"].ToString());
                requestedSong.title = song["Title"].ToString();
                requestedSong.artist = song["Artist"].ToString();
                requestedSong.lyrics = song["Lyrics"].ToString();
            }
            catch (Exception ex)
            {
                Logging.Log.ToDB($"Error attempting to retrieve song for SongID {songId}.  Details:\n{ex.Message}");
                requestedSong = null;
            }
            return requestedSong;
        }

        public int SaveSong(Song updateSong)
        {
            int resultId = -1;  // Indicate fail
            try
            {
                resultId = DataAccess.DataAccess.GetScalar("SaveSong", new Dictionary<string, object> { 
                        { "SongId", updateSong.songId },
                        { "Title", updateSong.title },
                        { "Artist", updateSong.artist },
                        { "Lyrics", updateSong.lyrics }});
            }
            catch (Exception ex)
            {
                Logging.Log.ToDB($"Error attempting to save song, \"{updateSong.title}\".  Details:\n{ex.Message}");
                // error flag already defaulted
            }
            return resultId;
        }

        public int DeleteSong(int songId)
        {
            int resultId = -1;  // Indicate fail
            try 
            {
                resultId = DataAccess.DataAccess.GetScalar("DeleteSong", new Dictionary<string, object> {
                        { "SongId", songId }});
            }
            catch (Exception ex)
            {
                Logging.Log.ToDB($"Error attempting to delete song with ID of {songId}.  Details:\n{ex.Message}");
            }
            return resultId;
        }
    }
}
