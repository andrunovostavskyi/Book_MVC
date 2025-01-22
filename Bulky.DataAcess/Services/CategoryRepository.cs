using Bulky.DataAccess.Data;
using Bulky.DataAccess.Services.IServices;
using Bulky.Models;

namespace Bulky.DataAccess.Services
{
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        public new readonly AplicationDbContext _db;
        public CategoryRepository(AplicationDbContext db):base(db)
        {
            _db = db;
        }

        public void Update(Category category)
        {
            _db.Categories.Update(category);
        }
    }
}
