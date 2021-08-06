using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using testingpage.Models;
using NLog;

namespace testingpage.Pages.ShoppingCartPage
{

    public class IndexModel : PageModel
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        public int GetPageSize { get; set; } = 5;
        [BindProperty]
        public List<checkboxShoppingCart> cartList { get; set; }

        [TempData]
        public string MessageKey { get; set; }

        [TempData]
        public string responses { get; set; }

        [TempData]
        public string message { get; set; }

        public async Task<IActionResult> OnGet()
        {
            try
            {

                var gettoken = HttpContext.Request.Cookies["JWTToken"];
                var user = HttpContext.Request.Cookies["User"];
                //List<Product> productList = new List<Product>();
                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", gettoken);

                    using (HttpResponseMessage response = await httpClient.GetAsync("https://localhost:44335/api/ShoppingCart"))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            var msg = new LogEventInfo(LogLevel.Info, logger.Name, "User access Shopping Cart Page");
                            msg.Properties.Add("user", user);
                            logger.Log(msg);
                            string apiResponse = await response.Content.ReadAsStringAsync();
                            List<checkboxShoppingCart> getcartlist = JsonConvert.DeserializeObject<List<checkboxShoppingCart>>(apiResponse);

                            if (getcartlist == null)
                            {
                                cartList = new List<checkboxShoppingCart>();
                            }
                            else
                            {
                                cartList = getcartlist;

                            }
                        }
                        else if ((int)response.StatusCode == 401)
                        {
                            //return Unauthorized();
                            var msg = new LogEventInfo(LogLevel.Info, logger.Name, "Unauthorized user trying to access User Shopping Cart Page");
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

        public async Task<IActionResult> OnPostPayCartAsync()
        {
            try
            {
                var gettoken = HttpContext.Request.Cookies["JWTToken"];
                var user = HttpContext.Request.Cookies["User"];
                List<ShoppingCart> shoppingCarts = new List<ShoppingCart>();
                int count = 0;
                foreach (checkboxShoppingCart checkboxShoppingCart in cartList)
                {
                    if (checkboxShoppingCart.IsSelected == true)
                    {
                        count++;
                        ShoppingCart NewShoppingCart = new ShoppingCart
                        {
                            productId = checkboxShoppingCart.product.ProductId,
                            quantity = checkboxShoppingCart.quantity,
                        };
                        shoppingCarts.Add(NewShoppingCart);
                    }
                }
                if (count == 0)
                {
                    var msg = new LogEventInfo(LogLevel.Info, logger.Name, "Error Message Prompt as user did not select any check box in Shopping Cart Page");
                    msg.Properties.Add("user", user);
                    logger.Log(msg);
                    MessageKey = "Please select items";
                    return RedirectToAction("OnGet");
                }

                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", gettoken);
                    var json = JsonConvert.SerializeObject(shoppingCarts);


                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    using (var response = await httpClient.PutAsync("https://localhost:44335/api/ShoppingCart/PayCart", content))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            string apiResponse = await response.Content.ReadAsStringAsync();
                            List<Response> getresponses = JsonConvert.DeserializeObject<List<Response>>(apiResponse);
                            if (getresponses.Count == 0)
                            {
                                
                                var msg = new LogEventInfo(LogLevel.Info, logger.Name, "User made payment successfully");
                                msg.Properties.Add("user", user);
                                logger.Log(msg);
                                MessageKey = "Pay successfully";
                                return RedirectToPage("/ProductPage/HomePage");
                            }
                            else
                            {
                                //responses = getresponses;
                                foreach(Response r in getresponses)
                                {
                                    if(r.Status== "OutOfStock")
                                    {
                                        var msg = new LogEventInfo(LogLevel.Info, logger.Name, "Error Message Prompt as the product is out of stock in Shopping Cart Page");
                                        msg.Properties.Add("user", user);
                                        logger.Log(msg);
                                    }
                                    else if (r.Status == "Fail")
                                    {
                                        var msg = new LogEventInfo(LogLevel.Info, logger.Name, "Error Message Prompt as the product quantity exceed the stock remaining in Shopping Cart Page");
                                        msg.Properties.Add("user", user);
                                        logger.Log(msg);
                                    }
                                    responses += r.Message;
                                    responses += "<br />";



                                }
                            }
                            //if (getresponse.Status == "Success")
                            //{
                            //    var msg = new LogEventInfo(LogLevel.Info, logger.Name, "User made payment successfully");
                            //    msg.Properties.Add("user", user);
                            //    logger.Log(msg);
                            //    MessageKey = "Pay successfully";
                            //    return RedirectToPage("/ProductPage/HomePage");
                            //}
                            //else 
                            //{
                            //    if (getresponse.Status == "OutOfStock")
                            //    {
                            //        var msg = new LogEventInfo(LogLevel.Info, logger.Name, "Error Message Prompt as the product is out of stock in Shopping Cart Page");
                            //        msg.Properties.Add("user", user);
                            //        logger.Log(msg);
                            //    }
                            //    else
                            //    {
                            //        var msg = new LogEventInfo(LogLevel.Info, logger.Name, "Error Message Prompt as the product quantity exceed the stock remaining in Shopping Cart Page");
                            //        msg.Properties.Add("user", user);
                            //        logger.Log(msg);
                            //    }
                            //    MessageKey = getresponse.Message;
                            //    return RedirectToAction("OnGet");
                            //}


                        }
                        else if((int)response.StatusCode == 401)
                        {
                            //Unauthorize
                            var msg = new LogEventInfo(LogLevel.Info, logger.Name, "Unauthorized user trying to access User Individual Product Page");
                            logger.Log(msg);
                            return RedirectToPage("/LoginPage/Login");
                        }
                        return RedirectToAction("OnGet");
                    }

                }
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

        public async Task<IActionResult> OnPostDeleteCartAsync()
        {
            try
            {
                var gettoken = HttpContext.Request.Cookies["JWTToken"];
                var user = HttpContext.Request.Cookies["User"];
                List<ProductIdOnly> Products = new List<ProductIdOnly>();
                int count = 0;
                foreach (checkboxShoppingCart checkboxShoppingCart in cartList)
                {
                    if (checkboxShoppingCart.IsSelected == true)
                    {
                        count++;
                        ProductIdOnly newProduct = new ProductIdOnly();
                        newProduct.productId = checkboxShoppingCart.product.ProductId;
                        Products.Add(newProduct);
                    }
                }
                if (count == 0)
                {
                    var msg = new LogEventInfo(LogLevel.Info, logger.Name, "Error Message Prompt as user did not select any check box in Shopping Cart Page");
                    msg.Properties.Add("user", user);
                    logger.Log(msg);
                    MessageKey = "Please select items";
                    return RedirectToAction("OnGet");

                }
                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", gettoken);

                    var json = JsonConvert.SerializeObject(Products);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    await httpClient.PutAsync("https://localhost:44335/api/ShoppingCart/Delete", content);

                }

                var msg2 = new LogEventInfo(LogLevel.Info, logger.Name, "Products deleted from cart in Shopping Cart Page");
                msg2.Properties.Add("user", user);
                logger.Log(msg2);
                MessageKey = "Delete successfully";
                return RedirectToPage("/ShoppingCartPage/Index");
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
