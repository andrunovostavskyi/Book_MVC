using Bulky_Razor_Pages_.Datas;
using Bulky_Razor_Pages_.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Bulky_Razor_Pages_.Pages.Category
{
    public class EditModel : PageModel
    {
        private readonly AplicationDbContext _db;
        [BindProperty]
        public Categories CategoryForUpdate { get; set; }

        public EditModel(AplicationDbContext db)
        {
            _db = db;
        }

        public void OnGet(int? id)
        {
            if (id != 0 && id!= null)
            {
                CategoryForUpdate = _db.Categories.FirstOrDefault(u => u.Id == id)!;
            }
        }

        public IActionResult OnPost()
        {
            if (ModelState.IsValid)
            {
                _db.Categories.Update(CategoryForUpdate);
                _db.SaveChanges();
                TempData["success"] = "Category updated success";
                return RedirectToPage("Index");
            }
            return Page();
        }
    }
}
