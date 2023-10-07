using System;
using ShimmyMySherbet.MySQL.EF.Models;

namespace ExampleApp.Database.Models
{
    [SQLCharSet(SQLCharSet.utf8mb4)]
    public class Permission
    {
        public string PermissionID { get; set; }
        public ulong GrantedBy { get; set; }
        public DateTime GrantedAt { get; set; } = DateTime.Now;
    }
}