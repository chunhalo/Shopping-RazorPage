using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NLog;

namespace testingpage.Pages.ProductPage
{
    public class DeleteProductModel : PageModel
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        public async Task<IActionResult> OnGet(int id)
        {
            try
            {
                var gettoken = HttpContext.Request.Cookies["JWTToken"];
                var user = HttpContext.Request.Cookies["User"];
                //List<Product> productList = new List<Product>();


                //List<Product> productList = new List<Product>();
                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", gettoken);
                    await httpClient.DeleteAsync("https://localhost:44335/api/products/" + id);
                }
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
