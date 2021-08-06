using System;
using System.Collections.Generic;
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
    public class individualProductModel : PageModel
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        [BindProperty]
        public Product product { get; set; }
        [TempData]
        public string MessageKey { get; set; }

        public async Task<IActionResult> OnGet(int id)
        {
            try
            {
                var gettoken = HttpContext.Request.Cookies["JWTToken"];
                var user = HttpContext.Request.Cookies["User"];
                using (var httpClient = new HttpClient())
                {

                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", gettoken);
                    using (var response = await httpClient.GetAsync("https://localhost:44335/api/Products/" + id))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            var msg = new LogEventInfo(LogLevel.Info, logger.Name, "User access Individual Product Page with id, "+id);
                            msg.Properties.Add("user", user);
                            logger.Log(msg);
                            string apiResponse = await response.Content.ReadAsStringAsync();
                            Product jsonProduct = JsonConvert.DeserializeObject<Product>(apiResponse);

                            product = jsonProduct;
                        }
                        else if ((int)response.StatusCode == 404)
                        {
                            var msg = new LogEventInfo(LogLevel.Info, logger.Name, "Product with id, " + id + " not found in Individual Product Page");
                            msg.Properties.Add("user", user);
                            logger.Log(msg);
                            ViewData["NotFound"] = "Item not found";
                        }
                        else if((int)response.StatusCode == 401)
                        {
                            //Unauthorize
                            var msg = new LogEventInfo(LogLevel.Info, logger.Name, "Unauthorized user trying to access User Individual Product Page");
                            logger.Log(msg);
                            MessageKey = "You are required to login to acess this page";
                            return RedirectToPage("/LoginPage/Login");
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
                return RedirectToPage("/UserError");
            }
        }

        public async Task<IActionResult> OnPost()
        {
            try
            {
                var qty = Request.Form["quantity"];
                var gettoken = HttpContext.Request.Cookies["JWTToken"];
                var user = HttpContext.Request.Cookies["User"];

                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", gettoken);
                    var content = new MultipartFormDataContent();

                    content.Add(new StringContent(product.ProductId.ToString()), "productId");
                    content.Add(new StringContent(qty), "Quantity");

                    await httpClient.PostAsync("https://localhost:44335/api/ShoppingCart", content);
                    var msg = new LogEventInfo(LogLevel.Info, logger.Name, "User Added Product id, "+product.ProductId+" with quantity,"+qty+" to his cart");
                    msg.Properties.Add("user", user);
                    logger.Log(msg);

                }
                return RedirectToPage("HomePage");
            }catch(Exception e)
            {
                var user = HttpContext.Request.Cookies["User"];
                var msg = new LogEventInfo(LogLevel.Error, logger.Name, "Exception occur here");
                msg.Properties.Add("user", user);
                msg.Exception = e;
                logger.Log(msg);
                return RedirectToPage("/UserError");
            }
        }
    }
}
