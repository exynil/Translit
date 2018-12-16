namespace Translit.Entity
{
	using System.Globalization;
	using Newtonsoft.Json;
	using Newtonsoft.Json.Converters;

	public class User
	{
		[JsonProperty("id")]
		public long Id { get; set; }

		[JsonProperty("name_f")]
		public string FirstName { get; set; }

		[JsonProperty("name_l")]
		public string LastName { get; set; }

		[JsonProperty("patronymic")]
		public string Patronymic { get; set; }

		[JsonProperty("room_id")]
		public long RoomId { get; set; }

		[JsonProperty("group_id")]
		public long GroupId { get; set; }

		[JsonProperty("type")]
		public long Type { get; set; }

		[JsonProperty("token")]
		public string Token { get; set; }
	}

	//public class User
	//{
	//	 User FromJson(string json) => JsonConvert.DeserializeObject<User>(json, Converter.Settings);
	//}

	//public static class Serialize
	//{
	//	public static string ToJson(this User self) => JsonConvert.SerializeObject(self, Converter.Settings);
	//}

	//internal static class Converter
	//{
	//	public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
	//	{
	//		MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
	//		DateParseHandling = DateParseHandling.None,
	//		Converters =
	//		{
	//			new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
	//		},
	//	};
	//}
}