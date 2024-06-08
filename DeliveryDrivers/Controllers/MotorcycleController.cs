using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MotorcycleRental.Data;
using DeliveryDrivers.Models;
using MotorcycleRental.RabbitService;

namespace MotorcycleRental.Controllers
{
    [ApiController]
    [Route("mottu/motorcycle")]
    public class MotorcycleController : ControllerBase
    {
        private readonly IMotorcycleService _motorcycleService;

        public MotorcycleController(IMotorcycleService motorcycleService)
        {
            _motorcycleService = motorcycleService;
        }

        [HttpGet("{plate}")]
        public async Task<IActionResult> GetMotorcyclePlate(string plate)
        {
            var getId = await _motorcycleService.GetMotorcyclePlate(plate, HttpContext.RequestAborted);
            return Ok(getId);
        }
        [HttpPost]
        public async Task<IActionResult> RegisterMotorcycle(MotorCycleModel motorCycle)
        {
            await _motorcycleService.RegisterMotorcycle(motorCycle, HttpContext.RequestAborted);
            return CreatedAtAction(nameof(GetMotorcyclePlate), new { plate = motorCycle.Id  }, motorCycle);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateMotorcycle(MotorCycleModel motorCycle)
        {
            await _motorcycleService.UpdateMotorcycle(motorCycle, HttpContext.RequestAborted);
            return Ok();
        }
        [HttpDelete]
        public async Task<IActionResult> DeleteMotorcycle(string plate)
        {
            await _motorcycleService.DeleteMotorcycle(plate, HttpContext.RequestAborted);
            return NoContent();
        }
        [HttpGet("listmotorcycles")]
        public async Task<ActionResult<List<MotorCycleModel>>> GetMotorcycle(MotorCycleModel motorCycle)
        {
            var motorCycles = await _motorcycleService.GetMotorcycle(motorCycle, HttpContext.RequestAborted);
            return Ok(motorCycles);
        }
        [HttpGet("available")]
        public async Task<ActionResult<List<MotorCycleModel>>> GetMotorcycleAvailable(MotorCycleModel motorcycleModel, CancellationToken cancellationToken)
        {
            var motorCycleAvailable = await _motorcycleService.GetMotorcycleAvailable(motorcycleModel, HttpContext.RequestAborted);
            return Ok(motorCycleAvailable);
        }

        [HttpPut("setavailable")]
        public async Task<IActionResult> SetRentalAvailable(string plate)
        {
            var setRentalAvailable = await _motorcycleService.SetRentalNotAvailable(plate, HttpContext.RequestAborted);
            return Ok(setRentalAvailable);
        }

        [HttpPut("setnotavailable")]
        public async Task<IActionResult> SetRentalNotAvailable(string plate)
        {
            var setRentalNotAvailable = await _motorcycleService.SetRentalNotAvailable(plate, HttpContext.RequestAborted);
            return Ok(setRentalNotAvailable);
        }


}
}