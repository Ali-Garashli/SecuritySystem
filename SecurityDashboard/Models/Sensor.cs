using System;
using System.ComponentModel.DataAnnotations;

namespace SecurityDashboard.Models {
    public class Sensor {
        [Key]
        public int ID { get; set; }

        [Required]
        public SensorType SensorType { get; set; }

        [Required]
        public double ReadingValue { get; set; }

        [MaxLength(20)]
        public string Unit { get; set; } = "";

        [Required]
        public DateTime ReadingTime { get; set; } = DateTime.Now;

        [Required]
        public SensorState State { get; set; }
    }
}

