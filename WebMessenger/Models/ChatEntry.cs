using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Tangle.Net.Entity;
using Tangle.Net.Mam;

namespace WebMessenger.Models {
    public class ChatEntry {

        public string Message { get; set; }
        public string TransactionID { get; set; }
        public string Name { get; set; }
        public long TimeStamp { get; set; }

        public ChatEntry(Bundle bundle, string name, string encryptionKey) {

            TransactionID = bundle.Transactions[0].Hash.ToString();
            Name = name;

            Message = bundle.Transactions[0].Fragment.ToAsciiString();
            TimeStamp = bundle.Transactions[0].Timestamp;

            if (string.IsNullOrEmpty(Message)) {
                Message = Utility.DecryptString(Message, encryptionKey);
            }

        }

        public ChatEntry() {
            Message = "empty";
            Name = "LOL";
            TimeStamp = 123123123;
            TransactionID = "123123123";
        }

    }
}
