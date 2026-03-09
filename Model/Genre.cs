using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_Raamatud.Model
{
    public class Genre
    {
        [PrimaryKey, AutoIncrement]
        public int Zanr_ID { get; set; }

        [MaxLength(100)]
        public string Nimetus { get; set; }

        public string Kirjeldus { get; set; }
    }
}
