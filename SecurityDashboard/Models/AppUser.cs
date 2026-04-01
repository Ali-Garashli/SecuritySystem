using System;
using Microsoft.AspNetCore.Identity;

namespace SecurityDashboard.Models {
    public class AppUser : IdentityUser {
        public string? ProfilePicturePath { get; set; }
    }
}

