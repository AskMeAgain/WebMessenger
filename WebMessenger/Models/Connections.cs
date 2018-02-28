using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebMessenger.Models
{
    public class Connections
    {
        public int ConnectionsID { get; set; }
        public byte RefreshCounter { get; set; }

        public string AddressA { get; set; }
        public string AddressB { get; set; }
        public User UserA_ { get; set; }
        public User UserB_ { get; set; }

        //public string getAddressA() {
        //    return AddressA.Substring(0, 3) + " [....] " + AddressA.Substring(AddressA.Count()-3 , AddressA.Count());
        //}

        //public string getAddressB() {
        //    return AddressB.Substring(0, 3) + " [....] " + AddressB.Substring(AddressB.Count() - 3, AddressB.Count());
        //}

    }
}
