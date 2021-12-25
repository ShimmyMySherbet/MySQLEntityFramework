using System.Collections.Generic;
using ShimmyMySherbet.MySQL.EF.Models;

namespace ExampleApp.Database.Models
{
    public class UserPermissions
    {
        [SQLPrimaryKey]
        public ulong UserID { get; set; }

        public bool IsAdmin { get; set; }

        public bool IsRoot { get; set; }

        [SQLSerialize(ESerializeFormat.JSON)] // Object serializing
        public List<Permission> Permissions { get; set; } = new List<Permission>();
    }
}