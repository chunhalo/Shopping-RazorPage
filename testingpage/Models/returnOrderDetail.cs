using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace testingpage.Models
{
    public class returnOrderDetail
    {
        [Display(Name ="Track Id")]
        public int orderId { get; set; }
        public Product product { get; set; }
        public int quantity { get; set; }
    }
}
