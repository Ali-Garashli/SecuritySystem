using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SecurityDashboard.Models;
using SecurityDashboard.ViewModels;

namespace SecurityDashboard.Controllers {
    public class AccountController : Controller {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;

        public AccountController(UserManager<AppUser> userManager,
                                 SignInManager<AppUser> signInManager) {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public IActionResult SignIn() {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SignIn(SignInViewModel model) {
            if (!ModelState.IsValid)
                return View(model);

            AppUser? user = await _userManager.FindByNameAsync(model.Username);
            if (user == null) {
                ModelState.AddModelError("", "Username or Password is incorrect.");
                return View(model);
            }

            // sign out just in case
            await _signInManager.SignOutAsync();

            var result = await _signInManager.PasswordSignInAsync(user, model.Password, true, true);

            if (!result.Succeeded) {
                ModelState.AddModelError("", "Username or Password is incorrect.");
                return View(model);
            }

            return RedirectToAction("Home", "Dashboard");
        }

        public async Task<IActionResult> LogOut() {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Home", "Dashboard");
        }
    }
}

