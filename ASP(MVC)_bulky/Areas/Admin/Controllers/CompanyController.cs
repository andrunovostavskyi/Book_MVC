using Bulky.DataAccess.Services.IServices;
using Bulky.Models;
using Microsoft.AspNetCore.JsonPatch.Internal;
using Microsoft.AspNetCore.Mvc;

namespace ASP_MVC__bulky.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CompanyController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public CompanyController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            List<Company> companies = _unitOfWork.Company.GetAll().ToList();
            return View(companies);
        }

        public IActionResult Upsert(int? id) 
        {
            if(id==null || id == 0)
            {
                return View(new Company());
            }
            else
            {
                return View(_unitOfWork.Company.Get(u=>u.Id==id));
            }
        }

        [HttpPost]
        public IActionResult Upsert(Company company)
        {
            if(ModelState.IsValid)
            {
                if (company.Id == 0)
                {
                    _unitOfWork.Company.Add(company);
                    _unitOfWork.Save();
                    TempData["success"] = "Company added success";
                    return RedirectToAction("Index");
                }
                else
                {
                    _unitOfWork.Company.Update(company);
                    _unitOfWork.Save();
                    TempData["success"] = "Company updated success";
                    return RedirectToAction("Index");
                }
            }
            return View(company);
        }

        public IActionResult Delete(int? id)
        {
            return View(_unitOfWork.Company.Get(u=>u.Id==id));
        }

        #region
        public IActionResult GetAll()
        {
            List<Company> companies=_unitOfWork.Company.GetAll().ToList();
            return Json(new {data = companies });
        }

        [HttpDelete]
        public IActionResult Delete(int id)
        {
            Company companyForDelete = _unitOfWork.Company.Get(u => u.Id == id);

            if (companyForDelete == null)
            {
                return Json(new { success = false, message = "Product not found" });
            }
            _unitOfWork.Company.Remove(companyForDelete);
            _unitOfWork.Save();
            TempData["success"] = "Company deleted success";
            return Json(new { success = true, message = "Product was deleted successfully" });
        }

        #endregion
    }
}
