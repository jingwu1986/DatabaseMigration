using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace DatabaseInterpreter.Profile
{
    public class ConnectionInfoProfileManager
    {
        public static string ProfileFolder { get; set; } = "Profiles";

        public static string ProfilePath => Path.Combine(ProfileFolder, "Connection.xml");

        public static string Save(ConnectionInfoProfile profile)
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

            XElement dbTypeElement = root.Element(profile.DbType.ToString());

            if (dbTypeElement == null)
            {
                dbTypeElement = new XElement(profile.DbType.ToString());
                root.Add(dbTypeElement);
            }

            string profileName = profile.Name;
            if (string.IsNullOrEmpty(profileName))
            {
                profileName = profile.ConnectionDescription;
            }

            XElement profileElement = dbTypeElement.Elements("Item").FirstOrDefault(item => item.Attribute("Name")?.Value == profile.Name);
            if (profileElement == null)
            {
                profileElement = new XElement("Item", new XAttribute("Name", profileName));
                dbTypeElement.Add(profileElement);
            }

            profileElement.RemoveNodes();
            ConnectionInfo connectionInfo = profile.ConnectionInfo;
            profileElement.Add(
                new XElement("Server", connectionInfo.Server),
                new XElement("Port", connectionInfo.Port),
                new XElement("IntegratedSecurity", connectionInfo.IntegratedSecurity),
                new XElement("UserId", connectionInfo.UserId),
                new XElement("Password", profile.RememberPassword ? AesHelper.Encrypt(connectionInfo.Password) : ""),
                new XElement("Database", connectionInfo.Database)
                );

            doc.Save(filePath);

            return profileName;
        }

        public static bool Remove(DatabaseType dbType, string profileName)
        {
            string filePath = ProfilePath;
            if (!File.Exists(filePath))
            {
                return false;
            }

            XDocument doc = XDocument.Load(filePath);
            XElement root = doc.Root;

            XElement dbTypeElement = root.Element(dbType.ToString());

            if (dbTypeElement != null)
            {
                XElement profileElement = dbTypeElement.Elements("Item").FirstOrDefault(item => item.Attribute("Name")?.Value == profileName);
                if (profileElement != null)
                {
                    profileElement.Remove();
                }

                doc.Save(filePath);
                return true;
            }
            return false;
        }

        public static List<ConnectionInfoProfile> GetProfiles(DatabaseType dbType)
        {
            List<ConnectionInfoProfile> profiles = new List<ConnectionInfoProfile>();
            string filePath = ProfilePath;
            if (!File.Exists(filePath))
            {
                return profiles;
            }

            XDocument doc = XDocument.Load(filePath);
            XElement root = doc.Root;

            XElement dbTypeElement = root.Element(dbType.ToString());

            if (dbTypeElement != null)
            {
                profiles.AddRange(dbTypeElement.Elements("Item").Select(item=> new ConnectionInfoProfile() { Name = item.Attribute("Name").Value, ConnectionInfo = SelectConnectionInfo(item) }));
            }

            return profiles;
        }        

        public static ConnectionInfo GetConnectionInfo(DatabaseType dbType, string profileName)
        {
            ConnectionInfo connectionInfo = new ConnectionInfo();

            string filePath = ProfilePath;
            if (File.Exists(filePath))
            {
                XDocument doc = XDocument.Load(filePath);
                XElement root = doc.Root;

                XElement dbTypeElement = root.Element(dbType.ToString());

                if (dbTypeElement != null)
                {
                    XElement profileElement = dbTypeElement.Elements("Item").FirstOrDefault(item => item.Attribute("Name")?.Value == profileName);
                    connectionInfo = SelectConnectionInfo(profileElement);

                }
            }

            return connectionInfo;
        }

        private static ConnectionInfo SelectConnectionInfo(XElement profileElement)
        {
            ConnectionInfo connectionInfo = null;
            if (profileElement != null)
            {
                connectionInfo = new ConnectionInfo();
                connectionInfo.Server = profileElement.Element("Server")?.Value;
                connectionInfo.Port = profileElement.Element("Port")?.Value;
                connectionInfo.IntegratedSecurity = profileElement.Element("IntegratedSecurity")?.Value.ToString().ToLower() == "true";
                connectionInfo.UserId = profileElement.Element("UserId")?.Value;

                string password = profileElement.Element("Password")?.Value;

                if (!string.IsNullOrEmpty(password))
                {
                    connectionInfo.Password = AesHelper.Decrypt(password);
                }

                connectionInfo.Database = profileElement.Element("Database")?.Value;
            }

            return connectionInfo;
        }
    }
}
