using System;
using SecurityDashboard.Models;

namespace SecurityDashboard.ViewModels {
    public class DashboardViewModel {
        public Sensor? GasReading { get; set; }
        public Sensor? FlameReading { get; set; }
        public Sensor? MotionReading { get; set; }

        public AlertSystem SystemStatus { get; set; } = new();

        public IReadOnlyList<History> RecentAlerts { get; set; } = new List<History>();

        // button label
        public string ToggleButtonLabel => SystemStatus.IsArmed switch {
                                                true => "Disarm System",
                                                false => "Arm system"
                                            };

        // css class
        public string SystemStatusClass => SystemStatus.IsArmed switch {
                                                true => "system-armed",
                                                false => "system-disarmed"
                                            };
    }
}

