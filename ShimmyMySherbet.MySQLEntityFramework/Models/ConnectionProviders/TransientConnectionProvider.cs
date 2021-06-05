using MySql.Data.MySqlClient;
using ShimmyMySherbet.MySQL.EF.Models.Interfaces;
using System.Threading.Tasks;

namespace ShimmyMySherbet.MySQL.EF.Models.ConnectionProviders
{
    /// <summary>
    /// Creates a new connection each time one is requested using the thread pool
    /// </summary>
    public class TransientConnectionProvider : IConnectionProvider
    {
        private string m_ConnectionString { get; set; }
        public string ConnectionString => m_ConnectionString;

        public bool Connected => true;

        public TransientConnectionProvider(MySqlConnection connection)
        {
            m_ConnectionString = connection.ConnectionString;
        }

        public TransientConnectionProvider(DatabaseSettings settings)
        {
            m_ConnectionString = settings.ToString();
        }

        public TransientConnectionProvider(string connectionString)
        {
            m_ConnectionString = connectionString;
        }

        public MySqlConnection GetConnection(bool autoOpen = true, bool forceNew = false)
        {
            var conn = new MySqlConnection(m_ConnectionString);
            if (autoOpen)
            {
                conn.Open();
            }
            return conn;
        }

        public async Task<MySqlConnection> GetConnectionAsync(bool autoOpen = true, bool forceNew = false)
        {
            var conn = new MySqlConnection(m_ConnectionString);
            if (autoOpen)
            {
                await conn.OpenAsync();
            }
            return conn;
        }

        public void ReleaseConnection(MySqlConnection connection)
        {
            connection.Close();
        }

        public async Task ReleaseConnectionAsync(MySqlConnection connection)
        {
            await connection.CloseAsync();
        }

        public void Open()
        {
        }

        public Task OpenAsync()
        {
            return Task.CompletedTask;
        }

        public void Disconnect()
        {
        }

        public Task DisconnectAsync()
        {
            return Task.CompletedTask;
        }

        public void Dispose()
        {
        }
    }
}