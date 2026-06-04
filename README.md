# Document Manager API

A RESTful API built with C# and ASP.NET Core for managing document metadata.
Demonstrates .NET minimal APIs, input validation with data annotations, and standard REST design.

Connected to [DocumentDashboard](https://github.com/ckim53/DocumentDashboard).

## Run

```bash
dotnet run
```

API runs at `http://localhost:5000`

## Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/documents` | List all documents |
| `GET` | `/documents?tag=work` | Filter by tag |
| `GET` | `/documents/{id}` | Get by ID |
| `POST` | `/documents` | Create a document |
| `PUT` | `/documents/{id}` | Update a document |
| `DELETE` | `/documents/{id}` | Delete a document |

## Request Body

```json
{
  "title": "Q3 Report",
  "description": "Quarterly summary",
  "tags": ["finance", "internal"]
}
```

`title` is required (max 200 chars). Invalid requests return `400` with field-level errors.
