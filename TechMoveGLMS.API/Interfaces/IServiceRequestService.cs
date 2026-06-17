using ST10398576_TechMoveGLMS.Models;

namespace ST10398576_TechMoveGLMS.Interfaces
{
    public interface IServiceRequestService
    {
        Task<IEnumerable<ServiceRequest>> GetAllAsync();
        Task<IEnumerable<ServiceRequest>> SearchAsync(string? status, DateTime? startDate, DateTime? endDate);
        Task<ServiceRequest?> GetByIdAsync(int id);
        Task<ServiceRequest> CreateAsync(ServiceRequest request);
        Task<ServiceRequest> UpdateAsync(ServiceRequest request);
        Task DeleteAsync(int id);
        bool Exists(int id);
    }

}
