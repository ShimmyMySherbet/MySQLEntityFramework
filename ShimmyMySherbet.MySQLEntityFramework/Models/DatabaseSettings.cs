namespace ShimmyMySherbet.MySQL.EF.Models
{
    /// <summary>
    /// Provides basic connection settings, useful to use as part of an app config.
    /// </summary>
    public class DatabaseSettings
    {
        public string DatabaseAddress;
        public string DatabaseUsername;
        public string DatabasePassword;
        public string DatabaseName;
        public ushort DatabasePort;

        public DatabaseSettings()
        {
        }

        public DatabaseSettings(string address, string username, string password, string database, ushort port = 3306)
        {
            DatabaseAddress = address;
            DatabaseUsername = username;
            DatabasePassword = password;
            DatabaseName = database;
            DatabasePort = port;
        }

        public static DatabaseSettings Default => new DatabaseSettings("127.0.0.1", "Username", "Password", "Database");

        public override bool Equals(object obj)
        {
            if (obj is DatabaseSettings set)
            {
                return
                    set.DatabaseAddress == DatabaseAddress &&
                    set.DatabaseName == DatabaseName &&
                    set.DatabasePassword == DatabasePassword &&
                    set.DatabasePort == DatabasePort &&
                    set.DatabaseUsername == DatabaseUsername;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static DatabaseSettings Parse(string connectionString) => ConnectionStringParser.Parse(connectionString);

        public override string ToString()
        {
            return $"{(DatabaseAddress != null ? $"Server={DatabaseAddress};" : "")}{(DatabaseName != null ? $"Database={DatabaseName};" : "")}{(DatabaseUsername != null ? $"User Id={DatabaseUsername};" : "")}{(DatabasePassword != null ? $"Password={DatabasePassword};" : "")}{(DatabasePort != 3306 ? $"Port={DatabasePort};" : "")}";
        }
    }
}