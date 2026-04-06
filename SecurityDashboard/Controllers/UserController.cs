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
                UserName = model.Username,
                Email = model.Email
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
        public async Task<IActionResult> ViewUsers() {
            Dictionary<AppUser, bool> usersRoles = new();

            foreach (AppUser user in _userManager.Users.ToList())
                usersRoles.Add(user, await _userManager.IsInRoleAsync(user, "Admin"));

            return View(usersRoles);
        }

        public async Task<IActionResult> FilterUsers(string searchTerm) {
            List<AppUser> users = _userManager.Users.ToList();
            // filter users
            if (!string.IsNullOrEmpty(searchTerm))
                users = users.Where(u => (u.NormalizedUserName ?? "").Contains(searchTerm.ToUpper()) ||
                                         (u.NormalizedEmail ?? "").Contains(searchTerm.ToUpper()))
                             .ToList();

            Dictionary<AppUser, bool> usersRoles = new();
            foreach (AppUser user in users)
                usersRoles.Add(user, await _userManager.IsInRoleAsync(user, "Admin"));

            return PartialView("../Partials/_ViewUsersPartial", usersRoles);
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
                Email = user.Email
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

            if (!string.IsNullOrEmpty(model.Email))
                user.Email = model.Email;

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

            if (!await _userManager.IsInRoleAsync(user, "Admin"))
                await _userManager.DeleteAsync(user);

            return RedirectToAction("ViewUsers");
        }
    }
}

