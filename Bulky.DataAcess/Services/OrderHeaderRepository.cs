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
    internal class OrderHeaderRepository : Repository<OrderHeader>, IOrderHeaderRepository
    {
        private new readonly AplicationDbContext _db;
        public OrderHeaderRepository(AplicationDbContext db):base(db)
        {
            _db = db;
        }
        public void Update(OrderHeader orderHeader)
        {
            _db.OrderHeaders.Update(orderHeader);
        }

        public void UpdateStatus(int id, string orderStatus, string? paymentStatus)
        {
            var itemUpdate = _db.OrderHeaders.FirstOrDefault(u=> u.Id==id);
            if (itemUpdate != null)
            {
                itemUpdate.OrderStatus = orderStatus;
                if (!string.IsNullOrEmpty(orderStatus))
                {
                    itemUpdate.PaymentStatus = paymentStatus;
                }
            }

            _db.SaveChanges();
        }

        public void UpdateStripeIntentId(int id, string sessioId, string paymentIntentId)
        {
            var itemUpdate = _db.OrderHeaders.FirstOrDefault(u => u.Id == id);

            if (!string.IsNullOrEmpty(sessioId))
            {
                itemUpdate.SessionId = sessioId;
            }
            if (!string.IsNullOrEmpty(paymentIntentId))
            {
                itemUpdate.PaymentIntentId = paymentIntentId;
                itemUpdate.PaymentDate = DateTime.UtcNow;
            }
            _db.SaveChanges();
        }
    }
}
