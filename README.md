# **Document Manager API**

A simple C# Web API built with ASP.NET Core to manage simple document metadata.
This project demonstrates familiarity with C#, minimal API routing, RESTful design, and in-memory data storage.

## **Run the Project**

```bash
dotnet run
```

The API will start on:

```
http://localhost:5000
```

## **Endpoints**

```
GET /documents
```
```
GET /documents?tag=tagname
```
```
GET /documents/{id}
```
```
POST /documents
```

**Body:**

```json
{
  "title": "My Doc",
  "description": "Sample description",
  "tags": ["work"]
}
```
```
PUT /documents/{id}
```
```
DELETE /documents/{id}
```
