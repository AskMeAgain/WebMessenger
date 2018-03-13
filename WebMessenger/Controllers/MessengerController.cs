using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebMessenger.Data;
using WebMessenger.Models;
using Microsoft.AspNetCore.Http;
using Tangle.Net.Entity;
using Tangle.Net.Cryptography;
using Tangle.Net.Repository;
using RestSharp;
using Tangle.Net.ProofOfWork;
using Tangle.Net.Utils;
using Tangle.Net.Mam;
using System.Text.RegularExpressions;

namespace WebMessenger.Controllers {

    public class MessengerController : Controller {

        private readonly DataBaseContext _context;

        public MessengerController(DataBaseContext context) {
            _context = context;
        }

        [HttpPost]
        public async Task<ActionResult> CheckLoginAsync(User user) {

            if (string.IsNullOrEmpty(user.Name) || string.IsNullOrEmpty(user.PW))
                return RedirectToAction("Login");

            //CHECK IF LOGIN IS CORRECT VIA DB!
            var entity = await _context.User
                .SingleAsync(m => m.Name == user.Name && m.PW == user.PW);
            if (entity == null)
                return Content("NOT FOUND! WEW");

            HttpContext.Session.SetObjectAsJson("User", entity);

            return RedirectToAction("ChatAsync");


        }

        public ActionResult Login() {
            return View();
        }

        public async Task<IActionResult> RegisterAsync(User user) {

            //first check if username is used!
            if (_context.User.Any(m => m.Name == user.Name))
                return Content("Sorry Mate your username is used!");

            _context.User.Add(user);

            await generateAddressFromUserAsync(user, 0, 4);
            await _context.SaveChangesAsync();

            return RedirectToAction("Login");

        }

        public ActionResult Home() {

            User user = HttpContext.Session.GetObjectFromJson<User>("User");

            if (user == null)
                return Content("empty!");
            List<AddressTable> addrList = getAllAddressTable(user);
            List<Connections> connList = getAllConnections(user);

            ViewModel model = new ViewModel() {
                AddressList = addrList,
                User = user,
                ConnectionList = connList
            };

            return View(model);

        }

        private List<Connections> getAllConnections(User user) {
            return (from c in _context.Connections
                    where (c.UserA_ == user || c.UserB_ == user)
                    select c).Include("UserA_").Include("UserB_").ToList();
        }

        private List<AddressTable> getAllAddressTable(User user) {
            return (from c in _context.AddressTable
                    where c.UserID == user.UserID
                    select c).ToList();
        }

        public async Task<IActionResult> MakeConnectionAsync(User model) {



            //Get both userIDs
            User temp = HttpContext.Session.GetObjectFromJson<User>("User");
            User userB = await _context.User.SingleAsync(m => m.Name == model.Name);
            User userA = await _context.User.SingleAsync(m => m.UserID == temp.UserID);

            //check if connection already exists!
            Connections testConn = await getConnectionFromTwoIDsAsync(userB.UserID, userA.UserID);
            if (testConn != null) {
                TempData["msg"] = "<script>alert('Connection Already established');</script>";
                return RedirectToAction("ChatAsync", new { id = HttpContext.Session.GetObjectFromJson<User>("SelectedUser")?.Name });
            }

            //Get Open addresses:
            AddressTable addrA = await _context.AddressTable.FirstAsync(m => m.UserID == userA.UserID);
            AddressTable addrB = await _context.AddressTable.FirstAsync(m => m.UserID == userB.UserID);

            //set refreshCounter
            byte counter = 0;

            Connections conn = new Connections {
                UserA_ = userA,
                UserB_ = userB,
                AddressA = addrA.generatedAddress,
                AddressB = addrB.generatedAddress,
                RefreshCounter = counter,
                EncryptionKey = generateEncryptionKey()
            };

            _context.Connections.Add(conn);

            //generate a new address for your own user:
            await generateAddressFromUserAsync(userA, userA.AddressIndex, userA.AddressIndex + 1);


            //remove them out of the list
            _context.AddressTable.Remove(addrA);
            _context.AddressTable.Remove(addrB);

            await _context.SaveChangesAsync();

            return RedirectToAction("ChatAsync", new { id = HttpContext.Session.GetObjectFromJson<User>("SelectedUser")?.Name });

        }

        private string generateEncryptionKey() {
            return Seed.Random().ToString();
        }

        public async Task generateAddressFromUserAsync(User user, int start, int end) {

            var addressGenerator = new AddressGenerator(user.getSeed());

            int i;

            for (i = start; i < end; i++) {
                AddressTable addr = new AddressTable() {
                    Index = user.AddressIndex + i,
                    generatedAddress = addressGenerator.GetAddress(user.AddressIndex + i).ToString(),
                    UserID = user.UserID
                };

                _context.AddressTable.Add(addr);
            }

            user.AddressIndex += i;

            var entity = _context.User.Find(user.UserID);
            _context.Entry(entity).CurrentValues.SetValues(user);

            await _context.SaveChangesAsync();

        }

        public async Task<ActionResult> ChatAsync(string id) {

            if (HttpContext.Session.GetObjectFromJson<User>("User") == null)
                return RedirectToAction("Login");

            List<User> userList = getFriends();

            List<ChatEntry> chatEntrys = new List<ChatEntry>();

            //if its not null we are looking into a chat
            if (!string.IsNullOrEmpty(id)) {

                //store selected User
                if (_context.User.Any(m => m.Name.Equals(id))) {
                    User sel = await _context.User.SingleAsync(m => m.Name.Equals(id));
                    HttpContext.Session.SetObjectAsJson("SelectedUser", sel);
                    chatEntrys = await getChatAsync(id);

                    //chatEntrys = new List<ChatEntry>() {
                    //new ChatEntry(),
                    //new ChatEntry(),
                    //new ChatEntry(),
                    //new ChatEntry(),
                    //new ChatEntry(),
                    //new ChatEntry(),
                    //new ChatEntry(),
                    //new ChatEntry()
                    //};
                }
            }

            User_Chat temp = new User_Chat {
                Chat = chatEntrys,
                selectedChat = id,
                Friends = userList,
                LocalUser = HttpContext.Session.GetObjectFromJson<User>("User")
            };

            return View(temp);

        }

        public async Task<List<ChatEntry>> getChatAsync(string name) {

            List<ChatEntry> chatEntrys = new List<ChatEntry>();

            User local = HttpContext.Session.GetObjectFromJson<User>("User");
            User other = _context.User.Single(m => m.Name.Equals(name));


            var repository = new RestIotaRepository(new RestClient("https://iotanode.us:443"), new PoWService(new CpuPowDiver()));

            //get connection of users
            Connections conn = await getConnectionFromTwoIDsAsync(local.UserID, other.UserID);

            List<Address> addresses = new List<Address>() {
                new Address(conn.AddressA),
                new Address(conn.AddressB)
            };

            var hashList = repository.FindTransactionsByAddresses(addresses);

            List<Bundle> bundles = repository.GetBundles(hashList.Hashes, true);

            foreach (Bundle b in bundles) {

                string entryName = "";

                if (b.Transactions[0].Address.ToString() == conn.AddressA)
                    entryName = conn.UserB_.Name;
                else
                    entryName = conn.UserA_.Name;

                ChatEntry entry = new ChatEntry(b, entryName, conn.GetKey());
                chatEntrys.Add(entry);
            }

            List<ChatEntry> sortedList = chatEntrys.OrderBy(o => o.TimeStamp).ToList();

            return sortedList;

        }

        public ActionResult Logout() {

            HttpContext.Session.Clear();

            return RedirectToAction("Login");
        }

        private List<User> getFriends() {

            //get user first
            User thisUser = HttpContext.Session.GetObjectFromJson<User>("User");

            //get all friends
            List<Connections> connList = getAllConnections(thisUser);

            List<User> userList = new List<User>();

            foreach (Connections conn in connList) {

                if (conn.UserA_.Name == thisUser.Name)
                    userList.Add(conn.UserB_);
                else
                    userList.Add(conn.UserA_);
            }

            return userList;

        }

        public async Task<Connections> getConnectionFromTwoIDsAsync(int a, int b) {

            return await _context.Connections.Include("UserA_").Include("UserB_").SingleOrDefaultAsync(
                m => ((m.UserA_.UserID == a && m.UserB_.UserID == b) || (m.UserB_.UserID == a && m.UserA_.UserID == b)));

        }

        public async Task<IActionResult> SendMessageAsync(string Message) {

            var repository = new RestIotaRepository(new RestClient("https://iotanode.us:443"), new PoWService(new CpuPowDiver()));
            var mask = new CurlMask();

            User sender = HttpContext.Session.GetObjectFromJson<User>("User");
            User receiver = HttpContext.Session.GetObjectFromJson<User>("SelectedUser");

            Connections connection = await getConnectionFromTwoIDsAsync(sender.UserID, receiver.UserID);

            string sendingAddress = (connection.UserA_.UserID == sender.UserID) ? connection.AddressB : connection.AddressA;


            Transfer trans = new Transfer() {
                Address = new Address(sendingAddress) { Balance = 0 },
                Message = TryteString.FromAsciiString(Message),
                Tag = new Tag("CSHARP"),
                Timestamp = Timestamp.UnixSecondsTimestamp
            };

            Bundle bundle = new Bundle();

            bundle.AddTransfer(trans);

            bundle.Finalize();
            bundle.Sign();

            //sending the message
            var resultTransactions = repository.SendTrytes(bundle.Transactions, 27, 14);

            return RedirectToAction("ChatAsync", new { id = receiver.Name });
        }

        private string sanitizeMessage(string message) {
            return Regex.Replace(message, @"[^\u0000-\u007F]+", string.Empty);
        }
    }
}