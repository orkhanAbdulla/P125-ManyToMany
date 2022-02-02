using FiorelloBack.Models;
using FiorelloBack.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace FiorelloBack.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        
        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            
        }
        public async Task<IActionResult> Register(RegisterVM register)
        {
            if (!ModelState.IsValid) return View();
            AppUser user = new AppUser()
            {
                Fullname = register.Fullname,
                UserName = register.Username,
                Email = register.Email
            };
            IdentityResult result = await _userManager.CreateAsync(user, register.Password);
            if (!result.Succeeded)
            {
                foreach (IdentityError error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return View();
            }
            string token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            string link = Url.Action(nameof(VetifyEmail),"Account",new { email= user.Email,token},Request.Scheme,Request.Host.ToString());
            MailMessage mail = new MailMessage();
            mail.From = new MailAddress("orkhanabdulla96@gmail.com","Fiorello");
            mail.To.Add(new MailAddress(user.Email));
            mail.Subject = "VerifyEmail";
            string body = string.Empty;
            using (StreamReader reader =new StreamReader("wwwroot/assets/template/verifyemail.html"))
            {
                body=reader.ReadToEnd(); 
            }
            mail.Body = body.Replace("{{link}}", link);
            mail.IsBodyHtml = true;

            SmtpClient smtp = new SmtpClient();
            smtp.Host = "smtp.gmail.com";
            smtp.Port = 587;
            smtp.EnableSsl = true;

            smtp.Credentials = new NetworkCredential("orkhanabdulla96@gmail.com", "orxan6571616");
            smtp.Send(mail);

            await _userManager.AddToRoleAsync(user, "Member");
            return RedirectToAction("Login", "Account");
        }
        public async Task<IActionResult> VetifyEmail()
        {
            return View();
        }
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginVM login)
        {
            if (!ModelState.IsValid) return View();
            var user = await _userManager.FindByNameAsync(login.Username);
            if (user == null)
            {
                ModelState.AddModelError("","Username or password incorrect");
                return View();
            }
            Microsoft.AspNetCore.Identity.SignInResult result =await _signInManager.PasswordSignInAsync(user, login.Password, login.RememberMe,true);
            if (result.IsLockedOut)
            {
                ModelState.AddModelError("", "Your account has been blocked for 5 minutes due to overtrying");
                return View();
            }
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Username or password incorrect");
                return View();
            }

         
            return RedirectToAction("Index","Home");
        }

        public IActionResult ForgetPassword()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgetPassword(AccountVM account)
        {
            AppUser user = await _userManager.FindByEmailAsync(account.AppUser.Email);
            if (user == null) return NotFound();
            string token = await _userManager.GeneratePasswordResetTokenAsync(user);
            string link = Url.Action(nameof(ResetPassword),"Account",new {email=user.Email,token},Request.Scheme,Request.Host.ToString());
            MailMessage mail = new MailMessage();
            mail.From = new MailAddress("orkhanabdulla96@gmail.com","Fiorello");
            mail.To.Add(new MailAddress(user.Email));

            mail.Subject = "Reset Password";
            mail.Body = $"<a href=\"{link}\">Please click here to reset password</a>";
            mail.IsBodyHtml=true;

            SmtpClient smtp = new SmtpClient();
            smtp.Host = "smtp.gmail.com";
            smtp.Port = 587;
            smtp.EnableSsl = true;
            smtp.Credentials = new NetworkCredential("orkhanabdulla96@gmail.com","orxan6571616");
            smtp.Send(mail);
            return RedirectToAction("index", "home");
        }
        public async Task<IActionResult> ResetPassword(string email,string token)
        {
            AppUser user = await _userManager.FindByEmailAsync(email);
            if (user == null) return BadRequest();
            AccountVM accountVM = new AccountVM()
            {
                AppUser = user,
                Token= token
            };
            return View(accountVM);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(AccountVM account)
        {
            AppUser user = await _userManager.FindByEmailAsync(account.AppUser.Email);
            if (user == null) return BadRequest();
            AccountVM accountVM = new AccountVM()
            {
                AppUser = user,
                Token = account.Token
            };
            if (!ModelState.IsValid) return View(accountVM);
            IdentityResult result= await _userManager.ResetPasswordAsync(user, account.Token, account.Password);
            if (!result.Succeeded)
            {
                foreach (IdentityError error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return View(accountVM);
            }
            return RedirectToAction("Index", "Home");
          
           
        }
        public async Task<IActionResult> Logout()
        {
           await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
        public IActionResult Show()
        {
            var name = User.Identity.Name;
            return Content(name);
        }
        [Authorize]
        public async Task<IActionResult> Edit()
        {
            AppUser user = await _userManager.FindByNameAsync(User.Identity.Name);
            UserEditVM EditUser = new UserEditVM()
            {
                Fullname= user.Fullname,
                Username=user.UserName,
                Email=user.Email

            };

            return View(EditUser);
        }
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UserEditVM userEditVM)
        {
            if (!ModelState.IsValid) return View();
            AppUser user = await _userManager.FindByNameAsync(User.Identity.Name);
            UserEditVM EditUser = new UserEditVM()
            {
                Fullname = user.Fullname,
                Username = user.UserName,
                Email = user.Email

            };

            if (user.UserName!=userEditVM.Username&& await _userManager.FindByNameAsync(userEditVM.Username)!=null)
            {
                ModelState.AddModelError("", $"{userEditVM.Username} is alredy taken");
                return View(EditUser);
            }
            if (string.IsNullOrWhiteSpace(userEditVM.CurrentPassword))
            {
                user.UserName = userEditVM.Username;
                user.Email = userEditVM.Email;
                user.Fullname = userEditVM.Fullname;
                await _userManager.UpdateAsync(user);
            }
            else
            {
                user.UserName = userEditVM.Username;
                user.Email = userEditVM.Email;
                user.Fullname = userEditVM.Fullname;

                IdentityResult result=await _userManager.ChangePasswordAsync(user, userEditVM.CurrentPassword, userEditVM.Password);
                if (!result.Succeeded)
                {
                    foreach (IdentityError error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    return View(EditUser);
                }
            }
            if (userEditVM.Password!=null)
            {
                await _signInManager.PasswordSignInAsync(user, userEditVM.Password, false, false);
            }
            await _signInManager.SignInAsync(user,false);


            return RedirectToAction("Index","Home");
        }


    }
}
