namespace LibraryManagement.Api.DTOs;

public sealed record BookResponse(
    int Id,
    string Title,
    string Author,
    string ISBN,
    int PublishedYear,
    bool IsAvailable,
    DateTime CreatedAt,
    DateTime UpdatedAt);
