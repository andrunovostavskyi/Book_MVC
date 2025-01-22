using Bulky.DataAccess.Data;
using Bulky.DataAccess.Services.IServices;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace ASP_MVC__bulky.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class UserController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IUnitOfWork _unitOfWork;
        public UserController(AplicationDbContext db,UserManager<IdentityUser> userManager, IUnitOfWork unitOfWork, RoleManager<IdentityRole> roleManager)
        {
            _unitOfWork = unitOfWork;
            _roleManager = roleManager;
            _userManager = userManager;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult RoleManagment(string userId)
        {

            RoleManagerVM RoleVM = new RoleManagerVM()
            {
                ApplicationUser = _unitOfWork.ApplicationUser.Get(u => u.Id == userId, ingludeProperties: "Company"),
                RoleList = _roleManager.Roles.Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Name
                }),
                CompanyList = _unitOfWork.Company.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                }),
            };

            RoleVM.ApplicationUser.Role = _userManager.GetRolesAsync(_unitOfWork.ApplicationUser.Get(u => u.Id == userId))
                    .GetAwaiter().GetResult().FirstOrDefault()!;
            return View(RoleVM);
        }

        [HttpPost]
        public IActionResult RoleManagment(RoleManagerVM obj)
        {
            string oldRole = _userManager.GetRolesAsync(_unitOfWork.ApplicationUser.Get(u=>u.Id == obj.ApplicationUser.Id))
                .GetAwaiter().GetResult().FirstOrDefault()!;
            var aplicationUser = _unitOfWork.ApplicationUser.Get(u => u.Id == obj.ApplicationUser.Id);
            if (obj.ApplicationUser.CompanyID != null)
            {
                aplicationUser!.CompanyID = obj.ApplicationUser.CompanyID;
            }
            if (!(obj.ApplicationUser.Role == oldRole)){
                if(oldRole == SD.Role_Company){
                    aplicationUser!.CompanyID = null;
                }
                _userManager.RemoveFromRoleAsync(aplicationUser!, oldRole!).GetAwaiter().GetResult().ToString();
                _userManager.AddToRoleAsync(aplicationUser!, obj.ApplicationUser.Role).GetAwaiter().GetResult();
            }
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }




        #region
        [HttpGet]
        public IActionResult GetAll()
        {
            List<ApplicationUser> users = _unitOfWork.ApplicationUser.GetAll(ingludeProperties:"Company").ToList();
            var claimsIdentity = (ClaimsIdentity)User.Identity!;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            users.Remove(users.FirstOrDefault(u => u.Id == userId)!);

            foreach (var user in users)
            {
                user.Role = _userManager.GetRolesAsync(user).GetAwaiter().GetResult().FirstOrDefault();
                if (user.Company == null)
                {
                    user.Company = new Company()
                    {
                        Name = ""
                    };
                }
            }
            return Json(new { data = users });
        }


        [HttpPost]
        public IActionResult LockUnlock([FromBody] string id)
        {
            var user = _unitOfWork.ApplicationUser.Get(u => u.Id == id);
            if (user == null) {
                return Json(new { success = true, message = "User wasn't exist" });
            }
            if (user.LockoutEnd is not null)
            {
                user.LockoutEnd = null;
            }
            else
            {
                user.LockoutEnd = DateTime.UtcNow.AddYears(1000);
            }
            _unitOfWork.Save();
            return Json(new { success = true, message = "Operation success" });
        }

        #endregion
    }
}
