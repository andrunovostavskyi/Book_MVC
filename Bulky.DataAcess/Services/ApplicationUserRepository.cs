using Bulky.DataAccess.Data;
using Bulky.DataAccess.Services.IServices;
using Bulky.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.DataAccess.Services
{
	public class ApplicationUserRepository:Repository<ApplicationUser>, IApplicationUserRepository
	{
        private new readonly AplicationDbContext _db;
        public ApplicationUserRepository(AplicationDbContext db):base(db) 
        {
            _db = db;
        }

        public void Update(ApplicationUser applicationUser)
        {
            _db.Update(applicationUser);
        }
    }
}
