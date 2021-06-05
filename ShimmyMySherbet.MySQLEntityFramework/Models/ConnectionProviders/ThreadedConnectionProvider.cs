using MySql.Data.MySqlClient;
using ShimmyMySherbet.MySQL.EF.Models.Interfaces;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ShimmyMySherbet.MySQL.EF.Models.ConnectionProviders
{
    public class ThreadedConnectionProvider : IConnectionProvider
    {
        private string m_ConnectionString;

        public bool Connected => true;

        public string ConnectionString => m_ConnectionString;

        private Dictionary<int, MySqlConnection> m_Connections = new Dictionary<int, MySqlConnection>();

        private int m_ThreadID => Thread.CurrentThread.ManagedThreadId;

        public ThreadedConnectionProvider(MySqlConnection connection)
        {
            m_ConnectionString = connection.ConnectionString;
            m_Connections[m_ThreadID] = connection;
        }

        public ThreadedConnectionProvider(DatabaseSettings settings)
        {
            m_ConnectionString = settings.ToString();
        }

        public ThreadedConnectionProvider(string connectionString)
        {
            m_ConnectionString = connectionString;
        }

        public void Disconnect()
        {
            lock (m_Connections)
            {
                if (m_Connections.ContainsKey(m_ThreadID))
                {
                    m_Connections[m_ThreadID].Close();
                }
            }
        }

        public async Task DisconnectAsync()
        {
            MySqlConnection conn;
            lock (m_Connections)
            {
                if (m_Connections.ContainsKey(m_ThreadID))
                {
                    conn = m_Connections[m_ThreadID];
                }
                else
                {
                    return;
                }
            }
            await conn.CloseAsync();
        }

        public void Dispose()
        {
            lock (m_Connections)
            {
                foreach (var conn in m_Connections.Values)
                    conn.Dispose();
            }
        }

        public MySqlConnection GetConnection(bool autoOpen = true, bool forceNew = false)
        {
            if (!forceNew)
            {
                lock (m_Connections)
                {
                    if (m_Connections.ContainsKey(m_ThreadID))
                    {
                        return m_Connections[m_ThreadID];
                    }
                }
            }

            var conn = new MySqlConnection(m_ConnectionString);
            if (autoOpen)
            {
                conn.Open();
            }

            lock (m_Connections)
            {
                m_Connections[m_ThreadID] = conn;
            }
            return conn;
        }

        public async Task<MySqlConnection> GetConnectionAsync(bool autoOpen = true, bool forceNew = false)
        {
            if (!forceNew)
            {
                lock (m_Connections)
                {
                    if (m_Connections.ContainsKey(m_ThreadID))
                    {
                        return m_Connections[m_ThreadID];
                    }
                }
            }

            var conn = new MySqlConnection(m_ConnectionString);
            if (autoOpen)
            {
                await conn.OpenAsync();
            }

            lock (m_Connections)
            {
                m_Connections[m_ThreadID] = conn;
            }
            return conn;
        }

        public void Open()
        {
            lock (m_Connections)
            {
                if (m_Connections.ContainsKey(m_ThreadID))
                {
                    return;
                }
            }

            var conn = new MySqlConnection(m_ConnectionString);
            conn.Open();

            lock (m_Connections)
            {
                m_Connections[m_ThreadID] = conn;
            }
        }

        public async Task OpenAsync()
        {
            lock (m_Connections)
            {
                if (m_Connections.ContainsKey(m_ThreadID))
                {
                    return;
                }
            }

            var conn = new MySqlConnection(m_ConnectionString);
            await conn.OpenAsync();

            lock (m_Connections)
            {
                m_Connections[m_ThreadID] = conn;
            }
        }

        public void ReleaseConnection(MySqlConnection connection)
        {
        }

        public Task ReleaseConnectionAsync(MySqlConnection connection)
        {
            return Task.CompletedTask;
        }
    }
}