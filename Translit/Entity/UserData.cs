using System;
using System.Drawing;
using System.Management;
using System.Reflection;
using System.Windows.Forms;
using Translit.Properties;

namespace Translit.Entity
{
    public class UserData
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string ComputerName { get; set; }
        public bool AdminPermissions { get; set; }
        public Size PrimaryMonitorSize { get; set; }
        public string MacAddress { get; set; }
        public Files TransliteratedFiles { get; set; }
        public DateTime FirstUsedDate { get; set; }
        public DateTime LastUsedDate { get; set; }
        public string TranslitVersion { get; set; }

        public UserData()
        {
            Id = FingerPrint.Value();
            AdminPermissions = Settings.Default.AdminPermissions;
            UserName = SystemInformation.UserName;
            ComputerName = SystemInformation.ComputerName;
            PrimaryMonitorSize = SystemInformation.PrimaryMonitorSize;
            TransliteratedFiles = new Files();
            FirstUsedDate = DateTime.Now;
            LastUsedDate = DateTime.Now;
            TranslitVersion = GetTranslitVersion();
        }

        public void UpdateAllData()
        {
            AdminPermissions = Settings.Default.AdminPermissions;
            UserName = SystemInformation.UserName;
            ComputerName = SystemInformation.ComputerName;
            PrimaryMonitorSize = SystemInformation.PrimaryMonitorSize;
            LastUsedDate = DateTime.Now;
            TranslitVersion = GetTranslitVersion();
        }

        public string GetMacAddress()
        {
            var mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
            var moc = mc.GetInstances();
            var macAddress = string.Empty;
            foreach (var mo in moc)
            {
                if (macAddress == string.Empty)  // only return MAC Address from first card
                {
                    if ((bool)mo["IPEnabled"]) macAddress = mo["MacAddress"].ToString();
                }
                mo.Dispose();
            }
            return macAddress;
        }

        string GetTranslitVersion()
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;

            return $"{version.Major}.{version.Minor} ({version.Build})";
        }

        public void SetFirstUsedDate()
        {
            FirstUsedDate = DateTime.Now;
        }

        public void IncreaseWord()
        {
            TransliteratedFiles.Word++;
        }

        public void IncreaseExcel()
        {
            TransliteratedFiles.Excel++;
        }

        public void IncreasePowerPoint()
        {
            TransliteratedFiles.PowerPoint++;
        }

        public void IncreasePdf()
        {
            TransliteratedFiles.Pdf++;
        }

        public void IncreaseRtf()
        {
            TransliteratedFiles.Rtf++;
        }

        public void IncreaseTxt()
        {
            TransliteratedFiles.Txt++;
        }
    }
}
