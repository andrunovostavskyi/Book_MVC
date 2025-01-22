using Bulky_Razor_Pages_.Model;
using Microsoft.EntityFrameworkCore;

namespace Bulky_Razor_Pages_.Datas
{
    public class AplicationDbContext:DbContext
    {
        public AplicationDbContext(DbContextOptions<AplicationDbContext> options):base(options) { }
        
        public DbSet<Categories> Categories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Categories>().HasData
                (
                new Categories { Id = 1, Name = "Remark", DisplayOrder = 1 },
                new Categories { Id = 2, Name = "Eger", DisplayOrder = 2 },
                new Categories { Id = 3, Name = "Soyear", DisplayOrder = 3 }
                );
        }
    }
}
