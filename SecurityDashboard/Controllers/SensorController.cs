using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SecurityDashboard.Data;
using SecurityDashboard.DTO;
using SecurityDashboard.Models;
using SecurityDashboard.Services;

namespace SecurityDashboard.Controllers {
    [ApiController]
    [Route("api/sensor")]
    public class SensorController : ControllerBase {
        private readonly DataContext _dataContext;

        public SensorController(DataContext dataContext)
            => _dataContext = dataContext;

        [HttpPost("reading")]
        public async Task<IActionResult> SubmitReading([FromBody] SensorReadingDto dto) {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // evaluate the reading value
            SensorState state = SensorCalculator.CalculateState(dto.SensorType, dto.ReadingValue);
            string unit = SensorCalculator.GetUnit(dto.SensorType);
            DateTime now = DateTime.Now;

            // update sensors
            Sensor? sensor = await _dataContext.Sensors
                .FirstOrDefaultAsync(s => s.SensorType.Equals(dto.SensorType));

            if (sensor == null)
                // create sensor if it doesn't exist
                _dataContext.Sensors.Add(new Sensor {
                    SensorType = dto.SensorType,
                    ReadingValue = dto.ReadingValue,
                    Unit = unit,
                    State = state,
                    ReadingTime = now
                });
            else {
                sensor.ReadingValue = dto.ReadingValue;
                sensor.Unit = unit;
                sensor.State = state;
                sensor.ReadingTime = now;
            }

            AlertSystem? system = await _dataContext.AlertSystems.FindAsync(1);
            bool isArmed = system?.IsArmed ?? true;
            bool isMotionDisabled = system?.MotionIsDisabled ?? false;


            // if system is armed, add to history & is not individually disabled
            bool sensorDisabled = (dto.SensorType == SensorType.Motion) && isMotionDisabled;

            if (SensorCalculator.IsAlertable(state) && isArmed && sensorDisabled) {
                _dataContext.SensorHistories.Add(new History {
                    SensorType = dto.SensorType,
                    ReadingValue = dto.ReadingValue,
                    Unit = unit,
                    State = state,
                    ReadingTime = now
                });
            }

            await _dataContext.SaveChangesAsync();

            // send system armed state as json
            return Ok(new {
                sensor = dto.SensorType.ToString(),
                rawValue = dto.ReadingValue,
                state = state.ToString(),
                unit,
                recordedAt = now,
                isArmed
            });
        }

        private static object MapToDto(Sensor sensor) => new {
            sensorType = (int)sensor.SensorType,
            sensorName = sensor.SensorType.ToString(),
            rawValue = sensor.ReadingValue,
            unit = sensor.Unit,
            state = (int)sensor.State,
            stateName = sensor.State.ToString(),
            stateClass = sensor.State switch {
                SensorState.Safe => "state-safe",
                SensorState.Unsafe => "state-unsafe",
                SensorState.Dangerous => "state-dangerous",
                _ => string.Empty
            },
            recordedAt = sensor.ReadingTime
        };

        [HttpGet("readings")]
        public async Task<IActionResult> GetAllReadings() {
            List<Sensor> sensors = await _dataContext.Sensors.AsNoTracking()
                                                             .ToListAsync();

            return Ok(sensors.Select(MapToDto));
        }

        [HttpGet("readings/{type:int}")]
        public async Task<IActionResult> GetReading(int type) {
            if (!Enum.IsDefined(typeof(SensorType), type))
                return BadRequest(new { error = "Unknown sensor type." });

            SensorType sensorType = (SensorType)type;
            Sensor? sensor = await _dataContext.Sensors.AsNoTracking()
                                                       .FirstOrDefaultAsync(s => s.SensorType.Equals(sensorType));

            if (sensor == null)
                return NotFound(new { error = $"No readings received from {sensorType}." });

            return Ok(MapToDto(sensor));
        }

        [HttpPost("buzzer")]
        public async Task<IActionResult> GetBuzzerState() {
            AlertSystem? alertSystem = await _dataContext.AlertSystems.FindAsync(1);
            bool isArmed = alertSystem?.IsArmed ?? true;
            bool motionIsDisabled = alertSystem?.MotionIsDisabled ?? false;

            bool alertingSensorExists = await _dataContext.Sensors
                .AnyAsync(s => s.State == SensorState.Dangerous &&
                               !(s.SensorType == SensorType.Motion && motionIsDisabled));

            return Ok(new { active = isArmed && alertingSensorExists });
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetHistory() {
            List<History> history = await _dataContext.SensorHistories.AsNoTracking()
                                                                      .OrderByDescending(h => h.ReadingTime)
                                                                      .Take(50)
                                                                      .ToListAsync();

            return Ok(history.Select(h => new {
                readingTime = h.ReadingTime,
                sensorType = h.SensorType.ToString(),
                readingValue = h.ReadingValue,
                unit = h.Unit,
                state = h.State.ToString(),
                stateClass = h.StateClass
            }));
        }
    }
}

