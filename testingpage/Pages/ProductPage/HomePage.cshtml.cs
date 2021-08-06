using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using testingpage.Models;

using NLog;

namespace testingpage.Pages.ProductPage
{
    public class HomePageModel : PageModel
    {

        //public int CurrentPage { get; set; }

        public int GetPageSize { get; set; } = 9;


        [BindProperty]
        public PagedList<List<Product>> productList { get; set; }
        [TempData]
        public string MessageKey { get; set; }
        private static Logger logger = LogManager.GetCurrentClassLogger();

        [TempData]
        public string PageNumberWarning { get; set; }

        public async Task<IActionResult> OnGet(int? GetPageNumber)
        {
            try
            {
                if (GetPageNumber == null)
                {
                    GetPageNumber = 1;
                }
                var gettoken = HttpContext.Request.Cookies["JWTToken"];
                var user = HttpContext.Request.Cookies["User"];
                //List<Product> productList = new List<Product>();
                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", gettoken);

                    using (var response = await httpClient.GetAsync("https://localhost:44335/api/products/ActiveProducts?PageNumber=" + GetPageNumber + "&PageSize=" + GetPageSize))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            var msg = new LogEventInfo(LogLevel.Info, logger.Name, "User access User Home Page");
                            msg.Properties.Add("user", user);
                            logger.Log(msg);
                            string apiResponse = await response.Content.ReadAsStringAsync();
                            PagedList<List<Product>> plist = JsonConvert.DeserializeObject<PagedList<List<Product>>>(apiResponse);
                            if (GetPageNumber > plist.TotalPages)
                            {
                                PageNumberWarning = "The page number is only until " + plist.TotalPages + " pages";
                            }
                            if (plist == null)
                            {
                                productList = new PagedList<List<Product>>();
                            }
                            else
                            {
                                productList = plist;

                            }
                        }
                        else if((int)response.StatusCode == 401)
                        {
                            //return Unauthorize()
                            var msg = new LogEventInfo(LogLevel.Info, logger.Name, "Unauthorized user trying to access User Home Page");
                            logger.Log(msg);
                            MessageKey = "You are required to login to acess this page";
                            return RedirectToPage("/LoginPage/Login");
                        }

                    }
                    //                string imageBase64Data =
                    //Convert.ToBase64String(img.ImageData);
                    //                string imageDataURL =
                    //            string.Format("data:image/jpg;base64,{0}",
                    //            imageBase64Data);
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
