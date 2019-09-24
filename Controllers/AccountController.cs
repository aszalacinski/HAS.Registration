using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.MongoDb;
using IdentityUser = Microsoft.AspNetCore.Identity.MongoDb.IdentityUser;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity.UI.Services;
using HAS.Registration.Models;
using System.Text.Encodings.Web;
using Microsoft.Extensions.Logging;
using HAS.Registration.Feature.GatedRegistration;
using HAS.Registration.ApplicationServices.Messaging;
using HAS.Registration.Configuration;

namespace HAS.Registration.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<AccountController> _logger;
        private readonly IGatedRegistrationService _gatedRegistrationService;
        private readonly IQueueService _queueService;

        public AccountController(
            UserManager<IdentityUser> userManager, 
            IEmailSender emailSender, 
            ILogger<AccountController> logger,
            IGatedRegistrationService gatedRegistrationSvc,
            CloudSettings cloudSettings)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _logger = logger;
            _gatedRegistrationService = gatedRegistrationSvc;
            _queueService = AzureStorageQueueService.Create(cloudSettings.Azure_Queue_ConnectionString);
            _queueService.CreateQueue(cloudSettings.Azure_Queue_Name_ReservationCompletedEvent);
        }


        public IActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Register(
            RegisterViewModel model,
            string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl = returnUrl ?? Url.Content("~/");
            if (ModelState.IsValid)
            {
                // check if user is registed with GatedRegistration
                var registerCheck = await _gatedRegistrationService.AttemptToRegister(model.Email, model.EntryCode);
                var regCheckResult = registerCheck.Result.Result;
                if(regCheckResult)
                {
                    var user = new IdentityUser(model.Email, model.Email);
                    var result = await _userManager.CreateAsync(user, model.Password);
                    if (result.Succeeded)
                    {
                        _logger.LogInformation("User created a new account with password.");

                        // Send an email with this link
                        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                        var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, Request.Scheme);

                        await _emailSender.SendEmailAsync(model.Email, "MyPractice.Yoga Email Confirmation",
                            "Please confirm your MyPractice.Yoga account by clicking this link: <a href=\"" + callbackUrl + "\">link</a>");

                        //await _signInManager.SignInAsync(user, isPersistent: false);

                        await _queueService.AddMessage<RegistrationCompletedEvent>(new RegistrationCompletedEvent { Email = user.Email.Value, UserId = user.Id });

                        return View("Registered");
                    }
                    AddErrors(result);
                }
                else
                {
                    switch(registerCheck.Result.StatusCode)
                    {
                        case 200:
                            return RedirectToAction("GatedRegistrationResult", "Account", new { code = 200 });

                        case 204:
                            return RedirectToAction("GatedRegistrationResult", "Account", new { code = 204 });

                        case 302:
                            return RedirectToAction("GatedRegistrationResult", "Account", new { code = 302 });

                        case 401:
                            return RedirectToAction("GatedRegistrationResult", "Account", new { code = 401 });

                        default:
                            return RedirectToAction("GatedRegistrationResult", "Account", new { code = 500 });

                    }
                }
            }
       
            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpGet]
        public IActionResult GatedRegistrationResult(int code)
        {
            return View(new GatedRegistrationResultViewModel(code));
        }

        [HttpGet]
        public IActionResult Registered()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return RedirectToAction("ConfirmError");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                //return NotFound($"Unable to load user with ID '{userId}'.");
                return RedirectToAction("ConfirmError");
            }

            var result = await _userManager.ConfirmEmailAsync(user, code);
            if (!result.Succeeded)
            {
                //throw new InvalidOperationException($"Error confirming email for user with ID '{userId}':");
                return RedirectToAction("ConfirmError");
            }

            return View();
        }

        [HttpGet]
        public IActionResult ConfirmError()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View(new ForgotPasswordViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return RedirectToAction("ForgotPasswordConfirmation");
                }

                // Send an email with this link
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                var callbackUrl = Url.Action("ResetPassword", "Account", new { code }, Request.Scheme);

                await _emailSender.SendEmailAsync(
                    model.Email,
                    "Reset Password for MyPractice.Yoga",
                    $"Please reset your password by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                return RedirectToAction("ForgotPasswordConfirmation");
            }

            return View();
        }

        [HttpGet]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ResetPassword(string code = null)
        {
            if (code == null)
            {
                return BadRequest("A code must be supplied for password reset.");
            }
            else
            {
                ResetPasswordViewModel model = new ResetPasswordViewModel
                {
                    Code = code
                };
                return View(model);
            }
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation");
            }

            var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View();
        }

        [HttpGet]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }


        #region Helpers

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
        }
        #endregion

    }
}