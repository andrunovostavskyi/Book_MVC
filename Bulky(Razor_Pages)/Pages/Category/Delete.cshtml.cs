using Bulky_Razor_Pages_.Datas;
using Bulky_Razor_Pages_.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Bulky_Razor_Pages_.Pages.Category
{
    public class DeleteModel : PageModel
    {
        private readonly AplicationDbContext _db;

        [BindProperty]
        public Categories CategoryForDelete { get; set; }=new Categories();

        public DeleteModel(AplicationDbContext db)
        {
            _db = db;
        }

        public void OnGet(int? id)
        {
            if (id != 0 && id!=null)
            {
                CategoryForDelete = _db.Categories.FirstOrDefault(u => u.Id == id)!;
            }
        }

        public IActionResult OnPost()
        {
            _db.Categories.Remove(CategoryForDelete);
            _db.SaveChanges();
            TempData["success"] = "Category deleted success";
            return RedirectToPage("Index");
        }
    }
}
