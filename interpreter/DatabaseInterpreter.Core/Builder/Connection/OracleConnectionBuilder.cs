﻿using DatabaseInterpreter.Model;
using System.Text;

namespace DatabaseInterpreter.Core
{
    public class OracleConnectionBuilder : IConnectionBuilder
    {
        public string BuildConntionString(ConnectionInfo connectionInfo)
        {
            string server = connectionInfo.Server;
            string serviceName = "ORCL";
            string port = connectionInfo.Port;

            if(string.IsNullOrEmpty(port))
            {
                port = "1521";
            }

            if(server.Contains("/"))
            {
                string[] serverService = connectionInfo.Server.Split('/');
                server = serverService[0];
                serviceName = serverService[1];
            }
            
            StringBuilder sb = new StringBuilder($"Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST={server})(PORT={port})))(CONNECT_DATA=(SERVICE_NAME={serviceName})));");

            if(connectionInfo.IntegratedSecurity)
            {
                
            }
            else
            {
                sb.Append($"User Id={connectionInfo.UserId};Password={connectionInfo.Password};");
            }

            return sb.ToString();
        }
    }
}
