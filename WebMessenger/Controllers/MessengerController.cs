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

        public async Task<IActionResult> LoginAsync(User model) {

            if (string.IsNullOrEmpty(model.Name))
                return View();

            //CHECK IF LOGIN IS CORRECT VIA DB!
            var user = await _context.User
                .SingleAsync(m => m.Name == model.Name && m.PW == model.PW);
            if (user == null) 
                return Content("NOT FOUND! WEW");
            

            HttpContext.Session.SetObjectAsJson("User", user);

            return RedirectToAction("Home");
        }

        public async Task<IActionResult> RegisterAsync(User model) {

            _context.Add(model);
            await _context.SaveChangesAsync();

            return RedirectToAction("LoginAsync");

        }

        public ActionResult Home() {

            User user = HttpContext.Session.GetObjectFromJson<User>("User");

            if (user == null)
                return Content("empty!");

            return View(user);

        }

    }
}