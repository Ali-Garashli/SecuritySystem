using System;
using System.ComponentModel.DataAnnotations;

namespace SecurityDashboard.Models {
    public class AlertSystem {
        [Key]
        public int Id { get; set; } = 1;

        [Required]
        public bool IsArmed { get; set; } = true;

        public bool MotionIsDisabled { get; set; } = false;

        public DateTime SwitchedTime { get; set; } = DateTime.Now;
    }
}

