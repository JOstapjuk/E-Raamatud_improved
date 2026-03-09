using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_Raamatud.Model
{
    public class BookWithGenre
    {
        public int Raamat_ID { get; set; }
        public string Pealkiri { get; set; }
        public string Kirjeldus { get; set; }
        public decimal Hind { get; set; }
        public string Zanr_Nimi { get; set; }
        public string Pilt { get; set; }
    }
}
