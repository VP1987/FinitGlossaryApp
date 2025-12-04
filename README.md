# FinitGlossaryApp# FinitiGlossary – Backend README (Short Enterprise Version)

Overview

FinitiGlossary is a modular backend built with .NET 8, following Onion/Clean Architecture, and structured into Api, Application, Domain, Infrastructure, and Tests projects.
The system provides:
✔ full authentication & JWT/refresh tokens
✔ password reset flow via email
✔ glossary management (public & admin)
✔ admin history, term versioning, and seeding
✔ strong separation of concerns and DI

# Solution Structure

📌 FinitiGlossary.Api (Presentation Layer)

Contains all HTTP endpoints, controllers, and startup configuration.

Folders

Controllers

AuthController.cs — login, register, refresh, password reset, profile update

AdminGlossaryController.cs — admin CRUD for glossary terms

PublicGlossaryController.cs — public read-only glossary API

Program.cs — app bootstrap

appsettings.json — config (email host, frontend URL, DB connection)

📌 FinitiGlossary.Application (Business Logic Layer)

Contains use cases, DTOs, interfaces, services, aggregators, and business rules.

Key folders

Aggregators

Combines glossary metadata + history for admin views.

DTOs

Request/ — login, register, reset password, term CRUD

Response/ — AuthResponse, RefreshTokenResponse, UnifiedResponse, etc.

Term/ — DTOs for admin/public glossary models

Interfaces

IAuthService, IEmailService, IUserRepository, IRefreshTokenRepository, admin & public glossary interfaces

Services

Auth: authentication, JWT issuing, refresh token rotation, password reset logic

Term.Admin: admin glossary CRUD

Term.Public: public glossary read-only service

This layer defines contracts, not implementations.

📌 FinitiGlossary.Domain (Core Entities)

Pure entity models, no infrastructure dependencies.

Entities

Auth / Token

RefreshToken.cs — refresh token store model

Terms

GlossaryTerm.cs — main glossary entity

ArchivedGlossaryTerm.cs — term versioning & history

TermStatus.cs — enum/status definitions

Users

User.cs — identity model (email, password hash, flags, reset token)

This is your enterprise business core.

📌 FinitiGlossary.Infrastructure (Data + External Services)

Implements repositories, EF Core context, migrations, and all external services.

Folders

DAL

AppDbContext.cs — EF Core DB context

Repositories

Admin glossary repository

Public glossary repository

User repository

Refresh token repository

Services

BCryptPasswordHasher.cs — password hashing

JwtTokenService.cs — JWT generation helper

EmailService.cs — SMTP email sending

Migrations

EF Core Migrations snapshot

Seeding

AdminSeeder.cs — seeds initial admin user

DependencyInjection

Registers all Application interfaces to Infrastructure implementations

This layer interacts with database, SMTP, JWT, password hashing.

📌 FinitiGlossary.Tests (Unit Tests)

Test coverage for:

AuthService

AdminGlossaryService

PublicGlossaryService

Uses mocks for repositories and verifies business rules.

# Main Features

✔ Authentication (JWT + Refresh Tokens)
✔ Password reset with expiring tokens + email link
✔ Full glossary management (admin side)
✔ Public glossary API with pagination & querying
✔ Term versioning (archived term snapshots)
✔ User profile workflow (force update, flags)
✔ Admin activity aggregators
✔ Clean DI + layered architecture

# Configuration (Short Version)

appsettings.json

Contains:

DB connection

SMTP settings (without password)

JWT settings

Frontend URL (Frontend:BaseUrl) for reset-password

Admin seed

Environment Variables (Required)
Email**Password="<smtp password>"
Jwt**Key="<secret>"
Frontend\_\_BaseUrl="http://localhost:5173"

# Password Reset Flow (Short)

User calls /auth/reset-password/request.

Backend generates token, stores it in User table, builds URL using Frontend:BaseUrl.

EmailService sends HTML email via SMTP.

User submits new password at /auth/reset-password/confirm.

# Database

Tables include:

Users

GlossaryTerms

ArchivedGlossaryTerms

RefreshTokens

Optional seeding adds initial admin user.

# Build & Run (Short)

dotnet restore
dotnet build
dotnet run --project FinitiGlossary.Api

Backend runs on:

https://localhost:7098

# Deploy Notes

Use production DB connection

Environment secrets required

Reverse proxy (Nginx/IIS) recommended

SSL required for secure password reset flow

# Troubleshooting

500 on email: SMTP password missing → set Email\_\_Password.

Reset link wrong domain: set Frontend\_\_BaseUrl.

JWT invalid: missing Jwt\_\_Key env var.

DB errors: run migrations or ensure DB exists.
