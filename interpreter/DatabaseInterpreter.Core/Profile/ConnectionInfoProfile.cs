using DatabaseInterpreter.Model;

namespace DatabaseInterpreter.Profile
{
    public class ConnectionInfoProfile
    {       
        public string Name { get; set; }
        public DatabaseType DbType { get; set; }          
        public ConnectionInfo ConnectionInfo { get; set; } 
        public bool RememberPassword { get; set; }

        public string ConnectionDescription => $"server={this.ConnectionInfo?.Server};database={this.ConnectionInfo?.Database}";

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
