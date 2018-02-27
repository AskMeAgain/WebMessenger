using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebMessenger.Data;
using WebMessenger.Models;
using Microsoft.AspNetCore.Http;

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
            bool entity = _context.User
                .Any(m => m.Name == user.Name);
            if (entity)
                return Content("Sorry Mate your username is used!");


            _context.Add(user);
            await generateAddressFromUserAsync(user);

            await _context.SaveChangesAsync();

            return RedirectToAction("Login");

        }

        public ActionResult Home() {

            User user = HttpContext.Session.GetObjectFromJson<User>("User");

            if (user == null)
                return Content("empty!");

            return View(user);

        }

        public async Task generateAddressFromUserAsync(User user) {

            Address addr = new Address() {
                Index = user.AddressIndex,
                generatedAddress = "LOL TEST",
                UserID = user.UserID
            };

            _context.Add(addr);
            await _context.SaveChangesAsync();

        }


    }
}