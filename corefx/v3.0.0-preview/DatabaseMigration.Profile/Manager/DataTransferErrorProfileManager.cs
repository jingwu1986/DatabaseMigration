using DatabaseMigration.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DatabaseMigration.Profile
{
    public class DataTransferErrorProfileManager
    {
        public static string ProfileFolder { get; set; } = "Profiles";

        public static string ProfilePath
        {
            get
            {
                return Path.Combine(ProfileFolder, "DataTransferError.xml");
            }
        }

        public static bool Save(DataTransferErrorProfile profile)
        {
            if (!Directory.Exists(ProfileFolder))
            {
                Directory.CreateDirectory(ProfileFolder);
            }

            string filePath = ProfilePath;
            if (!File.Exists(filePath))
            {
                using (StreamWriter sw = File.CreateText(filePath))
                {
                    sw.WriteLine(
$@"<?xml version=""1.0"" encoding=""utf-8""?>
<Config>
</Config>
");
                    sw.Flush();
                }
            }

            XDocument doc = XDocument.Load(filePath);
            XElement root = doc.Root;           

            XElement profileElement = root.Elements("Item").FirstOrDefault(item => 
            item.Attribute("SourceServer")?.Value == profile.SourceServer &&
            item.Attribute("SourceDatabase")?.Value == profile.SourceDatabase &&
            item.Attribute("TargetServer")?.Value == profile.TargetServer &&
            item.Attribute("TargetDatabase")?.Value == profile.TargetDatabase
            );

            if (profileElement == null)
            {
                profileElement = new XElement("Item", 
                    new XAttribute("SourceServer", profile.SourceServer),
                    new XAttribute("SourceDatabase", profile.SourceDatabase),
                    new XAttribute("SourceTableName", profile.SourceTableName),
                    new XAttribute("TargetServer", profile.TargetServer),
                    new XAttribute("TargetDatabase", profile.TargetDatabase),
                    new XAttribute("TargetTableName", profile.TargetTableName)
                    );
                root.Add(profileElement);
            }
            else
            {
                profileElement.Attribute("SourceTableName").Value = profile.SourceTableName;
                profileElement.Attribute("TargetTableName").Value = profile.TargetTableName;              
            }          

            doc.Save(filePath);

            return true;
        }

        public static bool Remove(DataTransferErrorProfile profile)
        {
            string filePath = ProfilePath;
            if (!File.Exists(filePath))
            {
                return false;
            }

            XDocument doc = XDocument.Load(filePath);
            XElement root = doc.Root;

            XElement profileElement = root.Elements("Item").FirstOrDefault(item =>
               item.Attribute("SourceServer")?.Value == profile.SourceServer &&
               item.Attribute("SourceDatabase")?.Value == profile.SourceDatabase &&
               item.Attribute("TargetServer")?.Value == profile.TargetServer &&
               item.Attribute("TargetDatabase")?.Value == profile.TargetDatabase
               );

            if(profileElement!=null)
            {
                profileElement.Remove();
                doc.Save(filePath);
                return true;
            }

            return false;
        }

        public static DataTransferErrorProfile GetProfile(ConnectionInfo sourceConnectionInfo, ConnectionInfo targetConnectionInfo)
        {
            DataTransferErrorProfile profile = null;
            string filePath = ProfilePath;
            if (!File.Exists(filePath) || sourceConnectionInfo==null || targetConnectionInfo==null)
            {
                return null;
            }

            XDocument doc = XDocument.Load(filePath);
            XElement root = doc.Root;

            XElement profileElement = root.Elements("Item").FirstOrDefault(item =>
               item.Attribute("SourceServer")?.Value == sourceConnectionInfo.Server &&
               item.Attribute("SourceDatabase")?.Value == sourceConnectionInfo.Database &&
               item.Attribute("TargetServer")?.Value == targetConnectionInfo.Server &&
               item.Attribute("TargetDatabase")?.Value == targetConnectionInfo.Database
               );

            if(profileElement!=null)
            {
                profile = new DataTransferErrorProfile()
                {
                    SourceServer= sourceConnectionInfo.Server,
                    SourceDatabase= sourceConnectionInfo.Database,
                    SourceTableName= profileElement.Element("SourceTableName")?.Value,

                    TargetServer=targetConnectionInfo.Server,
                    TargetDatabase=targetConnectionInfo.Database,
                    TargetTableName= profileElement.Element("TargetTableName")?.Value
                };
            }

            return profile;
        }
    }
}
