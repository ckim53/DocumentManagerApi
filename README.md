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

<<<<<<< HEAD
A lightweight C# Web API built with ASP.NET Core to manage simple document metadata.
This project demonstrates familiarity with C#, minimal API routing, RESTful design, and in-memory data storage.

=======
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
>>>>>>> 223843e2b9cf775ef082587f82e3628d1b449c77
