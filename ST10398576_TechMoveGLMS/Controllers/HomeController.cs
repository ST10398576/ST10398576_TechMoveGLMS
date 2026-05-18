using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ST10398576_TechMoveGLMS.DBContext;
using ST10398576_TechMoveGLMS.Models;
using System.Diagnostics;

namespace ST10398576_TechMoveGLMS.Controllers
{
    public class HomeController : Controller
    {
        private readonly TechMoveDBContext _context;

        public HomeController(TechMoveDBContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var dashboard = new
            {
                ClientCount = _context.Clients.Count(),
                ContractCount = _context.Contracts.Count(),
                ServiceRequestCount = _context.ServiceRequests.Count(),
                RecentContracts = _context.Contracts
                    .OrderByDescending(c => c.StartDate)
                    .Take(5)
                    .ToList(),
                RecentRequests = _context.ServiceRequests
                    .OrderByDescending(s => s.ServiceRequestId)
                    .Take(5)
                    .ToList()
            };

            return View(dashboard);
        }
    }
}
