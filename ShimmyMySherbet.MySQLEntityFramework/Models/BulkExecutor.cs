using MySql.Data.MySqlClient;
using ShimmyMySherbet.MySQL.EF.Internals;
using System.Text;
using System.Threading.Tasks;

namespace ShimmyMySherbet.MySQL.EF.Models
{
    /// <summary>
    /// Used to bulk execute commands against a db.
    /// NOTE: if your work is insert-heavy, use BulkInserter instead.
    /// </summary>
    public class BulkExecutor
    {
        private StringBuilder m_Commands = new StringBuilder();
        private MySqlConnection m_Connection;
        private PrefixAssigner m_Assigner = new PrefixAssigner();
        private PropertyList m_MasterPropertiesList = new PropertyList();


        public BulkExecutor(MySqlConnection connection)
        {
            m_Connection = connection;
            ExecuteNonQuery("START TRANSACTION");
        }

        public void ExecuteNonQuery(string command, params object[] parameters)
        {
            if (!command.EndsWith(";"))
            {
                command += ';';
            }
            string value = EntityCommandBuilder.BuildCommandContent(command, m_Assigner.AssignPrefix(), out var properties, parameters);
            lock (m_Commands)
            {
                m_Commands.AppendLine(value);
            }
            properties.Merge(m_MasterPropertiesList);
        }

        /// <summary>
        /// NOTE: Use BulkInserter instead where possible
        /// </summary>
        /// <see cref="BulkInserter{T}"/>
        public void Insert<T>(T Obj, string Table)
        {
            string value = EntityCommandBuilder.BuildInsertCommandContent(Obj, Table, m_Assigner.AssignPrefix(), out var properties);
            lock (m_Commands)
            {
                m_Commands.AppendLine(value);
            }
            properties.Merge(m_MasterPropertiesList);
        }

        public void InsertUpdate<T>(T Obj, string Table)
        {
            string value = EntityCommandBuilder.BuildInsertUpdateCommandContent(Obj, Table, m_Assigner.AssignPrefix(), out var properties);
            lock (m_Commands)
            {
                m_Commands.AppendLine(value);
            }
            properties.Merge(m_MasterPropertiesList);
        }

        public void Update<T>(T Obj, string Table)
        {
            string value = EntityCommandBuilder.BuildUpdateCommandContent(Obj, Table, m_Assigner.AssignPrefix(), out var properties);
            lock (m_Commands)
            {
                m_Commands.AppendLine(value);
            }
            properties.Merge(m_MasterPropertiesList);
        }

        public void Delete<T>(T Obj, string Table)
        {
            string value = EntityCommandBuilder.BuildDeleteCommandContent(Obj, Table, m_Assigner.AssignPrefix(), out var properties);
            lock (m_Commands)
            {
                m_Commands.AppendLine(value);
            }
            properties.Merge(m_MasterPropertiesList);
        }

        public int Commit()
        {
            ExecuteNonQuery("COMMIT");
            lock (m_Commands)
                lock (m_Connection)
                    lock (m_MasterPropertiesList)
                    {
                        using (MySqlCommand command = new MySqlCommand(m_Commands.ToString(), m_Connection))
                        {
                            command.CommandTimeout = 2147483;
                            command.EnableCaching = false;
                            foreach (var p in m_MasterPropertiesList)
                            {
                                command.Parameters.AddWithValue(p.Key, p.Value);
                            }

                            return command.ExecuteNonQuery();
                        }
                    }
        }

        public async Task<int> CommitAsync()
        {
            ExecuteNonQuery("COMMIT");
            string cmdContent;
            PropertyList properties = new PropertyList();

            lock (m_Commands)
            {
                cmdContent = m_Commands.ToString();
            }

            m_MasterPropertiesList.Merge(properties);

            using (MySqlCommand command = new MySqlCommand(cmdContent, m_Connection))
            {
                command.CommandTimeout = 2147483;
                foreach (var p in properties)
                {
                    command.Parameters.AddWithValue(p.Key, p.Value);
                }

                return await command.ExecuteNonQueryAsync();
            }
        }
    }
}