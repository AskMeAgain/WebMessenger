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

            return RedirectToAction("Home");


        }

        public ActionResult Login() {

            return View();

        }

        public async Task<IActionResult> RegisterAsync(User user) {

            //first check if username is used!
            if (_context.User.Any(m => m.Name == user.Name))
                return Content("Sorry Mate your username is used!");

            _context.User.Add(user);

            await generateAddressFromUserAsync(user, 4);
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
                RefreshCounter = counter
            };

            _context.Connections.Add(conn);

            //remove them out of the list
            _context.AddressTable.Remove(addrA);
            _context.AddressTable.Remove(addrB);


            await _context.SaveChangesAsync();

            return RedirectToAction("Home");

        }

        public async Task generateAddressFromUserAsync(User user, int num) {

            var addressGenerator = new AddressGenerator(user.getSeed());

            int i;

            for (i = 0; i < num; i++) {
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

        public ActionResult Chat(string id) {

            List<User> userList = getFriends();

            List<string> chatList = new List<string>();

            User_Chat temp = new User_Chat {
                Chat = chatList,
                selectedChat = id,
                Friends = userList
            };

            return View(temp);

        }

        public ActionResult DisplayChat(string name) {

            return Content("COOL CHAT" + name);

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

    }
}