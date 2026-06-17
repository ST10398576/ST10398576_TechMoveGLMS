using ST10398576_TechMoveGLMS.DBContext;
using ST10398576_TechMoveGLMS.Interfaces;
using ST10398576_TechMoveGLMS.Models;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace ST10398576_TechMoveGLMS.Services
{
    public class ServiceRequestService : IServiceRequestService
    {
        private readonly TechMoveDBContext _context;
        public ServiceRequestService(TechMoveDBContext context) => _context = context;

        public async Task<IEnumerable<ServiceRequest>> GetAllAsync()
        {
            // Eager-load related Contract and its Client so the UI can display the associated client name
            return await _context.ServiceRequests
                .Include(sr => sr.Contract)
                    .ThenInclude(c => c.Client)
                .ToListAsync();
        }

        public async Task<IEnumerable<ServiceRequest>> SearchAsync(string? status, DateTime? startDate, DateTime? endDate)
        {
            var query = _context.ServiceRequests.Include(sr => sr.Contract).AsQueryable();
            if (!string.IsNullOrEmpty(status)) query = query.Where(sr => sr.ServiceStatus == status);
            if (startDate.HasValue && endDate.HasValue) query = query.Where(sr => sr.Contract.StartDate >= startDate && sr.Contract.EndDate <= endDate);
            return await query.ToListAsync();
        }
        
        public async Task<ServiceRequest?> GetByIdAsync(int id) => await _context.ServiceRequests.FindAsync(id);

        public async Task<ServiceRequest> CreateAsync(ServiceRequest request)
        {
            var contract = await _context.Contracts.FindAsync(request.ContractId);
            if (contract == null || contract.ContractStatus == "Expired" || contract.ContractStatus == "On Hold")
                throw new InvalidOperationException("Cannot create request for expired or on-hold contracts.");

            request.ServiceCost = await ConvertUsdToZar(request.ServiceCost);
            _context.ServiceRequests.Add(request);
            await _context.SaveChangesAsync();
            return request;
        }

        public async Task<ServiceRequest> UpdateAsync(ServiceRequest request)
        {
            // Load existing entity and update scalar properties to avoid attaching incomplete graph
            var existing = await _context.ServiceRequests.FindAsync(request.ServiceRequestId);
            if (existing == null) throw new InvalidOperationException("Service request not found");

            existing.ServiceDescription = request.ServiceDescription;
            existing.ServiceCost = request.ServiceCost;
            existing.ServiceStatus = request.ServiceStatus;
            existing.ContractId = request.ContractId;

            _context.ServiceRequests.Update(existing);
            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task DeleteAsync(int id)
        {
            var request = await _context.ServiceRequests.FindAsync(id);
            if (request != null)
            {
                _context.ServiceRequests.Remove(request);
                await _context.SaveChangesAsync();
            }
        }

        public bool Exists(int id) => _context.ServiceRequests.Any(sr => sr.ServiceRequestId == id);

        private async Task<decimal> ConvertUsdToZar(decimal amount)
        {
            using var client = new HttpClient();
            var response = await client.GetStringAsync("https://api.exchangerate-api.com/v4/latest/USD");
            var data = JsonDocument.Parse(response);
            var rate = data.RootElement.GetProperty("rates").GetProperty("ZAR").GetDecimal();
            return amount * rate;
        }
    }

}
