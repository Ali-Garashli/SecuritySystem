using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SecurityDashboard.Models;

namespace SecurityDashboard.Controllers {
    public static class AlertSystemController {
        static AlertSystem alertSystem;

        static AlertSystemController() {
            alertSystem = new();
        }

        public static void ToggleAlertSystem() {
            alertSystem.IsActive = !alertSystem.IsActive;
            Console.WriteLine("System is active: " + alertSystem.IsActive.ToString());
        }
    }
}

