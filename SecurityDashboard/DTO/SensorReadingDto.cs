using System;
using System.ComponentModel.DataAnnotations;
using SecurityDashboard.Models;

namespace SecurityDashboard.DTO {
    public class SensorReadingDto {
        [Required]
        [Range(0, 2, ErrorMessage = "SensorType must be 0 (Gas), 1 (Flame), or 2 (Motion).")]
        public SensorType SensorType { get; set; }

        [Required]
        [Range(0, 1023, ErrorMessage = "Reading value must be between 0 and 1023.")]
        public int ReadingValue { get; set; }
    }
}

