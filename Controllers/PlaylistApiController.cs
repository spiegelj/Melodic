using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Melodic.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Melodic.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlaylistApiController : ControllerBase
    {
        // GET: api/PlaylistApi
        [HttpGet]
        public string Get()
        {
            PlaylistRepository repo = new PlaylistRepository();
            List<PlaylistDescriptor> playlists = repo.GetPlaylists();

            return JsonConvert.SerializeObject(playlists);
            //return new string[] { "value1", "value2" };
        }

        // GET: api/PlaylistApi/5
        [HttpGet("{id}", Name = "GetPlaylist")]
        public string Get(int id)
        {
            PlaylistRepository repo = new PlaylistRepository();
            Playlist playlist = repo.GetPlaylist(id);

            return JsonConvert.SerializeObject(playlist);
        }

        // POST: api/PlaylistApi
        [HttpPost]
        public string Post([FromBody] JsonElement updatePlaylist)
        {
            PlaylistRepository repo = new PlaylistRepository();
            string json = System.Text.Json.JsonSerializer.Serialize(updatePlaylist);
            Playlist playlist = (Playlist)JsonConvert.DeserializeObject(json, typeof(Playlist));
            int result = repo.SavePlaylist(playlist);
            return $"{{\"result\":{result}}}";
        }

        // PUT: api/PlaylistApi/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
