using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SecurityDashboard.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using static System.Runtime.InteropServices.JavaScript.JSType;
using SecurityDashboard.Models;
using Microsoft.AspNetCore.Authorization;
using System.Data;

namespace SecurityDashboard.Controllers {
    [Authorize(Roles = "Admin")]
    public class UserController : Controller {
        private readonly UserManager<AppUser> _userManager;

        public UserController(UserManager<AppUser> userManager)
            => _userManager = userManager;

        public IActionResult Add() {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Add(AddUserViewModel model) {
            // check profile picture
            if (model.ProfilePicture != null) {
                var extension = Path.GetExtension(model.ProfilePicture.FileName).ToLower();

                string[] imageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp", ".svg" };

                if (!imageExtensions.Contains(extension))
                    ModelState.AddModelError("ProfilePhoto", "Only image formats are allowed.");
            }

            if (!ModelState.IsValid)
                return View(model);

            AppUser user = new() {
                UserName = model.Username
            };

            // add profile picture
            if (model.ProfilePicture != null) {
                string fileName = Guid.NewGuid() + Path.GetExtension(model.ProfilePicture.FileName);

                string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img", fileName);

                using (FileStream stream = new FileStream(path, FileMode.Create)) {
                    model.ProfilePicture.CopyTo(stream);
                }

                user.ProfilePicturePath = fileName;
            }
            else
                user.ProfilePicturePath = "profile_pic_icon.png";

            IdentityResult result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded) {
                foreach (IdentityError error in result.Errors)
                    ModelState.AddModelError("", error.Description);

                return View(model);
            }

            return RedirectToAction("Home", "Dashboard");
        }

        [HttpGet]
        public IActionResult ViewUsers() {
            return View(_userManager.Users.ToList());
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id) {
            AppUser? user = await _userManager.FindByIdAsync(id ?? "");
            if (user == null)
                return NotFound();

            EditUserViewModel userViewModel = new() {
                Id = user.Id,
                ProfilePicturePath = user.ProfilePicturePath,
                Username = user.UserName,
            };

            return View(userViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EditUserViewModel model) {
            // check profile picture
            if (model.ProfilePicture != null) {
                var extension = Path.GetExtension(model.ProfilePicture.FileName).ToLower();

                string[] imageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp", ".svg" };

                if (!imageExtensions.Contains(extension))
                    ModelState.AddModelError("ProfilePhoto", "Only image formats are allowed.");
            }

            if (!ModelState.IsValid)
                return View(model);

            AppUser user = await _userManager.FindByIdAsync(model.Id ?? "");
            // update name if provided
            if (!string.IsNullOrEmpty(model.Username))
                user.UserName = model.Username;

            // update profile picture if provided
            if (model.ProfilePicture != null) {
                string fileName = Guid.NewGuid() + Path.GetExtension(model.ProfilePicture.FileName);

                string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img", fileName);

                using (FileStream stream = new FileStream(path, FileMode.Create)) {
                    model.ProfilePicture.CopyTo(stream);
                }

                user.ProfilePicturePath = fileName;
            }

            IdentityResult result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded) {
                foreach (IdentityError error in result.Errors)
                    ModelState.AddModelError("", error.Description);

                return View(model);
            }

            // update password if provided
            if (!string.IsNullOrEmpty(model.Password)) {
                await _userManager.RemovePasswordAsync(user);
                await _userManager.AddPasswordAsync(user, model.Password);
            }

            return RedirectToAction("ViewUsers");
        }

        public async Task<IActionResult> Delete(string id) {
            AppUser? user = await _userManager.FindByIdAsync(id ?? "");
            if (user == null)
                return NotFound();

            await _userManager.DeleteAsync(user);

            return RedirectToAction("ViewUsers");
        }
    }
}

