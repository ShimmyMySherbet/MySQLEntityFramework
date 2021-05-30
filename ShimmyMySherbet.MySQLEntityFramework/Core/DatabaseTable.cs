using MySql.Data.MySqlClient;
using ShimmyMySherbet.MySQL.EF.Models;
using ShimmyMySherbet.MySQL.EF.Models.Exceptions;
using ShimmyMySherbet.MySQL.EF.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ShimmyMySherbet.MySQL.EF.Core
{
    /// <summary>
    /// Provides methods to interact with a specific database table.
    /// Designed to be used in a class that inheits <see cref="DatabaseClient"/> though can also work alone
    /// </summary>
    /// <typeparam name="T">Table type model</typeparam>
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

        /// <summary>
        /// Recives the database client instance
        /// </summary>
        public virtual void SendClient(MySQLEntityClient client)
        {
            Client = client;
        }

        /// <summary>
        /// Creates a Bulk Inserter client.
        /// Use this when inserting large amounts of objects into the table, and where performance is critical.
        /// </summary>
        /// <param name="maxInsertsPerTransaction">Max insert operations per transaction. If you get Max Packet Size errors, reduce this number.</param>
        /// <returns>Bulk Inserter client for this table</returns>
        public IBulkInserter<T> CreateInserter(int maxInsertsPerTransaction = 5000)
        {
            return new TransactionalBulkInserter<T>(Client.GetConnection(), TableName, maxInsertsPerTransaction);
        }

        /// <summary>
        /// Checks if the table exists, and if it doesn't, creates the table.
        /// </summary>
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

        /// <summary>
        /// Creates the database table if it doesn't exist.
        /// NOTE: CheckSchema() also does this, and if you are using a database manager using a <seealso cref="DatabaseClient"/> call CheckSchema on that instead.
        /// </summary>
        public void CreateTableIfNotExists() => Client.CreateTableIfNotExists<T>(TableName);

        /// <summary>
        /// Deletes the specified instance from the table.
        /// Requires the type to have at least one <seealso cref="SQLPrimaryKey"/>
        /// </summary>
        public void Delete(T obj) => Client.Delete(obj, TableName);

        /// <summary>
        /// Deletes the specified instance from the table.
        /// Requires the type to have at least one <seealso cref="SQLPrimaryKey"/>
        /// </summary>
        public async Task DeleteAsync(T obj) => await Client.DeleteAsync(obj, TableName);

        /// <summary>
        /// Inserts an object instance into the table
        /// </summary>
        public void Insert(T obj) => Client.Insert(obj, TableName);

        /// <summary>
        /// Inserts an object instance into the table
        /// </summary>
        public async Task InsertAsync(T obj) => await Client.InsertAsync(obj, TableName);

        /// <summary>
        /// If a row with the same key exists, this updates that row, if not, this will insert the object instead
        /// </summary>
        public void InsertUpdate(T obj) => Client.InsertUpdate(obj, TableName);

        /// <summary>
        /// If a row with the same key exists, this updates that row, if not, this will insert the object instead
        /// </summary>
        public async Task InsertUpdateAsync(T obj) => await Client.InsertUpdateAsync(obj, TableName);

        /// <summary>
        /// Selects a list of instances from the table
        /// </summary>
        /// <param name="command">The MySQL query command. Use @TABLE to refer to the table, and @0 - @99 as arguments. Arguments ar esecurly escaped and formatted.</param>
        /// <param name="args">Arguments to escape and format into the command.</param>
        public List<T> Query(string command, params object[] args) => Client.Query<T>(InsertTableName(command), args);

        /// <summary>
        /// Selects a list of instances from the table
        /// </summary>
        /// <param name="command">The MySQL query command. Use @TABLE to refer to the table, and @0 - @99 as arguments. Arguments ar esecurly escaped and formatted.</param>
        /// <param name="args">Arguments to escape and format into the command.</param>
        public async Task<List<T>> QueryAsync(string command, params object[] args) => await Client.QueryAsync<T>(InsertTableName(command), args);

        /// <summary>
        /// Selects a single object from the table
        /// </summary>
        /// <param name="command">The MySQL query command. Use @TABLE to refer to the table, and @0 - @99 as arguments. Arguments ar esecurly escaped and formatted.</param>
        /// <param name="args">Arguments to escape and format into the command.</param>
        public T QuerySingle(string command, params object[] args) => Client.QuerySingle<T>(InsertTableName(command), args);

        /// <summary>
        /// Selects a single object from the table
        /// </summary>
        /// <param name="command">The MySQL query command. Use @TABLE to refer to the table, and @0 - @99 as arguments. Arguments ar esecurly escaped and formatted.</param>
        /// <param name="args">Arguments to escape and format into the command.</param>
        public async Task<T> QuerySingleAsync(string command, params object[] args) => await Client.QuerySingleAsync<T>(InsertTableName(command), args);

        /// <summary>
        /// Securely replaces @TABLE with the table name
        /// </summary>
        protected string InsertTableName(string command)
        {
            int i = 0;
            while (true)
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