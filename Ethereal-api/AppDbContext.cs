using Microsoft.EntityFrameworkCore;

namespace Ethereal_api;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
}