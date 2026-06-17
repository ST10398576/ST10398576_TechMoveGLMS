using ST10398576_TechMoveGLMS.Models;

namespace ST10398576_TechMoveGLMS.Interfaces
{
    public interface IContractService
    {
        Task<IEnumerable<Contract>> GetAllAsync();
        Task<IEnumerable<Contract>> SearchAsync(string? status, string? serviceLevel, DateTime? startDate, DateTime? endDate);
        Task<Contract?> GetByIdAsync(int id);
        Task<Contract> CreateAsync(Contract contract, IFormFile? file);
        Task<Contract> UpdateAsync(Contract contract, IFormFile? file);
        Task DeleteAsync(int id);
        bool Exists(int id);
    }
}
