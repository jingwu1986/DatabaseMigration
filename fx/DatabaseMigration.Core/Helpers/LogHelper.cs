using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;

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

            File.AppendAllText(filePath, $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}:{message}{Environment.NewLine}", Encoding.UTF8);
        }
    }
}
