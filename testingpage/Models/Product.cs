using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace testingpage.Models
{
    public class Product
    {
        public int ProductId { get; set; }
        [MaxLength(50)]
        [Display(Name = "Product Name")]
        [Required(ErrorMessage = "Product Name is required")]
        public string ProductName { get; set; }
        [DisplayFormat(DataFormatString = "{0:c}")]
        [Required(ErrorMessage = "Price is required")]
        [RegularExpression(@"\d+(\.\d{1,2})?", ErrorMessage = "Wrong format for price")]
        public decimal Price { get; set; }
        [Required(ErrorMessage = "Description is required")]
        [MaxLength(250)]
        public string Description { get; set; }
        [Required(ErrorMessage = "Image is required")]
        public string Image { get; set; }
        [Required(ErrorMessage = "Stock is required")]
        [Range(0, 10000000, ErrorMessage = "Value must be between 0 to 10000000")]
        public int Stock { get; set; }
        [Required(ErrorMessage = "Status is required")]
        public int Status { get; set; }
    }

    public class ProductAddRequest
    {
        [MaxLength(50)]
        [Display(Name = "Product Name")]
        [Required(ErrorMessage = "Product Name is required")]
        public string ProductName { get; set; }
        [Required(ErrorMessage = "Price is required")]
        [RegularExpression(@"\d+(\.\d{1,2})?", ErrorMessage = "Wrong format for price")]
        public decimal Price { get; set; }
        [Required(ErrorMessage = "Description is required")]
        [MaxLength(250)]
        public string Description { get; set; }
        [Required(ErrorMessage = "Image is required")]
        public IFormFile Image { get; set; }

        [Required(ErrorMessage = "Stock is required")]
        [Range(1, 10000000, ErrorMessage = "Value must be between 1 to 10000000")]
        public int Stock { get; set; }
        [Required(ErrorMessage = "Status is required")]
        public int Status { get; set; }
    }

    public class ProductIdOnly
    {
        public int productId { get; set; }
    }

    public class ProductWithStatusName
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        [DisplayFormat(DataFormatString = "{0:c}")]
        public decimal Price { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }
        public int Stock { get; set; }
        public string Status { get; set; }
    }

    //public class ProductUpdateModel
    //{
    //    public int ProductId { get; set; }
    //    public string ProductName { get; set; }
    //    public double Price { get; set; }
    //    public string Description { get; set; }
    //    public IFormFile Image { get; set; }
    //}
}
