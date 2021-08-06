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
    public class PurchaseHistoryModel : PageModel
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        public int GetPageSize { get; set; } = 5;
        [BindProperty]
        public PagedList<List<returnOrder>> returnOrder { get; set; }
        //[BindProperty]
        //public List<returnOrder> returnOrder { get; set; }
        [TempData]
        public string MessageKey { get; set; }

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
                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", gettoken);
                    using (HttpResponseMessage response = await httpClient.GetAsync("https://localhost:44335/api/Order?PageNumber=" + GetPageNumber + "&PageSize=" + GetPageSize))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            var msg = new LogEventInfo(LogLevel.Info, logger.Name, "User access Purchase History Page");
                            msg.Properties.Add("user", user);
                            logger.Log(msg);
                            string apiResponse = await response.Content.ReadAsStringAsync();
                            PagedList<List<returnOrder>> getHistory = JsonConvert.DeserializeObject<PagedList<List<returnOrder>>>(apiResponse);
                            if (GetPageNumber > getHistory.TotalPages)
                            {
                                PageNumberWarning = "The page number is only until " + getHistory.TotalPages + " pages";
                            }
                            if (getHistory == null)
                            {
                                returnOrder = new PagedList<List<returnOrder>>();
                            }
                            else
                            {
                                returnOrder = getHistory;
                            }
                        }
                        else if((int)response.StatusCode == 401)
                        {
                            //return Unauthorized();
                            var msg = new LogEventInfo(LogLevel.Info, logger.Name, "Unauthorized user trying to access User Purchase History Page");
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
