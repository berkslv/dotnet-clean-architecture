using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class BookConfiguration : IEntityTypeConfiguration<Book>
{
    public void Configure(EntityTypeBuilder<Book> builder)
    {
        builder.Property(t => t.Name)
            .HasMaxLength(255)
            .IsRequired();
        
        builder.Property(t => t.Description)
            .HasMaxLength(4095)
            .IsRequired();
        
        builder.Property(t => t.Cover)
            .HasConversion<int>()
            .IsRequired();
        
        builder.Property(t => t.AuthorId)
            .IsRequired();

        builder.HasMany(t => t.Categories)
            .WithMany(t => t.Books);
    }
}