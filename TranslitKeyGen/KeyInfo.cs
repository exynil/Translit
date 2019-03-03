namespace TranslitKeyGen
{
    using System;
    using System.Collections.Generic;

    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public partial class KeyInfo
    {
        [JsonProperty("ActivatedComputers")]
        public Dictionary<string, ActivatedComputer> ActivatedComputers { get; set; }

        [JsonProperty("Blocked")]
        public bool Blocked { get; set; }

        [JsonProperty("Key")]
        public string Key { get; set; }

        [JsonProperty("NumberOfComputers")]
        public long NumberOfComputers { get; set; }

        [JsonProperty("Owner")]
        public string Owner { get; set; }

        [JsonProperty("Note")]
        public string Note { get; set; }
    }

    public class ActivatedComputer
    {
        [JsonProperty("ActivationDate")]
        public DateTime ActivationDate { get; set; }

        [JsonProperty("Id")]
        public string Id { get; set; }
    }

    public partial class KeyInfo
    {
        public static KeyInfo FromJson(string json) => JsonConvert.DeserializeObject<KeyInfo>(json, Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this KeyInfo self) => JsonConvert.SerializeObject(self, Converter.Settings);
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }
}
