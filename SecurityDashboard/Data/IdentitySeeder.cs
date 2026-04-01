using System;
using Microsoft.AspNetCore.Identity;
using SecurityDashboard.Models;

namespace SecurityDashboard.Data {
    public static class IdentitySeeder {
        public static async Task SeedAsync(IServiceProvider services) {
            RoleManager<IdentityRole> roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            UserManager<AppUser> userManager = services.GetRequiredService<UserManager<AppUser>>();

            // create roles
            string[] roles = { "Admin", "User" };

            foreach (string role in roles)
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));

            // create admin user
            string adminUsername = "admin";
            var adminPassword = "P@ssw0rd";

            var adminUser = await userManager.FindByNameAsync(adminUsername);

            if (adminUser == null) {
                adminUser = new() {
                    UserName = adminUsername,
                };

                await userManager.CreateAsync(adminUser, adminPassword);
            }

            // assign admin role
            if (!await userManager.IsInRoleAsync(adminUser, "Admin")) {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }
    }
}

