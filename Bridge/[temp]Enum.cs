using System;
namespace SecurityDashboard.Models {
    public enum TEMP_SensorType {
        Gas,
        Flame,
        Motion
    }

    public enum TEMP_SensorState {
        Safe,
        Unsafe,
        Dangerous
    }
}

