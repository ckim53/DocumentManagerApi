using System.ComponentModel.DataAnnotations;
using Amazon.Runtime;
using Amazon.S3;
using DocumentManagerApi.Data;
using DocumentManagerApi.Models;
using DocumentManagerApi.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddProblemDetails();
builder.Services.AddCors();

// PostgreSQL
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
string connectionString;

if (!string.IsNullOrEmpty(databaseUrl))
{
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

// Cloudflare R2
var r2AccessKey = Environment.GetEnvironmentVariable("R2_ACCESS_KEY")
    ?? builder.Configuration["R2:AccessKey"];
var r2SecretKey = Environment.GetEnvironmentVariable("R2_SECRET_KEY")
    ?? builder.Configuration["R2:SecretKey"];
var r2Endpoint = Environment.GetEnvironmentVariable("R2_ENDPOINT")
    ?? builder.Configuration["R2:Endpoint"];

if (!string.IsNullOrEmpty(r2Endpoint))
{
    var s3Config = new AmazonS3Config
    {
        ServiceURL = r2Endpoint,
        ForcePathStyle = true
    };

    var s3Client = new AmazonS3Client(
        new BasicAWSCredentials(r2AccessKey!, r2SecretKey!),
        s3Config
    );

    builder.Services.AddSingleton<IAmazonS3>(s3Client);
}

builder.Services.AddScoped<R2Service>();

var app = builder.Build();

// Auto-migrate - runs on Railway where DATABASE_URL is set
if (!string.IsNullOrEmpty(databaseUrl))
{
    using var scope = app.Services.CreateScope();
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

app.MapPost("/documents", async (HttpRequest request, DocumentDbContext db, R2Service r2) =>
{
    var form = await request.ReadFormAsync();

    var title = form["title"].ToString();
    var description = form["description"].ToString();
    var tagsRaw = form["tags"].ToString();
    var file = form.Files.GetFile("file");

    // Validate file
    if (file is null)
        return Results.ValidationProblem(new Dictionary<string, string[]>
        {
            { "file", ["A PDF file is required"] }
        });

    if (file.ContentType != "application/pdf")
        return Results.ValidationProblem(new Dictionary<string, string[]>
        {
            { "file", ["Only PDF files are accepted"] }
        });

    if (file.Length > 10 * 1024 * 1024)
        return Results.ValidationProblem(new Dictionary<string, string[]>
        {
            { "file", ["File size cannot exceed 10MB"] }
        });

    var doc = new Document
    {
        Title = title,
        Description = description,
        Tags = tagsRaw
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(t => t.Trim())
            .ToList()
    };

    // Validate model fields
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

    var (fileUrl, fileName) = await r2.UploadAsync(file);
    doc.FileUrl = fileUrl;
    doc.FileName = fileName;
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
    return Results.NoContent();
});

app.MapDelete("/documents/{id}", async (int id, DocumentDbContext db, R2Service r2) =>
{
    var doc = await db.Documents.FindAsync(id);
    if (doc is null) return Results.NotFound();

    if (!string.IsNullOrEmpty(doc.FileUrl))
        await r2.DeleteAsync(doc.FileUrl);

    db.Documents.Remove(doc);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.Run();