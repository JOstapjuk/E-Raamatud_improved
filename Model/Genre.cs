using Postgrest.Attributes;
using Postgrest.Models;

namespace E_Raamatud.Model
{
    [Table("Genre")]
    public class Genre : BaseModel
    {
        [PrimaryKey("Zanr_ID", false)]
        public int Zanr_ID { get; set; }

        [Column("Nimetus")]
        public string Nimetus { get; set; }

        [Column("Kirjeldus")]
        public string Kirjeldus { get; set; }
    }
}