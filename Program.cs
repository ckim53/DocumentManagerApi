using DocumentManagerApi.Models;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
var documents = new List<Document>();
var nextId = 1;
