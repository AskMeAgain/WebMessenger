using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tangle.Net.Entity;

namespace WebMessenger.Models {
    public class ChatEntry {

        public string Message { get; set; }
        public string TransactionID { get; set; }

        public ChatEntry(Bundle bundle) {

            TransactionID = bundle.Transactions[0].Hash.ToString();

            List<string> messageList = bundle.GetMessages();

            if (messageList != null && messageList.Count > 0)
                Message = messageList[0].ToString();
            else
                Message = "";
        }

    }
}
