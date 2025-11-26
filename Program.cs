using DocumentManagerApi.Models;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
var documents = new List<Document>();
var nextId = 1;

// GET all documents
app.MapGet("/documents", () => documents);

// GET document by ID
app.MapGet("/documents/{id}", (int id) =>
{
    var doc = documents.FirstOrDefault(d => d.Id == id);
    return doc is null ? Results.NotFound() : Results.Ok(doc);
});

// POST - create new document
app.MapPost("/documents", (Document doc) =>
{
    doc.Id = nextId++;
    doc.CreatedAt = DateTime.UtcNow;
    documents.Add(doc);
    return Results.Created($"/documents/{doc.Id}", doc);
});

// PUT - update document
app.MapPut("/documents/{id}", (int id, Document updated) =>
{
    var existing = documents.FirstOrDefault(d => d.Id == id);
    if (existing is null) return Results.NotFound();

    existing.Title = updated.Title;
    existing.Description = updated.Description;
    existing.Tags = updated.Tags;

    return Results.NoContent();
});

// DELETE document by id
app.MapDelete("/documents/{id}", (int id) =>
{
    var doc = documents.FirstOrDefault(d => d.Id == id);
    if (doc is null) return Results.NotFound();

    documents.Remove(doc);
    return Results.NoContent();
});

app.Run();