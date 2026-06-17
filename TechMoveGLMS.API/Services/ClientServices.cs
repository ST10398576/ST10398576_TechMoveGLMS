using ST10398576_TechMoveGLMS.DBContext;
using ST10398576_TechMoveGLMS.Interfaces;
using ST10398576_TechMoveGLMS.Models;
using Microsoft.EntityFrameworkCore;


namespace ST10398576_TechMoveGLMS.Services
{
    public class ClientService : IClientService
    {
        private readonly TechMoveDBContext _context;
        public ClientService(TechMoveDBContext context) => _context = context;

        public async Task<IEnumerable<Client>> GetAllAsync() => await _context.Clients.ToListAsync();
        public async Task<Client?> GetByIdAsync(int id) => await _context.Clients.FindAsync(id);

        public async Task<Client> CreateAsync(Client client)
        {
            _context.Clients.Add(client);
            await _context.SaveChangesAsync();
            return client;
        }

        public async Task<Client> UpdateAsync(Client client)
        {
            _context.Clients.Update(client);
            await _context.SaveChangesAsync();
            return client;
        }

        public async Task DeleteAsync(int id)
        {
            var client = await _context.Clients.FindAsync(id);
            if (client != null)
            {
                _context.Clients.Remove(client);
                await _context.SaveChangesAsync();
            }
        }

        public bool Exists(int id) => _context.Clients.Any(c => c.ClientId == id);
    }
}
