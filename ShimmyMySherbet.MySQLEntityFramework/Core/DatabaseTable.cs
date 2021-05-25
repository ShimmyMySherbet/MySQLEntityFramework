using MySql.Data.MySqlClient;
using ShimmyMySherbet.MySQL.EF.Models;
using ShimmyMySherbet.MySQL.EF.Models.Exceptions;
using ShimmyMySherbet.MySQL.EF.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ShimmyMySherbet.MySQL.EF.Core
{
    public class DatabaseTable<T> : IDatabaseTable<T> where T : class
    {
        public string TableName { get; protected set; }

        protected string RealTableName => $"`{TableName.Trim('`')}`";


        protected MySQLEntityClient Client { get; set; }

        public DatabaseTable(string tableName)
        {
            if (tableName == null)
            {
                throw new ArgumentNullException("tableName");
            }
            TableName = tableName;
        }

        public DatabaseTable(string tableName, MySQLEntityClient client)
        {
            if (tableName == null)
            {
                throw new ArgumentNullException("tableName");
            }
            TableName = tableName;
            Client = client;
        }

        public DatabaseTable(string tableName, MySqlConnection connection)
        {
            if (tableName == null)
            {
                throw new ArgumentNullException("tableName");
            }
            TableName = tableName;
            Client = new MySQLEntityClient(connection, true);
        }

        public virtual void SendClient(MySQLEntityClient client)
        {
            Client = client;
        }

        public BulkInserter<T> CreateInserter()
        {
            return new BulkInserter<T>(Client.ActiveConnection, TableName);


        }

        public virtual void CheckSchema()
        {
            if (Client == null)
            {
                throw new NoConnectionException("No database connection available. Use SendClient() or provide a connection at construction.");
            }

            if (!Client.TableExists(TableName))
            {
                Client.CreateTable<T>(TableName);
            }
        }

        public void Delete(T obj) => Client.Delete(obj, TableName);

        public async Task DeleteAsync(T obj) => await Client.DeleteAsync(obj, TableName);

        public void Insert(T obj) => Client.Insert(obj, TableName);

        public async Task InsertAsync(T obj) => await Client.InsertAsync(obj, TableName);

        public void InsertUpdate(T obj) => Client.InsertUpdate(obj, TableName);

        public async Task InsertUpdateAsync(T obj) => await Client.InsertUpdateAsync(obj, TableName);

        public List<T> Query(string command, params object[] args) => Client.Query<T>(InsertTableName(command), args);

        public async Task<List<T>> QueryAsync(string command, params object[] args) => await Client.QueryAsync<T>(InsertTableName(command), args);

        public T QuerySingle(string command, params object[] args) => Client.QuerySingle<T>(InsertTableName(command), args);

        public async Task<T> QuerySingleAsync(string command, params object[] args) => await Client.QuerySingleAsync<T>(InsertTableName(command), args);

        protected string InsertTableName(string command)
        {
            int i = 0;
            while(true)
            {
                var index = command.IndexOf("@table", 0, command.Length, StringComparison.InvariantCultureIgnoreCase);
                if (index == -1)
                {
                    break;
                }
                command = command.Remove(index, 6).Insert(index, RealTableName);
                i++;
                if (i > 20) break;
            }
            return command;

        }
    }
}