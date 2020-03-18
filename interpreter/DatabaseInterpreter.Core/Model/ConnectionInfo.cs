namespace DatabaseInterpreter.Model
{
    public class ConnectionInfo
    {
        public string Server { get; set; }
        public string Port { get; set; }
        public string Database { get; set; }
        public bool IntegratedSecurity { get; set; }
        public string UserId { get; set; }
        public string Password { get; set; }
    }
}
