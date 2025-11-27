# **Document Manager API**

A simple C# Web API built with ASP.NET Core to manage document metadata.
This project demonstrates familiarity with C#, minimal API routing, and RESTful design.

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
```
PUT /documents/{id}
```
```
DELETE /documents/{id}
```
**Body:**

```json
{
  "title": "My Doc",
  "description": "Sample description",
  "tags": ["work"]
}
```
