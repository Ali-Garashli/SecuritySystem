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

        private async Task<AlertSystem> CheckSystemRowAsync() {
            var system = await _dataContext.AlertSystems.FindAsync(1);

            if (system == null) {
                system = new AlertSystem { Id = 1, IsArmed = true };
                _dataContext.AlertSystems.Add(system);
            }

            return system;
        }

        [HttpPost]
        public async Task<IActionResult> Toggle() {
            var system = await CheckSystemRowAsync();

            system.IsArmed = !system.IsArmed;
            system.SwitchedTime = DateTime.Now;

            await _dataContext.SaveChangesAsync();

            TempData["SystemMessage"] = system.IsArmed
                ? "System has been ARMED. Monitoring and alerts are active."
                : "System has been DISARMED. Alerts and logging are suspended.";

            return RedirectToAction("Index", "Dashboard");
        }

        // for arduino to check system status
        [HttpGet]
        [Route("api/system/status")]
        public async Task<IActionResult> GetStatus() {
            var status = await CheckSystemRowAsync();
            await _dataContext.SaveChangesAsync();

            return Ok(new {
                          isArmed = status.IsArmed,
                          lastChangedAt = status.SwitchedTime,
                      });
        }
    }
}

