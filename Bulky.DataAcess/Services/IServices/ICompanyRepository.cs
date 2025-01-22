using Bulky.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.DataAccess.Services.IServices
{
    public interface ICompanyRepository:IRepository<Company>
    {
        Company Update(Company company);
    }
}
