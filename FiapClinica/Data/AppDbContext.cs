using Microsoft.EntityFrameworkCore;
using FiapClinica.Models;

namespace FiapClinica.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Paciente> Pacientes { get; set; }
}