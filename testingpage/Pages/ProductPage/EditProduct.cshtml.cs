using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using NLog;
using testingpage.Models;

namespace testingpage.Pages.ProductPage
{
    public class EditProductModel : PageModel
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        [BindProperty]
        public Product product { get; set; }
        [BindProperty]
        public List<ProductStatus> productStatus { get; set; }
        [TempData]
        public string MessageKey { get; set; }

        public IFormFile GetImage { get; set; }


        public async Task<IActionResult> OnGet(int id)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Page();
                }
                var gettoken = HttpContext.Request.Cookies["JWTToken"];
                var user = HttpContext.Request.Cookies["User"];
                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", gettoken);
                    using (var response = await httpClient.GetAsync("https://localhost:44335/api/products/GetStatus"))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            string apiResponse1 = await response.Content.ReadAsStringAsync();
                            List<ProductStatus> productStatuses = JsonConvert.DeserializeObject<List<ProductStatus>>(apiResponse1);
                            productStatus = productStatuses;
                            using (var response2 = await httpClient.GetAsync("https://localhost:44335/api/Products/" + id))
                            {
                                if (response2.IsSuccessStatusCode)
                                {
                                    var msg = new LogEventInfo(LogLevel.Info, logger.Name, "Admin access Edit Product Page with id, "+id);
                                    msg.Properties.Add("user", user);
                                    logger.Log(msg);
                                    string apiResponse = await response2.Content.ReadAsStringAsync();
                                    Product jsonProduct = JsonConvert.DeserializeObject<Product>(apiResponse);
                                    if (jsonProduct == null)
                                    {
                                        product = new Product();
                                    }
                                    else
                                    {
                                        product = jsonProduct;
                                    }
                                }
                                else if ((int)response2.StatusCode == 404)
                                {
                                    var msg = new LogEventInfo(LogLevel.Info, logger.Name, "Product with id, "+id+" not found in Admin Edit Product Page");
                                    msg.Properties.Add("user", user);
                                    logger.Log(msg);
                                    ViewData["NotFound"] = "Item not found";
                                }
                            }
                        }
                        else if ((int)response.StatusCode == 401)
                        {
                            var msg = new LogEventInfo(LogLevel.Info, logger.Name, "Unauthorized user trying to access Admin Edit Product Page");
                            logger.Log(msg);
                            MessageKey = "You are required to login to acess this page";
                            return RedirectToPage("/LoginPage/Login");
                        }
                        else if ((int)response.StatusCode == 403)
                        {
                            var msg = new LogEventInfo(LogLevel.Info, logger.Name, "User with no privilege trying to access Admin Edit Product Page");
                            msg.Properties.Add("user", user);
                            logger.Log(msg);
                            return RedirectToPage("/PrivilegeError");
                        }

                    }

                }
                return Page();
            }catch(Exception e)
            {
                var user = HttpContext.Request.Cookies["User"];
                var msg = new LogEventInfo(LogLevel.Error, logger.Name, "Exception occur here");
                msg.Properties.Add("user", user);
                msg.Exception = e;
                logger.Log(msg);
                return RedirectToPage("/AdminError");
            }
        }

        public async Task<IActionResult> OnPost()
        {
            try
            {
                if (product.Stock == 0 && product.Status == 1)
                {
                    MessageKey = "0 quantity of stock cannot set to active";
                    return RedirectToAction("OnGet", new { id = product.ProductId });
                }
                var gettoken = HttpContext.Request.Cookies["JWTToken"];
                var user = HttpContext.Request.Cookies["User"];
                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", gettoken);
                    byte[] data;
                    // product.Price = Math.Round(product.Price * 100.0) / 100.0;
                    var content = new MultipartFormDataContent();
                    content.Add(new StringContent(product.ProductId.ToString()), "productId");
                    content.Add(new StringContent(product.ProductName), "productName");
                    content.Add(new StringContent(product.Price.ToString()), "price");
                    content.Add(new StringContent(product.Description), "description");
                    content.Add(new StringContent(product.Image), "Image");
                    content.Add(new StringContent(product.Stock.ToString()), "Stock");
                    content.Add(new StringContent(product.Status.ToString()), "Status");
                    if (GetImage != null)
                    {

                        using (var br = new BinaryReader(GetImage.OpenReadStream()))
                        {
                            data = br.ReadBytes((int)GetImage.OpenReadStream().Length);
                        }
                        ByteArrayContent bytes = new ByteArrayContent(data);
                        content.Add(bytes, "ImageFile", GetImage.FileName);
                    }
                    await httpClient.PutAsync("https://localhost:44335/api/products/" + product.ProductId, content);

                }
                var msg = new LogEventInfo(LogLevel.Info, logger.Name, "Admin updated the product with product name, "+product.ProductName+" successfully");
                msg.Properties.Add("user", user);
                logger.Log(msg);
                MessageKey = "Product has been edited successfully";
                return RedirectToPage("AdminHome");

            }catch (Exception e)
            {
                var user = HttpContext.Request.Cookies["User"];
                var msg = new LogEventInfo(LogLevel.Error, logger.Name, "Exception occur here");
                msg.Properties.Add("user", user);
                msg.Exception = e;
                logger.Log(msg);
                return RedirectToPage("/AdminError");
            }
        }
    }
}
