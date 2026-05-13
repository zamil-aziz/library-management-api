# Library Management API

ASP.NET Core Web API assessment project for a basic Library Management System. It implements Books CRUD using C#, .NET 8, Entity Framework Core, controllers, validation, and Swagger/OpenAPI.

## Tech Stack

- C# / .NET 8 LTS
- ASP.NET Core Web API with controllers
- Entity Framework Core
- SQLite for quick local run
- SQL Server supported through configuration
- xUnit integration tests

## Features

- Create, read, update, and delete books
- DTO-based request and response models
- Basic validation with data annotations
- Unique ISBN check
- Swagger UI in development
- Automatic local database creation on startup

## Book Fields

- `id`
- `title`
- `author`
- `isbn`
- `publishedYear`
- `isAvailable`
- `createdAt`
- `updatedAt`

## API Endpoints

| Method | Endpoint | Description |
| --- | --- | --- |
| `GET` | `/api/books` | Get all books |
| `GET` | `/api/books/{id}` | Get a book by ID |
| `POST` | `/api/books` | Create a book |
| `PUT` | `/api/books/{id}` | Update a book |
| `DELETE` | `/api/books/{id}` | Delete a book |

## Local Setup

Install .NET 8 SDK first.

On macOS with Homebrew:

```bash
brew install dotnet@8
export DOTNET_ROOT="/opt/homebrew/opt/dotnet@8/libexec"
export PATH="/opt/homebrew/opt/dotnet@8/bin:$PATH"
```

Restore, build, and run:

```bash
dotnet restore
dotnet build
dotnet run --project src/LibraryManagement.Api
```

Open Swagger:

```text
http://localhost:5241/swagger
```

The default database is SQLite and will be created as `library.db`.

## SQL Server Configuration

To run with SQL Server, set the provider and connection string:

```bash
export DatabaseProvider="SqlServer"
export ConnectionStrings__SqlServerConnection="Server=localhost;Database=LibraryManagementDb;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;"
dotnet run --project src/LibraryManagement.Api
```

For Windows trusted connection, use a connection string similar to:

```text
Server=(localdb)\mssqllocaldb;Database=LibraryManagementDb;Trusted_Connection=True;TrustServerCertificate=True;
```

## Example Requests

Create a book:

```bash
curl -X POST http://localhost:5241/api/books \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Clean Code",
    "author": "Robert C. Martin",
    "isbn": "9780132350884",
    "publishedYear": 2008,
    "isAvailable": true
  }'
```

Get all books:

```bash
curl http://localhost:5241/api/books
```

Update a book:

```bash
curl -X PUT http://localhost:5241/api/books/1 \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Clean Architecture",
    "author": "Robert C. Martin",
    "isbn": "9780134494166",
    "publishedYear": 2017,
    "isAvailable": false
  }'
```

Delete a book:

```bash
curl -X DELETE http://localhost:5241/api/books/1
```

## Tests

Run the test suite:

```bash
dotnet test
```

The tests use an in-memory SQLite database and cover create, list, get missing, update, delete, duplicate ISBN, and validation failure scenarios.

## Possible Future Enhancements

- Member management
- Book loan and return workflow
- Search and filtering
- Authentication and role-based access control
# library-management-api
