using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShimmyMySherbet.MySQL.EF.Models
{
    public interface IBulkInserter<T>
    {

        void Insert(T instance);


        int Commit();

        Task<int> CommitAsync();

    }
}
