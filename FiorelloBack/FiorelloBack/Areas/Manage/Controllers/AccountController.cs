using FiorelloBack.Models;
using FiorelloBack.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FiorelloBack.Areas.Manage.Controllers
{
    [Area("Manage")]
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginVM loginVM)
        {
            if (!ModelState.IsValid) return View();
            AppUser user = await _userManager.FindByNameAsync(loginVM.Username);
            if (user==null)
            {
                ModelState.AddModelError("","Username or Password incorrect");
                return View();
            }
            if (!user.isAdmin)
            {
                ModelState.AddModelError("", "Username or Password incorrect");
                return View();
            }
            Microsoft.AspNetCore.Identity.SignInResult result = await _signInManager.PasswordSignInAsync(user, loginVM.Password, loginVM.RememberMe, false);
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Username or Password incorrect");
                return View();
            }
            return RedirectToAction("Index", "Dashboard");

        }
        //public async Task CreateRole()
        //{
        //    await _roleManager.CreateAsync(new IdentityRole("SuperAdmin"));
        //    await _roleManager.CreateAsync(new IdentityRole("Admin"));
        //    await _roleManager.CreateAsync(new IdentityRole("Member"));
        //}
        //public async Task CreateAdmin()
        //{
        //    AppUser user = new AppUser()
        //    {
        //        UserName = "Orkhan.96",
        //        Fullname = "Orkhan Abdullayev",
        //        Email = "orkhan@code.edu.az",
        //        isAdmin = true
        //    };
        //    await _userManager.CreateAsync(user,"orxan6571616");
        //    await _userManager.AddToRoleAsync(user,"SuperAdmin");
           
        //}
    }
}
