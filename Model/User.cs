using Newtonsoft.Json;
using Postgrest.Attributes;
using Postgrest.Models;

namespace E_Raamatud.Model
{
    public enum UserRole
    {
        Kasutaja,
        Avaldaja,
        Admin
    }

    [Table("User")]
    public class User : BaseModel
    {
        [PrimaryKey("Id", false)]
        public int Id { get; set; }

        [Column("Username")]
        public string Username { get; set; } = string.Empty;

        [Column("Password")]
        public string Password { get; set; } = string.Empty;

        [Column("Role")]
        public string RoleString { get; set; } = "Kasutaja";

        [JsonIgnore]  // Newtonsoft to skip this property
        public UserRole Role
        {
            get => Enum.TryParse(RoleString, out UserRole r) ? r : UserRole.Kasutaja;
            set => RoleString = value.ToString();
        }

        [Column("IsApproved")]
        public bool IsApproved { get; set; } = true;
    }
}