# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## 语言设置

请使用中文回答。

## Project Overview

ChemicalTransportor is a .NET 10 application for managing chemical transport with Siemens PLC integration. It uses **Furion framework** (a rapid application development framework) following **Clean Architecture** principles with DDD patterns.

## Build and Run Commands

```bash
# Build the entire solution
dotnet build ChemicalTransportor.slnx

# Run the web application (development)
dotnet run --project src/ChemicalTransportor/ChemicalTransportor.Web.Entry

# Build specific project
dotnet build src/ChemicalTransportor/ChemicalTransportor.Web.Entry

# Restore packages
dotnet restore
```

## Architecture

The solution follows a modular Clean Architecture with dependency flow from outer to inner layers:

### Core Application (`src/ChemicalTransportor/`)

- **ChemicalTransportor.Web.Entry** - ASP.NET Core Web API entry point (Kestrel/IIS Express)
  - Contains `Program.cs` with single-line: `Serve.Run(RunOptions.Default.WithArgs(args));`
  - Furion's `Serve.Run` handles application startup and dependency injection

- **ChemicalTransportor.Web.Core** - Web infrastructure layer
  - `Startup.cs` configures middleware pipeline (JWT, CORS, routing, controllers)
  - Uses Furion's `AddInjectWithUnifyResult()` for unified API responses

- **ChemicalTransportor.Application** - Application services and business logic
  - Contains **AppService** classes (implement `IDynamicApiController`)
  - Service interfaces in `System/Services/` with implementations
  - DTOs and Mappers using Mapster in `System/Dtos/`

- **ChemicalTransportor.Core** - Domain entities and abstractions
  - Core domain models (currently minimal)

- **ChemicalTransportor.EntityFramework.Core** - Data access layer
  - `DefaultDbContext` configured for **Sqlite** via `[AppDbContext("ChemicalTransportor", DbProvider.Sqlite)]`

- **ChemicalTransportor.Database.Migrations** - EF Core migrations

### Modules (`module/`)

- **SiemensCommunicator** - Siemens PLC communication module
  - Uses **S7.Net Plus** (S7netplus v0.20.0) for PLC communication
  - Supports S7-200, S7-300, S7-400, S7-1200, S7-1500 PLCs
  - Separate Application/Core structure mirroring main app

## Key Technologies

- **Furion v4.9.8.15** - Provides `IDynamicApiController`, dependency injection, unified results
- **Mapster** - Object-object mapping (configured via Mapper.cs in each service)
- **Entity Framework Core 10.0** - ORM with Sqlite provider
- **S7.Net Plus** - Siemens PLC communication
- **JWT Bearer** - Authentication via `Furion.Extras.Authentication.JwtBearer`

## Global Usings Pattern

All projects use `GlobalUsings.cs` for common imports (Furion, EF Core, ASP.NET Core). When adding new services, add required namespaces to the appropriate layer's GlobalUsings.cs rather than individual files.

## Service Pattern

Services follow this convention:

1. Interface in `{Module}/Services/I{Service}Service.cs`
2. Implementation in `{Module}/Services/{Service}Service.cs`
3. AppService (API controller) implementing `IDynamicApiController` in `{Module}/SystemAppService.cs`
4. DTOs and Mapper in `{Module}/Dtos/`

The AppService is automatically exposed as an API endpoint by Furion's conventions.

## Database

Default provider is **Sqlite**. Connection string name is "ChemicalTransportor" (defined in `DefaultDbContext` attribute).

## Language Version

Uses **C# 10 preview** features (`<LangVersion>preview</LangVersion>` in all .csproj files).
