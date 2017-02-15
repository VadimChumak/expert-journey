using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using NewsWebSite.Models;
using NewsWebSite.Models.Default;
using NewsWebSite.Models.Repository;
using NewsWebSite.Models.ViewModel;
using System.Collections.Generic;
using System.Configuration;
using NewsWebSite.Models.Services;
using System.IO;

namespace NewsWebSite.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        public AccountController(
            UserManager<AppUser, int> userManager,
            SignInManager<AppUser, int> signInManager,
            IUserRepository repo,
            ITagRepository tagRepo,
            INotifiactionsRepository notifiRepo)
        {
            UserManager = userManager;
            SignInManager = signInManager;
            this.repo = repo;
            this.tagRepo = tagRepo;
            this.notifiRepo = notifiRepo;
            notifiCountCache = new NotificationsCountService(notifiRepo);


        }
        readonly NotificationsCountService notifiCountCache;
        readonly IUserRepository repo;
        readonly ITagRepository tagRepo;
        readonly INotifiactionsRepository notifiRepo;
        readonly SignInManager<AppUser, int> SignInManager;
        readonly UserManager<AppUser, int> UserManager;

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            if (User.Identity.IsAuthenticated) return RedirectToAction("Index", "News");
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }




        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl = "")
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, change to shouldLockout: true
            var user = new AppUser { UserName = model.Email, Password = model.Password };
            var result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, shouldLockout: false);
            switch (result)
            {
                case SignInStatus.Success:
                    {
                        if (string.IsNullOrEmpty(returnUrl) || returnUrl == "/") return RedirectToAction("Index", "News");
                        return Redirect(returnUrl);
                    }
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = "", RememberMe = model.RememberMe });
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid login attempt.");
                    return View(model);
            }
        }

        [HttpGet]
        public ActionResult Index()
        {
            AppUser currentUser = repo.GetById(User.Identity.GetUserId<int>());
            UserViewModel userView = new UserViewModel
            {
                Id = currentUser.Id,
                UserName = currentUser.UserName,
                ImageName = currentUser.Image,
                UserTags = currentUser.Tags,
            };
            return View(userView);
        }

        [HttpGet]
        public ActionResult Notifications()
        {
            var lst = notifiRepo.GetList(User.Identity.GetUserId<int>());
            return View(lst);
        }
        [HttpGet]
        public ActionResult ChangeImage()
        {
            AppUser currenrUser = repo.GetById(User.Identity.GetUserId<int>());
            EditUserImageModel editImage = new EditUserImageModel { ImageName = currenrUser.Image, Id = currenrUser.Id };
            return View(editImage);
        }

        [HttpPost]
        [Authorize]
        public ActionResult ChangeImage(EditUserImageModel editImage)
        {
            if (!ModelState.IsValid)
            {
                return View(editImage);
            }
            AppUser currentUser = repo.GetById(User.Identity.GetUserId<int>());
            if (editImage.Image != null)
            {
                currentUser.Image = editImage.Image.FileName;
                FileHelper fileHelper = new FileHelper();
                fileHelper.SaveFIle(Server.MapPath(ConfigurationManager.AppSettings["UserImagesFolder"]), editImage.Image, currentUser.Id);
            }
            else currentUser.Image = null;
            repo.Save(currentUser);
            return RedirectToAction("Index");
        }


        [HttpGet]
        public ActionResult EditTags()
        {
            AppUser currentUser = repo.GetById(User.Identity.GetUserId<int>());
            EditTagsModel editTags = new EditTagsModel
            {
                UserTags = currentUser.Tags,
                AllTags = tagRepo.GetAllTags()
            };
            return View(editTags);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditTags(string[] tags)
        {
            AppUser currentUser = repo.GetById(User.Identity.GetUserId<int>());
            currentUser.Tags.Clear();
            if (tags == null)
            {
                repo.Save(currentUser);
            }
            else
            {
                IEnumerable<Tag> tagsList = TagsHelper.CreateTagList(tags, tagRepo);
                TagsHelper.SetTagForModel(currentUser, tagsList);
                repo.Save(currentUser);
            }
            return RedirectToAction("Index");
        }


        [HttpGet]
        public async Task<ActionResult> EditPassword()
        {
            AppUser currentUser = await UserManager.FindByIdAsync(User.Identity.GetUserId<int>());
            if (currentUser != null)
            {
                EditPasswordModel editPassword = new EditPasswordModel();
                return View(editPassword);
            }
            return RedirectToAction("Index");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditPassword(EditPasswordModel editModel)
        {
            AppUser currentUser = await UserManager.FindByIdAsync(User.Identity.GetUserId<int>());
            if (!ModelState.IsValid)
            {
                return View(editModel);
            }
            IdentityResult validPass = null;
            if (!string.IsNullOrEmpty(editModel.NewPassword))
            {
                if (await UserManager.CheckPasswordAsync(currentUser, editModel.OldPassword))
                {
                    validPass = await UserManager.PasswordValidator.ValidateAsync(editModel.NewPassword);
                    if (!validPass.Succeeded)
                    {
                        AddErrors(validPass);
                    }
                    else
                    {
                        currentUser.Password = UserManager.PasswordHasher.HashPassword(editModel.NewPassword);
                        IdentityResult result = await UserManager.UpdateAsync(currentUser);
                        if (!result.Succeeded)
                        {
                            AddErrors(result);
                        }
                        else
                        {
                            return RedirectToAction("Index");
                        }
                    }

                }
                else
                {
                    ModelState.AddModelError("", "Введен неверный пароль");
                }
            }
            return View();
        }


        [HttpGet]
        public async Task<ActionResult> EditEmail()
        {
            AppUser currentUser = await UserManager.FindByIdAsync(User.Identity.GetUserId<int>());
            if (currentUser != null)
            {
                EditEmailModel editModel = new EditEmailModel { Email = currentUser.UserName };
                return View(editModel);
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditEmail(EditEmailModel editModel)
        {
            if (!ModelState.IsValid)
            {
                return View(editModel);
            }
            AppUser user = await UserManager.FindByIdAsync(User.Identity.GetUserId<int>());
            user.UserName = editModel.Email;
            IdentityResult validEmail = await UserManager.UserValidator.ValidateAsync(user);
            if (!validEmail.Succeeded)
            {
                AddErrors(validEmail);
            }
            else
            {
                IdentityResult result = await UserManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    AddErrors(result);
                }
                return RedirectToAction("Index");
            }
            return View();
        }

        //
        // POST: /Account/Login


        //
        // GET: /Account/VerifyCode
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            // Require that the user has already logged in via username/password or external login
            if (!await SignInManager.HasBeenVerifiedAsync())
            {
                return View("Error");
            }
            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // The following code protects for brute force attacks against the two factor codes. 
            // If a user enters incorrect codes for a specified amount of time then the user account 
            // will be locked out for a specified amount of time. 
            // You can configure the account lockout settings in IdentityConfig
            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent: model.RememberMe, rememberBrowser: model.RememberBrowser);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(model.ReturnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid code.");
                    return View(model);
            }
        }

        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            if (User.Identity.IsAuthenticated) return RedirectToAction("Index", "News");
            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {

                var user = new AppUser { UserName = model.Email };

                var isNull = (model.Image == null);
                if (!isNull)
                {
                    user.Image = Path.GetFileName(model.Image.FileName);
                }
                else user.Image = null;

                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {

                    if (!isNull)
                        (new FileHelper()).SaveFIle(Server.MapPath(ConfigurationManager.AppSettings["UserImagesFolder"]), model.Image, user.Id);
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                    return RedirectToAction("Index", "News");
                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(int userId, string code)
        {
            if (userId <= 0 || code == null)
            {
                return View("Error");
            }
            var result = await UserManager.ConfirmEmailAsync(userId, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByNameAsync(model.Email);
                if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return View("ForgotPasswordConfirmation");
                }

                // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                // Send an email with this link
                // string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                // var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);		
                // await UserManager.SendEmailAsync(user.Id, "Reset Password", "Please reset your password by clicking <a href=\"" + callbackUrl + "\">here</a>");
                // return RedirectToAction("ForgotPasswordConfirmation", "Account");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            return code == null ? View("Error") : View();
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await UserManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            AddErrors(result);
            return View();
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/SendCode
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        {
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (userId <= 0)
            {
                return View("Error");
            }
            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/SendCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // Generate the token and send it
            if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
            {
                return View("Error");
            }
            return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        }

        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login
            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
                case SignInStatus.Failure:
                default:
                    // If the user does not have an account, then prompt the user to create an account
                    ViewBag.ReturnUrl = returnUrl;
                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                    return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Manage");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new AppUser { UserName = model.Email };//, Email = model.Email };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // GET: /Account/LogOff

        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "News");
        }

        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }


        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion
    }
}