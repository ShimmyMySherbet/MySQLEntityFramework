using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ShimmyMySherbet.MySQL.EF.Models
{
    /// <summary>
    /// Similar to BulkInserter, but splits up commands as to now pass the max packet count
    /// Use this for really large inserts (e.g., 10K+).
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="BulkInserter{T}"/>
    public class TransactionalBulkInserter<T> : IBulkInserter<T>
    {
        private List<BulkInserter<T>> m_Inserters = new List<BulkInserter<T>>();
        private BulkInserter<T> m_Current;
        private MySqlConnection m_Connection;
        public string Table { get; private set; }
        private int m_Inserts = 0;
        private int m_max;
        private bool m_cnew = true;

        public TransactionalBulkInserter(MySqlConnection connection, string table, int maxInsertsPerTransaction = 5000)
        {
            Table = table;
            m_Connection = connection;
            m_max = maxInsertsPerTransaction;
        }

        public void Insert(T instance)
        {
            if (m_cnew)
            {
                m_Inserts = 0;
                m_Current = new BulkInserter<T>(m_Connection, Table);
                m_Inserters.Add(m_Current);
                m_cnew = false;
            }

            lock (m_Current)
                m_Current.Insert(instance);

            m_Inserts++;

            if (m_Inserts >= m_max)
            {
                m_cnew = true;
            }
        }

        public int Commit()
        {
            int c = 0;

            lock (m_Current)
            {
                foreach (var t in m_Inserters)
                {
                    c += t.Commit();
                }

                m_Inserters.Clear();

                m_cnew = true;
            }

            return c;
        }

        public async Task<int> CommitAsync()
        {
            int c = 0;

            IReadOnlyCollection<BulkInserter<T>> inserters;

            lock (m_Current)
            {
                inserters = m_Inserters.ToArray();
            }

            foreach (var t in inserters)
            {
                c += await t.CommitAsync();
            }

            m_Inserters.Clear();

            m_cnew = true;
            return c;
        }
    }
}