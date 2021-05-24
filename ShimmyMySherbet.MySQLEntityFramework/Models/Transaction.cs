using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShimmyMySherbet.MySQL.EF.Models
{
    public class Transaction : IDbTransaction
    {
        private MySqlConnection m_MySqlConnection;
        private bool m_dispose = false;
        public Transaction(MySqlConnection connection, bool autoDispose = false)
        {
            m_MySqlConnection = connection;
            m_dispose = autoDispose;

        }

        public IDbConnection Connection => m_MySqlConnection;

        public IsolationLevel IsolationLevel => IsolationLevel.Unspecified;

        public void Commit()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void Rollback()
        {
            throw new NotImplementedException();
        }
    }
}
