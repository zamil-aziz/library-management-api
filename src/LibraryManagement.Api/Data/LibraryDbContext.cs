using LibraryManagement.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Api.Data;

public sealed class LibraryDbContext(DbContextOptions<LibraryDbContext> options) : DbContext(options)
{
    public DbSet<Book> Books => Set<Book>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Book>(entity =>
        {
            entity.HasKey(book => book.Id);

            entity.Property(book => book.Title)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(book => book.Author)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(book => book.ISBN)
                .HasMaxLength(20)
                .IsRequired();

            entity.HasIndex(book => book.ISBN)
                .IsUnique();

            entity.Property(book => book.IsAvailable)
                .HasDefaultValue(true);

            entity.Property(book => book.CreatedAt)
                .IsRequired();

            entity.Property(book => book.UpdatedAt)
                .IsRequired();
        });
    }
}
