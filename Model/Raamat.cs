using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_Raamatud.Model
{
    public class Raamat
    {
        [PrimaryKey, AutoIncrement]
        public int Raamat_ID { get; set; }

        [MaxLength(200)]
        public string Pealkiri { get; set; }

        public string Kirjeldus { get; set; }

        public decimal Hind { get; set; }

        public int Avaldaja_ID { get; set; }

        public int Zanr_ID { get; set; }

        public string Pilt { get; set; }
        public string Tekstifail { get; set; }
    }
}
