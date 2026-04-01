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
                .FirstOrDefaultAsync(r => r.SensorType.Equals(dto.SensorType));

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

            // if system is armed, add to history
            if (SensorCalculator.IsAlertable(state)) {
                AlertSystem? system = await _dataContext.AlertSystems.FindAsync(1);
                if (system?.IsArmed == true)
                    _dataContext.SensorHistories.Add(new History {
                        SensorType = dto.SensorType,
                        ReadingValue = dto.ReadingValue,
                        Unit = unit,
                        State = state,
                        ReadingTime = now
                    });
            }

            await _dataContext.SaveChangesAsync();

            // send system armed state to arduino as json
            bool systemArmed = (await _dataContext.AlertSystems.FindAsync(1))?.IsArmed ?? true;

            return Ok(new {
                sensor = dto.SensorType.ToString(),
                rawValue = dto.ReadingValue,
                state = state.ToString(),
                unit,
                recordedAt = now,
                systemArmed
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

        
    }
}

