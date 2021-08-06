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

namespace testingpage.Pages.UserPage
{
    public class AdminControlModel : PageModel
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        public int GetPageSize { get; set; } = 10;
        [BindProperty]
        public PagedList<List<GetUsers>> getUsers { get; set; }
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
                    using (var response = await httpClient.GetAsync("https://localhost:44335/api/Authentication/GetUser?PageNumber=" + GetPageNumber + "&PageSize=" + GetPageSize))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            var msg = new LogEventInfo(LogLevel.Info, logger.Name, "Admin access Admin Control Page");
                            msg.Properties.Add("user", user);
                            logger.Log(msg);
                            string apiResponse1 = await response.Content.ReadAsStringAsync();
                            PagedList<List<GetUsers>> getUsersApi = JsonConvert.DeserializeObject<PagedList<List<GetUsers>>>(apiResponse1);
                            if (GetPageNumber > getUsersApi.TotalPages)
                            {
                                PageNumberWarning = "The page number is only until " + getUsersApi.TotalPages + " pages";
                            }
                            if (getUsersApi == null)
                            {
                                getUsers = new PagedList<List<GetUsers>>();
                            }
                            else
                            {
                                getUsers = getUsersApi;

                            }
                            
                        }
                        else if ((int)response.StatusCode == 401)
                        {
                            //Unauthorize
                            var msg = new LogEventInfo(LogLevel.Info, logger.Name, "Unauthorized user trying to access Admin Manage User Page");
                            logger.Log(msg);
                            MessageKey = "You are required to login to acess this page";
                            return RedirectToPage("/LoginPage/Login");
                        }
                        else if ((int)response.StatusCode == 403)
                        {
                            var msg = new LogEventInfo(LogLevel.Info, logger.Name, "User with no privilege trying to access Admin Manage User Page");
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
    }
}
