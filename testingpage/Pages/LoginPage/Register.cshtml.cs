using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using NLog;

namespace testingpage.Pages.LoginPage
{
    public class RegisterModel : PageModel
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        [BindProperty]
        public Models.RegisterModel register { get; set; }

        [TempData]
        public string alertMessage { get; set; }
        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPost()
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Page();
                }
                using (var httpClient = new HttpClient())
                {

                    var content = new MultipartFormDataContent();

                    content.Add(new StringContent(register.Username), "Username");
                    content.Add(new StringContent(register.Email), "Email");
                    content.Add(new StringContent(register.Password), "Password");
                    content.Add(new StringContent(register.PhoneNumber), "PhoneNumber");
                    content.Add(new StringContent("https://localhost:44395/LoginPage/EmailConfirmation"), "clientlink");

                    using (var response = await httpClient.PostAsync("https://localhost:44335/api/Authentication/Register", content))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            var msg = new LogEventInfo(LogLevel.Info, logger.Name, "Account with username, "+register.Username+" has been registered successfully without email confirmation");
                            logger.Log(msg);
                            alertMessage = "You have registered account successfully. Please Confirm your email.";
                            return RedirectToPage("Login");
                        }
                        else
                        {
                            var getresponse = await response.Content.ReadAsStringAsync();
                            var convertresponse = JsonConvert.DeserializeObject<Models.Response>(getresponse);
                            if (convertresponse.Status == "UserExist")
                            {
                                var msg = new LogEventInfo(LogLevel.Info, logger.Name, "Error message 'User Exist' has prompted in Register Page");
                                logger.Log(msg);
                                ViewData["UserExist"] = convertresponse.Message;
                            }
                            else if(convertresponse.Status=="EmailExist")
                            {
                                var msg = new LogEventInfo(LogLevel.Info, logger.Name, "Error message 'Email Exist' has prompted in Register Page");
                                logger.Log(msg);
                                ViewData["EmailExist"] = convertresponse.Message;
                            }

                        



                            return Page();
                        }
                    }
                }
            }catch(Exception e)
            {
                var msg = new LogEventInfo(LogLevel.Error, logger.Name, "Exception occur here");
                msg.Exception = e;
                logger.Log(msg);
                return RedirectToPage("/LoginError");
            }

        }
    }
}
