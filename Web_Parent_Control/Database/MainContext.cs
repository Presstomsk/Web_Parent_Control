using Microsoft.EntityFrameworkCore;
using Web_Parent_Control.Models;

namespace Web_Parent_Control.Database
{
    public class MainContext : DbContext
    {
        public DbSet<SiteModel> Sites { get; set; }
        public DbSet<FileModel> Files { get; set; }

        public MainContext(DbContextOptions options) : base(options)
        {
        }
    }
}
