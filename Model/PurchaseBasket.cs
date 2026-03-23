using Postgrest.Attributes;
using Postgrest.Models;

namespace E_Raamatud.Model
{
    [Table("PurchaseBasket")]
    public class PurchaseBasket : BaseModel
    {
        [PrimaryKey("Ostukorv_ID", false)]
        public int Ostukorv_ID { get; set; }

        [Column("Kasutaja_ID")]
        public int Kasutaja_ID { get; set; }

        [Column("Raamat_ID")]
        public int Raamat_ID { get; set; }

        [Column("Kogus")]
        public int Kogus { get; set; }

        [Column("Loppu_hind")]
        public decimal Lõppu_hind { get; set; }

        [Column("Status")]
        public string Status { get; set; } = "InCart";

        [Column("PurchaseDate")]
        public DateTime? PurchaseDate { get; set; }
    }
}