using System.Data.Common;
using LibraryManagement.Api.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace LibraryManagement.Api.Tests;

public sealed class LibraryApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<LibraryDbContext>>();
            services.RemoveAll<DbConnection>();

            var connectionString = $"Data Source={Guid.NewGuid():N};Mode=Memory;Cache=Shared";

            services.AddSingleton<DbConnection>(_ =>
            {
                var connection = new SqliteConnection(connectionString);
                connection.Open();
                return connection;
            });

            services.AddDbContext<LibraryDbContext>((serviceProvider, options) =>
            {
                _ = serviceProvider.GetRequiredService<DbConnection>();
                options.UseSqlite(connectionString);
            });
        });
    }
}
