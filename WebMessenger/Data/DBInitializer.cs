using WebMessenger.Models;
using System;
using System.Linq;

namespace WebMessenger.Data {
    public static class DbInitializer {
        public static void Initialize(DataBaseContext context) {

            context.Database.EnsureDeleted();

            context.Database.EnsureCreated();

            // Look for any students.
            if (context.User.Any()) {
               // return;   // DB has been seeded
            }




            var User = new User[]
            {
            new User{AddressIndex = 0, PW="asd", Name="asd",Seed="IROUCEXFVHJNJHTQQEQEIROUCEXFVHJNJHTQQEQEIROUCEXFVHJNJHTQQEQEIROUCEXFVHJNJHTQQEQE9"},
            new User{AddressIndex = 0, PW="asd",Name="TestBBBBB",Seed="SRJTAZWFTFQNBDWVEOMBSRJTAZWFTFQNBDWVEOMBSRJTAZWFTFQNBDWVEOMBSRJTAZWFTFQNBDWVEOMB9"},
            new User{AddressIndex = 0, PW="asd",  Name="TestCCCCC",Seed="VURPMWUWUVBQUNJOPGDPVURPMWUWUVBQUNJOPGDPVURPMWUWUVBQUNJOPGDPVURPMWUWUVBQUNJOPGDP9"},
            };

            foreach (User s in User) {
                context.User.Add(s);
            }

            context.SaveChanges();

        }
    }
}