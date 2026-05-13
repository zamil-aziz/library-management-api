using LibraryManagement.Api.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<LibraryDbContext>(options =>
    ConfigureDatabase(builder.Configuration, options));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

if (app.Configuration.GetValue("AutoCreateDatabase", true))
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
    await dbContext.Database.EnsureCreatedAsync();
}

app.Run();

static void ConfigureDatabase(IConfiguration configuration, DbContextOptionsBuilder options)
{
    var provider = configuration["DatabaseProvider"] ?? "Sqlite";

    if (provider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
    {
        var connectionString = configuration.GetConnectionString("SqlServerConnection")
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("SQL Server connection string is missing.");

        options.UseSqlServer(connectionString);
        return;
    }

    if (provider.Equals("Sqlite", StringComparison.OrdinalIgnoreCase))
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Data Source=library.db";

        options.UseSqlite(connectionString);
        return;
    }

    throw new InvalidOperationException($"Unsupported database provider '{provider}'. Use 'Sqlite' or 'SqlServer'.");
}

public partial class Program
{
}
