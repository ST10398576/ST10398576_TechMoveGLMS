using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ST10398576_TechMoveGLMS.Models;
using ST10398576_TechMoveGLMS.DBContext;

public class ClientsController : Controller
{
    private readonly TechMoveDBContext _context;

    public ClientsController(TechMoveDBContext context)
    {
        _context = context;
    }

    // GET: CLIENTS
    public async Task<IActionResult> Index()    
    {
        return View(await _context.Clients.ToListAsync());
    }

    // GET: CLIENTS/Details/5
    public async Task<IActionResult> Details(int? clientid)
    {
        if (clientid == null)
        {
            return NotFound();
        }

        var client = await _context.Clients
            .FirstOrDefaultAsync(m => m.ClientId == clientid);
        if (client == null)
        {
            return NotFound();
        }

        return View(client);
    }

    // GET: CLIENTS/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: CLIENTS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("ClientId,ClientName,ClientContactDetails,ClientRegion")] Client client)
    {
        if (ModelState.IsValid)
        {
            _context.Add(client);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(client);
    }

    // GET: CLIENTS/Edit/5
    public async Task<IActionResult> Edit(int? clientid)
    {
        if (clientid == null)
        {
            return NotFound();
        }

        var client = await _context.Clients.FindAsync(clientid);
        if (client == null)
        {
            return NotFound();
        }
        return View(client);
    }

    // POST: CLIENTS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? clientid, [Bind("ClientId,ClientsName,ClientsContactDetails,ClientsRegion,ClientsContracts")] Client client)
    {
        if (clientid != client.ClientId)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(client);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ClientExists(client.ClientId))
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
        return View(client);
    }

    // GET: CLIENTS/Delete/5
    public async Task<IActionResult> Delete(int? clientid)
    {
        if (clientid == null)
        {
            return NotFound();
        }

        var client = await _context.Clients
            .FirstOrDefaultAsync(m => m.ClientId == clientid);
        if (client == null)
        {
            return NotFound();
        }

        return View(client);
    }

    // POST: CLIENTS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? clientid)
    {
        var client = await _context.Clients.FindAsync(clientid);
        if (client != null)
        {
            _context.Clients.Remove(client);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool ClientExists(int? clientid)
    {
        return _context.Clients.Any(e => e.ClientId == clientid);
    }
}
