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
    public class CompanyRepository : Repository<Company>, ICompanyRepository
    {
        public new readonly AplicationDbContext _db;
        public CompanyRepository(AplicationDbContext db):base(db)
        {
            _db = db;
        }

        public Company Update(Company company)
        {
            Company companyForUpdate = _db.Companies.FirstOrDefault(u=>u.Id == company.Id)!;
            if(companyForUpdate == null)
            {
                return company;
            }
            companyForUpdate.StreetAddress = company.StreetAddress;
            companyForUpdate.City = company.City;
            companyForUpdate.State = company.State;
            companyForUpdate.PhoneNumber = company.PhoneNumber;
            companyForUpdate.Name = company.Name;
            companyForUpdate.PostalCode = company.PostalCode;

            return companyForUpdate;
        }
    }
}
