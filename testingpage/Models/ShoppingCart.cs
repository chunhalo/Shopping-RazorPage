using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace testingpage.Models
{
    public class ShoppingCart
    {
        public int productId { get; set; }
        public int quantity { get; set; }

    }

    //public class GetShoppingCart
    //{
        
    //    public Product product { get; set; }
    //    [Display(Name = "Quantity")]
    //    public int quantity { get; set; }
    //    public bool IsSelected { get; set; }
    //}

    public class checkboxShoppingCart
    {
        public Product product { get; set; }
        [Display(Name = "Quantity")]
        public int quantity { get; set; }
        public bool IsSelected { get; set; }
    }
}
