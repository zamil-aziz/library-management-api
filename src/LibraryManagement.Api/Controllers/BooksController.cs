using LibraryManagement.Api.Data;
using LibraryManagement.Api.DTOs;
using LibraryManagement.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class BooksController(LibraryDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<BookResponse>>> GetBooks()
    {
        var books = await dbContext.Books
            .AsNoTracking()
            .OrderBy(book => book.Title)
            .Select(book => new BookResponse(
                book.Id,
                book.Title,
                book.Author,
                book.ISBN,
                book.PublishedYear,
                book.IsAvailable,
                book.CreatedAt,
                book.UpdatedAt))
            .ToListAsync();

        return Ok(books);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<BookResponse>> GetBook(int id)
    {
        var book = await dbContext.Books
            .AsNoTracking()
            .FirstOrDefaultAsync(book => book.Id == id);

        return book is null ? NotFound() : Ok(ToResponse(book));
    }

    [HttpPost]
    public async Task<ActionResult<BookResponse>> CreateBook(CreateBookRequest request)
    {
        var isbn = request.ISBN.Trim();

        if (await IsbnExists(isbn))
        {
            return Conflict(new { message = "A book with the same ISBN already exists." });
        }

        var now = DateTime.UtcNow;
        var book = new Book
        {
            Title = request.Title.Trim(),
            Author = request.Author.Trim(),
            ISBN = isbn,
            PublishedYear = request.PublishedYear,
            IsAvailable = request.IsAvailable,
            CreatedAt = now,
            UpdatedAt = now
        };

        dbContext.Books.Add(book);

        try
        {
            await dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException exception) when (IsUniqueIsbnViolation(exception))
        {
            return Conflict(new { message = "A book with the same ISBN already exists." });
        }

        return CreatedAtAction(nameof(GetBook), new { id = book.Id }, ToResponse(book));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateBook(int id, UpdateBookRequest request)
    {
        var book = await dbContext.Books.FindAsync(id);

        if (book is null)
        {
            return NotFound();
        }

        if (request.IsAvailable is not { } isAvailable)
        {
            ModelState.AddModelError(nameof(UpdateBookRequest.IsAvailable), "The IsAvailable field is required.");
            return ValidationProblem(ModelState);
        }

        var isbn = request.ISBN.Trim();

        if (await IsbnExists(isbn, id))
        {
            return Conflict(new { message = "A book with the same ISBN already exists." });
        }

        book.Title = request.Title.Trim();
        book.Author = request.Author.Trim();
        book.ISBN = isbn;
        book.PublishedYear = request.PublishedYear;
        book.IsAvailable = isAvailable;
        book.UpdatedAt = DateTime.UtcNow;

        try
        {
            await dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException exception) when (IsUniqueIsbnViolation(exception))
        {
            return Conflict(new { message = "A book with the same ISBN already exists." });
        }

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteBook(int id)
    {
        var book = await dbContext.Books.FindAsync(id);

        if (book is null)
        {
            return NotFound();
        }

        dbContext.Books.Remove(book);
        await dbContext.SaveChangesAsync();

        return NoContent();
    }

    private Task<bool> IsbnExists(string isbn, int? excludingBookId = null)
    {
        return dbContext.Books.AnyAsync(book =>
            book.ISBN == isbn && (!excludingBookId.HasValue || book.Id != excludingBookId.Value));
    }

    private static bool IsUniqueIsbnViolation(DbUpdateException exception)
    {
        const int sqliteConstraint = 19;
        return exception.InnerException switch
        {
            SqliteException sqliteException
                => sqliteException.SqliteErrorCode == sqliteConstraint
                    && sqliteException.Message.Contains("Books.ISBN", StringComparison.OrdinalIgnoreCase),
            SqlException sqlException
                => sqlException.Number is 2601 or 2627,
            _ => false
        };
    }

    private static BookResponse ToResponse(Book book)
    {
        return new BookResponse(
            book.Id,
            book.Title,
            book.Author,
            book.ISBN,
            book.PublishedYear,
            book.IsAvailable,
            book.CreatedAt,
            book.UpdatedAt);
    }
}
