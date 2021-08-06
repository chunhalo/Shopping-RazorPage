using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using NLog;
using testingpage.Models;

namespace testingpage.Pages.ProductPage
{
    public class CreateProductModel : PageModel
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        [BindProperty]
        public ProductAddRequest product { get; set; }
        [BindProperty]
        public List<ProductStatus> productStatus { get; set; }
        [TempData]
        public string MessageKey { get; set; }
        public async Task<IActionResult> OnGet()
        {
            try
            {
                var gettoken = HttpContext.Request.Cookies["JWTToken"];
                var user = HttpContext.Request.Cookies["User"];

                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", gettoken);

                    using (var response = await httpClient.GetAsync("https://localhost:44335/api/products/GetStatus"))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            var msg = new LogEventInfo(LogLevel.Info, logger.Name, "Admin access Admin Create Product Page");
                            msg.Properties.Add("user", user);
                            logger.Log(msg);
                            string apiResponse = await response.Content.ReadAsStringAsync();
                            List<ProductStatus> productStatuses = JsonConvert.DeserializeObject<List<ProductStatus>>(apiResponse);
                            productStatus = productStatuses;


                        }
                        else if ((int)response.StatusCode == 401)
                        {
                            //return Unauthorized()
                            var msg = new LogEventInfo(LogLevel.Info, logger.Name, "Unauthorized user trying to access Admin Create Product Page");
                            logger.Log(msg);
                            MessageKey = "You are required to login to acess this page";
                            return RedirectToPage("/LoginPage/Login");

                        }
                        else if ((int)response.StatusCode == 403)
                        {
                            var msg = new LogEventInfo(LogLevel.Info, logger.Name, "User with no privilege trying to access Admin Create Product Page");
                            msg.Properties.Add("user", user);
                            return RedirectToPage("/PrivilegeError");
                        }
                        return Page();

                    }
                }
            }catch(Exception e)
            {
                var user = HttpContext.Request.Cookies["User"];
                var msg = new LogEventInfo(NLog.LogLevel.Error, logger.Name, "Exception occur here");
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
                if (!ModelState.IsValid)
                {
                    return Page();
                }
                var gettoken = HttpContext.Request.Cookies["JWTToken"];
                var user = HttpContext.Request.Cookies["User"];

                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", gettoken);
                    byte[] data;
                    var content = new MultipartFormDataContent();
                    //product.Price = Math.Round(product.Price * 100.0) / 100.0;


                    content.Add(new StringContent(product.ProductName), "productName");
                    content.Add(new StringContent(product.Price.ToString()), "price");
                    content.Add(new StringContent(product.Description), "description");
                    content.Add(new StringContent(product.Stock.ToString()), "Stock");
                    content.Add(new StringContent(product.Status.ToString()), "Status");

                    if (product.Image != null)
                    {
                        using (var br = new BinaryReader(product.Image.OpenReadStream()))
                        {
                            data = br.ReadBytes((int)product.Image.OpenReadStream().Length);
                        }
                        ByteArrayContent bytes = new ByteArrayContent(data);
                        content.Add(bytes, "Image", product.Image.FileName);
                    }
                    await httpClient.PostAsync("https://localhost:44335/api/products", content);

                }
                var msg = new LogEventInfo(LogLevel.Info, logger.Name, "Product with name," +product.ProductName+" has been created successfully");
                msg.Properties.Add("user", user);
                logger.Log(msg);
                MessageKey = "Product has been created successfully";
                return RedirectToPage("AdminHome");

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
    }
}
