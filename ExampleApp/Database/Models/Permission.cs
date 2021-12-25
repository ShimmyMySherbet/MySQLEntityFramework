using System;

namespace ExampleApp.Database.Models
{
    public class Permission
    {
        public string PermissionID { get; set; }
        public ulong GrantedBy { get; set; }
        public DateTime GrantedAt { get; set; } = DateTime.Now;
    }
}