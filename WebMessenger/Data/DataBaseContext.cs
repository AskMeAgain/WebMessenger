using Microsoft.EntityFrameworkCore;
using WebMessenger.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebMessenger.Data {
    public class DataBaseContext : DbContext {

        public DataBaseContext(DbContextOptions<DataBaseContext> options) : base(options) { }

        public DbSet<User> User { get; set; }
        public DbSet<AddressTable> AddressTable { get; set; }

    }
}
