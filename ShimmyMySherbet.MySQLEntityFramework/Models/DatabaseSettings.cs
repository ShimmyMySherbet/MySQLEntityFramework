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
    }
}