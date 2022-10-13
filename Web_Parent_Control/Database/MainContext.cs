using Microsoft.EntityFrameworkCore;
using Web_Parent_Control.Models;

namespace Web_Parent_Control.Database
{
    public class MainContext : DbContext
    {
        public DbSet<Site> Sites { get; set; }
        public DbSet<File> Files { get; set; }

        public MainContext(DbContextOptions options) : base(options)
        {
        }
    }
}
