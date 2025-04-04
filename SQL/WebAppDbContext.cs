﻿using Microsoft.EntityFrameworkCore;
using SQL.ModelConfigurations;
using SQL.Models;

namespace SQL;

public class WebAppDbContext : DbContext
{
    public WebAppDbContext(){}
    public WebAppDbContext(DbContextOptions<WebAppDbContext> options) : base(options){}
    
    public DbSet<DocumentItems> DocumentItems { get; set; }
    public DbSet<Documents> Documents { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new DocumentsConfiguration());
    }
    
}