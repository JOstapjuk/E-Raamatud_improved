using Postgrest.Attributes;
using Postgrest.Models;

namespace E_Raamatud.Model
{
    [Table("Raamat")]
    public class Raamat : BaseModel
    {
        [PrimaryKey("Raamat_ID", false)]
        public int Raamat_ID { get; set; }

        [Column("Pealkiri")]
        public string Pealkiri { get; set; }

        [Column("Kirjeldus")]
        public string Kirjeldus { get; set; }

        [Column("Hind")]
        public decimal Hind { get; set; }

        [Column("Avaldaja_ID")]
        public int Avaldaja_ID { get; set; }

        [Column("Zanr_ID")]
        public int Zanr_ID { get; set; }

        [Column("Pilt")]
        public string Pilt { get; set; }

        [Column("Tekstifail")]
        public string Tekstifail { get; set; }

        [Column("Audiofail")]
        public string Audiofail { get; set; }
    }
}