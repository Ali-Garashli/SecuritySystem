using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SecurityDashboard.Models {
    public class History {
        [Key]
        public int Id { get; set; }

        [Required]
        public SensorType SensorType {get; set; }

        [Required]
        public double ReadingValue { get; set; }

        [MaxLength(20)]
        public string Unit { get; set; } = "";

        [Required]
        public SensorState State { get; set; }

        [Required]
        public DateTime ReadingTime { get; set; } = DateTime.Now;

        // state for css
        [NotMapped]
        public string StateClass => State switch {
                                        SensorState.Unsafe => "state-unsafe",
                                        SensorState.Dangerous => "state-dangerous",
                                        _ => ""
                                    };
    }
}

