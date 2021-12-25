using MySql.Data.MySqlClient;
using ShimmyMySherbet.MySQL.EF.Models.Interfaces;
using System.Threading.Tasks;

namespace ShimmyMySherbet.MySQL.EF.Models.ConnectionProviders
{
    /// <summary>
    /// Maintains a single database connection
    /// </summary>
    public class SingleConnectionProvider : IConnectionProvider
    {
        private string m_ConnectionString;
        public string ConnectionString => m_ConnectionString;
        private MySqlConnection m_ConnectionInstance { get; set; }

        public bool Connected => m_ConnectionInstance != null && m_ConnectionInstance.State == System.Data.ConnectionState.Open;

        public SingleConnectionProvider(MySqlConnection connection)
        {
            m_ConnectionInstance = connection;
            m_ConnectionString = connection.ConnectionString;
        }

        public SingleConnectionProvider(DatabaseSettings settings)
        {
            m_ConnectionString = settings.ToString();
        }

        public SingleConnectionProvider(string connectionString)
        {
            m_ConnectionString = connectionString;
        }

        public MySqlConnection GetConnection(bool autoOpen = true, bool forceNew = false)
        {
            if (forceNew)
            {
                var conn = new MySqlConnection(m_ConnectionString);
                if (autoOpen)
                {
                    conn.Open();
                }

                return conn;
            }
            else
            {
                return m_ConnectionInstance;
            }
        }

        public async Task<MySqlConnection> GetConnectionAsync(bool autoOpen = true, bool forceNew = false)
        {
            if (forceNew)
            {
                var conn = new MySqlConnection(m_ConnectionString);
                if (autoOpen)
                {
                    await conn.OpenAsync();
                }

                return conn;
            }
            else
            {
                return m_ConnectionInstance;
            }
        }

        public void ReleaseConnection(MySqlConnection connection)
        {
            if (connection != m_ConnectionInstance)
            {
                connection.Close();
            }
        }

        public async Task ReleaseConnectionAsync(MySqlConnection connection)
        {
            if (connection != m_ConnectionInstance)
            {
                await connection.CloseAsync();
            }
        }

        public void Open()
        {
            if (m_ConnectionInstance == null)
            {
                m_ConnectionInstance = new MySqlConnection(m_ConnectionString);
                m_ConnectionInstance.Open();
            }
        }

        public async Task OpenAsync()
        {
            if (m_ConnectionInstance == null)
            {
                m_ConnectionInstance = new MySqlConnection(m_ConnectionString);
                await m_ConnectionInstance.OpenAsync();
            }
        }

        public void Disconnect()
        {
            m_ConnectionInstance?.Close();
        }

        public async Task DisconnectAsync()
        {
            await m_ConnectionInstance?.CloseAsync();
        }

        public void Dispose()
        {
            m_ConnectionInstance.Dispose();
        }
    }
}