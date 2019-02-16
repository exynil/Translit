using System;
using System.Drawing;
using System.Management;
using System.Reflection;
using System.Windows.Forms;
using Translit.Properties;

namespace Translit.Models.Other
{
    public class UserData : ICloneable
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string ComputerName { get; set; }
        public bool PermissionToChange { get; set; }
        public bool PermissionToUse { get; set; }
        public Size PrimaryMonitorSize { get; set; }
        public string MacAddress { get; set; }
        public FileCounter Counter { get; set; }
        public DateTime FirstUsedDate { get; set; }
        public DateTime LastUsedDate { get; set; }
        public string ProgramVersion { get; set; }

        public UserData()
        {
            Id = FingerPrint.Value();
            UserName = SystemInformation.UserName;
            ComputerName = SystemInformation.ComputerName;
            MacAddress = GetMacAddress();
            PermissionToChange = Settings.Default.PermissionToChange;
            PermissionToUse = true;
            PrimaryMonitorSize = SystemInformation.PrimaryMonitorSize;
            Counter = new FileCounter();
            FirstUsedDate = LastUsedDate = DateTime.Now;
            ProgramVersion = GetProgramVersion();
        }

        public void UpdateAllData()
        {
            UserName = SystemInformation.UserName;
            ComputerName = SystemInformation.ComputerName;
            MacAddress = GetMacAddress();
            PermissionToChange = Settings.Default.PermissionToChange;
            PrimaryMonitorSize = SystemInformation.PrimaryMonitorSize;
            LastUsedDate = DateTime.Now;
            ProgramVersion = GetProgramVersion();
        }

        public string GetMacAddress()
        {
            var mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
            var moc = mc.GetInstances();
            var macAddress = string.Empty;
            foreach (var mo in moc)
            {
                if (macAddress == string.Empty)
                {
                    if ((bool)mo["IPEnabled"]) macAddress = mo["MacAddress"].ToString();
                }
                mo.Dispose();
            }
            return macAddress;
        }

        string GetProgramVersion()
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;

            return $"{version.Major}.{version.Minor} ({version.Build})";
        }

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
                MacAddress = MacAddress,
                Counter = (FileCounter)Counter.Clone(),
                FirstUsedDate = FirstUsedDate,
                LastUsedDate = LastUsedDate,
                ProgramVersion = ProgramVersion
            };
        }
    }
}
