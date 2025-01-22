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
    public class ShoppingCardRepository : Repository<ShoppingCard>, IShoppingCardRepository
    {
        private new readonly AplicationDbContext _db;
        public ShoppingCardRepository(AplicationDbContext db) : base(db)
        {
            _db= db;
        }
        public void Update(ShoppingCard shoppingCard)
        {
            _db.ShoppingCards.Update(shoppingCard);
        }
    }
}
