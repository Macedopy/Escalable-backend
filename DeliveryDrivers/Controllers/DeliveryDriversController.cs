using System.Net;
using System.Runtime.CompilerServices;
using Amazon.S3.Model;
using DeliveryDrivers.Data;
using DeliveryDrivers.Infrastructure;
using DeliveryDrivers.Models;
using Microsoft.AspNetCore.Mvc;
using MotorcycleRental.RabbitService;

namespace DeliveryDrivers.Controllers
{
    [ApiController]
    [Route("mottu/drivers")]
    public class DeliveryDriversController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IDriverRepository _driverRepository;
        private readonly IDriverimageS3 _driverImageS3;

        public DeliveryDriversController(IDriverRepository driverRepository, IDriverimageS3 driverimageS3, ILogger<DriverModel> logger)
        {
            _driverRepository = driverRepository;
            _driverImageS3 = driverimageS3;
            _logger = logger;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var getId = await _driverRepository.GetById(id, HttpContext.RequestAborted);
            return Ok(getId);
        }
        [HttpPost]
        // [ProducesResponseType(typeof(DriverModel), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> SaveDriverInformations(DriverModel driver)
        {
            await _driverRepository.SaveDriverInformations(driver, HttpContext.RequestAborted);
            return CreatedAtAction(nameof(GetById), new { id = driver.Id }, driver);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateDriverInformations(Guid id ,DriverModel driver)
        {
            await _driverRepository.UpdateDriver(id, driver, HttpContext.RequestAborted);
            return NoContent();
        }

        [HttpGet("dowload")]
        public async Task<IActionResult> DownloadFile([FromQuery]string documentName)
        {
            try
            {
                if(string.IsNullOrEmpty(documentName))
                    throw new Exception("This document: " + documentName  + " not exists");

                byte[] content = await _driverImageS3.DownloadFile(documentName);

                return File(content, "application/octet-stream", documentName);
            } catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        [HttpDelete("deletedriver")]
        public async Task<IActionResult> DeleteDriver([FromQuery]Guid id)
        {
            try
            {
                await _driverRepository.DeleteDriver(id, HttpContext.RequestAborted);
                return Ok();
            }catch(Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }
    }
}