using Bulky.DataAccess.Services.IServices;
using Bulky.Models;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace ASP_MVC__bulky.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            IEnumerable<Product> productList = _unitOfWork.Product.GetAll(ingludeProperties: "Category,ProductImages");
            return View(productList);
        }

        public IActionResult Details(int productId)
        {
            ShoppingCard Card = new()
            {
                Product = _unitOfWork.Product.Get(u => u.Id == productId, ingludeProperties: "Category,ProductImages"),
                ProductId = productId,
            };

            return View(Card);
        }

        [HttpPost]
        [Authorize]
        public IActionResult Details(ShoppingCard Card)
        {
            var claimsUser = (ClaimsIdentity)User.Identity!;
            var userId = claimsUser.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            Card.ApplicationUserId = userId;
            ShoppingCard newCard= _unitOfWork.ShoppingCard.Get(u=>u.ProductId == Card.ProductId && u.ApplicationUserId == userId);
            if(newCard == null)
            {
                _unitOfWork.ShoppingCard.Add(Card);
                _unitOfWork.Save();
                HttpContext.Session.SetInt32(SD.SessionCart,_unitOfWork.ShoppingCard
                    .GetAll(u=>u.ApplicationUserId==userId).Count());
            }
            else
            {
                newCard.Count += Card.Count;
                _unitOfWork.ShoppingCard.Update(newCard);
                _unitOfWork.Save();
            }
            TempData["success"] = "Product added in basket";
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}