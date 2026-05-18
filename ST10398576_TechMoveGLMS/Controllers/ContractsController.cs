using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ST10398576_TechMoveGLMS.DBContext;
using ST10398576_TechMoveGLMS.Models;

public class ContractsController : Controller
{
    private readonly TechMoveDBContext _context;
    private readonly IWebHostEnvironment _env;

    public ContractsController(TechMoveDBContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    // GET: Contracts
    public async Task<IActionResult> Index()
    {
        var contracts = await _context.Contracts.Include(c => c.Client).ToListAsync();
        return View(contracts);
    }

    // GET: Contracts/Details/5
    public async Task<IActionResult> Details(int? contractid)
    {
        if (contractid == null) return NotFound();

        var contract = await _context.Contracts
            .Include(c => c.Client)
            .FirstOrDefaultAsync(m => m.ContractId == contractid);

        if (contract == null) return NotFound();

        return View(contract);
    }

    // GET: Contracts/Create
    public IActionResult Create()
    {
        ViewData["ClientId"] = new SelectList(_context.Clients, "ClientId", "ClientName");
        ViewData["StatusList"] = new SelectList(new[] { "Draft", "On Hold", "Active", "Expired" });
        ViewData["ServiceLevelList"] = new SelectList(new[] { "High", "Medium", "Low" });
        return View();
    }

    // POST: Contracts/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("ContractId,ClientId,StartDate,EndDate,ContractStatus,ContractServiceLevel")] Contract contract, IFormFile file)
    {
        if (ModelState.IsValid)
        {
            if (file != null && file.ContentType == "application/pdf")
            {
                var uploads = Path.Combine(_env.WebRootPath, "pdfs");
                Directory.CreateDirectory(uploads);
                var filePath = Path.Combine(uploads, Guid.NewGuid().ToString() + Path.GetExtension(file.FileName));

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                contract.PdfFilePath = "/pdfs/" + Path.GetFileName(filePath);
            }

            _context.Add(contract);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        ViewData["ClientId"] = new SelectList(_context.Clients, "ClientId", "ClientName", contract.ClientId);
        ViewData["StatusList"] = new SelectList(new[] { "Draft", "On Hold", "Active", "Expired" }, contract.ContractStatus);
        ViewData["ServiceLevelList"] = new SelectList(new[] { "High", "Medium", "Low" }, contract.ContractServiceLevel);
        return View(contract);
    }

    // GET: Contracts/Edit/5
    public async Task<IActionResult> Edit(int? contractid)
    {
        if (contractid == null) return NotFound();

        var contract = await _context.Contracts.FindAsync(contractid);
        if (contract == null) return NotFound();

        ViewData["ClientId"] = new SelectList(_context.Clients, "ClientId", "ClientName", contract.ClientId);
        ViewData["StatusList"] = new SelectList(new[] { "Draft","On Hold", "Active", "Expired" }, contract.ContractStatus);
        ViewData["ServiceLevelList"] = new SelectList(new[] { "High", "Medium", "Low" }, contract.ContractServiceLevel);
        return View(contract);
    }

    // POST: Contracts/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int contractid, [Bind("ContractId,ClientId,StartDate,EndDate,ContractStatus,ContractServiceLevel,PdfFilePath")] Contract contract, IFormFile file)
    {
        if (contractid != contract.ContractId) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                if (file != null && file.ContentType == "application/pdf")
                {
                    var uploads = Path.Combine(_env.WebRootPath, "pdfs");
                    Directory.CreateDirectory(uploads);
                    var filePath = Path.Combine(uploads, Guid.NewGuid().ToString() + Path.GetExtension(file.FileName));

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    contract.PdfFilePath = "/pdfs/" + Path.GetFileName(filePath);
                }

                _context.Update(contract);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ContractExists(contract.ContractId)) return NotFound();
                else throw;
            }
            return RedirectToAction(nameof(Index));
        }

        ViewData["ClientId"] = new SelectList(_context.Clients, "ClientId", "ClientName", contract.ClientId);
        ViewData["StatusList"] = new SelectList(new[] { "Draft", "On Hold", "Active", "Expired" }, contract.ContractStatus);
        ViewData["ServiceLevelList"] = new SelectList(new[] { "High", "Medium", "Low" }, contract.ContractServiceLevel);
        return View(contract);
    }

    // GET: Contracts/Delete/5
    public async Task<IActionResult> Delete(int? contractid)
    {
        if (contractid == null) return NotFound();

        var contract = await _context.Contracts
            .Include(c => c.Client)
            .FirstOrDefaultAsync(m => m.ContractId == contractid);

        if (contract == null) return NotFound();

        return View(contract);
    }

    // POST: Contracts/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? contractid)
    {
        var contract = await _context.Contracts.FindAsync(contractid);
        if (contract != null) _context.Contracts.Remove(contract);

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool ContractExists(int? contractid)
    {
        return _context.Contracts.Any(e => e.ContractId == contractid);
    }
}
