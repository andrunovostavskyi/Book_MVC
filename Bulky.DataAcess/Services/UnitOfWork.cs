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
    public class UnitOfWork : IUnitOfWork
    {
        public ICategoryRepository Category { get; private set; }
        public IProductRepository Product { get; private set; }
        public ICompanyRepository Company { get; private set; }
        public IShoppingCardRepository ShoppingCard { get; private set; }
        public IApplicationUserRepository ApplicationUser { get; private set; }
        public IOrderHeaderRepository OrderHeader { get; private set; }
        public IOrderDetailRepository OrderDetail { get; private set; }
        public IProductImageRepostory ProductImage { get; private set; }
        public readonly AplicationDbContext _db;
        public UnitOfWork(AplicationDbContext db)
        {
            _db = db;
            ProductImage = new ProductImageRepository(_db);
            Company = new CompanyRepository(_db);
            Category = new CategoryRepository(_db);
            Product = new ProductRepository(_db);
            ShoppingCard = new ShoppingCardRepository(_db);
            OrderHeader = new OrderHeaderRepository(_db);
            OrderDetail = new OrderDetailRepository(_db);
            ApplicationUser = new ApplicationUserRepository(_db);
        }

        public void Save()
        {
            _db.SaveChanges();
        }
    }
}
