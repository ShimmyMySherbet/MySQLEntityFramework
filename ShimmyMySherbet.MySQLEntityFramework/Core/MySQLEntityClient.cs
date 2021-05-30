using MySql.Data.MySqlClient;
using ShimmyMySherbet.MySQL.EF.Internals;
using ShimmyMySherbet.MySQL.EF.Models;
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
        public MySqlConnection ActiveConnection { get; protected set; }

        /// <summary>
        /// The connection string used when SingleConnection is disabled.
        /// </summary>
        public string ConnectionString { get; set; }

        protected bool AutoDispose = true;

        /// <summary>
        /// Declares if a single connection should be used. If this is enabled, all actions will utilise a single connection. If the connection is in use, the methods will block untill it is available.
        /// Enable this if you are just using this client on a single thread.
        /// This parameter is set when creating an instance of MySQLEntityClient
        /// </summary>
        public bool ReuseSingleConnection { get; private set; }

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
            this.ConnectionString = ConnectionString;
            this.ReuseSingleConnection = singleConnectionMode;
            Reader.IndexedHelper = IndexedTypeHelper;
        }

        /// <summary>
        /// If SingleConnection is enabled, returns the active connection. Otherwise returns a new connection.
        /// </summary>
        public MySqlConnection GetConnection(bool autoOpen = true, bool forceNew = false)
        {
            if (ReuseSingleConnection && !forceNew)
            {
                return ActiveConnection;
            }
            else
            {
                var c = new MySqlConnection(ConnectionString);
                if (autoOpen)
                    c.Open();
                return c;
            }
        }

        /// <summary>
        /// Initializes a new instance of MySQLEntityClient in ReuseConnection mode.
        /// </summary>
        /// <param name="connection">The conenction to use</param>
        /// <param name="autoDispose">Specifies if the connection should be disposed when the entity client is disposed.</param>
        public MySQLEntityClient(MySqlConnection connection, bool autoDispose = true)
        {
            ReuseSingleConnection = true;
            ActiveConnection = connection;
            AutoDispose = autoDispose;
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
            ConnectionString = $"Server={Address};Uid={Username}{(Password != null ? $";Pwd={Password}" : "")}{(Database != null ? $";Database={Database}" : "")};Port={Port};";
            this.ReuseSingleConnection = singleConnectionMode;
            Reader.IndexedHelper = IndexedTypeHelper;
            AutoDispose = singleConnectionMode;
        }

        /// <summary>
        /// Initializes a new connection using the details provided in DatabaseSettings
        /// </summary>
        /// <param name="settings">Database connectio nsettings</param>
        public MySQLEntityClient(DatabaseSettings settings, bool singleConnectionMode = true) : this(settings.DatabaseAddress, settings.DatabaseUsername, settings.DatabasePassword, settings.DatabaseName, settings.DatabasePort, singleConnectionMode)
        {
        }

        /// <summary>
        /// Gets the name of the connected database
        /// </summary>
        public string Database
        {
            get
            {
                if (ReuseSingleConnection)
                {
                    lock (ActiveConnection)
                    {
                        return ActiveConnection.Database;
                    }
                }
                else
                {
                    using (MySqlConnection Connection = GetConnection())
                    {
                        return Connection.Database;
                    }
                }
            }
        }

        /// <summary>
        /// Only required for when ReuseSingleConnection is enabled.
        /// </summary>
        /// <returns>Connected</returns>
        public bool Connect()
        {
            if (ReuseSingleConnection)
            {
                ActiveConnection = new MySqlConnection(ConnectionString);
                return TryConnect(ActiveConnection);
            }
            else
            {
                using (var conn = GetConnection(autoOpen: false))
                {
                    return TryConnect(conn);
                }
            }
        }

        /// <summary>
        /// Only required for when ReuseSingleConnection is enabled.
        /// </summary>
        public MySQLEntityClient Connect(out bool connected)
        {
            connected = Connect();
            return this;
        }

        /// <summary>
        /// Only required for when ReuseSingleConnection is enabled.
        /// </summary>
        /// <returns>Connected</returns>
        public async Task<bool> ConnectAsync()
        {
            if (ReuseSingleConnection)
            {
                ActiveConnection = new MySqlConnection(ConnectionString);
                return await TryConnectAsync(ActiveConnection);
            } else
            {
                using(var conn = GetConnection(autoOpen: false))
                {
                    return await TryConnectAsync(conn);
                }
            }
            
        }

        /// <summary>
        /// Disconnects the active connection when ReuseSingleConnection is enabled.
        /// </summary>
        public void Disconnect()
        {
            if (ReuseSingleConnection)
            {
                ActiveConnection.Close();
            }
        }

        /// <summary>
        /// Disconnects the active connection when ReuseSingleConnection is enabled.
        /// </summary>
        public async Task DisconnectAsync()
        {
            if (ReuseSingleConnection)
            {
                await ActiveConnection.CloseAsync();
            }
        }

        /// <summary>
        /// Creates the object in the database table.
        /// </summary>
        /// <param name="Obj">The object to create</param>
        /// <param name="Table">The table to create the object in</param>
        public void Insert<T>(T Obj, string Table)
        {
            if (ReuseSingleConnection)
            {
                lock (ActiveConnection)
                {
                    using (MySqlCommand Command = EntityCommandBuilder.BuildInsertCommand<T>(Obj, Table, ActiveConnection))
                    {
                        Command.ExecuteNonQuery();
                    }
                }
            }
            else
            {
                using (MySqlConnection connection = GetConnection())
                {
                    connection.Open();
                    using (MySqlCommand Command = EntityCommandBuilder.BuildInsertCommand<T>(Obj, Table, connection))
                    {
                        Command.ExecuteNonQuery();
                    }
                }
            }
        }

        /// <summary>
        /// Creates the object in the database table.
        /// </summary>
        /// <param name="Obj">The object to create</param>
        /// <param name="Table">The table to create the object in</param>
        public async Task InsertAsync<T>(T Obj, string Table)
        {
            using (MySqlConnection connection = GetConnection(autoOpen: false, forceNew: true))
            {
                await connection.OpenAsync();
                using (MySqlCommand Command = EntityCommandBuilder.BuildInsertCommand<T>(Obj, Table, connection))
                {
                    await Command.ExecuteNonQueryAsync();
                }
                await connection.CloseAsync();
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
            if (ReuseSingleConnection)
            {
                lock (ActiveConnection)
                {
                    using (MySqlCommand Command = EntityCommandBuilder.BuildInsertUpdateCommand<T>(Obj, Table, ActiveConnection))
                    {
                        Command.ExecuteNonQuery();
                    }
                }
            }
            else
            {
                using (MySqlConnection connection = new MySqlConnection(ConnectionString))
                {
                    connection.Open();
                    using (MySqlCommand Command = EntityCommandBuilder.BuildInsertUpdateCommand<T>(Obj, Table, connection))
                    {
                        Command.ExecuteNonQuery();
                    }
                }
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
            using (MySqlConnection connection = GetConnection(autoOpen: false, forceNew: true))
            {
                await connection.OpenAsync();
                using (MySqlCommand Command = EntityCommandBuilder.BuildInsertUpdateCommand<T>(Obj, Table, connection))
                {
                    await Command.ExecuteNonQueryAsync();
                }
                await connection.CloseAsync();
            }
        }

        /// <summary>
        /// Returns the connection state. If ReuseSingleConnection is disabled, this will try to create a new connection and test it.
        /// </summary>
        public bool Connected
        {
            get
            {
                if (ReuseSingleConnection)
                {
                    lock (ActiveConnection)
                    {
                        return ActiveConnection.State == System.Data.ConnectionState.Open;
                    }
                }
                else
                {
                    using (MySqlConnection Connection = new MySqlConnection(ConnectionString))
                    {
                        if (!TryConnect(Connection)) return false;
                        for (int i = 0; i < Connection.ConnectionTimeout; i += 100)
                        {
                            if (Connection.State == System.Data.ConnectionState.Open) return true;
                        }
                        return false;
                    }
                }
            }
        }

        /// <summary>
        /// Deletes the object from a database table. Object Model must have an associated Primary Key.
        /// </summary>
        public void Delete<T>(T Obj, string Table)
        {
            if (ReuseSingleConnection)
            {
                lock (ActiveConnection)
                {
                    using (MySqlCommand Command = EntityCommandBuilder.BuildDeleteCommand<T>(Obj, Table, ActiveConnection))
                    {
                        Command.ExecuteNonQuery();
                    }
                }
            }
            else
            {
                using (MySqlConnection Connection = new MySqlConnection(ConnectionString))
                {
                    Connection.Open();
                    using (MySqlCommand Command = EntityCommandBuilder.BuildDeleteCommand<T>(Obj, Table, Connection))
                    {
                        Command.ExecuteNonQuery();
                    }
                }
            }
        }

        /// <summary>
        /// Deletes the object from a database table. Object Model must have an associated Primary Key.
        /// </summary>
        public async Task DeleteAsync<T>(T Obj, string Table)
        {
            using (MySqlConnection Connection = GetConnection(autoOpen: false, forceNew: true))
            {
                Connection.Open();
                using (MySqlCommand Command = EntityCommandBuilder.BuildDeleteCommand<T>(Obj, Table, Connection))
                {
                    await Command.ExecuteNonQueryAsync();
                }
                Connection.Close();
            }
        }

        /// <summary>
        /// Creates a database table using the provided class model.
        /// </summary>
        public void CreateTable<T>(string TableName)
        {
            if (ReuseSingleConnection)
            {
                lock (ActiveConnection)
                {
                    using (MySqlCommand Command = CommandBuilder.BuildCreateTableCommand<T>(TableName, ActiveConnection))
                    {
                        Command.ExecuteNonQuery();
                    }
                }
            }
            else
            {
                using (MySqlConnection Connection = GetConnection(autoOpen: true, forceNew: true))
                {
                    using (MySqlCommand Command = CommandBuilder.BuildCreateTableCommand<T>(TableName, Connection))
                    {
                        Command.ExecuteNonQuery();
                    }
                }
            }
        }

        /// <summary>
        /// Creates a database table using the provided class model.
        /// </summary>
        public async Task CreateTableAsync<T>(string TableName)
        {
            using (MySqlConnection Connection = GetConnection(autoOpen: false, forceNew: true))
            {
                await Connection.OpenAsync();
                using (MySqlCommand Command = CommandBuilder.BuildCreateTableCommand<T>(TableName, Connection))
                {
                    await Command.ExecuteNonQueryAsync();
                }
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
            if (ReuseSingleConnection)
            {
                lock (ActiveConnection)
                {
                    using (MySqlCommand Command = EntityCommandBuilder.BuildUpdateCommand<T>(Obj, Table, ActiveConnection))
                    {
                        Command.ExecuteNonQuery();
                    }
                }
            }
            else
            {
                using (MySqlConnection Connection = GetConnection(autoOpen: true, forceNew: true))
                {
                    using (MySqlCommand Command = EntityCommandBuilder.BuildUpdateCommand<T>(Obj, Table, Connection))
                    {
                        Command.ExecuteNonQuery();
                    }
                }
            }
        }

        /// <summary>
        /// Updates an object in the specified database table. Object Model must have an associated Primary Key.
        /// </summary>
        public async Task UpdateAsync<T>(T Obj, string Table)
        {
            using (MySqlConnection Connection = GetConnection(autoOpen: false, forceNew: true))
            {
                await Connection.OpenAsync();
                using (MySqlCommand Command = EntityCommandBuilder.BuildUpdateCommand<T>(Obj, Table, Connection))
                {
                    await Command.ExecuteNonQueryAsync();
                }
            }
        }

        /// <summary>
        /// Checks the servers Information Schema to seee if a database table exists.
        /// </summary>
        /// <param name="Table">The table name</param>
        /// <returns></returns>
        public bool TableExists(string Table)
        {
            if (ReuseSingleConnection)
            {
                lock (ActiveConnection)
                {
                    using (MySqlCommand Command = EntityCommandBuilder.BuildCommand(ActiveConnection,
                        "SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = @0 AND TABLE_NAME = @1;", Database, Table))
                    using (MySqlDataReader Reader = Command.ExecuteReader())
                    {
                        return Reader.HasRows;
                    }
                }
            }
            else
            {
                using (MySqlConnection Connection = GetConnection(autoOpen: true, forceNew: true))
                {
                    using (MySqlCommand Command = EntityCommandBuilder.BuildCommand(Connection,
                        "SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = @0 AND TABLE_NAME = @1;", Database, Table))
                    using (MySqlDataReader Reader = Command.ExecuteReader())
                    {
                        return Reader.HasRows;
                    }
                }
            }
        }

        public void DeleteTable(string Table)
        {
            if (ReuseSingleConnection)
            {
                lock (ActiveConnection)
                {
                    using (MySqlCommand Command = EntityCommandBuilder.BuildCommand(ActiveConnection, $"DROP TABLE `{Table.Replace("`", "``")}`"))
                    {
                        Command.ExecuteNonQuery();
                    }
                }
            }
            else
            {
                using (MySqlConnection Connection = GetConnection(autoOpen: true, forceNew: true))
                {
                    using (MySqlCommand Command = EntityCommandBuilder.BuildCommand(Connection, $"DROP TABLE `{Table.Replace("`", "``")}`"))
                    {
                        Command.ExecuteNonQuery();
                    }
                }
            }
        }

        public async Task DeleteTableAsync(string Table)
        {
            using (MySqlConnection Connection = GetConnection(autoOpen: false, forceNew: true))
            {
                await Connection.OpenAsync();
                using (MySqlCommand Command = EntityCommandBuilder.BuildCommand(Connection, "DROP TABLE @0", Table))
                {
                    await Command.ExecuteNonQueryAsync();
                }
                await Connection.CloseAsync();
            }
        }

        private bool TryConnect(MySqlConnection connection)
        {
            if (connection.State == System.Data.ConnectionState.Open) return true;
            try
            {
                connection.Open();
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        private async Task<bool> TryConnectAsync(MySqlConnection connection)
        {
            if (connection.State == System.Data.ConnectionState.Open || ActiveConnection == null) return true;
            try
            {
                await connection.OpenAsync();
            }
            catch (Exception)
            {
                return false;
            }
            return true;
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
            if (ReuseSingleConnection)
            {
                lock (ActiveConnection)
                {
                    return Reader.RetriveFromDatabase<T>(ActiveConnection, Command, Parameters);
                }
            }
            else
            {
                using (MySqlConnection Connection = GetConnection(autoOpen: true, forceNew: true))
                {
                    return Reader.RetriveFromDatabase<T>(Connection, Command, Parameters);
                }
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
            using (MySqlConnection Connection = GetConnection(autoOpen: false, forceNew: true))
            {
                await Connection.OpenAsync();
                var value = await Reader.RetriveFromDatabaseAsync<T>(Connection, Command, Parameters);
                await Connection.CloseAsync();
                return value;
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
            if (ReuseSingleConnection)
            {
                lock (ActiveConnection)
                {
                    List<T> Results = Reader.RetriveFromDatabaseCapped<T>(ActiveConnection, 1, Command, Parameters);
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
            else
            {
                using (MySqlConnection Connection = GetConnection(autoOpen: true, forceNew: true))
                {
                    List<T> Results = Reader.RetriveFromDatabaseCapped<T>(Connection, 1, Command, Parameters);
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
        }

        /// <summary>
        /// Excecutes the given query and returns a single result.
        /// </summary>
        /// <param name="Command">The SQL Query</param>
        /// <param name="Parameters">The SQL Command Parameters (@i or {i} format)</param>
        /// <returns>First result, or null if no results.</returns>
        public async Task<T> QuerySingleAsync<T>(string Command, params object[] Parameters)
        {
            using (MySqlConnection Connection = GetConnection(autoOpen: false, forceNew: true))
            {
                await Connection.OpenAsync();
                List<T> Results = await Reader.RetriveFromDatabaseCappedAsync<T>(Connection, 1, Command, Parameters);
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

        public int ExecuteNonQuery(string Command, params object[] Parameters)
        {
            if (ReuseSingleConnection)
            {
                lock (ActiveConnection)
                {
                    using (MySqlCommand command = EntityCommandBuilder.BuildCommand(ActiveConnection, Command, Parameters))
                    {
                        return command.ExecuteNonQuery();
                    }
                }
            }
            else
            {
                using (MySqlConnection Connection = GetConnection(autoOpen: true, forceNew: true))
                {
                    using (MySqlCommand command = EntityCommandBuilder.BuildCommand(Connection, Command, Parameters))
                    {
                        return command.ExecuteNonQuery();
                    }
                }
            }
        }

        public async Task<int> ExecuteNonQueryAsync(string Command, params object[] Parameters)
        {
            using (MySqlConnection Connection = GetConnection(autoOpen: false, forceNew: true))
            {
                await Connection.OpenAsync();
                using (MySqlCommand command = EntityCommandBuilder.BuildCommand(Connection, Command, Parameters))
                {
                    return await command.ExecuteNonQueryAsync();
                }
            }
        }

        public void Dispose()
        {
            if (AutoDispose && ActiveConnection != null)
            {
                ActiveConnection.Dispose();
            }
        }
    }
}