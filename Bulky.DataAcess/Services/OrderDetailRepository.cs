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
    public class OrderDetailRepository : Repository<OrderDetail>, IOrderDetailRepository
    {
        private new readonly AplicationDbContext _db;
        public OrderDetailRepository(AplicationDbContext db):base(db) 
        {
            _db = db;
        }

        public void Update(OrderDetail orderDetail)
        {
            _db.OrderDetails.Update(orderDetail);   
        }
    }
}
