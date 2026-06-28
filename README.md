# Smart Learning Platform - Backend API

## Overview

This repository contains the backend API for our graduation project, **Smart Learning Platform**, an educational platform designed to provide an interactive learning experience for kindergarten children while supporting accessibility for learners with hearing impairments.

The backend is developed using **ASP.NET Core** and follows **Clean Architecture** to ensure scalability, maintainability, and separation of concerns.

---

## Features

* RESTful API built with ASP.NET Core
* Clean Architecture
* JWT Authentication & Authorization
* Role-Based Access Control (Admin, Teacher, Parent, Student)
* Entity Framework Core with SQL Server
* Repository Pattern
* Input Validation
* Global Exception Handling
* Swagger/OpenAPI Documentation
* Secure password hashing
* Email integration
* Redis caching and OTP management

---

## Tech Stack

* ASP.NET Core
* C#
* Entity Framework Core
* SQL Server
* Redis
* JWT Authentication
* Swagger / OpenAPI

---

## Architecture

The project follows **Clean Architecture**, separating responsibilities across four layers:

### API Layer

* Exposes RESTful endpoints
* Handles HTTP requests and responses
* Configures middleware, authentication, authorization, and dependency injection

### Application Layer

* Contains business logic
* Implements use cases
* Defines interfaces, DTOs, validation, and application services

### Domain Layer

* Contains the core business entities
* Business rules
* Domain models
* Repository contracts

### Infrastructure Layer

* Entity Framework Core implementation
* SQL Server database access
* Repository implementations
* Redis integration
* Email services
* External service implementations

---

## Project Structure

```text
SmartLearningPlatform
│
├── API
│   ├── Controllers
│   ├── Middleware
│   ├── Program.cs
│
├── Application
│   ├── DTOs
│   ├── Interfaces
│   ├── Services
│   └── Validators
│
├── Domain
│   ├── Entities
│   ├── Enums
│   ├── Interfaces
│   └── Common
│
└── Infrastructure
    ├── Data
    ├── Repositories
    ├── Services
    ├── Migrations
    └── Persistence
```

---

## Authentication

Authentication is implemented using **JSON Web Tokens (JWT)** with role-based authorization to secure API endpoints.

The API also supports:

* Refresh Tokens
* Email Verification
* OTP-based verification using Redis
* Password Reset

---

## API Documentation

Swagger is integrated for testing and documenting all API endpoints.

After running the application, navigate to:

```text
/swagger
```

---

## Getting Started

### Prerequisites

* .NET 9 SDK
* SQL Server
* Redis
* Visual Studio 2022

### Installation

```bash
git clone <repository-url>

cd SmartLearningPlatform.Backend

dotnet restore

dotnet ef database update

dotnet run
```

---

## Future Enhancements

* Docker support
* CI/CD pipeline
* Cloud deployment
* Monitoring and logging
* Performance optimization

---

## License

This project was developed as part of our graduation project at **Helwan University** for educational purposes.
