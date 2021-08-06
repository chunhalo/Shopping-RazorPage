using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Newtonsoft.Json;
using NLog;
using testingpage.Models;

namespace testingpage.Pages.LoginPage
{
    public class LoginModel : PageModel
    {

        [BindProperty]
        public Models.LoginModel model { get; set; }

        [BindProperty]
        public Response res { get; set; }
        [TempData]
        public string MessageKey { get; set; }
        [TempData]
        public string alertMessage { get; set; }
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public void OnGet()
        {

        }

        public async Task<IActionResult> OnPost()
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    var content = new MultipartFormDataContent();

                    content.Add(new StringContent(model.Username), "Username");
                    content.Add(new StringContent(model.Password), "Password");


                    using (var response = await httpClient.PostAsync("https://localhost:44335/api/Authentication/Login", content))
                    {
                        if (response.IsSuccessStatusCode)
                        {

                            var jsToken = await response.Content.ReadAsStringAsync();
                            var jwttoken = JsonConvert.DeserializeObject<Models.JsonToken>(jsToken);

                            HttpContext.Response.Cookies.Append("JWTToken", jwttoken.token, new Microsoft.AspNetCore.Http.CookieOptions { HttpOnly = true });
                            HttpContext.Response.Cookies.Append("User", model.Username, new Microsoft.AspNetCore.Http.CookieOptions { HttpOnly = true });
                            if (jwttoken.role.Contains("Admin"))
                            {
                                HttpContext.Response.Cookies.Append("Role", "Admin", new Microsoft.AspNetCore.Http.CookieOptions { HttpOnly = true });
                                var msg = new LogEventInfo(LogLevel.Info, logger.Name, "Admin has successfully login");
                                msg.Properties.Add("user", model.Username);
                                logger.Log(msg);
                                return Redirect("ProductPage/AdminHome");
                            }
                            else
                            {
                                HttpContext.Response.Cookies.Append("Role", "User", new Microsoft.AspNetCore.Http.CookieOptions { HttpOnly = true });
                                var msg = new LogEventInfo(LogLevel.Info, logger.Name, "User has successfully login");
                                msg.Properties.Add("user", model.Username);
                                logger.Log(msg);
                                return Redirect("ProductPage/HomePage");

                            }


                        }
                        else
                        {

                            var getresponse = await response.Content.ReadAsStringAsync();
                            var convertresponse = JsonConvert.DeserializeObject<Models.Response>(getresponse);
                            if (convertresponse.Status == "Locked")
                            {
                                var msg = new LogEventInfo(LogLevel.Info, logger.Name, "Error message 'Account locked' has prompted in Login Page");
                                logger.Log(msg);
                            }
                            else if(convertresponse.Status== "UnconfirmedEmail")
                            {
                                var msg = new LogEventInfo(LogLevel.Info, logger.Name, "Error message 'Unconfirmed email' has prompted in Login Page");
                                logger.Log(msg);
                            }
                            else
                            {
                                var msg = new LogEventInfo(LogLevel.Info, logger.Name, "Error message 'Invalid password' has prompted in Login Page");
                                logger.Log(msg);
                                
                            }
                            MessageKey = convertresponse.Message;
                            


                            return RedirectToAction("OnGet");
                        }

                        //var apiResponse = await response.Content.ReadAsStringAsync().Result;



                        //var jwttoken = JsonConvert.DeserializeObject<JToken>(jwtToken);

                    }
                }
            }catch(Exception e)
            {
                var msg = new LogEventInfo(LogLevel.Error, logger.Name, "Exception occur here");
                msg.Exception = e;
                logger.Log(msg);
                //_logger.LogError(msg.Message, msg);
                //_logger.LogError("", msg);
                //_logger.Info(typeof(_logger),msg);

                //_logger.LogError(e, "Stopped program because of exception");
                return RedirectToPage("/LoginError");
            }

        }
    }
}
