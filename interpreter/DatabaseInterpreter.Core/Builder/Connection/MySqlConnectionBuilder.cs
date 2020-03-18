﻿using DatabaseInterpreter.Model;
using System.Text;

namespace DatabaseInterpreter.Core
{
    public class MySqlConnectionBuilder : IConnectionBuilder
    {
        public string BuildConntionString(ConnectionInfo connectionInfo)
        {
            StringBuilder sb = new StringBuilder($"server={connectionInfo.Server};database={connectionInfo.Database};Charset=utf8;");

            if(connectionInfo.IntegratedSecurity)
            {
                sb.Append($"Integrated Security=True;");
            }
            else
            {
                sb.Append($"user id={connectionInfo.UserId};password={connectionInfo.Password};SslMode=none;");
            }

            return sb.ToString();
        }
    }
}
