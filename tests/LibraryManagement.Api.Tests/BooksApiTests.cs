using System.Net;
using System.Net.Http.Json;
using LibraryManagement.Api.DTOs;

namespace LibraryManagement.Api.Tests;

public sealed class BooksApiTests
{
    [Fact]
    public async Task CreateBook_ReturnsCreatedBook()
    {
        using var factory = new LibraryApiFactory();
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/books", NewBook("9780132350884"));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var book = await response.Content.ReadFromJsonAsync<BookResponse>();
        Assert.NotNull(book);
        Assert.Equal("Clean Code", book.Title);
        Assert.Equal("Robert C. Martin", book.Author);
        Assert.Equal("9780132350884", book.ISBN);
        Assert.True(book.IsAvailable);
        Assert.True(book.Id > 0);
    }

    [Fact]
    public async Task GetBooks_ReturnsCreatedBooks()
    {
        using var factory = new LibraryApiFactory();
        var client = factory.CreateClient();
        await client.PostAsJsonAsync("/api/books", NewBook("9780132350884"));

        var books = await client.GetFromJsonAsync<List<BookResponse>>("/api/books");

        Assert.NotNull(books);
        var book = Assert.Single(books);
        Assert.Equal("Clean Code", book.Title);
    }

    [Fact]
    public async Task GetBook_WhenMissing_ReturnsNotFound()
    {
        using var factory = new LibraryApiFactory();
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/books/999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateBook_ChangesBook()
    {
        using var factory = new LibraryApiFactory();
        var client = factory.CreateClient();
        var createdResponse = await client.PostAsJsonAsync("/api/books", NewBook("9780132350884"));
        var created = await createdResponse.Content.ReadFromJsonAsync<BookResponse>();

        var update = new UpdateBookRequest
        {
            Title = "Clean Architecture",
            Author = "Robert C. Martin",
            ISBN = "9780134494166",
            PublishedYear = 2017,
            IsAvailable = false
        };

        var response = await client.PutAsJsonAsync($"/api/books/{created!.Id}", update);
        var updated = await client.GetFromJsonAsync<BookResponse>($"/api/books/{created.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.NotNull(updated);
        Assert.Equal("Clean Architecture", updated.Title);
        Assert.Equal("9780134494166", updated.ISBN);
        Assert.False(updated.IsAvailable);
        Assert.True(updated.UpdatedAt >= updated.CreatedAt);
    }

    [Fact]
    public async Task UpdateBook_WithoutAvailability_ReturnsBadRequest()
    {
        using var factory = new LibraryApiFactory();
        var client = factory.CreateClient();
        var createdResponse = await client.PostAsJsonAsync("/api/books", NewBook("9780132350884"));
        var created = await createdResponse.Content.ReadFromJsonAsync<BookResponse>();

        var response = await client.PutAsJsonAsync($"/api/books/{created!.Id}", new
        {
            title = "Clean Architecture",
            author = "Robert C. Martin",
            isbn = "9780134494166",
            publishedYear = 2017
        });
        var unchanged = await client.GetFromJsonAsync<BookResponse>($"/api/books/{created.Id}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(unchanged);
        Assert.True(unchanged.IsAvailable);
    }

    [Fact]
    public async Task DeleteBook_RemovesBook()
    {
        using var factory = new LibraryApiFactory();
        var client = factory.CreateClient();
        var createdResponse = await client.PostAsJsonAsync("/api/books", NewBook("9780132350884"));
        var created = await createdResponse.Content.ReadFromJsonAsync<BookResponse>();

        var response = await client.DeleteAsync($"/api/books/{created!.Id}");
        var getResponse = await client.GetAsync($"/api/books/{created.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task CreateBook_WithDuplicateIsbn_ReturnsConflict()
    {
        using var factory = new LibraryApiFactory();
        var client = factory.CreateClient();
        await client.PostAsJsonAsync("/api/books", NewBook("9780132350884"));

        var response = await client.PostAsJsonAsync("/api/books", NewBook("9780132350884"));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task CreateBook_WithConcurrentDuplicateIsbn_ReturnsCreatedOrConflict()
    {
        using var factory = new LibraryApiFactory();
        var clients = Enumerable.Range(0, 8)
            .Select(_ => factory.CreateClient())
            .ToList();

        var responses = await Task.WhenAll(clients.Select(client =>
            client.PostAsJsonAsync("/api/books", NewBook("9780132350884"))));

        Assert.Equal(1, responses.Count(response => response.StatusCode == HttpStatusCode.Created));
        Assert.All(
            responses.Where(response => response.StatusCode != HttpStatusCode.Created),
            response => Assert.Equal(HttpStatusCode.Conflict, response.StatusCode));
    }

    [Fact]
    public async Task CreateBook_WithInvalidRequest_ReturnsBadRequest()
    {
        using var factory = new LibraryApiFactory();
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/books", new
        {
            author = "Robert C. Martin",
            isbn = "9780132350884",
            publishedYear = 2008
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private static CreateBookRequest NewBook(string isbn)
    {
        return new CreateBookRequest
        {
            Title = "Clean Code",
            Author = "Robert C. Martin",
            ISBN = isbn,
            PublishedYear = 2008,
            IsAvailable = true
        };
    }
}
