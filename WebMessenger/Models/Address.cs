using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebMessenger.Models {
    public class Address {

        public int AddressID { get; set; }
        public int Index { get; set; }
        public string generatedAddress { get; set; }
        public int UserID { get; internal set; }
    }
}
