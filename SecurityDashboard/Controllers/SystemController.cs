using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecurityDashboard.Data;
using SecurityDashboard.Models;

namespace SecurityDashboard.Controllers {
    [Authorize(Roles = "Admin")]
    public class SystemController : Controller {
        private readonly DataContext _dataContext;

        public SystemController(DataContext dataContext)
            => _dataContext = dataContext;

        // function for checking if the alert system exists
        private async Task<AlertSystem> CheckSystemRowAsync() {
            AlertSystem? system = await _dataContext.AlertSystems.FindAsync(1);

            // if it doesn't exists, create it
            if (system == null) {
                system = new AlertSystem { Id = 1, IsArmed = true };
                _dataContext.AlertSystems.Add(system);
            }

            return system;
        }

        // toggle system armed state
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Toggle() {
            AlertSystem system = await CheckSystemRowAsync();

            system.IsArmed = !system.IsArmed;
            system.SwitchedTime = DateTime.Now;

            // if system is armed, enable motion and vice versa
            system.MotionIsDisabled = !system.IsArmed;

            await _dataContext.SaveChangesAsync();

            TempData["SystemMessage"] = system.IsArmed
                ? "System has been ARMED. Monitoring and alerts are active."
                : "System has been DISARMED. Alerts and logging are suspended.";

            return RedirectToAction("Home", "Dashboard");
        }

        // toggle motion sensor
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleMotion() {
            AlertSystem system = await CheckSystemRowAsync();

            // toggle motion only if system is armed
            if (system.IsArmed) {
                system.MotionIsDisabled = !system.MotionIsDisabled;
                system.SwitchedTime = DateTime.Now;

                await _dataContext.SaveChangesAsync();

                TempData["SystemMessage"] = system.MotionIsDisabled
                    ? "Motion sensor has been DISABLED. Motion alerts and logging are suspended."
                    : "Motion sensor has been ENABLED. Motion alerts and logging are active.";
            }
            else {
                TempData["SystemMessage"] = "Cannot toggle motion sensor. You must arm the system first.";
            }

            return RedirectToAction("Home", "Dashboard");
        }


        // for arduino to check system status
        [HttpGet]
        [Route("api/system/status")]
        public async Task<IActionResult> GetStatus() {
            AlertSystem alertSystem = await CheckSystemRowAsync();
            await _dataContext.SaveChangesAsync();

            return Ok(new {
                          isArmed = alertSystem.IsArmed,
                          motionIsDisabled = alertSystem.MotionIsDisabled,
                          lastChangedAt = alertSystem.SwitchedTime,
                      });
        }
    }
}

