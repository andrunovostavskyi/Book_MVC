using Bulky_Razor_Pages_.Datas;
using Bulky_Razor_Pages_.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Bulky_Razor_Pages_.Pages.Category
{

    public class IndexModel : PageModel
    {
        private readonly AplicationDbContext _db;
        public List<Categories> categories=new List<Categories>();
        public IndexModel(AplicationDbContext db)
        {
            _db= db;
        }

        public void OnGet()
        {
            categories=_db.Categories.ToList();
        }
    }
}
