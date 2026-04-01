using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SecurityDashboard.Data;
using SecurityDashboard.Models;
using SecurityDashboard.ViewModels;

namespace SecurityDashboard.Controllers {
    [Authorize]
    public class DashboardController : Controller {
        public readonly DataContext _dataContext;

        public DashboardController(DataContext context)
            => _dataContext = context;

        public async Task<IActionResult> Home() {
            // check if the alert system exists
            AlertSystem? system = await _dataContext.AlertSystems.FindAsync(1);
            if (system is null) {
                // create if it doesn't
                system = new AlertSystem { Id = 1, IsArmed = true };
                _dataContext.AlertSystems.Add(system);
                await _dataContext.SaveChangesAsync();
            }

            // get latest readings
            List<Sensor> sensors = await _dataContext.Sensors.AsNoTracking()
                                                              .ToListAsync();

            // get history
            List<History> history = await _dataContext.SensorHistories.AsNoTracking()
                                                                      .OrderByDescending(a => a.ReadingTime)
                                                                      .Take(50)
                                                                      .ToListAsync();

            // set up view model
            DashboardViewModel viewModel = new() {
                GasReading = sensors.FirstOrDefault(r => r.SensorType == SensorType.Gas),
                FlameReading = sensors.FirstOrDefault(r => r.SensorType == SensorType.Flame),
                MotionReading = sensors.FirstOrDefault(r => r.SensorType == SensorType.Motion),
                RecentAlerts = history,
                SystemStatus = system
            };

            return View(viewModel);
        }

        public IActionResult AboutUs() {
            return View();
        }
    }
}

