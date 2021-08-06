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

namespace testingpage.Pages.ShoppingCartPage
{
    public class TrackOrderModel : PageModel
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        [BindProperty]
        public List<returnOrderDetail> returnOrderDetails { get; set; }
        [TempData]
        public string MessageKey { get; set; }
        public async Task<IActionResult> OnGet(int id)
        {
            try
            {
                var gettoken = HttpContext.Request.Cookies["JWTToken"];
                var user = HttpContext.Request.Cookies["User"];
                //List<Product> productList = new List<Product>();
                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", gettoken);

                    using (var response = await httpClient.GetAsync("https://localhost:44335/api/Order/trackId?id=" + id))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            var msg = new LogEventInfo(LogLevel.Info, logger.Name, "User access Track Order Page with track id, "+id);
                            msg.Properties.Add("user", user);
                            logger.Log(msg);
                            string apiResponse = await response.Content.ReadAsStringAsync();
                            List<returnOrderDetail> GetReturnOrderDetails = JsonConvert.DeserializeObject<List<returnOrderDetail>>(apiResponse);
                            if (GetReturnOrderDetails == null)
                            {
                                GetReturnOrderDetails = new List<returnOrderDetail>();
                            }
                            else
                            {
                                returnOrderDetails = GetReturnOrderDetails;

                            }
                        }
                        else if((int)response.StatusCode==401)
                        {
                            //return Unauthorized();
                            var msg = new LogEventInfo(LogLevel.Info, logger.Name, "Unauthorized user trying to access User Track Order Page");
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
    }
}
