using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebMessenger.Models
{
    public class User_Chat
    {
        public string selectedChat { get; set; }
        public List<User> Friends { get; set; }
        public List<ChatEntry> Chat { get; set; }
        public User LocalUser { get; set; }
    }
}
