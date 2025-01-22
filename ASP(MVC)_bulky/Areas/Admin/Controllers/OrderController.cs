using Bulky.DataAccess.Services.IServices;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using Stripe.Issuing;
using System.Security.Claims;

namespace ASP_MVC__bulky.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public OrderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork= unitOfWork;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Details(int orderId) 
        {
            OrderVM orderVm = new()
            {
                OrderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == orderId, ingludeProperties: "ApplicationUser"),
                OrderDetailsList = _unitOfWork.OrderDetail.GetAll(u => u.OrderHeaderId == orderId, ingludeProperties: "Product")
            };

            return View(orderVm);
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult UpdateOrderDetail(OrderVM order)
        {
            var orderHeaderFromDb = _unitOfWork.OrderHeader.Get(u => u.Id == order.OrderHeader.Id);
            orderHeaderFromDb.Name = order.OrderHeader.Name;
            orderHeaderFromDb.PhoneNumber = order.OrderHeader.PhoneNumber;
            orderHeaderFromDb.StreetAddress = order.OrderHeader.StreetAddress;
            orderHeaderFromDb.City = order.OrderHeader.City;
            orderHeaderFromDb.State = order.OrderHeader.State;
            orderHeaderFromDb.PostalCode = order.OrderHeader.PostalCode;
            if (!string.IsNullOrEmpty(order.OrderHeader.Carried))
            {
                orderHeaderFromDb.Carried = order.OrderHeader.Carried;
            }
            if (!string.IsNullOrEmpty(order.OrderHeader.TrackingNumber))
            {
                orderHeaderFromDb.Carried = order.OrderHeader.TrackingNumber;
            }
            _unitOfWork.OrderHeader.Update(orderHeaderFromDb);
            _unitOfWork.Save();
            TempData["Success"] = "Order Details Updated Successfully.";
            return RedirectToAction(nameof(Details), new { orderId = orderHeaderFromDb.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult StartProcessing(OrderVM orderVM)
        {
            var orderFromDb = _unitOfWork.OrderHeader.Get(u=>u.Id== orderVM.OrderHeader.Id);
            _unitOfWork.OrderHeader.UpdateStatus(orderFromDb.Id, SD.StatusInProcess, orderFromDb.PaymentStatus);
            _unitOfWork.Save();
            TempData["Success"] = "Order status updated success";
            return RedirectToAction(nameof(Details), new { orderId = orderFromDb.Id});
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + ","+SD.Role_Employee)]
        public IActionResult ShipOrder(OrderVM orderVM)
        {
            var orderFromDb = _unitOfWork.OrderHeader.Get(u => u.Id == orderVM.OrderHeader.Id);
            orderFromDb.Carried = orderVM.OrderHeader.Carried;
            orderFromDb.TrackingNumber = orderVM.OrderHeader.TrackingNumber;
            orderFromDb.ShippingDate = DateTime.UtcNow;
            orderFromDb.OrderStatus = SD.StatusShipped;
            if(orderFromDb.PaymentStatus == SD.PaymentStatusDelayedPayment)
            {
                orderFromDb.PaymentDueDate = DateTime.UtcNow.AddDays(30);
            }

            _unitOfWork.OrderHeader.Update(orderFromDb);
            _unitOfWork.Save();
            TempData["Success"] = "Order shipped successfully";
            return RedirectToAction(nameof(Details), new { orderId = orderFromDb.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult CancelOrder(OrderVM orderVM)
        {
            var orderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == orderVM.OrderHeader.Id);

            if (orderHeader.PaymentStatus == SD.PaymentStatusApproved)
            {
                var options = new RefundCreateOptions
                {
                    Reason = RefundReasons.RequestedByCustomer,
                    PaymentIntent = orderVM.OrderHeader.PaymentIntentId
                };

                var servise = new RefundService();
                Refund refund = servise.Create(options);

                _unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, SD.StatusCancelled, SD.StatusRefunded);
            }
            else
            {
                _unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, SD.StatusCancelled, SD.StatusCancelled);
            }
            _unitOfWork.Save();
            TempData["Success"] = "Order canceles successfully";
            return RedirectToAction(nameof(Details), new { orderId = orderVM.OrderHeader.Id });
        }

        [HttpPost]
        public IActionResult Detail_Pay_Now(OrderVM orderVM)
        {
            var orderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == orderVM.OrderHeader.Id);
            var orderDetailList = _unitOfWork.OrderDetail.GetAll(u => u.OrderHeaderId == orderHeader.Id, ingludeProperties:"Product");

            var domain = "https://localhost:7214/";
            var options = new Stripe.Checkout.SessionCreateOptions
            {
                SuccessUrl = domain + $"admin/order/PaymentConfirmation?orderHeaderId={orderHeader.Id}",
                CancelUrl = domain + $"admin/order/details?orderHeaderId={orderHeader.Id}",
                LineItems = new List<Stripe.Checkout.SessionLineItemOptions>(),
                Mode = "payment",
            };

            foreach (var item in orderDetailList)
            {
                double realPrice;
                if (item.Count > 100) { realPrice = item.Product.Price100; }
                else if (item.Count > 50) { realPrice = item.Product.Price50; }
                else { realPrice = item.Product.Price; }
                var sesionLineItems = new SessionLineItemOptions()
                {
                    PriceData = new SessionLineItemPriceDataOptions()
                    {
                        UnitAmount = (long)(realPrice * 100),
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions()
                        {
                            Name = item.Product.Title
                        }

                    },
                    Quantity = item.Count
                };
                options.LineItems.Add(sesionLineItems);
            }

            var service = new Stripe.Checkout.SessionService();
            Session session = service.Create(options);
            _unitOfWork.OrderHeader.UpdateStripeIntentId(orderHeader.Id, session.Id, session.PaymentIntentId);
            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }


        public IActionResult PaymentConfirmation(int orderHeaderId)
        {
            OrderHeader header = _unitOfWork.OrderHeader.Get(u => u.Id == orderHeaderId);
            if (header.PaymentStatus == SD.PaymentStatusDelayedPayment)
            {
                var service = new SessionService();
                Session session = service.Get(header.SessionId);

                if (session.Status.ToLower() == "complete")
                {
                    _unitOfWork.OrderHeader.UpdateStripeIntentId(orderHeaderId, session.Id, session.PaymentIntentId);
                    _unitOfWork.OrderHeader.UpdateStatus(orderHeaderId, header.OrderStatus, SD.PaymentStatusApproved);
                    _unitOfWork.Save();
                }

            }
            return View(orderHeaderId);
        }

        #region
        [HttpGet]
        public IActionResult GetAll(string status)
		{
			IEnumerable<OrderHeader> orders = Enumerable.Empty<OrderHeader>(); 

			if (!User.Identity.IsAuthenticated)
			{
				return Json(new { data = orders.ToList() });
			}

			if (User.IsInRole(SD.Role_Admin) || User.IsInRole(SD.Role_Employee))
			{
				orders = _unitOfWork.OrderHeader.GetAll(ingludeProperties: "ApplicationUser");
			}
			else
			{
				var claim = (ClaimsIdentity)User.Identity;
				var userId = claim.FindFirst(ClaimTypes.NameIdentifier).Value;
				orders = _unitOfWork.OrderHeader.GetAll(u => u.ApplicationUser!.Id == userId, ingludeProperties: "ApplicationUser");
			}

			switch (status)
			{
				case "pending":
					orders = orders.Where(u => u.OrderStatus == SD.StatusPending);
					break;
				case "inprocess":
					orders = orders.Where(u => u.OrderStatus == SD.StatusInProcess);
					break;
				case "completed":
					orders = orders.Where(u => u.OrderStatus == SD.StatusShipped);
					break;
				case "approved":
					orders = orders.Where(u => u.OrderStatus == SD.StatusApproved);
					break;
			}

			return Json(new { data = orders.ToList() });
		}


		#endregion
	}
}
