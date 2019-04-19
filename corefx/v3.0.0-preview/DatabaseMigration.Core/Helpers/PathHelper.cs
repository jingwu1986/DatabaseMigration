using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseMigration.Core
{
    public class PathHelper
    {
        public static string GetAssemblyFolder()
        {
            string dllFolder = Assembly.GetExecutingAssembly().CodeBase;
            return Path.GetDirectoryName(dllFolder.Substring(8, dllFolder.Length - 8));
        }
    }
}
