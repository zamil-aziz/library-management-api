namespace LibraryManagement.Api.Models;

public sealed class Book
{
    public int Id { get; set; }

    public required string Title { get; set; }

    public required string Author { get; set; }

    public required string ISBN { get; set; }

    public int PublishedYear { get; set; }

    public bool IsAvailable { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
