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
    public class SongApiController : ControllerBase
    {
        // GET: api/SongApi
        [HttpGet]
        //public IEnumerable<string> Get()
        public string Get()
        {
            SongRepository repo = new SongRepository();
            List<Song> songs = repo.GetSongs();
            
            return JsonConvert.SerializeObject(songs);
            //return new string[] { "value1", "value2" };
        }

        // GET: api/SongApi/5
        [HttpGet("{id}", Name = "GetSong")]
        public string Get(int id)
        {
            SongRepository repo = new SongRepository();
            Song song = repo.GetSong(id);

            return JsonConvert.SerializeObject(song);
        }

        // POST: api/SongApi
        [HttpPost]
        public string Post([FromBody] JsonElement updateSong)
        {
            SongRepository repo = new SongRepository();
            string json = System.Text.Json.JsonSerializer.Serialize(updateSong);
            Song song = (Song)JsonConvert.DeserializeObject(json, typeof(Song));
            int result = repo.SaveSong(song);
            return $"{{\"result\":{result}}}";
        }

        // PUT: api/SongApi/5
        // Use of SQL MERGE and optional inclusing of an ID make this unnecessary but implementation might be considered for consistency
        // or other needs.
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public string Delete(int id)
        {
            SongRepository repo = new SongRepository();
            int result = repo.DeleteSong(id);

            return $"{{\"result\":{result}}}";
        }
    }
}
