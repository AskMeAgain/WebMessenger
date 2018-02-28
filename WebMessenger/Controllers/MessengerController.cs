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

            await generateAddressFromUserAsync(user);
            await _context.SaveChangesAsync();

            return RedirectToAction("Login");

        }

        public ActionResult Home() {

            User user = HttpContext.Session.GetObjectFromJson<User>("User");

            if (user == null)
                return Content("empty!");

            List<AddressTable> list = (from c in _context.AddressTable
                                       where c.UserID == user.UserID
                                       select c).ToList();

            ViewModel model = new ViewModel() {
                AddressList = list,
                User = user
            };

            return View(model);

        }

        public async Task generateAddressFromUserAsync(User user) {

            var addressGenerator = new AddressGenerator(user.getSeed());

            AddressTable addr = new AddressTable() {
                Index = user.AddressIndex,
                generatedAddress = addressGenerator.GetAddress(user.AddressIndex).ToString(),
                UserID = user.UserID
            };

            _context.AddressTable.Add(addr);
            await _context.SaveChangesAsync();

        }


    }
}