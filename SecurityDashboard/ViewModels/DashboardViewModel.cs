using System;
using SecurityDashboard.Models;

namespace SecurityDashboard.ViewModels {
    public class DashboardViewModel {
        public Sensor? GasReading { get; set; }
        public Sensor? FlameReading { get; set; }
        public Sensor? MotionReading { get; set; }

        public AlertSystem SystemStatus { get; set; } = new();

        public IReadOnlyList<History> RecentAlerts { get; set; } = new List<History>();

        // button labels
        public string ToggleButtonLabel => SystemStatus.IsArmed
                                                ? "Disarm System"
                                                : "Arm system";

        public string MotionToggleButtonLabel => SystemStatus.MotionIsDisabled
                                                    ? "Enable Motion"
                                                    : "Disable Motion";


        // css classes
        public string SystemStatusClass => SystemStatus.IsArmed
                                                ? "system-armed"
                                                : "system-disarmed";
    }
}

