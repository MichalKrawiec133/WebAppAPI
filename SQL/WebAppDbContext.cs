using Microsoft.EntityFrameworkCore;
using SQL.ModelConfigurations;
using SQL.Models;

namespace SQL;

public class WebAppDbContext: DbContext
{
    
    public WebAppDbContext(DbContextOptions<WebAppDbContext> options) : base(options){}
    
    public virtual DbSet<DocumentItems> DocumentItems { get; set; }
    public virtual DbSet<Documents> Documents { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new DocumentsConfiguration());
    }
}