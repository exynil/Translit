using Newtonsoft.Json;

namespace Translit.Entity
{
	public class GeneralSettings
	{
		[JsonProperty("language")]
		public string Language { get; set; }
	}
}
