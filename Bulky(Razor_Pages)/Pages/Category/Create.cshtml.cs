using Bulky_Razor_Pages_.Datas;
using Bulky_Razor_Pages_.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Bulky_Razor_Pages_.Pages.Category
{
    public class CreateModel : PageModel
    {
        private readonly AplicationDbContext _db;

        [BindProperty]
        public Categories Category { get; set; }= new Categories();
        public CreateModel(AplicationDbContext db)
        {
            _db = db;
        }

        public void OnGet()
        {
        }

        public IActionResult OnPost() 
        {
            if (ModelState.IsValid)
            {
                _db.Categories.Add(Category);
                _db.SaveChanges();
                TempData["success"] = "Category added success";
                return RedirectToPage("Index");
            }
            return Page();
        }
    }
}
