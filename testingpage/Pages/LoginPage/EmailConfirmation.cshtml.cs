using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NLog;
using testingpage.Models;

namespace testingpage.Pages.LoginPage
{
    public class EmailConfirmationModel : PageModel
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        public async Task<IActionResult> OnGet(EmailToken emailToken)
        {
            try
            {
                if (emailToken != null)
                {
                    using (var httpClient = new HttpClient())
                    {

                        var content = new MultipartFormDataContent();


                        content.Add(new StringContent(emailToken.Token), "Token");
                        content.Add(new StringContent(emailToken.Email), "Email");

                        using (var response = await httpClient.PostAsync("https://localhost:44335/api/Authentication/EmailConfirmation", content))
                        {
                            if (response.IsSuccessStatusCode)
                            {
                                var msg = new LogEventInfo(LogLevel.Info, logger.Name, "Email, "+emailToken.Email+" has been verified");
                                logger.Log(msg);
                                return RedirectToPage("/LoginPage/EmailSuccess");
                            }
                        }
                    }
                }
                return Page();
            }
            catch (Exception e)
            {
                var msg = new LogEventInfo(LogLevel.Error, logger.Name, "Exception occur here");
                msg.Exception = e;
                logger.Log(msg);
                return RedirectToPage("/LoginError");
            }
        }
    }
}
