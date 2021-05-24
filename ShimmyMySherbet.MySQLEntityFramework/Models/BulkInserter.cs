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
    public class BulkInserter<T>
    {
        public string Table { get; private set; }

        private MySqlConnection m_Connection;

        private PrefixAssigner m_Assigner = new PrefixAssigner();

        private PropertyList m_BuildProperties = new PropertyList();

        private StringBuilder m_CommandBuilder { get; set; } = new StringBuilder();
        private List<SQLMetaField> m_SQLMetas = new List<SQLMetaField>();
        private bool m_FirstInsert = true;

        public BulkInserter(MySqlConnection connection, string table)
        {
            m_Connection = connection;
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
            string Command = $"INSERT INTO `{Table}` ({string.Join(", ", m_SQLMetas.CastEnumeration(x => x.Name))}) VALUES";
            m_CommandBuilder.Append(Command);
        }

        public void Insert(T instance)
        {
            int prefix = m_Assigner.AssignPrefix();
            lock (m_SQLMetas)
            {
                lock (m_CommandBuilder)
                {
                    m_CommandBuilder.Append($"{(m_FirstInsert ? "" : ",")}\n({string.Join(", ", m_SQLMetas.CastEnumeration(x => $"@{prefix}_{x.Index}"))})");
                }
                m_FirstInsert = false;

                lock (m_BuildProperties)
                {
                    foreach (var meta in m_SQLMetas)
                    {
                        m_BuildProperties.Add($"@{prefix}_{meta.Index}", meta.Field.GetValue(instance));
                    }
                }
            }
        }

        public void Commit()
        {
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

                        command.ExecuteNonQuery();
                    }
                }

                m_CommandBuilder = new StringBuilder();
            }
        }

        public async Task CommitAsync()
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

                await command.ExecuteNonQueryAsync();
            }
        }
    }
}