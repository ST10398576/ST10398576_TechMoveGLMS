using ST10398576_TechMoveGLMS.DBContext;
using ST10398576_TechMoveGLMS.Interfaces;

namespace ST10398576_TechMoveGLMS.Services
{
    public class HomeService : IHomeService
    {
        private readonly TechMoveDBContext _context;
        public HomeService(TechMoveDBContext context) => _context = context;

        public object GetDashboardSummary()
        {
            return new
            {
                ClientCount = _context.Clients.Count(),
                ContractCount = _context.Contracts.Count(),
                ServiceRequestCount = _context.ServiceRequests.Count(),
                RecentContracts = _context.Contracts.OrderByDescending(c => c.StartDate).Take(5).ToList(),
                RecentRequests = _context.ServiceRequests.OrderByDescending(s => s.ServiceRequestId).Take(5).ToList()
            };
        }
    }
}
