using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_Raamatud.Model
{
    public enum UserRole
    {
        Kasutaja,
        Avaldaja,
        Admin
    }

    public class User
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [Unique, NotNull]
        public string Username { get; set; } = string.Empty;
        [NotNull]
        public string Password { get; set; } = string.Empty; 
        [NotNull]
        public UserRole Role { get; set; }

        public bool IsApproved { get; set; } = true;
    }
}
