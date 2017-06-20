using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TopoMojo.Models;
using TopoMojo.Models.AccountViewModels;
using TopoMojo.Services;
using TopoMojo.Data;
using TopoMojo.Core;
using Microsoft.EntityFrameworkCore;
using TopoMojo.Web;
using Step.Accounts;
using System.IdentityModel.Tokens.Jwt;

namespace TopoMojo.Controllers
{
    [Route("api/[controller]/[action]")]
    public class AccountController : _Controller
    {
        private readonly IAccountManager<Account> _accountManager;
        private readonly IEmailSender _emailSender;
        private readonly ISmsSender _smsSender;
        private readonly TopoMojoDbContext _db;
        private readonly AccountOptions _accountOptions;

        public AccountController(
            //SignInManager<ApplicationUser> signInManager,
            IAccountManager<Account> accountManager,
            AccountOptions accountOptions,
            IEmailSender emailSender,
            ISmsSender smsSender,
            TopoMojoDbContext db,
            IServiceProvider sp
        ) : base(sp)
        {
            //_signInManager = signInManager;
            _accountManager = accountManager;
            _accountOptions = accountOptions;
            _emailSender = emailSender;
            _smsSender = smsSender;
            _db = db;
        }

        // login(u,p), return token
        // register(u, code), return token
        // reset(p, code)
        // confirm(account), return bool

        [HttpPost]
        [JsonExceptionFilter]
        public async Task<IActionResult> Login([FromBody] Credentials model)
        {
            _logger.LogDebug($"Attempting login for {model.Username}");
            Account account = await _accountManager.AuthenticateWithCredentialAsync(model, "");
            if (account == null)
                throw new InvalidOperationException();

            return Json(_accountManager.GenerateJwtToken(account));
        }
        [HttpPost]
        [JsonExceptionFilter]
        public async Task<IActionResult> Otp([FromBody] Credentials model)
        {
            _logger.LogDebug($"Attempting login for {model.Username}");
            Account account = await _accountManager.AuthenticateWithCodeAsync(model);
            if (account == null)
                throw new InvalidOperationException();

            return Json(_accountManager.GenerateJwtToken(account));
        }

        [HttpPost]
        [JsonExceptionFilter]
        public async Task<IActionResult> Tfa([FromBody] Credentials model)
        {
            _logger.LogDebug($"Attempting login for {model.Username}");
            Account account = await _accountManager.AuthenticateWithCredentialAsync(model, "");
            if (account == null)
                throw new InvalidOperationException();

            return Json(_accountManager.GenerateJwtToken(account));
        }

        [HttpPost]
        [JsonExceptionFilter]
        public async Task<IActionResult> Register([FromBody] Credentials model)
        {
            _logger.LogDebug($"Attempting registration for {model.Username}");

            Account account = await _accountManager.RegisterWithCredentialsAsync(model, "");
            if (account == null)
                throw new InvalidOperationException();

            return Json(_accountManager.GenerateJwtToken(account));
        }

        [HttpPost]
        [JsonExceptionFilter]
         public async Task<IActionResult> Reset([FromBody] Credentials model)
        {
            _logger.LogDebug($"Attempting reset for {model.Username}");

            Account account = model.Password.HasValue()
                ? await _accountManager.AuthenticateWithResetAsync(model)
                : await _accountManager.AuthenticateWithCodeAsync(model);

            if (account == null)
                throw new InvalidOperationException();

            return Json(_accountManager.GenerateJwtToken(account));
        }

        [HttpPost]
        [JsonExceptionFilter]
        public async Task<bool> Confirm([FromBody] Credentials model)
        {
            int code = await _accountManager.GenerateAccountCodeAsync(model.Username);
            _logger.LogDebug("Confirmation code {0} {1}", model.Username, code);

            //TODO: send code via email or text

            return true;
        }
    }

}
//         //
//         // GET: /Account/Login
//         [HttpGet]
//         [AllowAnonymous]
//         public IActionResult Login(string returnUrl = null)
//         {
//             ViewData["ReturnUrl"] = returnUrl;
//             return View();
//         }

//         //
//         // POST: /Account/Login
//         [HttpPost]
//         [AllowAnonymous]
//         [ValidateAntiForgeryToken]
//         public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
//         {
//             ViewData["ReturnUrl"] = returnUrl;
//             if (ModelState.IsValid)
//             {
//                 // This doesn't count login failures towards account lockout
//                 // To enable password failures to trigger account lockout, set lockoutOnFailure: true
//                 var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
//                 if (result.Succeeded)
//                 {
//                     ApplicationUser user = await _userManager.FindByEmailAsync(model.Email);
//                     _logger.LogInformation($"{user.UserName} [{user.Id}] logged-in null null []");

//                     return RedirectToLocal(returnUrl);
//                 }
//                 if (result.RequiresTwoFactor)
//                 {
//                     return RedirectToAction(nameof(SendCode), new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
//                 }
//                 if (result.IsLockedOut)
//                 {
//                     _logger.LogWarning(2, "User account locked out.");
//                     return View("Lockout");
//                 }
//                 else
//                 {
//                     ModelState.AddModelError(string.Empty, "Invalid login attempt.");
//                     return View(model);
//                 }
//             }

//             // If we got this far, something failed, redisplay form
//             return View(model);
//         }

//         //
//         // GET: /Account/Register
//         [HttpGet]
//         // [AllowAnonymous]
//         public IActionResult Register(string returnUrl = null)
//         {
//             ViewData["ReturnUrl"] = returnUrl;
//             return View();
//         }

//         //
//         // POST: /Account/Register
//         [HttpPost]
//         // [AllowAnonymous]
//         [ValidateAntiForgeryToken]
//         public async Task<IActionResult> Register(RegisterViewModel model, string returnUrl = null)
//         {
//             ViewData["ReturnUrl"] = returnUrl;
//             if (ModelState.IsValid)
//             {
//                 var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
//                 var result = await _userManager.CreateAsync(user, model.Password);
//                 if (result.Succeeded)
//                 {
//                     // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=532713
//                     // Send an email with this link
//                     //var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
//                     //var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: HttpContext.Request.Scheme);
//                     //await _emailSender.SendEmailAsync(model.Email, "Confirm your account",
//                     //    $"Please confirm your account by clicking this link: <a href='{callbackUrl}'>link</a>");
//                     await _signInManager.SignInAsync(user, isPersistent: false);
//                     _logger.LogInformation(3, "User created a new account with password.");
//                     return RedirectToLocal(returnUrl);
//                 }
//                 AddErrors(result);
//             }

//             // If we got this far, something failed, redisplay form
//             return View(model);
//         }

//         //
//         // POST: /Account/LogOff
//         [HttpPost]
//         [ValidateAntiForgeryToken]
//         public async Task<IActionResult> LogOff()
//         {
//             await _signInManager.SignOutAsync();
//             _logger.LogInformation(4, "User logged out.");
//             Log("logged-off", null);
//             return RedirectToAction(nameof(HomeController.Index), "Home");
//         }

//         //
//         // POST: /Account/ExternalLogin
//         [HttpPost]
//         [AllowAnonymous]
//         [ValidateAntiForgeryToken]
//         public IActionResult ExternalLogin(string provider, string returnUrl = null)
//         {
//             // Request a redirect to the external login provider.
//             var redirectUrl = Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl });
//             var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
//             return Challenge(properties, provider);
//         }

//         //
//         // GET: /Account/ExternalLoginCallback
//         [HttpGet]
//         [AllowAnonymous]
//         public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
//         {
//             if (remoteError != null)
//             {
//                 ModelState.AddModelError(string.Empty, $"Error from external provider: {remoteError}");
//                 return View(nameof(Login));
//             }
//             var info = await _signInManager.GetExternalLoginInfoAsync();
//             if (info == null)
//             {
//                 return RedirectToAction(nameof(Login));
//             }

//             // Sign in the user with this external login provider if the user already has a login.
//             var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false);
//             if (result.Succeeded)
//             {
//                 _logger.LogInformation(5, "User logged in with {Name} provider.", info.LoginProvider);
//                 return RedirectToLocal(returnUrl);
//             }
//             if (result.RequiresTwoFactor)
//             {
//                 return RedirectToAction(nameof(SendCode), new { ReturnUrl = returnUrl });
//             }
//             if (result.IsLockedOut)
//             {
//                 return View("Lockout");
//             }
//             else
//             {
//                 // If the user does not have an account, then ask the user to create an account.
//                 ViewData["ReturnUrl"] = returnUrl;
//                 ViewData["LoginProvider"] = info.LoginProvider;
//                 var email = info.Principal.FindFirstValue(ClaimTypes.Email);
//                 return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = email });
//             }
//         }

//         //
//         // POST: /Account/ExternalLoginConfirmation
//         [HttpPost]
//         [AllowAnonymous]
//         [ValidateAntiForgeryToken]
//         public async Task<IActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl = null)
//         {
//             if (ModelState.IsValid)
//             {
//                 // Get the information about the user from the external login provider
//                 var info = await _signInManager.GetExternalLoginInfoAsync();
//                 if (info == null)
//                 {
//                     return View("ExternalLoginFailure");
//                 }
//                 var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
//                 var result = await _userManager.CreateAsync(user);
//                 if (result.Succeeded)
//                 {
//                     result = await _userManager.AddLoginAsync(user, info);
//                     if (result.Succeeded)
//                     {
//                         await _signInManager.SignInAsync(user, isPersistent: false);
//                         _logger.LogInformation(6, "User created an account using {Name} provider.", info.LoginProvider);
//                         return RedirectToLocal(returnUrl);
//                     }
//                 }
//                 AddErrors(result);
//             }

//             ViewData["ReturnUrl"] = returnUrl;
//             return View(model);
//         }

//         // GET: /Account/ConfirmEmail
//         [HttpGet]
//         [AllowAnonymous]
//         public async Task<IActionResult> ConfirmEmail(string userId, string code)
//         {

//             if (userId == null || code == null)
//             {
//                 return View("Error");
//             }
//             var user = await _userManager.FindByIdAsync(userId);
//             if (user == null)
//             {
//                 return View("Error");
//             }
//             var result = await _userManager.ConfirmEmailAsync(user, code);
//             return View(result.Succeeded ? "ConfirmEmail" : "Error");
//         }

//         [HttpPost("/api/[controller]/[action]")]
//         //[ValidateAntiForgeryToken]
//         [JsonExceptionFilter]
//         public async Task<IActionResult> UpdateProfile([FromBody]ProfileUpdateModel model)
//         {
//             Person person = _db.People.Find(_user.PersonId);
//             person.Name = model.Name;
//             await _db.SaveChangesAsync();
//             return Json(await GetAuthToken(_user, person));
//         }

//         [HttpPost("/api/[controller]/[action]")]
//         //[ValidateAntiForgeryToken]
//         [JsonExceptionFilter]
//         public async Task<IActionResult> ChangePassword([FromBody]ChangePasswordViewModel model)
//         {
//             var user = await GetCurrentUserAsync();
//             if (user != null)
//             {
//                 var result = await _userManager.ChangePasswordAsync(user, model.Current, model.Password);
//                 if (!result.Succeeded)
//                 {
//                     throw new InvalidOperationException(String.Join("\n", result.Errors.Select(e => e.Description).ToArray()));
//                 }
//             }
//             return Json(true);
//         }

//         //
//         // GET: /Account/ForgotPassword
//         [HttpGet]
//         [AllowAnonymous]
//         public IActionResult ForgotPassword()
//         {
//             return View();
//         }

//         //
//         // POST: /Account/ForgotPassword
//         [HttpPost("/api/[controller]/[action]")]
//         [AllowAnonymous]
//         //[ValidateAntiForgeryToken]
//         [JsonExceptionFilter]
//         public async Task<IActionResult> ForgotPassword([FromBody]ForgotPasswordViewModel model)
//         {

//             if (ModelState.IsValid)
//             {
//                 var user = await _userManager.FindByNameAsync(model.Email);
//                 if (user != null && (await _userManager.IsEmailConfirmedAsync(user)))
//                 {

//                 //     // Don't reveal that the user does not exist or is not confirmed
//                 //     return View("ForgotPasswordConfirmation");
//                 // }

//                     user.ResetCode = new Random().Next(10000, 100000);
//                     user.ResetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
//                     await _userManager.UpdateAsync(user);
//                     // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=532713
//                     // Send an email with this link
//                     //var code = await _userManager.GeneratePasswordResetTokenAsync(user);
//                     //var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: HttpContext.Request.Scheme);
//                     //callbackUrl = callbackUrl.Replace("http://localhost:5000", _options.Site.ExternalUrl);
//                     await _emailSender.SendEmailAsync(model.Email, "TopoMojo password reset", $"TopoMojo password reset code: {user.ResetCode}");
//                     _logger.LogInformation($"{user.UserName} [{user.Id}] requested-reset null null [{user.ResetCode}]");
//                 }
//             }
//             return Json(true);
//             // If we got this far, something failed, redisplay form
//             //return View(model);
//         }

//         //
//         // GET: /Account/ForgotPasswordConfirmation
//         [HttpGet]
//         [AllowAnonymous]
//         public IActionResult ForgotPasswordConfirmation()
//         {
//             return View();
//         }

//         //
//         // GET: /Account/ResetPassword
//         [HttpGet]
//         [AllowAnonymous]
//         public IActionResult ResetPassword(string code = null)
//         {
//             return code == null ? View("Error") : View();
//         }

//         //
//         // POST: /Account/ResetPassword
//         [HttpPost("/api/[controller]/[action]")]
//         [AllowAnonymous]
//         //[ValidateAntiForgeryToken]
//         [JsonExceptionFilter]
//         public async Task<IActionResult> ResetPassword([FromBody]ResetPasswordViewModel model)
//         {
//             var user = await _userManager.FindByNameAsync(model.Email);
//             if (user != null)
//             {
//                 if (user.ResetCode>0 && user.ResetCode.ToString() == model.Code)
//                 {
//                     var result = await _userManager.ResetPasswordAsync(user, user.ResetToken, model.Password);
//                     if (result.Succeeded)
//                     {
//                         _logger.LogInformation($"{user.UserName} [{user.Id}] reset-password null null []");
//                         user.ResetCode = 0;
//                         user.ResetToken = "";
//                         await _userManager.UpdateAsync(user);
//                     }
//                 }
//             }
//             return await Login(new { u = model.Email, p = model.Password });
//         }

//         //
//         // GET: /Account/ResetPasswordConfirmation
//         [HttpGet]
//         [AllowAnonymous]
//         public IActionResult ResetPasswordConfirmation()
//         {
//             return View();
//         }

//         //
//         // GET: /Account/SendCode
//         [HttpGet]
//         [AllowAnonymous]
//         public async Task<ActionResult> SendCode(string returnUrl = null, bool rememberMe = false)
//         {
//             var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
//             if (user == null)
//             {
//                 return View("Error");
//             }
//             var userFactors = await _userManager.GetValidTwoFactorProvidersAsync(user);
//             var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
//             return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
//         }

//         //
//         // POST: /Account/SendCode
//         [HttpPost]
//         [AllowAnonymous]
//         [ValidateAntiForgeryToken]
//         public async Task<IActionResult> SendCode(SendCodeViewModel model)
//         {
//             if (!ModelState.IsValid)
//             {
//                 return View();
//             }

//             var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
//             if (user == null)
//             {
//                 return View("Error");
//             }

//             // Generate the token and send it
//             var code = await _userManager.GenerateTwoFactorTokenAsync(user, model.SelectedProvider);
//             if (string.IsNullOrWhiteSpace(code))
//             {
//                 return View("Error");
//             }

//             var message = "Your security code is: " + code;
//             if (model.SelectedProvider == "Email")
//             {
//                 await _emailSender.SendEmailAsync(await _userManager.GetEmailAsync(user), "Security Code", message);
//             }
//             else if (model.SelectedProvider == "Phone")
//             {
//                 await _smsSender.SendSmsAsync(await _userManager.GetPhoneNumberAsync(user), message);
//             }

//             return RedirectToAction(nameof(VerifyCode), new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
//         }

//         //
//         // GET: /Account/VerifyCode
//         [HttpGet]
//         [AllowAnonymous]
//         public async Task<IActionResult> VerifyCode(string provider, bool rememberMe, string returnUrl = null)
//         {
//             // Require that the user has already logged in via username/password or external login
//             var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
//             if (user == null)
//             {
//                 return View("Error");
//             }
//             return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
//         }

//         //
//         // POST: /Account/VerifyCode
//         [HttpPost]
//         [AllowAnonymous]
//         [ValidateAntiForgeryToken]
//         public async Task<IActionResult> VerifyCode(VerifyCodeViewModel model)
//         {
//             if (!ModelState.IsValid)
//             {
//                 return View(model);
//             }

//             // The following code protects for brute force attacks against the two factor codes.
//             // If a user enters incorrect codes for a specified amount of time then the user account
//             // will be locked out for a specified amount of time.
//             var result = await _signInManager.TwoFactorSignInAsync(model.Provider, model.Code, model.RememberMe, model.RememberBrowser);
//             if (result.Succeeded)
//             {
//                 return RedirectToLocal(model.ReturnUrl);
//             }
//             if (result.IsLockedOut)
//             {
//                 _logger.LogWarning(7, "User account locked out.");
//                 return View("Lockout");
//             }
//             else
//             {
//                 ModelState.AddModelError(string.Empty, "Invalid code.");
//                 return View(model);
//             }
//         }

//         private async Task<object> GetAuthToken(ApplicationUser user, Person person)
//         {
//             if (person == null)
//             {
//                 person = await _db.People.FindAsync(user.PersonId);
//             }

//             return JwtTokenGenerator.Generate(new
//             {
//                 sub = user.Id,
//                 tmnm = person.Name ?? user.UserName,
//                 tmid = user.PersonId,
//                 tmad = user.IsAdmin
//             }, new IdentityOptions());

//         }
//         [HttpPost]
//         [AllowAnonymous]
//         [RouteAttribute("/api/[controller]/[action]")]
//         [JsonExceptionFilter]
//         public async Task<IActionResult> Login([FromBody] dynamic creds)
//         {
//             string username = creds.u;
//             string password = creds.p;
//             var result = await _signInManager.PasswordSignInAsync(username, password, false, lockoutOnFailure: false);
//             if (result.Succeeded)
//             {
//                 ApplicationUser user = await _userManager.FindByEmailAsync(username);
//                 _logger.LogInformation(user.Email + " logged in.");
//                 // var token = JwtTokenGenerator.Generate(new
//                 //     {
//                 //         sub = user.Id,
//                 //         tmnm = user.Email,
//                 //         tmid = user.PersonId,
//                 //         tmad = user.IsAdmin
//                 //     }, new IdentityOptions());
//                 return Json(await GetAuthToken(user, null));
//             }
//             return BadRequest();
//         }

//         [HttpGet]
//         [RouteAttribute("/api/[controller]/[action]")]
//         [JsonExceptionFilter]
//         public async Task<IActionResult> Renew()
//         {
//             // ApplicationUser user = _user;
//             // var token = JwtTokenGenerator.Generate(new
//             //     {
//             //         sub = user.Id,
//             //         tmnm = user.Email,
//             //         tmid = user.PersonId,
//             //         tmad = user.IsAdmin
//             //     }, new IdentityOptions());
//             _logger.LogInformation(_user.Email + " renewed token.");

//             await _signInManager.SignInAsync(_user, false);
//             return Json(await GetAuthToken(_user, null));
//         }

//         [HttpPost]
//         [RouteAttribute("/api/[controller]/[action]")]
//         [JsonExceptionFilter]
//         public async Task<IActionResult> Logout()
//         {
//             await _signInManager.SignOutAsync();
//             _logger.LogInformation(4, "User logged out.");
//             Log("logged-off", null);
//             return Json(true);
//         }

//         [RouteAttribute("/api/[controller]/[action]")]
//         [JsonExceptionFilter]
//         [HttpPost]
//         public async Task<SearchResult<Person>> Roster([FromBody]Search search)
//         {
//             if (!_user.IsAdmin)
//                 throw new InvalidOperationException();

//             IQueryable<Person> q = _db.People;
//             if (search.Term.HasValue())
//                 q = q.Where(p => p.Name.IndexOf(search.Term, StringComparison.CurrentCultureIgnoreCase) >=0);
//             SearchResult<Person> result = new SearchResult<Person>();
//             result.Search = search;
//             result.Total = await q.CountAsync();
//             if (search.Take == 0) search.Take = 50;
//             q = q.OrderBy(p => p.Name).Skip(search.Skip).Take(search.Take);
//             result.Results = await q.ToArrayAsync();
//             return result;
//         }

//         [RouteAttribute("/api/[controller]/[action]")]
//         [HttpPost]
//         [JsonExceptionFilter]
//         public async Task<Person> Grant([FromBody] Person person)
//         {
//             if (!_user.IsAdmin)
//                 throw new InvalidOperationException();
//             return await Adjudicate(person, true);
//         }

//         [RouteAttribute("/api/[controller]/[action]")]
//         [HttpPost]
//         [JsonExceptionFilter]
//         public async Task<Person> Deny([FromBody] Person person)
//         {
//             if (!_user.IsAdmin)
//                 throw new InvalidOperationException();
//             return await Adjudicate(person, false);
//         }

//         private async Task<Person> Adjudicate(Person person, bool approved)
//         {
//             if (!_user.IsAdmin)
//                 throw new InvalidOperationException();

//             if (person.Id == _user.PersonId)
//                 return person; // can't set your own privs

//             person.IsAdmin = approved;
//             //_db.Attach(person);
//             if (person.Id == 0)
//             {
//                 person.GlobalId = Guid.NewGuid().ToString();
//                 person.WhenCreated = DateTime.UtcNow;
//                 _db.Add(person);
//             }
//             else
//                 _db.Update(person);
//             await _db.SaveChangesAsync();

//             ApplicationUser user = await _userManager.FindByIdAsync(person.GlobalId);
//             if (user != null)
//             {
//                 user.IsAdmin = approved;
//                 user.EmailConfirmed = true;
//                 await _userManager.UpdateAsync(user);
//             }
//             else
//             {
//                 await _userManager.CreateAsync(new ApplicationUser {
//                     Id = person.GlobalId,
//                     UserName = person.Name,
//                     Email = person.Name,
//                     IsAdmin = approved,
//                     PersonId = person.Id,
//                     EmailConfirmed = true
//                 }, "Compl3XP@ss" + Guid.NewGuid().ToString());
//             }

//             return person;

//         }

//         [RouteAttribute("/api/[controller]/[action]")]
//         [HttpPost]
//         [JsonExceptionFilter]
//         public async Task<Person> AddUser([FromBody]Person person)
//         {
//             if (!_user.IsAdmin)
//                 throw new InvalidOperationException();

//             return await Adjudicate(person, false);
//         }

//         [RouteAttribute("/api/[controller]/[action]")]
//         [HttpPost]
//         [JsonExceptionFilter]
//         public async Task<IActionResult> Upload([FromBody]dynamic upload)
//         {
//             if (!_user.IsAdmin)
//                 throw new InvalidOperationException();

//             string list = upload.list.ToString();
//             List<Person> results = new List<Person>();
//             string[] emails = list.Split(new char[] { ' ', '\t', '\n', ',', '\r'}, StringSplitOptions.RemoveEmptyEntries);
//             foreach (string email in emails)
//             {
//                 ApplicationUser user = await _userManager.FindByEmailAsync(email);
//                 if (user == null)
//                 {
//                     results.Add(Adjudicate(new Person { Name = email }, false).Result);
//                 }
//             }
//             await _db.SaveChangesAsync();
//             return Json(results.ToArray());
//         }

//         [RouteAttribute("/api/[controller]/[action]")]
//         [HttpPost]
//         [JsonExceptionFilter]
//         public async Task<bool> AddTopoUser([FromBody] dynamic model)
//         {
//             int topoId = model.topoId;
//             string emails = model.emails;
//             List<int> ids = new List<int>();
//             // if (model.Emails.HasValue())
//             // {
//                 string[] list = emails.Split(new char[] { ',', ' ', ';', '\t', '|'}, StringSplitOptions.RemoveEmptyEntries);
//                 foreach (string email in list)
//                 {
//                     ApplicationUser user = await _userManager.FindByEmailAsync(email);
//                     if (user != null)
//                         ids.Add(user.PersonId);
//                 }
//             // }
//             // else
//             // {
//                 if (model.PersonId > 0)
//                     ids.Add(model.PersonId);
//             //}

//             foreach (int id in ids)
//             {
//                 await SetPermission(topoId, id, PermissionFlag.Editor);
//             }
//             return true;
//         }

//         [RouteAttribute("/api/[controller]/[action]")]
//         [HttpPost]
//         [JsonExceptionFilter]
//         public async Task<bool> RemoveTopoUser([FromBody] dynamic model)
//         {
//             int tid = model.topoId;
//             int pid = model.personId;
//             await SetPermission(tid, pid, PermissionFlag.None);
//             return true;
//         }

//         private async Task SetPermission(int tid, int pid, PermissionFlag flag)
//         {
//             if (pid == _user.PersonId)
//                 return;  //can't set permission on self

//             if (!_user.IsAdmin) {
//                 TopoMojo.Core.Permission self = _db.Permissions
//                 .Where(p => p.TopologyId == tid
//                     && p.PersonId == _user.PersonId
//                     && p.Value.HasFlag(PermissionFlag.Manager))
//                 .SingleOrDefault();

//                 if (self == null)
//                     throw new InvalidOperationException();
//             }

//             TopoMojo.Core.Permission perm = _db.Permissions
//                 .Where(p => p.TopologyId == tid
//                     && p.PersonId == pid)
//                 .SingleOrDefault();

//             if (flag == PermissionFlag.None)
//             {
//                 _db.Permissions.Remove(perm);
//             }
//             else
//             {
//                 if (perm == null)
//                 {
//                     perm = new TopoMojo.Core.Permission {
//                         TopologyId = tid,
//                         PersonId = pid,
//                     };
//                     _db.Permissions.Add(perm);
//                 }
//                 perm.Value = flag;
//             }
//             await _db.SaveChangesAsync();
//         }


//         #region Helpers

//         private void AddErrors(IdentityResult result)
//         {
//             foreach (var error in result.Errors)
//             {
//                 ModelState.AddModelError(string.Empty, error.Description);
//             }
//         }


//         private IActionResult RedirectToLocal(string returnUrl)
//         {
//             if (Url.IsLocalUrl(returnUrl))
//             {
//                 return Redirect(returnUrl);
//             }
//             else
//             {
//                 return RedirectToAction(nameof(HomeController.Index), "Home");
//             }
//         }

//         #endregion
//     }
// }
