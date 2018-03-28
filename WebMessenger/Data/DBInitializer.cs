using WebMessenger.Models;
using System;
using System.Linq;
using Tangle.Net.Entity;
using Tangle.Net.Cryptography;

namespace WebMessenger.Data {
    public static class DbInitializer {
        public static void Initialize(DataBaseContext context) {

            //context.Database.EnsureDeleted();

            context.Database.EnsureCreated();

        }
    }
}