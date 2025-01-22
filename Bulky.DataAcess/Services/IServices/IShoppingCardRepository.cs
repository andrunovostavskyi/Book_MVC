using Bulky.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.DataAccess.Services.IServices
{
    public interface IShoppingCardRepository:IRepository<ShoppingCard>
    {
        void Update(ShoppingCard shoppingCart);
    }
}
