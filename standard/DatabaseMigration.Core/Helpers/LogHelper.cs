using System;
using System.Text;
using System.IO;

namespace DatabaseMigration.Core
{
    public class LogHelper
    {
        public static void Log(string message)
        {
            string logFolder = "log";// Path.Combine(PathHelper.GetAssemblyFolder(), "log");
            if(!Directory.Exists(logFolder))
            {
                Directory.CreateDirectory(logFolder);
            }

            string filePath = Path.Combine(logFolder, DateTime.Today.ToString("yyyyMMdd") + ".txt");

            File.AppendAllText(filePath, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}:{message}{Environment.NewLine}", Encoding.UTF8);
        }
    }
}
