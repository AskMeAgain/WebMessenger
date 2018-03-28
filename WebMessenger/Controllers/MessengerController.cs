﻿using System;
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

        //Main page
        public ActionResult Overview(string id) {
            User local = HttpContext.Session.GetObjectFromJson<User>("User");

            ViewData["Local"] = local;
            ViewData["Current"] = id;
            ViewData["Color"] = local.Color;


            //set specific data
            if (id != null && id.Equals("OpenRequests")) {

                //get all open requests
                Request[] list = GetAllReceiverRequests(local.Name);
                ViewData["RequestList1"] = list;
                Request[] list2 = GetAllSenderRequests(local.Name);
                ViewData["RequestList2"] = list2;
            }

            if (id != null && id.Equals("ShowChats")) {
                Connections[] list = getAllConnections(local);
                ViewData["ConnectionList"] = list;
            }

            if (id != null && id.Equals("Chat")) {
                ViewData["ChatList"] = HttpContext.Session.GetObjectFromJson<List<ChatEntry>>("ChatList");
            }

            return View();
        }

        #region simple redirects

        public ActionResult Login() {
            return View();
        }

        public ActionResult ShowChats() {
            return RedirectToAction("Overview", new { id = "ShowChats" });
        }

        public ActionResult ShowAddFriend() {
            return RedirectToAction("Overview", new { id = "AddFriend" });
        }

        public ActionResult ShowRequests() {
            return RedirectToAction("Overview", new { id = "OpenRequests" });
        }

        public ActionResult Logout() {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        #endregion

        #region All async methods

        [HttpPost]
        public async Task<ActionResult> CheckLoginAsync(User user) {

            if (string.IsNullOrEmpty(user.Name) || string.IsNullOrEmpty(user.PW))
                return RedirectToAction("Login");

            //check login
            var entity = await _context.User
                .SingleOrDefaultAsync(m => m.Name == user.Name && m.PW == user.PW);
            if (entity == null) {
                TempData["msg"] = "<script>alert('Wrong Username/Password');</script>";
                return RedirectToAction("Login");
            }

            HttpContext.Session.SetObjectAsJson("User", entity);

            return RedirectToAction("Overview");

        }

        public async Task<ActionResult> ShowSpecificChatAsync(string connection) {

            Connections conn = await _context.Connections.Include("UserA_").Include("UserB_").SingleAsync(m => m.ConnectionsID == int.Parse(connection));

            //storing the connection as selected
            HttpContext.Session.SetObjectAsJson("SelectedConnection", conn);

            List<ChatEntry> list = await GetChatAsync(conn);
            HttpContext.Session.SetObjectAsJson("ChatList", list);

            return RedirectToAction("Overview", new { id = "Chat" });
        }

        public async Task<ActionResult> SetSettingsAsync(string url, string color) {

            User temp = HttpContext.Session.GetObjectFromJson<User>("User");
            User local = await _context.User.SingleAsync(m => m.Name.Equals(temp.Name));

            local.Color = color;

            await _context.SaveChangesAsync();

            //update the stored object inside session too
            HttpContext.Session.SetObjectAsJson("User", local);

            //prepare url
            string[] realUrl = url.Split("/");

            //if we are on mainpage, then remove parameter
            if (realUrl.Length < 3)
                realUrl[realUrl.Length - 1] = "";

            return RedirectToAction("Overview", new { id = realUrl[realUrl.Length - 1] });
        }

        public async Task<IActionResult> RegisterAsync(User user) {

            //first check if username is used!
            if (_context.User.Any(m => m.Name == user.Name)) {
                TempData["msg"] = "<script>alert('Username already used');</script>";
                return RedirectToAction("Login");
            }

            //check if password is the same
            if (!user.PW.Equals(user.PW2)) {
                TempData["msg"] = "<script>alert('Password repeat is not the same');</script>";
                return RedirectToAction("Login");
            }

            user.Seed = Seed.Random().ToString();
            user.Color = "cornflowerblue";

            _context.User.Add(user);

            await generateAddressFromUserAsync(user, 0, 4);
            await _context.SaveChangesAsync();

            return RedirectToAction("Login");

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

            Connections conn = new Connections {
                UserA_ = userA,
                UserB_ = userB,
                AddressA = addrA.generatedAddress,
                AddressB = addrB.generatedAddress,
                Refresh_A = false,
                Refresh_B = false,
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

        public async Task<IActionResult> AcceptRequestAsync(string connection) {

            int ID = int.Parse(connection);

            //check first if ID exists
            Request req = await _context.Requests.Include("Sender").Include("Receiver").SingleOrDefaultAsync(m => m.RequestID == ID);

            if (req == null)
                return RedirectToAction("ShowRequests");

            //make connection
            //find other user
            User local = HttpContext.Session.GetObjectFromJson<User>("User");
            User other;

            other = (local.Name.Equals(req.Sender.Name)) ? req.Receiver : req.Sender;

            await MakeConnectionAsync(other);

            //add new address
            await generateAddressFromUserAsync(other, other.AddressIndex, other.AddressIndex + 1);

            //remove request
            _context.Requests.Remove(req);
            await _context.SaveChangesAsync();

            return RedirectToAction("ShowRequests");
        }

        public async Task<IActionResult> MakeRequestAsync(string name) {

            //get receiver
            User temp = HttpContext.Session.GetObjectFromJson<User>("User");
            User receiver = await _context.User.SingleAsync(m => m.Name.Equals(name));
            User sender = await _context.User.SingleAsync(m => m.Name.Equals(temp.Name));

            //add address
            await generateAddressFromUserAsync(sender, sender.AddressIndex, sender.AddressIndex + 1);

            //create Request
            Request req = new Request() {
                Sender = sender,
                Receiver = receiver
            };

            _context.Requests.Add(req);
            await _context.SaveChangesAsync();

            return RedirectToAction("ShowAddFriend");

        }

        public async Task<IActionResult> SendMessageAsync(string Message) {

            //connection to a iota node
            var repository = new RestIotaRepository(new RestClient("https://iotanode.us:443"), new PoWService(new CpuPowDiver()));

            User sender = HttpContext.Session.GetObjectFromJson<User>("User");

            Connections connection = HttpContext.Session.GetObjectFromJson<Connections>("SelectedConnection");

            //set refresh bools
            if (connection.UserA_.Name.Equals(sender.Name))
                connection.Refresh_B = true;
            else
                connection.Refresh_A = true;

            //updating entry
            _context.Connections.Update(connection);
            await _context.SaveChangesAsync();

            string sendingAddress = (connection.UserA_.UserID == sender.UserID) ? connection.AddressB : connection.AddressA;

            string encryptedMsg = Utility.EncryptString(Message, connection.EncryptionKey);

            Transfer trans = new Transfer() {
                Address = new Address(sendingAddress) { Balance = 0 },
                Message = TryteString.FromAsciiString(encryptedMsg),
                Tag = new Tag("CSHARP"),
                Timestamp = Timestamp.UnixSecondsTimestamp
            };

            Bundle bundle = new Bundle();

            bundle.AddTransfer(trans);

            bundle.Finalize();
            bundle.Sign();

            //sending the message to the tangle
            var resultTransactions = repository.SendTrytes(bundle.Transactions, 27, 14);

            return RedirectToAction("ShowSpecificChatAsync", new { connection = connection.ConnectionsID });
        }

        #endregion

        #region GETTERS and Helpers

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

        public Request[] GetAllReceiverRequests(string name) {
            return (from c in _context.Requests
                    where (c.Receiver.Name.Equals(name))
                    select c).Include("Sender").Include("Receiver").ToArray();
        }

        public Request[] GetAllSenderRequests(string name) {
            return (from c in _context.Requests
                    where (c.Sender.Name.Equals(name))
                    select c).Include("Sender").Include("Receiver").ToArray();
        }

        public async Task<List<ChatEntry>> GetChatAsync(Connections conn) {

            List<ChatEntry> chatEntrys = new List<ChatEntry>();

            User User_A = await _context.User.SingleAsync(m => m.Name.Equals(conn.UserA_.Name));
            User User_B = await _context.User.SingleAsync(m => m.Name.Equals(conn.UserB_.Name));

            User local = HttpContext.Session.GetObjectFromJson<User>("User");

            var repository = new RestIotaRepository(new RestClient("https://iotanode.us:443"), new PoWService(new CpuPowDiver()));

            //set refresh bools
            if (conn.UserA_.Name.Equals(local.Name))
                conn.Refresh_A = false;
            else
                conn.Refresh_B = false;

            //updating entry
            _context.Connections.Update(conn);
            _context.SaveChanges();

            //setting addresses to check for new messages
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

                ChatEntry entry = new ChatEntry(b, entryName, conn.EncryptionKey);
                chatEntrys.Add(entry);
            }

            List<ChatEntry> sortedList = chatEntrys.OrderBy(o => o.TimeStamp).ToList();

            return sortedList;

        }

        private Connections[] getAllConnections(User user) {
            return (from c in _context.Connections
                    where (c.UserA_ == user || c.UserB_ == user)
                    select c).Include("UserA_").Include("UserB_").ToArray();
        }

        private List<AddressTable> getAllAddressTable(User user) {
            return (from c in _context.AddressTable
                    where c.UserID == user.UserID
                    select c).ToList();
        }

        private List<User> getFriends() {

            //get user first
            User thisUser = HttpContext.Session.GetObjectFromJson<User>("User");

            //get all friends
            Connections[] connList = getAllConnections(thisUser);

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

        #endregion

    }
}