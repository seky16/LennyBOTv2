using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace LennyBOTv2.Models.UrbanDictionary
{
    public class UrbanModel : BaseJsonModel<UrbanModel>
    {
        [JsonProperty("list")]
        public List<List>? List { get; set; }
    }

    public class List
    {
        [JsonProperty("definition")]
        public string? Definition { get; set; }

        [JsonProperty("permalink")]
        public Uri? Permalink { get; set; }

        [JsonProperty("thumbs_up")]
        public long? ThumbsUp { get; set; }

        [JsonProperty("sound_urls")]
        public List<Uri>? SoundUrls { get; set; }

        [JsonProperty("author")]
        public string? Author { get; set; }

        [JsonProperty("word")]
        public string? Word { get; set; }

        [JsonProperty("defid")]
        public long? Defid { get; set; }

        [JsonProperty("current_vote")]
        public string? CurrentVote { get; set; }

        [JsonProperty("written_on")]
        public DateTimeOffset? WrittenOn { get; set; }

        [JsonProperty("example")]
        public string? Example { get; set; }

        [JsonProperty("thumbs_down")]
        public long? ThumbsDown { get; set; }
    }
}
