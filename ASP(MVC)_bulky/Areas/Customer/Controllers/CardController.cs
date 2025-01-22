using Bulky.DataAccess.Services.IServices;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using System.Security.Claims;

namespace ASP_MVC__bulky.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class CardController:Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        [BindProperty]
        public ShoppingCardVM? cardVm { get; set; }

        public CardController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [Authorize]
        public IActionResult Index()
        {
            var claimUser = (ClaimsIdentity)User.Identity!;
            var userId = claimUser.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            ShoppingCardVM cardVM = new()
            {
                ShoppingCardList = _unitOfWork.ShoppingCard.GetAll(u=> u.ApplicationUserId == userId,
                ingludeProperties:"Product"),
                OrderHeader = new()
            };
            IEnumerable<ProductImage> images = _unitOfWork.ProductImage.GetAll();

            foreach (var item in cardVM.ShoppingCardList)
            {
                item.Product.ProductImages=images.Where(u=>u.ProductId==item.ProductId).ToList();
                double price = GetPriceForProduct(item);
                cardVM.OrderHeader.OrderTotal += price*item.Count;
            }
            return View(cardVM);
        }

        public IActionResult Increment(int cartId)
        {
            ShoppingCard card = _unitOfWork.ShoppingCard.Get(u => u.Id == cartId);
            card.Count++;
            _unitOfWork.ShoppingCard.Update(card);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }


        public IActionResult Decrement(int cartId)
        {
            ShoppingCard card = _unitOfWork.ShoppingCard.Get(u => u.Id == cartId);
            
            if (card.Count <= 1)
            {
                _unitOfWork.ShoppingCard.Remove(card);
                _unitOfWork.Save();
                HttpContext.Session.SetInt32(SD.SessionCart, _unitOfWork.ShoppingCard
                    .GetAll(u=>u.ApplicationUserId==card.ApplicationUserId).Count());
            }
            else
            {
                card.Count--;
                _unitOfWork.ShoppingCard.Update(card);
                _unitOfWork.Save();
            }
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Summary()
        {
            var claimUser = (ClaimsIdentity)User.Identity!;
            var userId = claimUser.FindFirst(ClaimTypes.NameIdentifier)!.Value;
			cardVm = new()
            {
                ShoppingCardList = _unitOfWork.ShoppingCard.GetAll(u => u.ApplicationUserId == userId,
                ingludeProperties: "Product"),
                OrderHeader = new()
            };
			cardVm.OrderHeader.ApplicationUser=_unitOfWork.ApplicationUser.Get(u=>u.Id == userId);

			cardVm.OrderHeader.Name = cardVm.OrderHeader.ApplicationUser.Name;
			cardVm.OrderHeader.StreetAddress = cardVm.OrderHeader.ApplicationUser.StreetAddress;
			cardVm.OrderHeader.State = cardVm.OrderHeader.ApplicationUser.State;
			cardVm.OrderHeader.PhoneNumber = cardVm.OrderHeader.ApplicationUser.PhoneNumber;
			cardVm.OrderHeader.PostalCode = cardVm.OrderHeader.ApplicationUser.PostalCode;
			cardVm.OrderHeader.City = cardVm.OrderHeader.ApplicationUser.City;

			foreach (var item in cardVm.ShoppingCardList)
            {
                double priceCat = GetPriceForProduct(item);
				cardVm.OrderHeader.OrderTotal += priceCat*item.Count;
            }

			return View(cardVm);
        }

        [HttpPost]
        [ActionName("Summary")]
		public IActionResult SummaryPost()
		{
			var claimUser = (ClaimsIdentity)User.Identity!;
			var userId = claimUser.FindFirst(ClaimTypes.NameIdentifier)!.Value;
			cardVm.ShoppingCardList=_unitOfWork.ShoppingCard.GetAll(u=>u.ApplicationUserId == userId,
                ingludeProperties:"Product");
            cardVm.OrderHeader.ApplicationUserId = userId;
			ApplicationUser applicationUser = _unitOfWork.ApplicationUser.Get(u => u.Id == userId);
            cardVm.OrderHeader.OrderDate = System.DateTime.UtcNow;

			foreach (var item in cardVm.ShoppingCardList)
			{
				double priceCat = GetPriceForProduct(item);
				cardVm.OrderHeader.OrderTotal += priceCat * item.Count;
			}

            if(applicationUser.CompanyID.GetValueOrDefault() == 0)
            {
                cardVm.OrderHeader.OrderStatus = SD.StatusPending;
                cardVm.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
            }
            else
            {
                cardVm.OrderHeader.OrderStatus = SD.StatusApproved;
                cardVm.OrderHeader.PaymentStatus = SD.PaymentStatusDelayedPayment;
            }
            _unitOfWork.OrderHeader.Add(cardVm.OrderHeader);
            _unitOfWork.Save();

            foreach(var item in cardVm.ShoppingCardList)
            {
                OrderDetail orderDetail = new()
                {
                    OrderHeaderId = cardVm.OrderHeader.Id,
                    Count = item.Count,
                    Price = GetPriceForProduct(item),
                    ProductId = item.ProductId,
                };
				_unitOfWork.OrderDetail.Add(orderDetail);
				_unitOfWork.Save();
			}

            if(applicationUser.CompanyID.GetValueOrDefault() == 0)
            {
                var domain = "https://localhost:7214/";
                var options = new Stripe.Checkout.SessionCreateOptions
                {
                    SuccessUrl = domain + $"customer/card/OrderConfirmation?id={cardVm.OrderHeader.Id}",
                    CancelUrl = domain + "customer/card/index",
                    LineItems = new List<Stripe.Checkout.SessionLineItemOptions>(),
                    Mode = "payment",
                };

                foreach(var item in cardVm.ShoppingCardList)
                {
                    double realPrice;
                    if (item.Count > 100) { realPrice = item.Product.Price100; }
                    else if(item.Count>50) { realPrice = item.Product.Price50; }
                    else { realPrice = item.Product.Price;}
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
                _unitOfWork.OrderHeader.UpdateStripeIntentId(cardVm.OrderHeader.Id, session.Id, session.PaymentIntentId);
                Response.Headers.Add("Location", session.Url);
                return new StatusCodeResult(303);
            }

            return RedirectToAction("OrderConfirmation", new { id = cardVm.OrderHeader.Id });
        }

        public IActionResult OrderConfirmation(int id)
        {
            OrderHeader header = _unitOfWork.OrderHeader.Get(u=>u.Id == id, ingludeProperties: "ApplicationUser");
            if (header.PaymentStatus != SD.PaymentStatusDelayedPayment)
            {
                var service = new SessionService();
                Session session = service.Get(header.SessionId);

                if(session.Status.ToLower() == "complete")
                {
                    _unitOfWork.OrderHeader.UpdateStripeIntentId(id, session.Id, session.PaymentIntentId);
                    _unitOfWork.OrderHeader.UpdateStatus(id, SD.StatusApproved, SD.PaymentStatusApproved);
                    _unitOfWork.Save();
                }

            }
            List<ShoppingCard> ShoppingCards = _unitOfWork.ShoppingCard
                .GetAll(u=>u.ApplicationUserId == header.ApplicationUserId).ToList();
            _unitOfWork.ShoppingCard.RemoveRange(ShoppingCards);
            _unitOfWork.Save();

            return View(id);
        }

		public IActionResult Remove(int cartId)
        {
            ShoppingCard card = _unitOfWork.ShoppingCard.Get(u => u.Id == cartId);
            if(card == null)
            {
                TempData["Error"] = "Something go wrong";
                return RedirectToAction(nameof(Index));
            }
            _unitOfWork.ShoppingCard.Remove(card);
            _unitOfWork.Save();
            HttpContext.Session.SetInt32(SD.SessionCart, _unitOfWork.ShoppingCard
                .GetAll(u => u.ApplicationUserId == card.ApplicationUserId).Count());
            return RedirectToAction(nameof(Index));
        }

        private double GetPriceForProduct(ShoppingCard cart)
        {
            double endPrice = 0;
            if (cart.Count < 50)
            {
                endPrice = cart.Product!.Price;
            }
            else if( cart.Count < 100)
            {
                endPrice = cart.Product!.Price50;
            }
            else if (cart.Count >= 100)
            {
                endPrice = cart.Product!.Price100;
            }
            return endPrice;
        }

    }
}
