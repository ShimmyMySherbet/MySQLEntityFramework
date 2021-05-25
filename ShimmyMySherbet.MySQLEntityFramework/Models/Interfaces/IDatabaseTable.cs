using ShimmyMySherbet.MySQL.EF.Core;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ShimmyMySherbet.MySQL.EF.Models.Interfaces
{
    public interface IDatabaseTable<T> : IDatabaseTableInitializer
    {
        T QuerySingle(string command, params object[] args);

        List<T> Query(string command, params object[] args);

        Task<T> QuerySingleAsync(string command, params object[] args);

        Task<List<T>> QueryAsync(string command, params object[] args);

        void Insert(T obj);

        void Delete(T obj);

        void InsertUpdate(T obj);

        Task InsertAsync(T obj);

        Task DeleteAsync(T obj);

        Task InsertUpdateAsync(T obj);
    }
}