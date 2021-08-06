using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace testingpage.Models
{
    public class returnOrder
    {
        [Display(Name ="Track Id")]
        public int OrderId { get; set; }
        [Display(Name ="Order Date")]
        public DateTime OrderDate { get; set; }
    }
}
