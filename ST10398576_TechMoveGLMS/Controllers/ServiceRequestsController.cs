
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ST10398576_TechMoveGLMS.DBContext;
using ST10398576_TechMoveGLMS.Models;
using System.Text.Json;

public class ServiceRequestsController : Controller
{
    private readonly TechMoveDBContext _context;

    public ServiceRequestsController(TechMoveDBContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Search(DateTime? startDate, DateTime? endDate, string status)
    {
        var query = _context.ServiceRequests.AsQueryable();

        if (startDate.HasValue && endDate.HasValue)
            query = query.Where(sr => sr.Contract.StartDate >= startDate && sr.Contract.EndDate <= endDate);

        if (!string.IsNullOrEmpty(status))
            query = query.Where(sr => sr.ServiceStatus == status);

        return View("Index", await query.ToListAsync());
    }


    // GET: SERVICEREQUESTS
    public async Task<IActionResult> Index()    
    {
        return View(await _context.ServiceRequests.ToListAsync());
    }

    // GET: SERVICEREQUESTS/Details/5
    public async Task<IActionResult> Details(int? servicerequestid)
    {
        if (servicerequestid == null)
        {
            return NotFound();
        }

        var servicerequest = await _context.ServiceRequests
            .FirstOrDefaultAsync(m => m.ServiceRequestId == servicerequestid);
        if (servicerequest == null)
        {
            return NotFound();
        }

        return View(servicerequest);
    }

    // GET: SERVICEREQUESTS/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: SERVICEREQUESTS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("ServiceRequestId,ContractId,ServiceDescription,ServiceCost,ServiceStatus")] ServiceRequest servicerequest)
    {
        var contract = await _context.Contracts.FindAsync(servicerequest.ContractId);
        if (contract == null)
        {
            ModelState.AddModelError("", "Contract not found.");
            return View(servicerequest);
        }

        if (contract.ContractStatus == "Expired" || contract.ContractStatus == "OnHold")
        {
            ModelState.AddModelError("", "Cannot create a request for expired or on-hold contracts.");
            return View(servicerequest);
        }

        // Currency conversion: example for USD → ZAR
        servicerequest.ServiceCost = await ConvertUsdToZar(servicerequest.ServiceCost);

        if (ModelState.IsValid)
        {
            _context.Add(servicerequest);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        return View(servicerequest);
    }

    private async Task<decimal> ConvertUsdToZar(decimal amount)
    {
        using var client = new HttpClient();
        var response = await client.GetStringAsync("https://api.exchangerate-api.com/v4/latest/USD");
        var data = JsonDocument.Parse(response);
        var rate = data.RootElement.GetProperty("rates").GetProperty("ZAR").GetDecimal();
        return amount * rate;
    }

    // GET: SERVICEREQUESTS/Edit/5
    public async Task<IActionResult> Edit(int? servicerequestid)
    {
        if (servicerequestid == null)
        {
            return NotFound();
        }

        var servicerequest = await _context.ServiceRequests.FindAsync(servicerequestid);
        if (servicerequest == null)
        {
            return NotFound();
        }
        return View(servicerequest);
    }

    // POST: SERVICEREQUESTS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? servicerequestid, [Bind("ServiceRequestId,ContractId,ServiceContract,ServiceDescription,ServiceCost,ServiceStatus")] ServiceRequest servicerequest)
    {
        if (servicerequestid != servicerequest.ServiceRequestId)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(servicerequest);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ServiceRequestExists(servicerequest.ServiceRequestId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }
        return View(servicerequest);
    }

    // GET: SERVICEREQUESTS/Delete/5
    public async Task<IActionResult> Delete(int? servicerequestid)
    {
        if (servicerequestid == null)
        {
            return NotFound();
        }

        var servicerequest = await _context.ServiceRequests
            .FirstOrDefaultAsync(m => m.ServiceRequestId == servicerequestid);
        if (servicerequest == null)
        {
            return NotFound();
        }

        return View(servicerequest);
    }

    // POST: SERVICEREQUESTS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? servicerequestid)
    {
        var servicerequest = await _context.ServiceRequests.FindAsync(servicerequestid);
        if (servicerequest != null)
        {
            _context.ServiceRequests.Remove(servicerequest);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool ServiceRequestExists(int? servicerequestid)
    {
        return _context.ServiceRequests.Any(e => e.ServiceRequestId == servicerequestid);
    }
}
