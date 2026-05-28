using DocumentManagerApi.Models;
using Microsoft.EntityFrameworkCore;

namespace DocumentManagerApi.Data;

public class DocumentDbContext(DbContextOptions<DocumentDbContext> options) : DbContext(options)
{
    public DbSet<Document> Documents => Set<Document>();
}