using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebMessenger.Models {
    public class User {

        public int UserID { get; set; }
        public string Name { get; set; }
        public string Seed { get; set; }
        public string PW { get; set; }
        public int AddressIndex { get; set; }

        public ICollection<Address> Address { get; set; }
    }
}
