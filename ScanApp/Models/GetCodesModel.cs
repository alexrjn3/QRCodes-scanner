using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ScanApp.Models
{
    //Model pentru DonwloadExcel
    public class GetCodesModel
    {
        public int ID { get; set; }

        public string Material { get; set; }
        public string Descriere { get; set; }
        public int Cantitate { get; set; }
    }
}
