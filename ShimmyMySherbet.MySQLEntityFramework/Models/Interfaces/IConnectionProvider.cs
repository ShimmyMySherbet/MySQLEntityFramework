using MySql.Data.MySqlClient;
using System;
using System.Threading.Tasks;

namespace ShimmyMySherbet.MySQL.EF.Models.Interfaces
{
    public interface IConnectionProvider : IDisposable
    {
        bool Connected { get; }

        MySqlConnection GetConnection(bool autoOpen = true, bool forceNew = false);

        Task<MySqlConnection> GetConnectionAsync(bool autoOpen = true, bool forceNew = false);

        void ReleaseConnection(MySqlConnection connection);

        Task ReleaseConnectionAsync(MySqlConnection connection);

        void Open();

        Task OpenAsync();

        void Disconnect();

        Task DisconnectAsync();
    }
}