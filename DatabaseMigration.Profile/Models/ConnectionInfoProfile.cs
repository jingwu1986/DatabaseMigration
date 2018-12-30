using DatabaseMigration.Core;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace DatabaseMigration.Profile
{
    public class ConnectionInfoProfile
    {       
        public string Name { get; set; }
        public DatabaseType DbType { get; set; }          
        public ConnectionInfo ConnectionInfo { get; set; } 
        public bool RememberPassword { get; set; }

        public string ConnectionDescription
        {
            get
            {
                return $"server={this.ConnectionInfo?.Server};database={this.ConnectionInfo?.Database}";
            }
        }

        public string Description
        {
            get
            {
                string connectionDescription = this.ConnectionDescription;
                if (this.Name == connectionDescription)
                {
                    return this.Name;
                }
                else
                {
                    return $"{this.Name}({connectionDescription})";
                }
            }
        }
    }   
}
