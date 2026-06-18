using Microsoft.EntityFrameworkCore;
using ST10398576_TechMoveGLMS.Models;

namespace ST10398576_TechMoveGLMS.DBContext
{
    public class TechMoveDBContext : DbContext
    {
        public TechMoveDBContext(DbContextOptions<TechMoveDBContext> options) : base(options) { }

        public DbSet<Client> Clients { get; set; }
        public DbSet<Contract> Contracts { get; set; }
        public DbSet<ServiceRequest> ServiceRequests { get; set; }
        public DbSet<User> Users { get; set; }
    }
}
