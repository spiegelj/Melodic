using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Melodic.Models
{
    public class PlaylistRepository : IPlaylistRepository
    {
        public List<PlaylistDescriptor> GetPlaylists()
        {
            List<PlaylistDescriptor> playlists = new List<PlaylistDescriptor>();
            PlaylistDescriptor playlist;
            try
            {
                DataTable playlistTable = DataAccess.DataAccess.GetTable("GetPlaylists", new Dictionary<string, object>());
                foreach (DataRow r in playlistTable.Rows)
                {
                    playlist = new PlaylistDescriptor();
                    playlist.playlistId = Int32.Parse(r["PlaylistId"].ToString());
                    playlist.name = r["Name"].ToString();
                    playlist.description = r["Description"].ToString();
                    playlists.Add(playlist);
                }
            }
            catch (Exception ex)
            {
                Logging.Log.ToDB($"Error attempting to retrieve playlists.  Details:\n{ex.Message}");
                playlists = null;
            }
            return playlists;
        }

        public Playlist GetPlaylist(int playlistId)
        {
            Playlist requestedPlaylist;

            try
            {
                // A bit taken on faith.  Safer would be to check for rows returned, possibly for exactly one row.
                DataRow playlist = DataAccess.DataAccess.GetTable("GetPlaylist", new Dictionary<string, object> { { "PlaylistId", playlistId } }).Rows[0];
                requestedPlaylist = new Playlist();
                PlaylistDescriptor descriptor = new PlaylistDescriptor();
                descriptor.playlistId = Int32.Parse(playlist["PlaylistId"].ToString());
                descriptor.name = playlist["Name"].ToString();
                descriptor.description = playlist["Description"].ToString();
                requestedPlaylist.descriptor = descriptor;

                // Add list of songs
                DataTable songTable = DataAccess.DataAccess.GetTable("GetPlaylistSongs", new Dictionary<string, object> { { "PlaylistId", playlistId } });
                Song currentSong;
                foreach (DataRow r in songTable.Rows)
                {
                    currentSong = new Song() { 
                        songId = Int32.Parse(r["SongId"].ToString()),
                        title = r["Title"].ToString(),
                        artist = r["Artist"].ToString(),
                        lyrics = r["Lyrics"].ToString()
                    };
                    requestedPlaylist.songs.Add(currentSong);
                }
            }
            catch (Exception ex)
            {
                Logging.Log.ToDB($"Error attempting to retrieve playlist for ID {playlistId}.  Details:\n{ex.Message}");
                requestedPlaylist = null;
            }
            return requestedPlaylist;
        }

        public int SavePlaylist(Playlist updatePlaylist)
        {
            int resultId = -1;  // Indicate fail
            bool partialResult = true;
            try
            {
                string songJson = JsonConvert.SerializeObject(updatePlaylist.songs.Select(x => new { songId = x.songId }));

                resultId = DataAccess.DataAccess.GetScalar("SavePlaylist", new Dictionary<string, object> {
                    { "PlaylistID", updatePlaylist.descriptor.playlistId },
                    { "Name", updatePlaylist.descriptor.name },
                    { "Description", updatePlaylist.descriptor.description },
                    { "SongList", songJson } });
            }
            catch (Exception ex)
            {
                Logging.Log.ToDB($"Error attempting to save song, \"{updatePlaylist.descriptor.name}\".  Details:\n{ex.Message}");
                // error flag already defaulted
            }
            return resultId;
        }
    }
}
