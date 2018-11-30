using Newtonsoft.Json;

namespace Translit.Entity
{
	class Word
	{
		[JsonProperty("id")]
		public int Id { get; set; }
		[JsonProperty("cyrl")]
		public string Cyryllic { get; set; }
		[JsonProperty("latn")]
		public string Latin { get; set; }
	}
}
