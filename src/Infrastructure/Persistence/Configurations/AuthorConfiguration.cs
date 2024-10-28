using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class AuthorConfiguration : IEntityTypeConfiguration<Author>
{
    public void Configure(EntityTypeBuilder<Author> builder)
    {
        builder.Property(t => t.Name)
            .HasMaxLength(255)
            .IsRequired();
        
        builder.Property(t => t.Birth)
            .IsRequired();
        
        builder.HasMany(t => t.Books)
            .WithOne(t => t.Author)
            .HasForeignKey(t => t.AuthorId)
            .IsRequired();
    }
}