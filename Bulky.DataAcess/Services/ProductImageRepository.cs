using Bulky.DataAccess.Data;
using Bulky.DataAccess.Services.IServices;
using Bulky.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.DataAccess.Services
{
    public class ProductImageRepository : Repository<ProductImage>, IProductImageRepostory
    {
        private new readonly AplicationDbContext _db;
        public ProductImageRepository(AplicationDbContext db) : base(db)
        {
                _db = db;     
        }

        public void Update(ProductImage productImage)
        {
            _db.Update(productImage);
        }
    }
}