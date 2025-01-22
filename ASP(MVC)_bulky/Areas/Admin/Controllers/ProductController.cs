using Bulky.DataAccess.Services.IServices;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ASP_MVC__bulky.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }
        
        public IActionResult Index()
        {
            List<Product> products=_unitOfWork.Product.GetAll(ingludeProperties: "Category").ToList();
            return View(products);
        }

        public IActionResult Upsert(int? id)
        {
            ProductVM productVm = new ProductVM
            {
                CategoryList = _unitOfWork.Category.GetAll()
                .Select(c => new SelectListItem
                {
                    Text = c.Name,
                    Value = c.Id.ToString(),
                }),
                Product = new Product()
            };
            if(id==null || id == 0)
            {
                return View(productVm);
            }
            else
            {
                productVm.Product = _unitOfWork.Product.Get(u => u.Id == id, ingludeProperties: "ProductImages");
                return View(productVm);
            }
        }

        [HttpPost]
        public IActionResult Upsert(ProductVM productVm, List<IFormFile?> files)
        {
            if (ModelState.IsValid)
            {
                if (productVm.Product.Id == 0)
                {
                    _unitOfWork.Product.Add(productVm.Product);
                    _unitOfWork.Save();
                }
                else
                {
                    _unitOfWork.Product.Update(productVm.Product);
                    _unitOfWork.Save();
                }


                string pathwwwroot = _webHostEnvironment.WebRootPath;
                if (files != null)
                {
                    foreach (IFormFile file in files)
                    {
                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName); 
                        string productImgPath = @"images\products\product-" + productVm.Product.Id;
                        string finalPath = Path.Combine(pathwwwroot, productImgPath);
                        if (!Directory.Exists(finalPath))
                        {
                            Directory.CreateDirectory(finalPath);
                        }

                        using (var fileStream = new FileStream(Path.Combine(finalPath, fileName), FileMode.Create))
                        {
                            file.CopyTo(fileStream);
                        }

                        ProductImage image = new()
                        {
                            ProductId = productVm.Product.Id,
                            ImgUrl = @"\" + productImgPath + @"\" + fileName,
                        };

                        if(productVm.Product.ProductImages == null)
                        {
                            productVm.Product.ProductImages = new List<ProductImage>();
                        }
                        productVm.Product.ProductImages.Add(image);
                        _unitOfWork.ProductImage.Add(image);
                    }
                    _unitOfWork.Save();

                }

                TempData["success"] = "Product added/updated success";

            }
            return RedirectToAction("Index");
        }


        public IActionResult DeleteImage(int imageId)
        {
            var imgFromDb = _unitOfWork.ProductImage.Get(u => u.Id == imageId);

            if (!string.IsNullOrEmpty(imgFromDb.ImgUrl))
            {
                var oldImagePath =
                               Path.Combine(_webHostEnvironment.WebRootPath,
                               imgFromDb.ImgUrl.TrimStart('\\'));

                if (System.IO.File.Exists(oldImagePath))
                {
                    System.IO.File.Delete(oldImagePath);
                }
            }
            _unitOfWork.ProductImage.Remove(imgFromDb);
            _unitOfWork.Save();
            TempData["Success"] = "Image was deleted success";

            return RedirectToAction(nameof(Upsert), new { id = imgFromDb.ProductId });
        }




        #region
        [HttpGet]
        public IActionResult GetAll()
        {
            List<Product> products = _unitOfWork.Product.GetAll(ingludeProperties: "Category").ToList();
            return Json(new { data = products });
        }

        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var productForDelete = _unitOfWork.Product.Get(u => u.Id == id);
            if (productForDelete == null)
            {
                return Json(new { success = false, message = "Product not found" });
            }
            _unitOfWork.Product.Remove(productForDelete);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Product was deleted successfully" });
        }

        #endregion
    }
}
