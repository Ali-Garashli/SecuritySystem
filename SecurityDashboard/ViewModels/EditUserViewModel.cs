using System;
using System.ComponentModel.DataAnnotations;

namespace SecurityDashboard.ViewModels {
    public class EditUserViewModel {
        public string Id { get; set; }

        public IFormFile? ProfilePicture { get; set; }
        public string? ProfilePicturePath { get; set; }

        [MaxLength(20)]
        public string? Username { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        [DataType(DataType.Password)]
        public string? Password { get; set; }

        [Compare("Password")]
        public string? RepeatPassword { get; set; }
    }
}

