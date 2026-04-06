using System;
using System.ComponentModel.DataAnnotations;

namespace SecurityDashboard.ViewModels {
    public class AddUserViewModel {
        public IFormFile? ProfilePicture { get; set; }

        [Required]
        [MaxLength(20)]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [Compare("Password")]
        public string RepeatPassword { get; set; }
    }
}

