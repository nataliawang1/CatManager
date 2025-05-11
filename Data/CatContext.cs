using Microsoft.EntityFrameworkCore;
using CatManager.Models;

namespace CatManager.Data
{
    public class CatContext : DbContext
    {
        public CatContext(DbContextOptions<CatContext> options)
            : base(options)
        {
        }

        public DbSet<Cat> Cats { get; set; }
    }
}
