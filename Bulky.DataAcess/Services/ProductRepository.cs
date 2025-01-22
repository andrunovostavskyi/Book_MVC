using Bulky.DataAccess.Data;
using Bulky.DataAccess.Services.IServices;
using Bulky.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.DataAccess.Services
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        private new readonly AplicationDbContext _db;
        public ProductRepository(AplicationDbContext db) : base(db)
        {
            _db = db;
        }


        public void Update(Product product)
        {
            var objForUpdate = _db.Products.FirstOrDefault(u => u.Id == product.Id);
            objForUpdate!.Title=product.Title;
            objForUpdate.Description=product.Description;
            objForUpdate.ISBN=product.ISBN;
            objForUpdate.ListPrice=product.ListPrice;
            objForUpdate.Price=product.Price;
            objForUpdate.Price50=product.Price50;
            objForUpdate.Price100=product.Price100;
            objForUpdate.Author=product.Author;
            objForUpdate.CategoryId=product.CategoryId;
            objForUpdate.Category = product.Category;
        }
    }
}
