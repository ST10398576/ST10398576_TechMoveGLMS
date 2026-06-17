using Microsoft.AspNetCore.Mvc;
using ST10398576_TechMoveGLMS.Interfaces;
using ST10398576_TechMoveGLMS.Models;

namespace ST10398576_TechMoveGLMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ServiceRequestsController : ControllerBase
    {
        private readonly IServiceRequestService _serviceRequestService;
        public ServiceRequestsController(IServiceRequestService serviceRequestService) => _serviceRequestService = serviceRequestService;

        [HttpGet]
        public async Task<IActionResult> GetRequests(string? status, DateTime? startDate, DateTime? endDate)
        {
            if (string.IsNullOrEmpty(status) && !startDate.HasValue && !endDate.HasValue)
            {
                var allRequests = await _serviceRequestService.GetAllAsync();
                return Ok(allRequests);
            }

            var requests = await _serviceRequestService.SearchAsync(status, startDate, endDate);
            return Ok(requests);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetRequest(int id)
        {
            var request = await _serviceRequestService.GetByIdAsync(id);
            if (request == null) return NotFound();
            return Ok(request);
        }

        [HttpPost]
        public async Task<IActionResult> CreateRequest([FromBody] ServiceRequest request)
        {
            try
            {
                var created = await _serviceRequestService.CreateAsync(request);
                return CreatedAtAction(nameof(GetRequest), new { id = created.ServiceRequestId }, created);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRequest(int id, [FromBody] ServiceRequest request)
        {
            if (id != request.ServiceRequestId) return BadRequest();
            var updated = await _serviceRequestService.UpdateAsync(request);
            return Ok(updated);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRequest(int id)
        {
            await _serviceRequestService.DeleteAsync(id);
            return NoContent();
        }
    }
}
