using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_Raamatud.Model
{
    public class Library
    {
        [PrimaryKey, AutoIncrement]
        public int Library_ID { get; set; }

        public int Kasutaja_ID { get; set; }
        public int Raamat_ID { get; set; }
    }
}
