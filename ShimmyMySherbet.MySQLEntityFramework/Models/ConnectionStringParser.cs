using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShimmyMySherbet.MySQL.EF.Models
{
    /// <summary>
    /// Provides a parser to parse a MySQL connection string to <seealso cref="DatabaseSettings"/>
    /// </summary>
    public static class ConnectionStringParser
    {
        private static readonly Dictionary<string, string[]> m_Aliases = new Dictionary<string, string[]>(StringComparer.InvariantCultureIgnoreCase)
        {
            {"Host", new string[] { "Host", "Server", "Data Source", "DataSource", "Address", "Addr", "Network Address" } },
            {"Port", new string[] { "Port" } },
            {"Password", new string[] { "Password", "pwd" } },
            {"Username", new string[] { "Username", "User Id", "Uid", "User name" } },
            {"Database", new string[] { "Database", "db" } }
        };
        /// <summary>
        /// Parses a MySQL connection string into a <seealso cref="DatabaseSettings"/>
        /// </summary>
        public static DatabaseSettings Parse(string conn)
        {
            string[] parts = conn.Split(';').ToArray();
            Dictionary<string, string> connParts = new Dictionary<string, string>();
            foreach (var prtts in parts)
            {
                if (prtts.Contains("="))
                {
                    string k = prtts.Split('=')[0];
                    string v = prtts.Substring(k.Length + 1);
                    connParts[k.Trim(' ')] = v.Trim(' ');
                }
            }

            Dictionary<string, string> parsed = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

            foreach (var ent in connParts)
            {
                var h = m_Aliases.FirstOrDefault(x => x.Value.Contains(ent.Key, StringComparer.InvariantCultureIgnoreCase));
                parsed[h.Key] = ent.Value;
            }

            DatabaseSettings settings = DatabaseSettings.Default;

            if (parsed.ContainsKey("Host"))
            {
                settings.DatabaseAddress = parsed["Host"];
            }

            if (parsed.ContainsKey("Password"))
            {
                settings.DatabasePassword = parsed["Password"];
            }

            if (parsed.ContainsKey("Username"))
            {
                settings.DatabaseUsername = parsed["Username"];
            }

            if (parsed.ContainsKey("Database"))
            {
                settings.DatabaseName = parsed["Database"];
            }

            if (parsed.ContainsKey("Port") && ushort.TryParse(parsed["Port"], out var p))
            {
                settings.DatabasePort = p;
            }
            return settings;
        }
    }
}
