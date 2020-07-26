using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShimmyMySherbet.MySQL.EF.Internals
{
    public static class Extensions
    {
        public static IEnumerable<Output> CastEnumeration<Input, Output>(this IEnumerable<Input> Enumerable, Func<Input, Output> Converter)
        {
            var Res = new List<Output>();
            foreach (var Item in Enumerable)
                Res.Add(Converter(Item));
            return Res;
        }
    }
}
