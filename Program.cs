using System.ComponentModel.DataAnnotations;
using DocumentManagerApi.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddProblemDetails();

var app = builder.Build();
app.UseExceptionHandler();

var documents = new List<Document>();
var nextId = 1;

// GET all documents or filter by tag
app.MapGet("/documents", (string? tag) =>
{
    if (string.IsNullOrEmpty(tag)) return Results.Ok(documents);
    var filtered = documents
        .Where(d => d.Tags.Contains(tag, StringComparer.OrdinalIgnoreCase))
        .ToList();
    return Results.Ok(filtered);
});

// GET document by ID
app.MapGet("/documents/{id}", (int id) =>
{
    var doc = documents.FirstOrDefault(d => d.Id == id);
    return doc is null ? Results.NotFound() : Results.Ok(doc);
});

// POST - create new document
app.MapPost("/documents", (Document doc) =>
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

    doc.Id = nextId++;
    doc.CreatedAt = DateTime.UtcNow;
    documents.Add(doc);
    return Results.Created($"/documents/{doc.Id}", doc);
});

// PUT - update existing document
app.MapPut("/documents/{id}", (int id, Document updated) =>
{
    var existing = documents.FirstOrDefault(d => d.Id == id);
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

// DELETE document by ID
app.MapDelete("/documents/{id}", (int id) =>
{
    var doc = documents.FirstOrDefault(d => d.Id == id);
    if (doc is null) return Results.NotFound();
    documents.Remove(doc);
    return Results.NoContent();
});

app.Run();