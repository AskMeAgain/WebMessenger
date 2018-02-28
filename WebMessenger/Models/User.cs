using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tangle.Net.Entity;

namespace WebMessenger.Models {
    public class User {

        public int UserID { get; set; }
        public string Name { get; set; }

        public string Seed { get; set; }
        public string PW { get; set; }
        public int AddressIndex { get; set; }

        public ICollection<AddressTable> Address { get; set; }

        public Seed getSeed() {
            return new Seed(Seed);
        }
    }
}
