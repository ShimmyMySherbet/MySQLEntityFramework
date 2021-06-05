using MySql.Data.MySqlClient;
using ShimmyMySherbet.MySQL.EF.Internals;
using ShimmyMySherbet.MySQL.EF.Models;
using ShimmyMySherbet.MySQL.EF.Models.ConnectionProviders;
using ShimmyMySherbet.MySQL.EF.Models.Interfaces;
using ShimmyMySherbet.MySQL.EF.Models.TypeModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ShimmyMySherbet.MySQL.EF.Core
{
    public class MySQLEntityClient : IDisposable
    {
        /// <summary>
        /// The active connection when SingleConnection is enabled
        /// </summary>
        //public MySqlConnection ActiveConnection { get; protected set; }

        public IConnectionProvider ConnectionProvider { get; private set; }

        /// <summary>
        /// The connection string used when SingleConnection is disabled.
        /// </summary>
        public string ConnectionString => ConnectionProvider.ConnectionString;

        protected bool AutoDispose = true;

        protected EntityCommandBuilder CommandBuilder = new EntityCommandBuilder();
        protected MySQLEntityReader Reader = new MySQLEntityReader();
        protected SQLTypeHelper IndexedTypeHelper = new SQLTypeHelper();

        /// <summary>
        /// </summary>
        /// <param name="ConnectionString">The SQL Connection String to use.</param>
        /// <param name="singleConnectionMode">Declares if a single connection should be used. If this is enabled, all actions will utilise a single connection. If the connection is in use, the methods will block untill it is available.
        /// Enable this if you are just using this client on a single thread.
        /// NOTE: If this is enabled, you need to call Connect()</param>
        public MySQLEntityClient(string ConnectionString, bool singleConnectionMode = true)
        {
            if (singleConnectionMode)
            {
                ConnectionProvider = new SingleConnectionProvider(ConnectionString);
            }
            else
            {
                ConnectionProvider = new TransientConnectionProvider(ConnectionString);
            }

            Reader.IndexedHelper = IndexedTypeHelper;
        }

        /// <summary>
        /// Legacy proxy method for <seealso cref="ConnectionProvider"/>.GetConnection
        /// </summary>
        public MySqlConnection GetConnection(bool autoOpen = true, bool forceNew = false)
        {
            return ConnectionProvider.GetConnection(autoOpen: autoOpen, forceNew: forceNew);
        }

        /// <summary>
        /// Initializes a new instance of MySQLEntityClient using the specified connection provider
        /// </summary>
        /// <param name="connectionProvider"></param>
        public MySQLEntityClient(IConnectionProvider connectionProvider)
        {
            ConnectionProvider = connectionProvider;
        }

        /// <summary>
        /// Initializes a new instance of MySQLEntityClient in ReuseConnection mode.
        /// </summary>
        /// <param name="connection">The conenction to use</param>
        /// <param name="autoDispose">Specifies if the connection should be disposed when the entity client is disposed.</param>
        public MySQLEntityClient(MySqlConnection connection, bool autoDispose = true)
        {
            AutoDispose = autoDispose;
            ConnectionProvider = new SingleConnectionProvider(connection);
        }

        /// <summary>
        /// </summary>
        /// <param name="Address">The Database Address</param>
        /// <param name="Username">The Username to use</param>
        /// <param name="Password">The User Password</param>
        /// <param name="Database">The scoped detebase</param>
        /// <param name="Port">The port to use. Default: 3306</param>
        /// <param name="singleConnectionMode">Declares if a single connection should be used. If this is enabled, all actions will utilise a single connection. If the connection is in use, the methods will block untill it is available.
        /// Enable this if you are just using this client on a single thread.
        /// NOTE: If this is enabled, you need to call Connect()</param>
        public MySQLEntityClient(string Address, string Username, string Password = null, string Database = null, int Port = 3306, bool singleConnectionMode = true)
        {
            var connectionString = $"Server={Address};Uid={Username}{(Password != null ? $";Pwd={Password}" : "")}{(Database != null ? $";Database={Database}" : "")};Port={Port};";
            if (singleConnectionMode)
            {
                ConnectionProvider = new SingleConnectionProvider(connectionString);
            }
            else
            {
                ConnectionProvider = new TransientConnectionProvider(connectionString);
            }

            Reader.IndexedHelper = IndexedTypeHelper;
            AutoDispose = singleConnectionMode;
        }

        /// <summary>
        /// Initializes a new connection using the details provided in DatabaseSettings
        /// </summary>
        /// <param name="settings">Database connectio nsettings</param>
        public MySQLEntityClient(DatabaseSettings settings, bool singleConnectionMode = true) : this(settings.ToString(), singleConnectionMode)
        {
        }

        /// <summary>
        /// Gets the name of the connected database
        /// </summary>
        public string Database
        {
            get
            {
                var connection = ConnectionProvider.GetConnection();
                try
                {
                    lock (connection)
                    {
                        return connection.Database;
                    }
                }
                finally
                {
                    ConnectionProvider.ReleaseConnection(connection);
                }
            }
        }

        /// <summary>
        /// Only required for when ReuseSingleConnection is enabled.
        /// </summary>
        /// <returns>Connected</returns>
        public bool Connect() => Connect(out string _);

        public bool Connect(out string errorMessage)
        {
            try
            {
                ConnectionProvider.Open();
                errorMessage = "Connected.";
                return true;
            }
            catch (MySqlException ex)
            {
                errorMessage = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// Only required for when ReuseSingleConnection is enabled.
        /// </summary>
        /// <returns>Connected</returns>
        public async Task<bool> ConnectAsync()
        {
            try
            {
                await ConnectionProvider.OpenAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Only required for when ReuseSingleConnection is enabled.
        /// </summary>
        public MySQLEntityClient Connect(out bool connected, out string errorMessage)
        {
            connected = Connect(out errorMessage);
            return this;
        }

        /// <summary>
        /// Disconnects the active connection when ReuseSingleConnection is enabled.
        /// </summary>
        public void Disconnect()
        {
            ConnectionProvider.Disconnect();
        }

        /// <summary>
        /// Disconnects the active connection when ReuseSingleConnection is enabled.
        /// </summary>
        public async Task DisconnectAsync()
        {
            await ConnectionProvider.DisconnectAsync();
        }

        /// <summary>
        /// Creates the object in the database table.
        /// </summary>
        /// <param name="Obj">The object to create</param>
        /// <param name="Table">The table to create the object in</param>
        public void Insert<T>(T Obj, string Table)
        {
            var connection = ConnectionProvider.GetConnection();
            try
            {
                lock (connection)
                {
                    using (MySqlCommand Command = EntityCommandBuilder.BuildInsertCommand<T>(Obj, Table, connection))
                    {
                        Command.ExecuteNonQuery();
                    }
                }
            }
            finally
            {
                ConnectionProvider.ReleaseConnection(connection);
            }
        }

        /// <summary>
        /// Creates the object in the database table.
        /// </summary>
        /// <param name="Obj">The object to create</param>
        /// <param name="Table">The table to create the object in</param>
        public async Task InsertAsync<T>(T Obj, string Table)
        {
            var connection = await ConnectionProvider.GetConnectionAsync(forceNew: true);
            try
            {
                using (MySqlCommand Command = EntityCommandBuilder.BuildInsertCommand<T>(Obj, Table, connection))
                {
                    await Command.ExecuteNonQueryAsync();
                }
            }
            finally
            {
                await ConnectionProvider.ReleaseConnectionAsync(connection);
            }
        }

        /// <summary>
        /// Creates the object in the database table.
        /// If an item with the same primary or unique key exists, it will update it instead.
        /// </summary>
        /// <param name="Obj">The object to create</param>
        /// <param name="Table">The table to create the object in</param>
        public void InsertUpdate<T>(T Obj, string Table)
        {
            var connection = ConnectionProvider.GetConnection();
            try
            {
                lock (connection)
                {
                    using (MySqlCommand Command = EntityCommandBuilder.BuildInsertUpdateCommand<T>(Obj, Table, connection))
                    {
                        Command.ExecuteNonQuery();
                    }
                }
            }
            finally
            {
                ConnectionProvider.ReleaseConnection(connection);
            }
        }

        /// <summary>
        /// Creates the object in the database table.
        /// If an item with the same primary or unique key exists, it will update it instead.
        /// </summary>
        /// <param name="Obj">The object to create</param>
        /// <param name="Table">The table to create the object in</param>
        public async Task InsertUpdateAsync<T>(T Obj, string Table)
        {
            var connection = await ConnectionProvider.GetConnectionAsync(forceNew: true);
            try
            {
                using (MySqlCommand Command = EntityCommandBuilder.BuildInsertUpdateCommand<T>(Obj, Table, connection))
                {
                    await Command.ExecuteNonQueryAsync();
                }
            }
            finally
            {
                await ConnectionProvider.ReleaseConnectionAsync(connection);
            }
        }

        /// <summary>
        /// Returns the connection state. If ReuseSingleConnection is disabled, this will try to create a new connection and test it.
        /// </summary>
        public bool Connected
        {
            get
            {
                return ConnectionProvider.Connected;
            }
        }

        /// <summary>
        /// Deletes the object from a database table. Object Model must have an associated Primary Key.
        /// </summary>
        public void Delete<T>(T Obj, string Table)
        {
            var connection = ConnectionProvider.GetConnection();
            try
            {
                lock (connection)
                {
                    using (MySqlCommand Command = EntityCommandBuilder.BuildDeleteCommand<T>(Obj, Table, connection))
                    {
                        Command.ExecuteNonQuery();
                    }
                }
            }
            finally
            {
                ConnectionProvider.ReleaseConnection(connection);
            }
        }

        /// <summary>
        /// Deletes the object from a database table. Object Model must have an associated Primary Key.
        /// </summary>
        public async Task DeleteAsync<T>(T Obj, string Table)
        {
            var connection = await ConnectionProvider.GetConnectionAsync();
            try
            {
                using (MySqlCommand Command = EntityCommandBuilder.BuildDeleteCommand<T>(Obj, Table, connection))
                {
                    await Command.ExecuteNonQueryAsync();
                }
            }
            finally
            {
                await ConnectionProvider.ReleaseConnectionAsync(connection);
            }
        }

        /// <summary>
        /// Creates a database table using the provided class model.
        /// </summary>
        public void CreateTable<T>(string TableName)
        {
            var connection = ConnectionProvider.GetConnection();
            try
            {
                lock (connection)
                {
                    using (MySqlCommand Command = CommandBuilder.BuildCreateTableCommand<T>(TableName, connection))
                    {
                        Command.ExecuteNonQuery();
                    }
                }
            }
            finally
            {
                ConnectionProvider.ReleaseConnection(connection);
            }
        }

        /// <summary>
        /// Creates a database table using the provided class model.
        /// </summary>
        public async Task CreateTableAsync<T>(string TableName)
        {
            var connection = await ConnectionProvider.GetConnectionAsync();
            try
            {
                using (MySqlCommand Command = CommandBuilder.BuildCreateTableCommand<T>(TableName, connection))
                {
                    await Command.ExecuteNonQueryAsync();
                }
            }
            finally
            {
                await ConnectionProvider.ReleaseConnectionAsync(connection);
            }
        }

        public void CreateTableIfNotExists<T>(string tableName)
        {
            if (!TableExists(tableName))
            {
                CreateTable<T>(tableName);
            }
        }

        public async Task CreateTableIfNotExistsAsync<T>(string tableName)
        {
            if (!TableExists(tableName))
            {
                await CreateTableAsync<T>(tableName);
            }
        }

        /// <summary>
        /// Updates an object in the specified database table. Object Model must have an associated Primary Key.
        /// </summary>
        public void Update<T>(T Obj, string Table)
        {
            var connection = ConnectionProvider.GetConnection();
            try
            {
                lock (connection)
                {
                    using (MySqlCommand Command = EntityCommandBuilder.BuildUpdateCommand<T>(Obj, Table, connection))
                    {
                        Command.ExecuteNonQuery();
                    }
                }
            }
            finally
            {
                ConnectionProvider.ReleaseConnection(connection);
            }
        }

        /// <summary>
        /// Updates an object in the specified database table. Object Model must have an associated Primary Key.
        /// </summary>
        public async Task UpdateAsync<T>(T Obj, string Table)
        {
            var connection = await ConnectionProvider.GetConnectionAsync();
            try
            {
                using (MySqlCommand Command = EntityCommandBuilder.BuildUpdateCommand<T>(Obj, Table, connection))
                {
                    await Command.ExecuteNonQueryAsync();
                }
            }
            finally
            {
                await ConnectionProvider.ReleaseConnectionAsync(connection);
            }
        }

        /// <summary>
        /// Checks the servers Information Schema to seee if a database table exists.
        /// </summary>
        /// <param name="Table">The table name</param>
        /// <returns></returns>
        public bool TableExists(string Table)
        {
            var connection = ConnectionProvider.GetConnection();
            try
            {
                lock (connection)
                {
                    using (MySqlCommand Command = EntityCommandBuilder.BuildCommand(connection,
                      "SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = @0 AND TABLE_NAME = @1;", Database, Table))
                    using (MySqlDataReader Reader = Command.ExecuteReader())
                    {
                        return Reader.HasRows;
                    }
                }
            }
            finally
            {
                ConnectionProvider.ReleaseConnection(connection);
            }
        }

        public void DeleteTable(string Table)
        {
            var connection = ConnectionProvider.GetConnection();
            try
            {
                lock (connection)
                {
                    using (MySqlCommand Command = EntityCommandBuilder.BuildCommand(connection, $"DROP TABLE `{Table.Replace("`", "``")}`"))
                    {
                        Command.ExecuteNonQuery();
                    }
                }
            }
            finally
            {
                ConnectionProvider.ReleaseConnection(connection);
            }
        }

        public async Task DeleteTableAsync(string Table)
        {
            var connection = await ConnectionProvider.GetConnectionAsync();
            try
            {
                using (MySqlCommand Command = EntityCommandBuilder.BuildCommand(connection, $"DROP TABLE `{Table.Replace("`", "``")}`"))
                {
                    await Command.ExecuteNonQueryAsync();
                }
            }
            finally
            {
                await ConnectionProvider.ReleaseConnectionAsync(connection);
            }
        }

        [Obsolete]
        private bool TryConnect(MySqlConnection connection)
        {
            return false;
        }

        [Obsolete]
        private Task<bool> TryConnectAsync(MySqlConnection connection)
        {
            return Task.FromResult(true);
        }

        /// <summary>
        /// Executes the given query and returns the results.
        /// </summary>
        /// <typeparam name="T">The query return type</typeparam>
        /// <param name="Command">The SQL Query</param>
        /// <param name="Parameters">The SQL Command Parameters (@i or {i} format)</param>
        /// <returns>Results from query</returns>
        public List<T> Query<T>(string Command, params object[] Parameters)
        {
            var connection = ConnectionProvider.GetConnection();
            try
            {
                lock (connection)
                {
                    return Reader.RetriveFromDatabase<T>(connection, Command, Parameters);
                }
            }
            finally
            {
                ConnectionProvider.ReleaseConnection(connection);
            }
        }

        /// <summary>
        /// Executes the given query and returns the results.
        /// </summary>
        /// <typeparam name="T">The query return type</typeparam>
        /// <param name="Command">The SQL Query</param>
        /// <param name="Parameters">The SQL Command Parameters (@i or {i} format)</param>
        /// <returns>Results from query</returns>
        public async Task<List<T>> QueryAsync<T>(string Command, params object[] Parameters)
        {
            var connection = await ConnectionProvider.GetConnectionAsync();
            try
            {
                return await Reader.RetriveFromDatabaseAsync<T>(connection, Command, Parameters);
            }
            finally
            {
                await ConnectionProvider.ReleaseConnectionAsync(connection);
            }
        }

        /// <summary>
        /// Excecutes the given query and returns a single result.
        /// </summary>
        /// <param name="Command">The SQL Query</param>
        /// <param name="Parameters">The SQL Command Parameters (@i or {i} format)</param>
        /// <returns>First result, or null if no results.</returns>
        public T QuerySingle<T>(string Command, params object[] Parameters)
        {
            var connection = ConnectionProvider.GetConnection();
            try
            {
                lock (connection)
                {
                    List<T> Results = Reader.RetriveFromDatabaseCapped<T>(connection, 1, Command, Parameters);
                    if (Results.Count != 0)
                    {
                        return Results[0];
                    }
                    else
                    {
                        return default;
                    }
                }
            }
            finally
            {
                ConnectionProvider.ReleaseConnection(connection);
            }
        }

        /// <summary>
        /// Excecutes the given query and returns a single result.
        /// </summary>
        /// <param name="Command">The SQL Query</param>
        /// <param name="Parameters">The SQL Command Parameters (@i or {i} format)</param>
        /// <returns>First result, or null if no results.</returns>
        public async Task<T> QuerySingleAsync<T>(string Command, params object[] Parameters)
        {
            var connection = await ConnectionProvider.GetConnectionAsync();
            try
            {
                List<T> Results = await Reader.RetriveFromDatabaseCappedAsync<T>(connection, 1, Command, Parameters);
                if (Results.Count != 0)
                {
                    return Results[0];
                }
                else
                {
                    return default;
                }
            }
            finally
            {
                await ConnectionProvider.ReleaseConnectionAsync(connection);
            }
        }

        public int ExecuteNonQuery(string Command, params object[] Parameters)
        {
            var connection = ConnectionProvider.GetConnection();
            try
            {
                lock (connection)
                {
                    using (MySqlCommand command = EntityCommandBuilder.BuildCommand(connection, Command, Parameters))
                    {
                        return command.ExecuteNonQuery();
                    }
                }
            }
            finally
            {
                ConnectionProvider.ReleaseConnection(connection);
            }
        }

        public async Task<int> ExecuteNonQueryAsync(string Command, params object[] Parameters)
        {
            var connection = await ConnectionProvider.GetConnectionAsync();
            try
            {
                using (MySqlCommand command = EntityCommandBuilder.BuildCommand(connection, Command, Parameters))
                {
                    return await command.ExecuteNonQueryAsync();
                }
            }
            finally
            {
                await ConnectionProvider.ReleaseConnectionAsync(connection);
            }
        }

        public void Dispose()
        {
            ConnectionProvider.Dispose();
        }
    }
}