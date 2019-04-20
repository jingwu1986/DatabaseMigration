using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DatabaseMigration.Test
{
    public class OutputHelper
    {
        public static void Output(string name, object obj, bool useJson)
        {
            string content = "";
            if (useJson)
            {
                content = JsonConvert.SerializeObject(obj, Formatting.Indented);
            }
            else
            {
                content = obj?.ToString();
            }

            Console.WriteLine(content);

            string folder=DateTime.Today.ToString("yyyyMMdd");

            if(!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            File.WriteAllText($@"{folder}\\{name}.txt", content);
        }
    }
}
