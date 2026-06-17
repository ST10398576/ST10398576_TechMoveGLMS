using ST10398576_TechMoveGLMS.DBContext;
using ST10398576_TechMoveGLMS.Interfaces;
using ST10398576_TechMoveGLMS.Models;
using Microsoft.EntityFrameworkCore;

namespace ST10398576_TechMoveGLMS.Services
{
    public class ContractService : IContractService
    {
        private readonly TechMoveDBContext _context;
        private readonly IWebHostEnvironment _env;

        public ContractService(TechMoveDBContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<IEnumerable<Contract>> GetAllAsync()
        {
            // Ensure client navigation is eager-loaded so callers (API/Views) can access Client properties
            return await _context.Contracts.Include(c => c.Client).ToListAsync();
        }

        public async Task<IEnumerable<Contract>> SearchAsync(string? status, string? serviceLevel, DateTime? startDate, DateTime? endDate)
        {
            var query = _context.Contracts.Include(c => c.Client).AsQueryable();
            if (!string.IsNullOrEmpty(status)) query = query.Where(c => c.ContractStatus == status);
            if (!string.IsNullOrEmpty(serviceLevel)) query = query.Where(c => c.ContractServiceLevel == serviceLevel);
            if (startDate.HasValue && endDate.HasValue) query = query.Where(c => c.StartDate >= startDate && c.EndDate <= endDate);
            return await query.ToListAsync();
        }

        public async Task<Contract?> GetByIdAsync(int id) => await _context.Contracts.Include(c => c.Client).FirstOrDefaultAsync(c => c.ContractId == id);

        public async Task<Contract> CreateAsync(Contract contract, IFormFile? file)
        {
            if (file != null && file.ContentType == "application/pdf")
            {
                var uploads = Path.Combine(_env.WebRootPath, "pdfs");
                Directory.CreateDirectory(uploads);
                var filePath = Path.Combine(uploads, Guid.NewGuid().ToString() + Path.GetExtension(file.FileName));
                using var stream = new FileStream(filePath, FileMode.Create);
                await file.CopyToAsync(stream);
                contract.PdfFilePath = "/pdfs/" + Path.GetFileName(filePath);
            }
            _context.Contracts.Add(contract);
            await _context.SaveChangesAsync();
            return contract;
        }

        public async Task<Contract> UpdateAsync(Contract contract, IFormFile? file)
        {
            // Load existing entity from DB to avoid unintentionally clearing PdfFilePath
            var existing = await _context.Contracts.FindAsync(contract.ContractId);
            if (existing == null) throw new InvalidOperationException("Contract not found");

            // Handle uploaded PDF (if provided) and update the stored path
            if (file != null && file.ContentType == "application/pdf")
            {
                var uploads = Path.Combine(_env.WebRootPath, "pdfs");
                Directory.CreateDirectory(uploads);
                var filePath = Path.Combine(uploads, Guid.NewGuid().ToString() + Path.GetExtension(file.FileName));
                using var stream = new FileStream(filePath, FileMode.Create);
                await file.CopyToAsync(stream);
                existing.PdfFilePath = "/pdfs/" + Path.GetFileName(filePath);
            }

            // If no file was uploaded but the incoming contract contains a PdfFilePath (frontend hosted file),
            // update the stored path so the API persists the frontend's file location.
            else if (!string.IsNullOrEmpty(contract.PdfFilePath) && contract.PdfFilePath != existing.PdfFilePath)
            {
                existing.PdfFilePath = contract.PdfFilePath;
            }

            // Update scalar properties only (preserve navigation and existing PdfFilePath when no file uploaded)
            existing.ClientId = contract.ClientId;
            existing.StartDate = contract.StartDate;
            existing.EndDate = contract.EndDate;
            existing.ContractStatus = contract.ContractStatus;
            existing.ContractServiceLevel = contract.ContractServiceLevel;

            _context.Contracts.Update(existing);
            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task DeleteAsync(int id)
        {
            var contract = await _context.Contracts.FindAsync(id);
            if (contract != null)
            {
                _context.Contracts.Remove(contract);
                await _context.SaveChangesAsync();
            }
        }

        public bool Exists(int id) => _context.Contracts.Any(c => c.ContractId == id);
    }

}
