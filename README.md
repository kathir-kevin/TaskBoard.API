# Task Board API

## Overview

Task Board API is a RESTful backend service built using **ASP.NET Core Web API**.
It provides endpoints to create, update, retrieve, and delete tasks for a task management board.

The API communicates with a database using **Entity Framework Core** and exposes endpoints consumed by the Angular frontend.

---

# Technology Stack

* .NET 6+ / ASP.NET Core Web API
* Entity Framework Core
* SQL Server / SQLite
* Swagger (API Documentation)
* Async programming

---

# Project Structure

```
backend-api
│
├── Controllers
│     └── TasksController.cs
│
├── Models
│     └── TaskItem.cs
│
├── Data
│     └── AppDbContext.cs
│
├── Services
│     └── TaskService.cs
│
├── Repositories
│     └── TaskRepository.cs
│
└── Program.cs
```

---

# Features

* Create tasks
* Update task details
* Delete tasks
* Retrieve all tasks
* Retrieve task by ID
* Change task status

---

# Database Schema

### Tasks Table

| Column      | Type     | Description              |
| ----------- | -------- | ------------------------ |
| Id          | int      | Primary key              |
| Title       | string   | Task title               |
| Description | string   | Task description         |
| Status      | string   | Todo / InProgress / Done |
| CreatedAt   | datetime | Task creation date       |
| UpdatedAt   | datetime | Last update timestamp    |

---

# API Endpoints

## Get All Tasks

GET /api/tasks

---

## Get Task By ID

GET /api/tasks/{id}

---

## Create Task

POST /api/tasks

Example Body

{
  "title": "Create backend API",
  "description": "Build REST endpoints",
  "status": "Todo"
}

---

## Update Task

PUT /api/tasks/{id}

---

## Delete Task

DELETE /api/tasks/{id}

---

# Running the API

### 1 Install dependencies

```
dotnet restore
```

### 2 Run application

```
dotnet run
```

Default API URL

```
http://localhost:5000
```

Swagger documentation

```
http://localhost:5000/swagger
```

---

# Environment Requirements

* .NET SDK 6 or later
* SQL Server or SQLite
* Visual Studio / VS Code

---

# Future Improvements

* JWT authentication
* Role-based access control
* Pagination
* Task comments
* Activity logs
* Real-time updates using SignalR
