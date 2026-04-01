using System;
namespace SecurityDashboard.Models {
    public enum SensorType {
        Gas,
        Flame,
        Motion
    }

    public enum SensorState {
        Safe,
        Unsafe,
        Dangerous
    }
}

