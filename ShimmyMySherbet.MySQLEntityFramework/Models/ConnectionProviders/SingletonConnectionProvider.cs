using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace ShimmyMySherbet.MySQL.EF.Models.ConnectionProviders
{
    /// <summary>
    /// Maintains a single working connection, and manages connection locking for using the same connection multithreaded.
    /// An updated revision of <seealso cref="SingleConnectionProvider"/> that will create a new connection if the connection instance breaks or disconnects.
    /// This provider will also lock the connection and block other threads accessing it while it is in use. You have to call <seealso cref="ReleaseConnection(MySqlConnection)"/> or <seealso cref="ReleaseConnectionAsync(MySqlConnection)"/> to release the lock on the connection.
    /// 
    /// Warning:
    /// Trying to obtain a connection lock on a thread that already owns the connection lock will not cause a thread lock, but obtaining a lock on 1 thread then running work on another thread that also tries to obtain a lock will cause a connection lock.
    /// If you need more reliability over speed, you can use the <seealso cref="TransientConnectionProvider"/> to create a new connection all the time to avoid any thread locking issues.
    /// For backwards compatibility, you can still use <seealso cref="SingleConnectionProvider"/> that does not do any connection locking, but also does not ensure connection integrity.
    /// </summary>
    public class SingletonConnectionProvider
    {
        private string m_ConnectionString;
        public string ConnectionString => m_ConnectionString;
        private MySqlConnection m_ConnectionInstance { get; set; }
        private SemaphoreSlim m_Semaphore = new SemaphoreSlim(0);
        private int m_LockedBy = -1;
        public bool Connected => m_ConnectionInstance != null && m_ConnectionInstance.State == System.Data.ConnectionState.Open;

        public SingletonConnectionProvider(MySqlConnection connection)
        {
            m_ConnectionInstance = connection;
            m_ConnectionString = connection.ConnectionString;
        }

        public SingletonConnectionProvider(DatabaseSettings settings)
        {
            m_ConnectionString = settings.ToString();
        }

        public SingletonConnectionProvider(string connectionString)
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
                // avoid thread locking
                if (Thread.CurrentThread.ManagedThreadId == m_LockedBy)
                {
                    return m_ConnectionInstance;
                }
                m_Semaphore.Wait();
                // Ensure connetcion is valid
                if (m_ConnectionInstance == null || m_ConnectionInstance.State == ConnectionState.Closed || m_ConnectionInstance.State == ConnectionState.Broken)
                {
                    m_ConnectionInstance?.Dispose();
                    m_ConnectionInstance = GetConnection(forceNew: true);
                }
                m_LockedBy = Thread.CurrentThread.ManagedThreadId;
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
                if (Thread.CurrentThread.ManagedThreadId == m_LockedBy)
                {
                    return m_ConnectionInstance;
                }
                await m_Semaphore.WaitAsync();
                // Ensure connetcion is valid
                if (m_ConnectionInstance == null || m_ConnectionInstance.State == ConnectionState.Closed || m_ConnectionInstance.State == ConnectionState.Broken)
                {
                    m_ConnectionInstance?.Dispose();
                    m_ConnectionInstance = await GetConnectionAsync(forceNew: true);
                }
                m_LockedBy = Thread.CurrentThread.ManagedThreadId;
                return m_ConnectionInstance;
            }
        }

        public void ReleaseConnection(MySqlConnection connection)
        {
            if (connection != m_ConnectionInstance)
            {
                connection.Close();
            }
            else
            {
                m_LockedBy = -1;
                m_Semaphore.Release();
            }
        }

        public async Task ReleaseConnectionAsync(MySqlConnection connection)
        {
            if (connection != m_ConnectionInstance)
            {
                await connection.CloseAsync();
            }
            else
            {
                m_LockedBy = -1;
                m_Semaphore.Release();
            }
        }

        public void Open()
        {
            if (m_ConnectionInstance == null)
            {
                m_ConnectionInstance = new MySqlConnection(m_ConnectionString);
                m_ConnectionInstance.Open();
                m_Semaphore.Release();
            }
        }

        public async Task OpenAsync()
        {
            if (m_ConnectionInstance == null)
            {
                m_ConnectionInstance = new MySqlConnection(m_ConnectionString);
                await m_ConnectionInstance.OpenAsync();
                m_Semaphore.Release();
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
            m_Semaphore.Dispose();
        }
    }
}
