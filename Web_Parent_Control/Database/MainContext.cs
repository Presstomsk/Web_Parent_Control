using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Web_Parent_Control.Models;

namespace Web_Parent_Control.Database
{
    public class MainContext : DbContext
    {
        public DbSet<SiteModel> Sites { get; set; }
        public DbSet<FileModel> Files { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<BlockedItem> BlockedItems { get; set; }

        public MainContext() 
        {
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            
            optionsBuilder.UseSqlite(new ConfigurationBuilder().AddJsonFile("appsettings.json").Build()["ConnectionString"]);
          
        }
    }
}
