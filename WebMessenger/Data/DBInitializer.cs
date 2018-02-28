using WebMessenger.Models;
using System;
using System.Linq;
using Tangle.Net.Entity;
using Tangle.Net.Cryptography;

namespace WebMessenger.Data {
    public static class DbInitializer {
        public static void Initialize(DataBaseContext context) {

            context.Database.EnsureDeleted();

            context.Database.EnsureCreated();

            // Look for any students.
            if (context.User.Any()) {
                return;   // DB has been seeded
            }

            var User = new User[]
            {
            new User{AddressIndex = 0, PW="asd", Name="connA",Seed = "ABCUCEXFVHJNJHTQQEQEIROUCEXFVHJNJHTQQEQEIROUCEXFVHJNJHTQQEQEIROUCEXFVHJNJHTQQEQE9"},
            new User{AddressIndex = 0, PW="asd",Name="connB",Seed = "ABCUCEXFVHJNJHTQQEQEIROUCEXFVHJNJHTQQEQEIROUCEXFVHJNJHTQQEQEIROUCEXFVHJNJHTQQEQE9"},
            };

            foreach (User s in User) 
                context.User.Add(s);

            var Address = new AddressTable[] {
                new AddressTable{Index = 0, generatedAddress = "MRHHNDWNDHXXFSKHMXWMKHCZIEYIXRWEKPSIQENJUBSOIKPOENDRA9EZUJXVLQHPUSRIHGWTAXXQLIVKB", UserID = User[0].UserID },
                new AddressTable{Index = 1, generatedAddress = "XAXFKQO9TEEQNRYZLRSSBRPQHZ9TJTBCNLPOTSGFNYWKSLJEMASQHFBSJBHNGIWCDMDPLHUPHYIFICMWZ", UserID = User[0].UserID },
                new AddressTable{Index = 2, generatedAddress = "BXIGYMFVFMEXBGNCPPQ9LLFHNYQGBSBSTHWKKICBDZJTEQNWEV9XCTBYGRJFDVJKGIVKBJBTJXCHRRZZW", UserID = User[0].UserID },

                new AddressTable{Index = 0, generatedAddress = "MRHHNDWNDHXXFSKHMXWMKHCZIEYIXRWEKPSIQENJUBSOIKPOENDRA9EZUJXVLQHPUSRIHGWTAXXQLIVKB", UserID = User[1].UserID },
                new AddressTable{Index = 1, generatedAddress = "XAXFKQO9TEEQNRYZLRSSBRPQHZ9TJTBCNLPOTSGFNYWKSLJEMASQHFBSJBHNGIWCDMDPLHUPHYIFICMWZ", UserID = User[1].UserID },
                new AddressTable{Index = 2, generatedAddress = "BXIGYMFVFMEXBGNCPPQ9LLFHNYQGBSBSTHWKKICBDZJTEQNWEV9XCTBYGRJFDVJKGIVKBJBTJXCHRRZZW", UserID = User[1].UserID }
            };

            foreach (AddressTable s in Address)
                context.AddressTable.Add(s);

            context.SaveChanges();



        }

        public static string generateAddress(User user, int num) {

            var addressGenerator = new AddressGenerator(user.getSeed());

            return addressGenerator.GetAddress(num).ToString();

        }
    }
}