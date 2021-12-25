using MySql.Data.MySqlClient;
using ShimmyMySherbet.MySQL.EF.Internals;
using ShimmyMySherbet.MySQL.EF.Models.Internals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ShimmyMySherbet.MySQL.EF.Models
{
    /// <summary>
    /// Provides a way to quickly insert large amount of objects into a table.
    /// For larger insert (e.g., 10K+, use TransactionalBulkInserter)
    /// </summary>
    /// <typeparam name="T">Database table class type</typeparam>
    /// <seealso cref="TransactionalBulkInserter{T}"/>
    public class BulkInserter<T> : IBulkInserter<T>
    {
        public string Table { get; private set; }

        private MySqlConnection m_Connection;

        private PrefixAssigner m_Assigner = new PrefixAssigner();

        private PropertyList m_BuildProperties = new PropertyList();

        private StringBuilder m_CommandBuilder { get; set; } = new StringBuilder();
        private List<SQLMetaField> m_SQLMetas = new List<SQLMetaField>();
        private bool m_FirstInsert = true;
        private EInsertMode m_Mode;


        public BulkInserter(MySqlConnection connection, string table, EInsertMode mode = EInsertMode.INSERT)
        {
            m_Connection = connection;
            m_Mode = mode;
            Table = table;

            foreach (FieldInfo Field in typeof(T).GetFields())
            {
                bool Include = true;
                string Name = Field.Name;
                foreach (Attribute Attrib in Attribute.GetCustomAttributes(Field))
                {
                    if (Attrib is SQLOmit || Attrib is SQLIgnore)
                    {
                        Include = false;
                        break;
                    }
                    else if (Attrib is SQLPropertyName)
                    {
                        Name = ((SQLPropertyName)Attrib).Name;
                    }
                }
                if (Include)
                {
                    if (m_SQLMetas.Where(x => string.Equals(x.Name, Name, StringComparison.InvariantCultureIgnoreCase)).Count() != 0) continue;
                    m_SQLMetas.Add(new SQLMetaField(Name, m_SQLMetas.Count, Field));
                }
            }
            Reset();
        }

        /// <summary>
        /// Discard all queued inserts and resets the inserter.
        /// </summary>
        public void Reset()
        {
            m_Assigner.Reset();
            m_BuildProperties.Reset();
            lock (m_CommandBuilder)
            {
                m_CommandBuilder = new StringBuilder();
                string Command = $"INSERT{(m_Mode == EInsertMode.INSERT_IGNORE ? " IGNORE" : "")} INTO `{Table}` ({string.Join(", ", m_SQLMetas.CastEnumeration(x => x.Name))}) VALUES";
                m_CommandBuilder.Append(Command);
            }
        }

        /// <summary>
        /// Adds an object to the insert list
        /// </summary>
        public void Insert(T instance)
        {
            int prefix = m_Assigner.AssignPrefix();
            lock (m_SQLMetas)
            {
                lock (m_CommandBuilder)
                {
                    m_CommandBuilder.Append($"{(m_FirstInsert ? "" : ",")}\n({string.Join(", ", m_SQLMetas.CastEnumeration(x => $"@{prefix}_{x.FieldIndex}"))})");
                }
                m_FirstInsert = false;

                lock (m_BuildProperties)
                {
                    foreach (var meta in m_SQLMetas)
                    {
                        m_BuildProperties.Add($"@{prefix}_{meta.FieldIndex}", meta.Field.GetValue(instance));
                    }
                }
            }
        }

        /// <summary>
        /// Writes all inserts to the database
        /// </summary>
        /// <returns>Rows modified</returns>
        public int Commit()
        {
            int a;
            lock (m_CommandBuilder)
            {
                m_CommandBuilder.Append(";");
                lock (m_Connection)
                {
                    using (MySqlCommand command = new MySqlCommand(m_CommandBuilder.ToString(), m_Connection))
                    {
                        command.CommandTimeout = 2147483;
                        lock (m_BuildProperties)
                        {
                            foreach (var p in m_BuildProperties)
                            {
                                command.Parameters.AddWithValue(p.Key, p.Value);
                            }
                        }


                        a =  command.ExecuteNonQuery();
                    }
                }
            }
            Reset();
            return a;
        }
        /// <summary>
        /// Writes all inserts to the database
        /// </summary>
        /// <returns>Rows modified</returns>
        public async Task<int> CommitAsync()
        {
            string cmdTxt;
            lock (m_CommandBuilder)
            {
                m_CommandBuilder.Append(";");
                cmdTxt = m_CommandBuilder.ToString();
            }
            PropertyList properties = new PropertyList();
            m_BuildProperties.Merge(properties);

            using (MySqlCommand command = new MySqlCommand(cmdTxt, m_Connection))
            {
                command.CommandTimeout = 2147483;

                foreach (var p in properties)
                {
                    command.Parameters.AddWithValue(p.Key, p.Value);
                }
                Reset();
                return await command.ExecuteNonQueryAsync();
            }
        }
    }
}