using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ST10398576_TechMoveGLMS.DBContext;
using ST10398576_TechMoveGLMS.Models;

namespace ST10398576_TechMoveGLMS.API.Controllers
{
    [Authorize]
    [Route("api/home")]
    [ApiController]
    public class HomeApiController : ControllerBase
    {
        private readonly TechMoveDBContext _context;

        public HomeApiController(TechMoveDBContext context)
        {
            _context = context;
        }

        // GET: api/home/dashboard
        [HttpGet("dashboard")]
        public async Task<ActionResult<DashboardSummary>> GetDashboardSummary()
        {
            var summary = new DashboardSummary
            {
                TotalClients = await _context.Clients.CountAsync(),
                TotalContracts = await _context.Contracts.CountAsync(),
                ActiveContracts = await _context.Contracts.CountAsync(c => c.ContractStatus == "Active"),
                ExpiredContracts = await _context.Contracts.CountAsync(c => c.ContractStatus == "Expired"),
                TotalServiceRequests = await _context.ServiceRequests.CountAsync(),
                PendingRequests = await _context.ServiceRequests.CountAsync(r => r.ServiceStatus == "Pending"),
                CompletedRequests = await _context.ServiceRequests.CountAsync(r => r.ServiceStatus == "Completed")
            };

            return Ok(summary);
        }
    }
}
