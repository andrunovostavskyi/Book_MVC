using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.Models.ViewModels
{
    public class ShoppingCardVM
    {
        public IEnumerable<ShoppingCard>? ShoppingCardList { get; set; }
        public OrderHeader? OrderHeader { get; set; }
    }
}
