using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using Translit.Properties;

namespace Translit.Models.Other
{
    public class UserData : ICloneable
    {
        public UserData()
        {
            Id = FingerPrint.Value();
            UserName = SystemInformation.UserName;
            ComputerName = SystemInformation.ComputerName;
            PermissionToChange = Settings.Default.PermissionToChange;
            PermissionToUse = true;
            PrimaryMonitorSize = SystemInformation.PrimaryMonitorSize;
            Counter = new FileCounter();
            FirstUsedDate = LastUsedDate = DateTime.Now;
            ProgramVersion = GetProgramVersion();
        }

        public string Id { get; set; }
        public string UserName { get; set; }
        public string ComputerName { get; set; }
        public bool PermissionToChange { get; set; }
        public bool PermissionToUse { get; set; }
        public Size PrimaryMonitorSize { get; set; }
        public FileCounter Counter { get; set; }
        public DateTime FirstUsedDate { get; set; }
        public DateTime LastUsedDate { get; set; }
        public string ProgramVersion { get; set; }

        public object Clone()
        {
            return new UserData
            {
                Id = Id,
                UserName = UserName,
                ComputerName = ComputerName,
                PermissionToChange = PermissionToChange,
                PermissionToUse = PermissionToUse,
                PrimaryMonitorSize = PrimaryMonitorSize,
                Counter = (FileCounter) Counter.Clone(),
                FirstUsedDate = FirstUsedDate,
                LastUsedDate = LastUsedDate,
                ProgramVersion = ProgramVersion
            };
        }

        public void UpdateAllData()
        {
            UserName = SystemInformation.UserName;
            ComputerName = SystemInformation.ComputerName;
            PermissionToChange = Settings.Default.PermissionToChange;
            PrimaryMonitorSize = SystemInformation.PrimaryMonitorSize;
            LastUsedDate = DateTime.Now;
            ProgramVersion = GetProgramVersion();
        }

        private string GetProgramVersion()
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;

            return $"{version.Major}.{version.Minor} ({version.Build})";
        }
    }
}