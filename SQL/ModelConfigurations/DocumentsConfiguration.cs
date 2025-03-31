using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SQL.Models;

namespace SQL.ModelConfigurations;

public class DocumentsConfiguration:IEntityTypeConfiguration<Documents>
{
    public void Configure(EntityTypeBuilder<Documents> builder)
    {
        builder.HasKey(d => d.DocumentId);

        builder.Property(d => d.Type)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(d => d.Date)
            .IsRequired();

        builder.Property(d => d.FirstName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(d => d.LastName)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasMany(d => d.DocumentItems)
            .WithOne(di => di.Document)
            .HasForeignKey(di => di.DocumentId)
            .OnDelete(DeleteBehavior.Cascade);

    }    
}