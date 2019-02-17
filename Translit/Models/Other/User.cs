using Newtonsoft.Json;

namespace Translit.Models.Other
{
    public class User
    {
        [JsonProperty("id")] public static long Id { get; set; }

        [JsonProperty("name_f")] public static string FirstName { get; set; }

        [JsonProperty("name_l")] public static string LastName { get; set; }

        [JsonProperty("patronymic")] public static string Patronymic { get; set; }

        [JsonProperty("token")] public static string Token { get; set; }

        public static string ToShortString()
        {
            return $"{LastName} {FirstName}";
        }

        public static void Clear()
        {
            Id = 0;
            FirstName = "";
            LastName = "";
            Patronymic = "";
            Token = "";
        }
    }
}