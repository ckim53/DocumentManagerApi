using System.ComponentModel.DataAnnotations;
using DocumentManagerApi.Data;
using DocumentManagerApi.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddProblemDetails();
builder.Services.AddCors();

// Railway injects DATABASE_URL as a standard postgres:// URI.
// EF Core's Npgsql provider needs it in ADO.NET connection string format,
// so we convert it when the variable is present.
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
string connectionString;

if (!string.IsNullOrEmpty(databaseUrl))
{
    // Railway format: postgres://user:password@host:port/database
    var uri = new Uri(databaseUrl);
    var userInfo = uri.UserInfo.Split(':');
    connectionString =
        $"Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.TrimStart('/')};" +
        $"Username={userInfo[0]};Password={userInfo[1]};SSL Mode=Require;Trust Server Certificate=true";
}
else
{
    connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;
}

builder.Services.AddDbContext<DocumentDbContext>(options =>
    options.UseNpgsql(connectionString));

var app = builder.Build();

// Auto-migrate on startup — applies any pending migrations before serving traffic.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DocumentDbContext>();
    db.Database.Migrate();
}

app.UseExceptionHandler();
app.UseCors(policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

app.MapGet("/documents", async (string? tag, DocumentDbContext db) =>
{
    var query = db.Documents.AsQueryable();
    if (!string.IsNullOrEmpty(tag))
        query = query.Where(d => d.Tags.Contains(tag));
    return Results.Ok(await query.ToListAsync());
});

app.MapGet("/documents/{id}", async (int id, DocumentDbContext db) =>
{
    var doc = await db.Documents.FindAsync(id);
    return doc is null ? Results.NotFound() : Results.Ok(doc);
});

app.MapPost("/documents", async (Document doc, DocumentDbContext db) =>
{
    var validationResults = new List<ValidationResult>();
    var context = new ValidationContext(doc);
    if (!Validator.TryValidateObject(doc, context, validationResults, validateAllProperties: true))
    {
        var errors = validationResults.ToDictionary(
            v => v.MemberNames.FirstOrDefault() ?? "error",
            v => new[] { v.ErrorMessage ?? "Invalid value" }
        );
        return Results.ValidationProblem(errors);
    }
    doc.CreatedAt = DateTime.UtcNow;
    db.Documents.Add(doc);
    await db.SaveChangesAsync();
    return Results.Created($"/documents/{doc.Id}", doc);
});

app.MapPut("/documents/{id}", async (int id, Document updated, DocumentDbContext db) =>
{
    var existing = await db.Documents.FindAsync(id);
    if (existing is null) return Results.NotFound();

    var validationResults = new List<ValidationResult>();
    var context = new ValidationContext(updated);
    if (!Validator.TryValidateObject(updated, context, validationResults, validateAllProperties: true))
    {
        var errors = validationResults.ToDictionary(
            v => v.MemberNames.FirstOrDefault() ?? "error",
            v => new[] { v.ErrorMessage ?? "Invalid value" }
        );
        return Results.ValidationProblem(errors);
    }
    existing.Title = updated.Title;
    existing.Description = updated.Description;
    existing.Tags = updated.Tags;
    existing.UpdatedAt = DateTime.UtcNow;
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.MapDelete("/documents/{id}", async (int id, DocumentDbContext db) =>
{
    var doc = await db.Documents.FindAsync(id);
    if (doc is null) return Results.NotFound();
    db.Documents.Remove(doc);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.Run();