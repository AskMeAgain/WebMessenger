using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tangle.Net.Entity;

namespace WebMessenger.Models
{
    public class Connections
    {
        public int ConnectionsID { get; set; }
        public byte RefreshCounter { get; set; }
        public string EncryptionKey { get; set; }

        public string AddressA { get; set; }
        public string AddressB { get; set; }
        public User UserA_ { get; set; }
        public User UserB_ { get; set; }


        public TryteString GetKey() {
            return TryteString.FromAsciiString(EncryptionKey);
        }


    }
}
