// !!!Because of issues with Join in EF core denormalize group, region, category and tag tables!!! //
#define DENORMALIZE
#define INCLUDE_ON

using Contracts;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Services
{
    /// <summary>
    /// ApplicationUser provider.
    /// </summary>
    public class UserProvider : BaseProvider
    {
        private bool _InternalCall = false;
        private readonly IEmailSender _emailSender;

        /// <summary>
        /// ApplicationUser provider constructor.
        /// </summary>
        /// <param name="appContext"></param>
        public UserProvider(WcmsAppContext appContext, IEmailSender emailSender = null)
            : base(appContext)
        {
            _emailSender = emailSender;
        }

        /// <summary>
        /// Count users.
        /// </summary>
        /// <param name="filters"></param>
        /// <returns></returns>
        public async Task<int> Count(Dictionary<string, object> filters)
        {
            try
            {
                // Checking...
                if ((AppContext?.IsValid() ?? false) == false)
                {
                    _Log?.LogError("Failed to get users count: Invalid contexts.");
                    LastError = UserMessage.InternalError;
                    return 0;
                }
                // Return pages...
                return await UserAuthorizationHandler.Count(AppContext, filters);
            }
            catch (Exception e)
            {
                _Log?.LogError("Failed to get users count.", e);
                LastError = UserMessage.UnexpectedError;
                return 0;
            }
        }

        /// <summary>
        /// Get users.
        /// </summary>
        /// <param name="filters"></param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <param name="allFields"></param>
        /// <returns></returns>
        public async Task<IEnumerable<ApplicationUser>> Get(Dictionary<string, object> filters, int skip = 0, int take = 20, bool allFields = false)
        {
            try
            {
                _Log?.LogDebug("Getting users...");
                // Checking...
                if ((AppContext?.IsValid() ?? false) == false)
                {
                    _Log?.LogError("Failed to get users: Invalid contexts.");
                    LastError = UserMessage.InternalError;
                    return null;
                }
                // Return pages...
                return await UserAuthorizationHandler.Get(AppContext, filters, skip, take, allFields);
            }
            catch (Exception e)
            {
                _Log?.LogError("Failed to get users.", e);
                LastError = UserMessage.UnexpectedError;
                return null;
            }
        }

        /// <summary>
        /// Get a user from its id.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="hidePassword"></param>
        /// <returns></returns>
        public async Task<ApplicationUser> Get(string id, bool hidePassword = true)
        {
            try
            {
                _Log?.LogDebug("Getting user of id {0}.", id);
                // Checking...
                if ((AppContext?.IsValid(false) ?? false) == false)
                {
                    _Log?.LogError("Failed to get user: Invalid context.");
                    LastError = UserMessage.InternalError;
                    // Trace performance and exit...
                    AppContext?.AddPerfLog("UserProvider::Get::Invalid context");
                    return null;
                }
                else if (AppContext.AuthzProvider == null)
                {
                    _Log?.LogError("Failed to get user: Invalid authz provider.");
                    LastError = UserMessage.InternalError;
                    // Trace performance and exit...
                    AppContext?.AddPerfLog("UserProvider::Get::Invalid authz provider");
                    return null;
                }
                else if (string.IsNullOrEmpty(id) == true)
                {
                    _Log?.LogError("Failed to get user: Invalid user if.");
                    LastError = UserMessage.InternalError;
                    // Trace performance and exit...
                    AppContext?.AddPerfLog("UserProvider::Get::Invalid user id");
                    return null;
                }

                // Query the DB to get the specified user...
                ApplicationUser user = await AppContext.AppDbContext.Users
                    .Include(p => p.Claims)
                    .SingleOrDefaultAsync(u => u.Id == id);
                if (user != null)
                {
                    // Provide the site on which we make the request...
                    user.RequestSite = AppContext.Site;
                    // Check for authorization...
                    if (((await AppContext.AuthzProvider.AuthorizeAsync(AppContext.UserPrincipal,
                        user, new List<OperationAuthorizationRequirement>() { new OperationAuthorizationRequirement { Name = AuthorizationRequirement.Read } }))?.Succeeded ?? false) == false)
                    {
                        _Log?.LogWarning("Failed to get user {0}: Access denied.", id);
                        LastError = UserMessage.AccessDenied;
                        // Trace performance and exit...
                        AppContext?.AddPerfLog("UserProvider::Get::Access denied");
                        return null;
                    }
                }
                // Hide the password...
                if (_InternalCall == true && hidePassword == false)
                {
                    // Do not hide the password.
                }
                else
                {
                    user.PasswordHash = null;
                }

                // Trace performance and exit...
                AppContext?.AddPerfLog("UserProvider::Get");
                return user;
            }
            catch (Exception e)
            {
                _Log?.LogError("Failed to get user {0}: {1}.", id, e.Message);
                LastError = UserMessage.UnexpectedError;
                // Trace performance and exit...
                AppContext?.AddPerfLog("UserProvider::Get::Exception");
                return null;
            }
        }

        /// <summary>
        /// Register a user.
        /// </summary>
        /// <param name="fname"></param>
        /// <param name="lname"></param>
        /// <param name="email"></param>
        /// <param name="phone"></param>
        /// <param name="zip"></param>
        /// <param name="password"></param>
        /// <param name="confirmPassword"></param>
        /// <param name="Url"></param>
        /// <returns></returns>
        public async Task<IdentityResult> Register(string fname, string lname, string email, string phone, string zip, 
            string password, string confirmPassword, Microsoft.AspNetCore.Mvc.IUrlHelper Url)
        {
            try
            {
                // Checking...
                if ((AppContext?.IsValid(false) ?? false) == false)
                {
                    _Log?.LogError("Failed to register {0}: Invalid context.", email);
                    LastError = UserMessage.InternalError;
                    // Trace performance and exit...
                    AppContext?.AddPerfLog("UserProvider::Register::Invalid context");
                    return IdentityResult.Failed(new IdentityError { Code = "ERR", Description = LastError });
                }
                else if (AppContext.UserManager == null || AppContext.SignInManager == null)
                {
                    _Log?.LogError("Failed to register {0}: Invalid sign provider.", email);
                    LastError = UserMessage.InternalError;
                    // Trace performance and exit...
                    AppContext.AddPerfLog("UserProvider::Register::Invalid sign provider");
                    return IdentityResult.Failed(new IdentityError { Code = "ERR", Description = LastError });
                }
                else if (string.IsNullOrEmpty(fname) == true
                    || string.IsNullOrEmpty(lname) == true
                    || string.IsNullOrEmpty(email) == true
                    || string.IsNullOrEmpty(password) == true
                    || string.IsNullOrEmpty(confirmPassword) == true)
                {
                    _Log?.LogError("Failed to register {0}: Invalid inputs.", email);
                    LastError = UserMessage.InvalidInputs;
                    // Trace performance and exit...
                    AppContext.AddPerfLog("UserProvider::Register::Invalid inputs");
                    return IdentityResult.Failed(new IdentityError { Code = "ERR", Description = LastError });
                }
                else if (password != confirmPassword)
                {
                    _Log?.LogError("Failed to register {0}: Invalid passwords.", email);
                    LastError = UserMessage.PasswordsDontMatch;
                    // Trace performance and exit...
                    AppContext.AddPerfLog("UserProvider::Register::Invalid passwords");
                    return IdentityResult.Failed(new IdentityError { Code = "ERR", Description = LastError });
                }

                var user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    SiteId = AppContext.Site.Id,
                    Group1 = -1,
                    Group2 = -1,
                    Group3 = -1,
                    Group4 = -1,
                    Group5 = -1,
                    Group6 = -1,
                    Group7 = -1,
                    Group8 = -1,
                    Group9 = -1,
                    Group10 = -1,
                    Region1 = (AppContext.Region?.Id ?? -1),
                    Region2 = -1,
                    Region3 = -1,
                    Region4 = -1,
                    Region5 = -1,
                    Region6 = -1,
                    Region7 = -1,
                    Region8 = -1,
                    Region9 = -1,
                    Region10 = -1,
                    Enabled = AppContext.Site?.IsPublicRegistration ?? false,
                };
                var result = await AppContext.UserManager.CreateAsync(user, password);
                if (result.Succeeded == true)
                {
                    // Add user claims...
                    AppContext.AppDbContext.UserClaims.Add(new IdentityUserClaim<string>
                    {
                        UserId = user.Id,
                        ClaimType = UserClaimType.FirstName,
                        ClaimValue = fname,
                    });
                    AppContext.AppDbContext.UserClaims.Add(new IdentityUserClaim<string>
                    {
                        UserId = user.Id,
                        ClaimType = UserClaimType.LastName,
                        ClaimValue = lname,
                    });
                    if (string.IsNullOrEmpty(phone) == false)
                    {
                        AppContext.AppDbContext.UserClaims.Add(new IdentityUserClaim<string>
                        {
                            UserId = user.Id,
                            ClaimType = UserClaimType.Phone,
                            ClaimValue = phone,
                        });
                    }
                    if (string.IsNullOrEmpty(zip) == false)
                    {
                        AppContext.AppDbContext.UserClaims.Add(new IdentityUserClaim<string>
                        {
                            UserId = user.Id,
                            ClaimType = UserClaimType.Zip,
                            ClaimValue = zip,
                        });
                    }
                    // Commit DB changes...
                    if (await AppContext.AppDbContext.SaveChangesAsync() == 0)
                    {
                        // Something failed...
                        {
                            // Delete the added user...
                            AppContext.AppDbContext.Users.Remove(user);
                            await AppContext.AppDbContext.SaveChangesAsync();
                        }
                        _Log?.LogError("Failed to register {0}: DB access failure.", email);
                        LastError = UserMessage.InternalError;
                        // Trace performance and exit...
                        AppContext.AddPerfLog("UserProvider::Register::Failed to save user claims");
                        return IdentityResult.Failed(new IdentityError { Code = "ERR", Description = LastError });
                    }
                    // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=532713
                    // Send an email to confirm the account...
                    var code = await AppContext.UserManager.GenerateEmailConfirmationTokenAsync(user);
                    var callbackUrl = Url?.Action(
                        new Microsoft.AspNetCore.Mvc.Routing.UrlActionContext
                        {
                            Action = "ConfirmEmail",
                            Controller = "Account",
                            Values = new { userId = user.Id, code = code },
                            Protocol = "https"/*HttpContext.Request.Scheme*/
                        });
                    if (_emailSender != null)
                    {
                        await _emailSender.SendEmailAsync(user.Email, "Confirmez votre compte",
                            $"Veuillez confirmer votre compte en cliquant <a href='{callbackUrl}'>ICI</a>.");
                        await _emailSender.SendEmailAsync(await _GetAdmin(user.Region1), $"Enregistrement de {user.Email} - {user.Id}",
                            $"Enregistrement de {user.Email} (Id:{user.Id}).");
                    }
                    if ((AppContext.Site?.IsPublicRegistration ?? false) == true)
                    {
                        await AppContext.SignInManager.SignInAsync(user, isPersistent: false);
                    }
                    else
                    {
                        return IdentityResult.Failed(new IdentityError { Code = "ACTNEED", Description = string.Empty });
                    }
                }

                // Trace performance and exit...
                AppContext?.AddPerfLog("UserProvider::Register");
                return result;
            }
            catch (Exception e)
            {
                _Log?.LogError("Failed to register {0}: {1}.", email, e.Message);
                LastError = UserMessage.UnexpectedError;
                // Trace performance and exit...
                AppContext?.AddPerfLog("UserProvider::Register::Exception");
                return IdentityResult.Failed(new IdentityError { Code = "ERR", Description = LastError });
            }
        }

        /// <summary>
        /// Confirm user registration.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="code"></param>
        /// <param name="Url"></param>
        /// <returns></returns>
        public async Task<IdentityResult> Confirm(string id, string code, Microsoft.AspNetCore.Mvc.IUrlHelper Url)
        {
            try
            {
                // Checking...
                if ((AppContext?.IsValid(false) ?? false) == false)
                {
                    _Log?.LogError("Failed to confirm user {0}: Invalid context.", id);
                    LastError = UserMessage.InternalError;
                    // Trace performance and exit...
                    AppContext?.AddPerfLog("UserProvider::Confirm::Invalid context");
                    return IdentityResult.Failed(new IdentityError { Code = "ERR", Description = LastError });
                }
                else if (AppContext.UserManager == null || AppContext.SignInManager == null)
                {
                    _Log?.LogError("Failed to confirm user {0}: Invalid sign provider.", id);
                    LastError = UserMessage.InternalError;
                    // Trace performance and exit...
                    AppContext.AddPerfLog("UserProvider::Confirm::Invalid sign provider");
                    return IdentityResult.Failed(new IdentityError { Code = "ERR", Description = LastError });
                }
                else if (string.IsNullOrEmpty(id) == true
                    || string.IsNullOrEmpty(code) == true)
                {
                    _Log?.LogError("Failed to confirm user {0}: Invalid inputs.", id);
                    LastError = UserMessage.InvalidInputs;
                    // Trace performance and exit...
                    AppContext.AddPerfLog("UserProvider::Confirm::Invalid inputs");
                    return IdentityResult.Failed(new IdentityError { Code = "ERR", Description = LastError });
                }

                // Query the DB to get the specified user...
                ApplicationUser user = await AppContext.AppDbContext.Users
                    //.Include(p => p.Claims)
                    .SingleOrDefaultAsync(u => u.Id == id && u.SiteId == AppContext.Site.Id);
                if (user == null)
                {
                    _Log?.LogError("Failed to confirm user {0} password: Invalid user.", id);
                    LastError = UserMessage.InvalidInputs;
                    // Trace performance and exit...
                    AppContext.AddPerfLog("UserProvider::Confirm::Invalid user");
                    return IdentityResult.Failed(new IdentityError { Code = "ERR", Description = LastError });
                }

                // Confirm user account...
                var result = await AppContext.UserManager.ConfirmEmailAsync(user, code);
                if (result.Succeeded == true)
                {
                    await _emailSender.SendEmailAsync(await _GetAdmin(user.Region1), $"Confirmation d'enregistrement de {user.Email} - {user.Id}",
                                $"Confirmation d'enregistrement de {user.Email} (Id:{user.Id}).");
                }

                // Trace performance and exit...
                AppContext?.AddPerfLog("UserProvider::Confirm");
                return result;
            }
            catch (Exception e)
            {
                _Log?.LogError("Failed to register {0}: {1}.", id, e.Message);
                LastError = UserMessage.UnexpectedError;
                // Trace performance and exit...
                AppContext?.AddPerfLog("UserProvider::Confirm::Exception");
                return IdentityResult.Failed(new IdentityError { Code = "ERR", Description = LastError });
            }
        }

        /// <summary>
        /// Signin a user.
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <param name="rememberMe"></param>
        /// <returns></returns>
        public async Task<SignInResult> SignIn(string email, string password, bool rememberMe)
        {
            try
            {
                string perfString = null;
                // Checking...
                if ((AppContext?.IsValid(false) ?? false) == false)
                {
                    _Log?.LogError("Failed to sign {0}: Invalid context.", email);
                    LastError = UserMessage.InternalError;
                    // Trace performance and exit...
                    AppContext?.AddPerfLog("UserProvider::SignIn::Invalid context");
                    return SignInResult.Failed;
                }
                else if (AppContext.UserManager == null || AppContext.SignInManager == null)
                {
                    _Log?.LogError("Failed to sign {0}: Invalid sign provider.", email);
                    LastError = UserMessage.InternalError;
                    // Trace performance and exit...
                    AppContext.AddPerfLog("UserProvider::SignIn::Invalid sign provider");
                    return SignInResult.Failed;
                }
                else if (string.IsNullOrEmpty(email) == true
                    || string.IsNullOrEmpty(password) == true)
                {
                    _Log?.LogError("Failed to sign {0}: Invalid inputs.", email);
                    LastError = UserMessage.InvalidInputs;
                    // Trace performance and exit...
                    AppContext.AddPerfLog("UserProvider::SignIn::Invalid inputs");
                    return SignInResult.Failed;
                }

                // Query the DB to get the specified user...
                ApplicationUser user = await AppContext.AppDbContext.Users
                    .Include(p => p.Claims)
                    .SingleOrDefaultAsync(u => u.Email == email && u.SiteId == AppContext.Site.Id);

                // If needed, check if the user has been enabled...
                if (AppContext.Site.IsPublicRegistration == false
                    && user.Enabled == false)
                {
                    _Log?.LogInformation("{0} is not enabled.", email);
                    LastError = UserMessage.UserNotEnabled;
                    // Trace performance and exit...
                    AppContext.AddPerfLog("UserProvider::SignIn::NotEnabled");
                    return SignInResult.NotAllowed;
                }
                else
                {
                    // This doesn't count login failures towards account lockout
                    // To enable password failures to trigger account lockout, set lockoutOnFailure: true
                    var result = await AppContext.SignInManager.PasswordSignInAsync(user, password, rememberMe, lockoutOnFailure: AppContext.Site.LockoutUserOnFailure);
                    if (result.Succeeded == true)
                    {
                        _Log?.LogInformation("{0} signed in.", email);
                        // Trace performance and exit...
                        perfString = "UserProvider::SignIn";
                    }
                    else if (result.RequiresTwoFactor)
                    {
                        _Log?.LogInformation("Two factor Required for {0}.", email);
                        LastError = UserMessage.UserRequiresTwoFactor;
                        // Trace performance and exit...
                        perfString = "UserProvider::SignIn::RequiresTwoFactor";
                    }
                    else if (result.IsLockedOut)
                    {
                        _Log?.LogInformation("{0} account is locked out.", email);
                        LastError = UserMessage.UserLockedOut;
                        // Trace performance and exit...
                        perfString = "UserProvider::SignIn::IsLockedOut";
                    }
                    else
                    {
                        _Log?.LogError("Failed to sign {0}: Invalid password.", email);
                        LastError = UserMessage.InvalidUserEmailPassword;
                        // Trace performance and exit...
                        perfString = "UserProvider::SignIn::Invalid email and\\or password";
                    }
                    // Trace performance and exit...
                    AppContext.AddPerfLog(perfString);
                    return result;

                    // return IsLockedOut ? "Lockedout" : 
                    // IsNotAllowed ? "NotAllowed" : 
                    // RequiresTwoFactor ? "RequiresTwoFactor" : 
                    // Succeeded ? "Succeeded" : "Failed";
                }
            }
            catch (Exception e)
            {
                _Log?.LogError("Failed to sign {0}: {1}.", email, e.Message);
                LastError = UserMessage.UnexpectedError;
                // Trace performance and exit...
                AppContext?.AddPerfLog("UserProvider::SignIn::Exception");
                return SignInResult.Failed;
            }
        }

        /// <summary>
        /// Signout a user.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task SignOut(HttpContext context)
        {
            try
            {
                // Checking...
                if ((AppContext?.IsValid(false) ?? false) == false)
                {
                    _Log?.LogError("Failed to signout: Invalid context.");
                    LastError = UserMessage.InternalError;
                    // Trace performance and exit...
                    AppContext?.AddPerfLog("UserProvider::SignOut::Invalid context");
                }
                // Sign out...
                else if (await AppContext.SignOut(context) == false)
                {
                    _Log?.LogError("Failed to signout: Invalid sign provider.");
                    LastError = UserMessage.InternalError;
                    // Trace performance and exit...
                    AppContext.AddPerfLog("UserProvider::SignOut::Invalid sign provider");
                }
                else
                {
                    // Trace performance and exit...
                    AppContext.AddPerfLog("UserProvider::SignOut");
                }
            }
            catch (Exception e)
            {
                _Log?.LogError("Failed to signout: {0}.", e.Message);
                LastError = UserMessage.UnexpectedError;
                // Trace performance and exit...
                AppContext?.AddPerfLog("UserProvider::SignOut::Exception");
            }
        }

        /// <summary>
        /// Retrieve user password.
        /// </summary>
        /// <param name="email"></param>
        /// <param name="Url"></param>
        /// <returns></returns>
        public async Task ForgotPassword(string email, Microsoft.AspNetCore.Mvc.IUrlHelper Url)
        {
            try
            {
                // Checking...
                if ((AppContext?.IsValid(false) ?? false) == false)
                {
                    _Log?.LogError("Failed to retrieve {0} password: Invalid context.", email);
                    LastError = UserMessage.InternalError;
                    // Trace performance and exit...
                    AppContext?.AddPerfLog("UserProvider::ForgotPassword::Invalid context");
                    return;
                }
                else if (AppContext.UserManager == null || AppContext.SignInManager == null)
                {
                    _Log?.LogError("Failed to retrieve {0} password: Invalid sign provider.", email);
                    LastError = UserMessage.InternalError;
                    // Trace performance and exit...
                    AppContext.AddPerfLog("UserProvider::ForgotPassword::Invalid sign provider");
                    return;
                }
                else if (string.IsNullOrEmpty(email) == true)
                {
                    _Log?.LogError("Failed to retrieve {} password: Invalid inputs.", email);
                    LastError = UserMessage.InvalidInputs;
                    // Trace performance and exit...
                    AppContext.AddPerfLog("UserProvider::ForgotPassword::Invalid inputs");
                    return;
                }

                // Query the DB to get the specified user...
                ApplicationUser user = await AppContext.AppDbContext.Users
                    //.Include(p => p.Claims)
                    .SingleOrDefaultAsync(u => u.Email == email && u.SiteId == AppContext.Site.Id);
                if (user == null)
                {
                    _Log?.LogError("Failed to retrieve {0} password: Invalid user.", email);
                    LastError = UserMessage.InvalidInputs;
                    // Trace performance and exit...
                    AppContext.AddPerfLog("UserProvider::ForgotPassword::Invalid user");
                    return;
                }
                else if (!(await AppContext.UserManager.IsEmailConfirmedAsync(user)))
                {
                    _Log?.LogError("Failed to retrieve {0} password: Not confirmed.", email);
                    LastError = UserMessage.InvalidInputs;
                    // Trace performance and exit...
                    AppContext.AddPerfLog("UserProvider::ForgotPassword::Not confirmed");
                    return;
                }

                // Send an email to reset the password...
                var code = await AppContext.UserManager.GeneratePasswordResetTokenAsync(user);
                var callbackUrl = Url?.Action(
                    new Microsoft.AspNetCore.Mvc.Routing.UrlActionContext
                    {
                        Action = "ResetPassword",
                        Controller = "Account",
                        Values = new { userId = user.Id, code = code },
                        Protocol = "https"/*HttpContext.Request.Scheme*/
                    });
                if (_emailSender != null)
                {
                    await _emailSender.SendEmailAsync(user.Email, "réinitialiser le mot de passe",
                    $"Veuillez réinitialiser votre mot de passe en cliquant <a href='{callbackUrl}'>ICI</a>.");
                }

                // Trace performance and exit...
                AppContext?.AddPerfLog("UserProvider::ForgotPassword");
            }
            catch (Exception e)
            {
                _Log?.LogError("Failed to retrieve {0} password: {1}.", email, e.Message);
                LastError = UserMessage.UnexpectedError;
                // Trace performance and exit...
                AppContext?.AddPerfLog("UserProvider::ForgotPassword::Exception");
            }
        }

        /// <summary>
        /// Reset user password.
        /// </summary>
        /// <param name="email"></param>
        /// <param name="Url"></param>
        /// <returns></returns>
        public async Task<IdentityResult> ResetPassword(string email, string code, string password,
            Microsoft.AspNetCore.Mvc.IUrlHelper Url)
        {
            try
            {
                // Checking...
                if ((AppContext?.IsValid(false) ?? false) == false)
                {
                    _Log?.LogError("Failed to reset {0} password: Invalid context.", email);
                    LastError = UserMessage.InternalError;
                    // Trace performance and exit...
                    AppContext?.AddPerfLog("UserProvider::ResetPassword::Invalid context");
                    return IdentityResult.Failed(new IdentityError { Code = "ERR", Description = LastError });
                }
                else if (AppContext.UserManager == null || AppContext.SignInManager == null)
                {
                    _Log?.LogError("Failed to reset {0} password: Invalid sign provider.", email);
                    LastError = UserMessage.InternalError;
                    // Trace performance and exit...
                    AppContext.AddPerfLog("UserProvider::ResetPassword::Invalid sign provider");
                    return IdentityResult.Failed(new IdentityError { Code = "ERR", Description = LastError });
                }
                else if (string.IsNullOrEmpty(email) == true)
                {
                    _Log?.LogError("Failed to reset {0} password: Invalid inputs.", email);
                    LastError = UserMessage.InvalidInputs;
                    // Trace performance and exit...
                    AppContext.AddPerfLog("UserProvider::ResetPassword::Invalid inputs");
                    return IdentityResult.Failed(new IdentityError { Code = "ERR", Description = LastError });
                }

                // Query the DB to get the specified user...
                ApplicationUser user = await AppContext.AppDbContext.Users
                    //.Include(p => p.Claims)
                    .SingleOrDefaultAsync(u => u.Email == email && u.SiteId == AppContext.Site.Id);
                if (user == null)
                {
                    _Log?.LogError("Failed to reset {0} password: Invalid user.", email);
                    LastError = UserMessage.InvalidInputs;
                    // Trace performance and exit...
                    AppContext.AddPerfLog("UserProvider::ResetPassword::Invalid user");
                    return IdentityResult.Failed(new IdentityError { Code = "ERR", Description = LastError });
                }
                else if (!(await AppContext.UserManager.IsEmailConfirmedAsync(user)))
                {
                    _Log?.LogError("Failed to reset {0} password: Not confirmed.", email);
                    LastError = UserMessage.InvalidInputs;
                    // Trace performance and exit...
                    AppContext.AddPerfLog("UserProvider::ResetPassword::Not confirmed");
                    return IdentityResult.Failed(new IdentityError { Code = "ERR", Description = LastError });
                }

                var result = await AppContext.UserManager.ResetPasswordAsync(user, code, password);

                // Trace performance and exit...
                AppContext?.AddPerfLog("UserProvider::ResetPassword");
                return result;
            }
            catch (Exception e)
            {
                _Log?.LogError("Failed to reset {0} password: {1}.", email, e.Message);
                LastError = UserMessage.UnexpectedError;
                // Trace performance and exit...
                return IdentityResult.Failed(new IdentityError { Code = "ERR", Description = LastError });
            }
        }

        /// <summary>
        /// Update the user.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="fname"></param>
        /// <param name="lname"></param>
        /// <param name="phone"></param>
        /// <param name="zip"></param>
        /// <param name="regions"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public async Task<ApplicationUser> Update(string id, string fname, string lname, string phone, string zip, IEnumerable<int> regions, string password)
        {
            try
            {
                ApplicationUser data = null;
                ApplicationUser user = null;

                // Checking...
                if ((AppContext?.IsValid(false) ?? false) == false)
                {
                    _Log?.LogError("Failed to update user {0}: Invalid context.", id);
                    LastError = UserMessage.InternalError;
                    // Trace performance and exit...
                    AppContext?.AddPerfLog("UserProvider::Update::Invalid context");
                    return null;
                }
                else if (AppContext.AuthzProvider == null)
                {
                    _Log?.LogError("Failed to update user {0}: Invalid authz provider.", id);
                    LastError = UserMessage.InternalError;
                    // Trace performance and exit...
                    AppContext?.AddPerfLog("UserProvider::Update::Invalid authz provider");
                    return null;
                }
                else if ((user = await AppContext.GetUser()) == null)
                {
                    _Log?.LogError("Failed to update user {0}: Invalid user.", id);
                    LastError = UserMessage.AccessDenied;
                    // Trace performance and exit...
                    AppContext?.AddPerfLog("UserProvider::Update::Invalid user");
                    return null;
                }
                // Retrieve the specified user...
                _InternalCall = true;
                {
                    data = await Get(id, false);
                }
                _InternalCall = false;
                if (data == null)
                {
                    _Log?.LogError("Failed to update user {0}: Invalid user id.", id);
                    LastError = UserMessage.InvalidInputs;
                    // Trace performance and exit...
                    AppContext?.AddPerfLog("UserProvider::Update::Invalid user id");
                    return null;
                }

                // Check for authorization...
                data.RequestSite = AppContext.Site;
                if (((await AppContext.AuthzProvider.AuthorizeAsync(AppContext.UserPrincipal,
                    data,
                    new List<OperationAuthorizationRequirement>()
                    {
                        new OperationAuthorizationRequirement
                        {
                            Name = AuthorizationRequirement.Update
                        }
                    }))?.Succeeded ?? false) == false)
                {
                    _Log?.LogWarning("Failed to update user {0}: Access denied.", id);
                    LastError = UserMessage.AccessDenied;
                    // Trace performance and exit...
                    AppContext?.AddPerfLog("UserProvider::Update::Access denied");
                    return null;
                }

                // Apply change to the user...
                if (regions != null && regions.Count() > 0)
                {
                    int[] regs = regions.ToArray();
                    data.Region1 = (regions.Count() > 1) ? regs[0] : -1;
                    data.Region2 = (regions.Count() > 2) ? regs[1] : -1;
                    data.Region3 = (regions.Count() > 3) ? regs[2] : -1;
                    data.Region4 = (regions.Count() > 4) ? regs[3] : -1;
                    data.Region5 = (regions.Count() > 5) ? regs[4] : -1;
                    data.Region6 = (regions.Count() > 6) ? regs[5] : -1;
                    data.Region7 = (regions.Count() > 7) ? regs[6] : -1;
                    data.Region8 = (regions.Count() > 8) ? regs[7] : -1;
                    data.Region9 = (regions.Count() > 8) ? regs[8] : -1;
                    data.Region10 = (regions.Count() > 10) ? regs[9] : -1;
                }
                if (data.Claims != null)
                {
                    foreach (IdentityUserClaim<string> claim in data.Claims)
                    {
                        if (claim.ClaimType == UserClaimType.FirstName && string.IsNullOrEmpty(fname) == false)
                        {
                            claim.ClaimValue = fname;
                        }
                        else if (claim.ClaimType == UserClaimType.LastName && string.IsNullOrEmpty(lname) == false)
                        {
                            claim.ClaimValue = lname;
                        }
                        else if (claim.ClaimType == UserClaimType.Phone && string.IsNullOrEmpty(phone) == false)
                        {
                            claim.ClaimValue = phone;
                        }
                        else if (claim.ClaimType == UserClaimType.Zip && string.IsNullOrEmpty(zip) == false)
                        {
                            claim.ClaimValue = zip;
                        }
                    }
                }

                /*// Commit DB changes...
                if (await AppContext.AppDbContext.SaveChangesAsync() == 0)
                {
                    _Log?.LogError($"Failed to update user {id}: DB access failure.");
                    LastError = UserMessage.InternalError;
                    // Trace performance and exit...
                    AppContext?.AddPerfLog("UserProvider::Update::DB access failure");
                    return null;
                }*/

                // Update history...
                _UpdateHistory(id, user, data, SiteActionType.Modification);

                // Commit DB changes...
                if (await AppContext.AppDbContext.SaveChangesAsync() == 0)
                {
                    _Log?.LogError("Failed to update user {0}: DB access failure.", id);
                    LastError = UserMessage.InternalError;
                    // Trace performance and exit...
                    AppContext?.AddPerfLog("UserProvider::Update::DB access failure");
                    return null;
                }

                // Hide the password...
                user.PasswordHash = null;

                // Trace performance and exit...
                AppContext?.AddPerfLog("UserProvider::Update");
                // Return the updated user...
                return user;
            }
            catch (Exception e)
            {
                _Log?.LogError("Failed to update user {0}: {1}.", id, e.Message);
                LastError = UserMessage.UnexpectedError;
                // Trace performance and exit...
                AppContext?.AddPerfLog("UserProvider::Update::Exception");
                return null;
            }
        }

        /// <summary>
        /// Update user settings.
        /// </summary
        /// <param name="id"></param>
        /// <param name="email"></param>
        /// <param name="groups"></param>
        /// <param name="enabled"></param>
        /// <returns></returns>
        public async Task<ApplicationUser> Update(string id, string email, IEnumerable<int> groups, bool enabled)
        {
            try
            {
                bool prevEnabled = false;
                ApplicationUser data = null;
                ApplicationUser user = null;

                // Checking...
                if ((AppContext?.IsValid(false) ?? false) == false)
                {
                    _Log?.LogError("Failed to update user {0}: Invalid context.", id);
                    LastError = UserMessage.InternalError;
                    // Trace performance and exit...
                    AppContext?.AddPerfLog("UserProvider::Update::Invalid context");
                    return null;
                }
                else if (AppContext.AuthzProvider == null)
                {
                    _Log?.LogError("Failed to update user {0}: Invalid authz provider.", id);
                    LastError = UserMessage.InternalError;
                    // Trace performance and exit...
                    AppContext?.AddPerfLog("UserProvider::Update::Invalid authz provider");
                    return null;
                }
                else if ((user = await AppContext.GetUser()) == null)
                {
                    _Log?.LogError("Failed to update user {0}: Invalid user.", id);
                    LastError = UserMessage.AccessDenied;
                    // Trace performance and exit...
                    AppContext?.AddPerfLog("UserProvider::Update::Invalid user");
                    return null;
                }
                // Retrieve the specified user...
                _InternalCall = true;
                {
                    data = await Get(id, false);
                }
                _InternalCall = false;
                if (data == null)
                {
                    _Log?.LogError("Failed to update user {0}: Invalid user id.", id);
                    LastError = UserMessage.InvalidInputs;
                    // Trace performance and exit...
                    AppContext?.AddPerfLog("UserProvider::Update::Invalid user id");
                    return null;
                }

                // Check for authorization...
                data.RequestSite = AppContext.Site;
                if (((await AppContext.AuthzProvider.AuthorizeAsync(AppContext.UserPrincipal,
                    data,
                    new List<OperationAuthorizationRequirement>()
                    {
                        new OperationAuthorizationRequirement
                        {
                            Name = AuthorizationRequirement.Publish
                        }
                    }))?.Succeeded ?? false) == false)
                {
                    _Log?.LogWarning("Failed to update user {0}: Access denied.", id);
                    LastError = UserMessage.AccessDenied;
                    // Trace performance and exit...
                    AppContext?.AddPerfLog("UserProvider::Update::Access denied");
                    return null;
                }

                // Apply change to the user...
                if (string.IsNullOrEmpty(email) == false)
                {
                    data.Email = email;
                    data.NormalizedEmail = email.ToUpper();
                    data.NormalizedUserName = email.ToUpper();
                    data.UserName = email;
                }
                if (groups != null && groups.Count() > 0)
                {
                    int[] grps = groups.ToArray();
                    data.Group1 = (groups.Count() > 1) ? grps[0] : -1;
                    data.Group2 = (groups.Count() > 2) ? grps[1] : -1;
                    data.Group3 = (groups.Count() > 3) ? grps[2] : -1;
                    data.Group4 = (groups.Count() > 4) ? grps[3] : -1;
                    data.Group5 = (groups.Count() > 5) ? grps[4] : -1;
                    data.Group6 = (groups.Count() > 6) ? grps[5] : -1;
                    data.Group7 = (groups.Count() > 7) ? grps[6] : -1;
                    data.Group8 = (groups.Count() > 8) ? grps[7] : -1;
                    data.Group9 = (groups.Count() > 8) ? grps[8] : -1;
                    data.Group10 = (groups.Count() > 10) ? grps[9] : -1;
                }
                prevEnabled = data.Enabled;
                data.Enabled = enabled;

                // Update history...
                _UpdateHistory(id, user, data, SiteActionType.Modification);

                // Commit DB changes...
                if (await AppContext.AppDbContext.SaveChangesAsync() == 0)
                {
                    _Log?.LogError("Failed to update user {0}: DB access failure.", id);
                    LastError = UserMessage.InternalError;
                    // Trace performance and exit...
                    AppContext?.AddPerfLog("UserProvider::Update::DB access failure");
                    return null;
                }

                // Hide the password...
                user.PasswordHash = null;

                // Notifiy user about the activation...
                if (prevEnabled != enabled && enabled == true)
                {
                    await _emailSender.SendEmailAsync(user.Email, "Activation de votre compte", "Votre compte est activé.");
                }

                // Trace performance and exit...
                AppContext?.AddPerfLog("UserProvider::Update");
                // Return the updated user...
                return user;
            }
            catch (Exception e)
            {
                _Log?.LogError("Failed to update user {0}: {1}.", id, e.Message);
                LastError = UserMessage.UnexpectedError;
                // Trace performance and exit...
                AppContext?.AddPerfLog("UserProvider::Update::Exception");
                return null;
            }
        }

        /// <summary>
        /// Add a file to the user.
        /// Always send the cover before the cropped cover.
        /// </summary>
        /// <param name="id">ApplicationUser Id</param>
        /// <param name="fileName"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        public async Task<string> AddCover(string id, string fileName, int fileType, Stream file)
        {
            try
            {
                string ext = string.Empty;
                ApplicationUser data = null;
                ApplicationUser user = null;

                // Checking...
                if ((AppContext?.IsValid(false) ?? false) == false)
                {
                    _Log?.LogError("Failed to add cover to user {0}: Invalid context.", id);
                    LastError = UserMessage.InternalError;
                    // Trace performance and exit...
                    AppContext?.AddPerfLog("UserProvider::AddCover::Invalid context");
                    return null;
                }
                else if (AppContext.AuthzProvider == null)
                {
                    _Log?.LogError("Failed to add cover to user {0}: Invalid authz provider.", id);
                    LastError = UserMessage.InternalError;
                    // Trace performance and exit...
                    AppContext?.AddPerfLog("UserProvider::AddCover::Invalid authz provider");
                    return null;
                }
                else if ((user = await AppContext.GetUser()) == null)
                {
                    _Log?.LogError("Failed to add cover to user {0}: Invalid user.", id);
                    LastError = UserMessage.AccessDenied;
                    // Trace performance and exit...
                    AppContext?.AddPerfLog("UserProvider::AddCover::Invalid user");
                    return null;
                }
                else if (!(fileType == 1 || fileType == 2))
                {
                    _Log?.LogError("Failed to add cover to user {0}: Invalid file type.", id);
                    LastError = UserMessage.AccessDenied;
                    // Trace performance and exit...
                    AppContext?.AddPerfLog("UserProvider::AddCover::Invalid file type");
                    return null;
                }
                // Retrieve the specified user...
                _InternalCall = true;
                {
                    data = await Get(id, false);
                }
                _InternalCall = false;
                if (data == null)
                {
                    _Log?.LogError("Failed to add cover to user {0}: Invalid user id.", id);
                    LastError = UserMessage.InvalidInputs;
                    // Trace performance and exit...
                    AppContext?.AddPerfLog("UserProvider::AddCover::Invalid user id");
                    return null;
                }

                // Check for authorization...
                data.RequestSite = AppContext.Site;
                if (((await AppContext.AuthzProvider.AuthorizeAsync(AppContext.UserPrincipal,
                    data,
                    new List<OperationAuthorizationRequirement>()
                    {
                        new OperationAuthorizationRequirement
                        {
                            Name = AuthorizationRequirement.Update
                        }
                    }))?.Succeeded ?? false) == false)
                {
                    _Log?.LogWarning("Failed to add cover to user {0}: Access denied.", id);
                    LastError = UserMessage.AccessDenied;
                    // Trace performance and exit...
                    AppContext?.AddPerfLog("UserProvider::AddCover::Access denied");
                    return null;
                }

                // Get file extension...
                fileName = fileName.ToLower();
                ext = Path.GetExtension(fileName);

                // Delete previous cover bassed on the previous ext...
                if (fileType == 1)
                {
                    string oldCoverPath = user.GetFilePath(AppContext, "cover" + user.Cover);
                    string oldCroppedPath = user.GetFilePath(AppContext, "cover.crop" + user.Cover);
                    if (File.Exists(oldCoverPath) == true) File.Delete(oldCoverPath);
                    if (File.Exists(oldCroppedPath) == true) File.Delete(oldCroppedPath);
                }
                // Update cover extension...
                if (fileType == 1 && user.Cover != ext)
                {
                    data.Cover = ext;
                    
                    // Update history...
                    _UpdateHistory(id, user, data, SiteActionType.Modification);

                    // Commit DB changes...
                    if (await AppContext.AppDbContext.SaveChangesAsync() == 0)
                    {
                        _Log?.LogError("Failed to add cover {0} of type {1} to user {2}: DB access failure.", fileName, fileType, id);
                        LastError = UserMessage.InternalError;
                        // Trace performance and exit...
                        AppContext?.AddPerfLog("UserProvider::AddCover::DB access failure");
                        return null;
                    }
                }

                // Add file to FS...
                fileName = ((fileType == 1) ? "cover" : "cover.crop") + ext;
                string outPutFile = user.GetFilePath(AppContext, fileName);
                try
                {
                    user.InitDirectory(AppContext);
                    using (FileStream fileStream = System.IO.File.Create(outPutFile))
                    {
                        await file.CopyToAsync(fileStream);
                    }
                    // Trace performance and exit...
                    AppContext?.AddPerfLog("UserProvider::AddCover");
                    // Return file url...
                    return  user.GetFileUrl(fileName);
                }
                catch(Exception edb)
                {
                    _Log?.LogError("Failed to add cover {0} of type {1} to user {2}: {3}.", fileName, fileType, id, edb.Message);
                    LastError = UserMessage.UnexpectedError;
                    // Trace performance and exit...
                    AppContext?.AddPerfLog("UserProvider::AddCover::ExceptionDb");
                    return null;
                }
            }
            catch (Exception e)
            {
                _Log?.LogError("Failed to add cover {0} of type {1} to user {2}: {3}.", fileName, fileType, id, e.Message);
                LastError = UserMessage.UnexpectedError;
                // Trace performance and exit...
                AppContext?.AddPerfLog("UserProvider::AddCover::Exception");
                return null;
            }
        }

        /// <summary>
        /// Delete a file from a user.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<bool> DeleteCover(string id)
        {
            try
            {
                string ext = string.Empty;
                ApplicationUser data = null;
                ApplicationUser user = null;

                // Checking...
                if ((AppContext?.IsValid(false) ?? false) == false)
                {
                    _Log?.LogError("Failed to delete cover from user {0}: Invalid context.", id);
                    LastError = UserMessage.InternalError;
                    // Trace performance and exit...
                    AppContext?.AddPerfLog("UserProvider::DeleteCover::Invalid context");
                    return false;
                }
                else if (AppContext.AuthzProvider == null)
                {
                    _Log?.LogError("Failed to delete cover from user {0}: Invalid authz provider.", id);
                    LastError = UserMessage.InternalError;
                    // Trace performance and exit...
                    AppContext?.AddPerfLog("UserProvider::DeleteCover::Invalid authz provider");
                    return false;
                }
                else if ((user = await AppContext.GetUser()) == null)
                {
                    _Log?.LogError("Failed to delete cover from user {0}: Invalid user.", id);
                    LastError = UserMessage.AccessDenied;
                    // Trace performance and exit...
                    AppContext?.AddPerfLog("UserProvider::DeleteCover::Invalid user");
                    return false;
                }
                // Retrieve the specified user...
                _InternalCall = true;
                {
                    data = await Get(id, false);
                }
                _InternalCall = false;
                if (data == null)
                {
                    _Log?.LogError("Failed to delete cover from user {0}: Invalid user id.", id);
                    LastError = UserMessage.InvalidInputs;
                    // Trace performance and exit...
                    AppContext?.AddPerfLog("UserProvider::DeleteCover::Invalid user id");
                    return false;
                }

                // Check for authorization...
                data.RequestSite = AppContext.Site;
                if (((await AppContext.AuthzProvider.AuthorizeAsync(AppContext.UserPrincipal,
                    data,
                    new List<OperationAuthorizationRequirement>()
                    {
                        new OperationAuthorizationRequirement
                        {
                            Name = AuthorizationRequirement.Update
                        }
                    }))?.Succeeded ?? false) == false)
                {
                    _Log?.LogWarning("Failed to delete cover from user {0}: Access denied.", id);
                    LastError = UserMessage.AccessDenied;
                    // Trace performance and exit...
                    AppContext?.AddPerfLog("UserProvider::DeleteCover::Access denied");
                    return false;
                }

                // Remove cover from the DB...
                ext = user.Cover;
                if (user.Cover != null)
                {
                    user.Cover = null;
                    
                    // Commit DB changes...
                    if ( await AppContext.AppDbContext.SaveChangesAsync() == 0)
                    {
                        _Log?.LogError("Failed to delete cover from user {0}: : DB access failure.", id);
                        LastError = UserMessage.InternalError;
                        // Trace performance and exit...
                        AppContext?.AddPerfLog("UserProvider::DeleteCover::DB access failure");
                        return false;
                    }
                }

                // Remove cover from the FS...
                if (ext != null)
                {
                    string outPutFile1 = user.GetFilePath(AppContext, "cover" + ext);
                    string outPutFile2 = user.GetFilePath(AppContext, "cover.crop" + ext);
                    if (outPutFile1 != null && File.Exists(outPutFile1) == true) File.Delete(outPutFile1);
                    if (outPutFile2 != null && File.Exists(outPutFile2) == true) File.Delete(outPutFile2);
                }

                // Trace performance and exit...
                AppContext?.AddPerfLog("UserProvider::DeleteCover");
                // Return...
                return true;
            }
            catch (Exception e)
            {
                _Log?.LogError("Failed to delete cover from user {0}: {1}.", id, e.Message);
                LastError = UserMessage.UnexpectedError;
                // Trace performance and exit...
                AppContext?.AddPerfLog("UserProvider::DeleteCover::Exception");
                return false;
            }
        }

        /// <summary>
        /// Convert filter got from a request.
        /// </summary>
        /// <param name="filters"></param>
        /// <returns></returns>
        public Dictionary<string, object> ConvertFilter(Dictionary<string, string> filters)
        {
            return UserAuthorizationHandler.ConvertFilter(AppContext, filters);
        }

        /// <summary>
        /// Update user history.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="user"></param>
        /// <param name="dataUser"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        private void _UpdateHistory(string id, ApplicationUser user, ApplicationUser dataUser, string action)
        {
            AppContext.AppDbContext.SiteActions.Add(new SiteAction()
            {
                Table = "ApplicationUser",
                Element = dataUser.Id,
                Type = action,
                Actor = user,
                ActionDate = DateTime.Now,
                SiteId = AppContext.Site.Id
            });
        }

        private async Task<List<string>> _GetAdmin(int region)
        {
            var adminClaims = await AppContext?.AppDbContext?.UserClaims.Where(uc
                => uc.ClaimType == UserClaimType.Role && uc.ClaimValue == ClaimValueRole.Administrator)
                .Select(uc => uc.UserId).ToListAsync();
            if (adminClaims != null && adminClaims.Count != 0)
            {
                return await AppContext?.AppDbContext?.Users?.Where(u 
                    => adminClaims.Contains(u.Id) && (u.Region1 == 0 || u.Region1 == region
                        || u.Region2 == 0 || u.Region2 == region
                        || u.Region3 == 0 || u.Region3 == region
                        || u.Region4 == 0 || u.Region4 == region
                        || u.Region5 == 0 || u.Region5 == region
                        || u.Region6 == 0 || u.Region6 == region
                        || u.Region7 == 0 || u.Region7 == region
                        || u.Region8 == 0 || u.Region8 == region
                        || u.Region9 == 0 || u.Region9 == region
                        || u.Region10 == 0 || u.Region10 == region
                    ) && u.Enabled == true && u.EmailConfirmed == true)
                    .Select(uc => uc.Email).ToListAsync();
            }
            return null;
        }
    }
}
