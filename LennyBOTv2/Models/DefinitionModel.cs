using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace LennyBOTv2.Models.Definition
{
    public class DefinitionModel : BaseJsonModel<DefinitionModel>
{
        [JsonProperty("word")]
        public string? Word { get; set; }

        [JsonProperty("phonetic")]
        public string? Phonetic { get; set; }

        [JsonProperty("phonetics")]
        public List<Phonetic>? Phonetics { get; set; }

        [JsonProperty("origin")]
        public string? Origin { get; set; }

        [JsonProperty("meanings")]
        public List<Meaning>? Meanings { get; set; }
    }

    public class Meaning
    {
        [JsonProperty("partOfSpeech")]
        public string? PartOfSpeech { get; set; }

        [JsonProperty("definitions")]
        public List<DefinitionElement>? Definitions { get; set; }
    }

    public class DefinitionElement
    {
        [JsonProperty("definition")]
        public string? Definition { get; set; }

        [JsonProperty("example")]
        public string? Example { get; set; }

        [JsonProperty("synonyms")]
        public List<string?>? Synonyms { get; set; }

        [JsonProperty("antonyms")]
        public List<string?>? Antonyms { get; set; }
    }

    public class Phonetic
    {
        [JsonProperty("text")]
        public string? Text { get; set; }

        [JsonProperty("audio")]
        public string? Audio { get; set; }
    }
}
