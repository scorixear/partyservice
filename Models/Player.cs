using Newtonsoft.Json;
using System.Collections.Generic;

namespace PartyService.Model
{
    public class Player
    {
        [JsonIgnore]
        public int Id { get; set; }

        [JsonProperty]
        public string Name { get; set; }
    }
}
