using Postgrest.Attributes;
using Postgrest.Models;

namespace E_Raamatud.Model
{
    [Table("Library")]
    public class Library : BaseModel
    {
        [PrimaryKey("Library_ID", false)]
        public int Library_ID { get; set; }

        [Column("Kasutaja_ID")]
        public int Kasutaja_ID { get; set; }

        [Column("Raamat_ID")]
        public int Raamat_ID { get; set; }
    }
}