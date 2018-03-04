using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tangle.Net.Entity;

namespace WebMessenger.Models {
    public class ChatEntry {

        public string Message { get; set; }
        public string TransactionID { get; set; }
        public string Name { get; set; }
        public long TimeStamp { get; set; }

        public ChatEntry(Bundle bundle, string name) {

            TransactionID = bundle.Transactions[0].Hash.ToString();
            Name = name;

            List<string> messageList = bundle.GetMessages();

            if (messageList != null && messageList.Count > 0) {

                Message = messageList[0].ToString();

                TimeStamp = bundle.Transactions[0].Timestamp;

            } else
                Message = "";

        }

    }
}
