using System;
using SecurityDashboard.Models;

namespace SecurityDashboard.Services {
    public class SensorCalculator {

        const int _gasUnsafeLevel = 250;
        const int _gasDangerousLevel = 300;

        public static SensorState CalculateState(SensorType sensor, int readingValue)
            => sensor switch {
                SensorType.Gas => DetermineGasState(readingValue),
                SensorType.Flame => DetermineFlameState(readingValue),
                SensorType.Motion => DetermineMotionState(readingValue),
                _ => SensorState.Safe
            };

        public static string GetUnit(SensorType sensor)
            => sensor switch {
                SensorType.Gas => "ppm",
                SensorType.Flame => "IR",
                _ => ""
            };

        public static bool IsAlertable(SensorState state)
            => state is SensorState.Unsafe or SensorState.Dangerous;

        static SensorState DetermineGasState(int readingValue) {
            if (readingValue >= _gasDangerousLevel) return SensorState.Dangerous;
            if (readingValue >= _gasUnsafeLevel) return SensorState.Unsafe;
            return SensorState.Safe;
        }

        private static SensorState DetermineFlameState(int readingValue) {
            if (readingValue == 0)
                return SensorState.Dangerous;

            return SensorState.Safe;
        }

        static SensorState DetermineMotionState(int readingValue) {
            if (readingValue != 0)
                return SensorState.Dangerous;

            return SensorState.Safe;
        }
    }
}

