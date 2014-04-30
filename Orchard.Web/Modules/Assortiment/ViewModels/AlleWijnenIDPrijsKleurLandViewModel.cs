using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Assortiment.ViewModels
{
    public class AlleWijnenIDPrijsKleurLandViewModel
    {
        public int WijnID { get; set; }
        public int prijs { get; set; }
        public int KleurID { get; set; }
        public int LandID { get; set; }
        public int Jaar { get; set; }
    }
}