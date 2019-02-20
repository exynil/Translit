using System.Configuration;
using System.Linq;
using LiteDB;

namespace Translit.Models.Other
{
    public class StatisticsModel
    {
        public StatisticsModel()
        {
            ConnectionString = ConfigurationManager.ConnectionStrings["LiteDbConnection"].ConnectionString;
        }

        public string ConnectionString { get; set; }

        public UserData GetLocalUserData()
        {
            using (var db = new LiteDatabase(ConnectionString))
            {
                var analytics = db.GetCollection<UserData>("Analytics");
                var localUserData = analytics.FindAll().FirstOrDefault(u => u.Id == FingerPrint.Value());

                return localUserData ?? new UserData();
            }
        }

        public UserData GetUnsentUserData()
        {
            using (var db = new LiteDatabase(ConnectionString))
            {
                var unsentAnalytics = db.GetCollection<UserData>("UnsentAnalytics");
                var unsentUserData = unsentAnalytics.FindAll().FirstOrDefault(u => u.Id == FingerPrint.Value());

                return unsentUserData ?? new UserData();
            }
        }
    }
}