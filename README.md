# CarRentalApp

A layered ASP.NET Core + Blazor Server car rental application with search and booking flows.

## Overview

CarRentalApp lets users:

- Search available cars by city and date range.
- View car details and calculated rental price.
- Create bookings with overlap protection.

The solution uses Clean Architecture style boundaries:

- `CarRental.Domain`: core entities and business rules.
- `CarRental.Application`: use cases, validation, mapping, pricing service.
- `CarRental.Infrastructure`: EF Core persistence with SQLite.
- `CarRental.WebUI`: Blazor Server UI + Minimal API endpoints.
- `CarRental.Common`: shared DTOs/enums.

## Tech Stack

- .NET `10.0`
- ASP.NET Core Minimal APIs
- Blazor Server
- Entity Framework Core (`Microsoft.EntityFrameworkCore.Sqlite`)
- MediatR
- FluentValidation
- AutoMapper
- MudBlazor
- Swagger / OpenAPI (development only)

## Solution Structure

```text
CarRentalApp/
  src/
    CarRental.Domain/
    CarRental.Application/
    CarRental.Infrastructure/
    CarRental.Common/
    CarRental.WebUI/
  tests/
    CarRental.Application.UnitTests/
    CarRental.Domain.UnitTests/
    CarRental.IntegrationTests/
  CarRentalApp.slnx
```

## Core Features

- Search cars by `city`, `startDate`, `endDate`.
- Availability check excludes overlapping bookings.
- Booking creation in DB transaction.
- Domain value object for date ranges (`DateRange`).
- Seeded sample data for 5 German cities and multiple brands.
- Unified API envelope for success/error responses.

## Architecture and Request Flow

1. User searches in Blazor UI (`/search`).
2. `ApiClient` calls `GET /api/cars/available`.
3. Endpoint dispatches `GetAvailableCarsQuery` via MediatR.
4. Query handler reads EF Core context, filters cars, maps DTO, calculates price.
5. User confirms booking in UI.
6. `ApiClient` calls `POST /api/bookings`.
7. Endpoint dispatches `CreateBookingCommand`.
8. Command handler checks overlap and inserts booking inside a transaction.

## Data Structure

### Entity Relationship (Conceptual)

- `Car (1) -> (many) Booking`
- `Car (1) -> (many) CarDamage`
- `Car (1) -> (many) ServiceLog`
- `Car (1) -> (many) CarStatusReport`

### Domain Entities

#### `Car`

| Field | Type | Notes |
|---|---|---|
| `Id` | `int` | Inherited from `BaseEntity` |
| `Brand` | `string` | Manufacturer |
| `Model` | `string` | Model name |
| `VinCode` | `string` | Vehicle identifier |
| `LicensePlate` | `string` | Plate number |
| `Year` | `int` | Production year |
| `Location` | `string` | City |
| `BasePrice` | `decimal(18,2)` | Base daily price |
| `TransmissionType` | `Transmission` | `Manual` / `Automatic` |
| `Fuel` | `FuelType` | Fuel enum |
| `Class` | `CarClass` | Car class enum |
| `SeatsCount` | `int` | Number of seats |
| `Status` | `CarStatus` | Default `Available` |
| `CurrentMileage` | `int` | Current odometer |
| `FeaturesJson` | `string` | JSON metadata (gps/ac/bluetooth etc.) |
| `Bookings` | `IReadOnlyList<Booking>` | Related bookings |

#### `Booking`

| Field | Type | Notes |
|---|---|---|
| `Id` | `int` | Inherited from `BaseEntity` |
| `CarId` | `int` | FK to car |
| `Period` | `DateRange` | Complex value object in EF |
| `CreatedAt` | `DateTime` | UTC creation time |

#### `CarDamage`

| Field | Type | Notes |
|---|---|---|
| `Id` | `int` | Inherited from `BaseEntity` |
| `CarId` | `int` | FK to car |
| `ReportedDate` | `DateTime` | Damage report date |
| `Description` | `string` | Damage details |
| `Severity` | `DamageSeverity` | `Minor`, `Moderate`, `Severe` |
| `IsRepaired` | `bool` | Repair status |

#### `ServiceLog`

| Field | Type | Notes |
|---|---|---|
| `Id` | `int` | Inherited from `BaseEntity` |
| `CarId` | `int` | FK to car |
| `ServiceDate` | `DateTime` | Service date |
| `ServiceType` | `string` | Service category |
| `MileageAtService` | `int` | Odometer at service |
| `Cost` | `decimal` | Service cost |

#### `CarStatusReport`

| Field | Type | Notes |
|---|---|---|
| `Id` | `int` | Inherited from `BaseEntity` |
| `CarId` | `int` | FK to car |
| `Reason` | `UnavailabilityReason` | `Maintenance`, `Accident`, `Other` |
| `StartDate` | `DateTime` | Unavailability start |
| `EstimatedEndDate` | `DateTime?` | Optional end date |
| `Comments` | `string` | Additional notes |

### Value Objects

#### `DateRange`

| Field | Type | Notes |
|---|---|---|
| `Start` | `DateTime` | Inclusive start |
| `End` | `DateTime` | Exclusive-like overlap logic |
| `DurationInDays` | `int` | `(End - Start).Days` |

Rules:

- `End` must be greater than `Start`.
- Overlap: `Start < other.End && End > other.Start`.

### Enums

- `CarStatus`: `Available`, `Maintenance`, `Retired`
- `CarClass`: `Economy`, `Comfort`, `Business`, `Luxury`
- `FuelType`: `Gasoline`, `Diesel`, `Electric`, `Hybrid`
- `Transmission`: `Manual`, `Automatic`
- `DamageSeverity`: `Minor`, `Moderate`, `Severe`
- `UnavailabilityReason`: `Maintenance`, `Accident`, `Other`

### Persistence Model

`ApplicationDbContext` tables:

- `Cars`
- `Bookings`
- `CarDamages`
- `ServiceLogs`
- `CarStatusReports`

Special EF configurations:

- `Booking.Period` mapped as complex property.
- `Car.BasePrice` column type set to `decimal(18,2)`.
- Composite keys:
  - `CarDamage`: `{ CarId, Id }`
  - `ServiceLog`: `{ CarId, Id }`
  - `CarStatusReport`: `{ CarId, Id }`

## API Reference

Base path: `/api`

### Get Available Cars

- Method: `GET`
- Route: `/api/cars/available`
- Query params:
  - `city` (`string`, required)
  - `startDate` (`datetime`, required, UTC date normalized)
  - `endDate` (`datetime`, required, must be greater than start)

Sample request:

```http
GET /api/cars/available?city=Berlin&startDate=2026-02-20T00:00:00.0000000Z&endDate=2026-02-23T00:00:00.0000000Z
```

Success response body:

```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "brand": "BMW",
      "model": "3 Series",
      "location": "Berlin",
      "carPrice": 300,
      "transmissionType": 1,
      "fuel": 2,
      "class": 1,
      "seatsCount": 5,
      "featuresJson": "{\"gps\":\"true\",\"ac\":\"false\"}"
    }
  ],
  "error": null
}
```

### Create Booking

- Method: `POST`
- Route: `/api/bookings`
- Body:

```json
{
  "carId": 1,
  "startDate": "2026-02-20T00:00:00Z",
  "endDate": "2026-02-23T00:00:00Z"
}
```

Success response:

```json
{
  "success": true,
  "data": {
    "bookingId": 101
  },
  "error": null
}
```

Conflict example (overlap):

```json
{
  "success": false,
  "data": null,
  "error": {
    "message": "Car is already booked for these dates.",
    "errors": null
  }
}
```

## Validation Rules

`GetAvailableCarsQuery` and `CreateBookingCommand` enforce:

- `City` is required (search).
- `CarId` is required (booking).
- `StartDate >= DateTime.UtcNow.Date`.
- `EndDate > StartDate`.

## Configuration

File: `src/CarRental.WebUI/appsettings.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=../../CarRentalApp.db"
  }
}
```

Notes:

- SQLite DB file is created in repository root as `CarRentalApp.db`.
- On app startup, database is initialized and seeded (`InitialiseDatabaseAsync`).

## Local Setup

### Prerequisites

- .NET SDK `10.0`
- Docker Desktop (optional, for containerized run)

## Run the App

From repository root:

```bash
dotnet restore CarRentalApp.slnx
dotnet run --project src/CarRental.WebUI/CarRental.WebUI.csproj
```

Then open:

- App: `https://localhost:xxxx` (port from console output)
- Swagger (Development): `https://localhost:xxxx/swagger`

## Run with Docker

The repository includes:

- `Dockerfile` for a production-style image build.
- `docker-compose.yml` for local container startup with a persistent SQLite volume.

### Option 1: Docker Compose (recommended)

```bash
docker compose up --build
```

Open:

- App: `http://localhost:8080`
- Swagger: `http://localhost:8080/swagger`

Stop containers:

```bash
docker compose down
```

This keeps SQLite data in the named volume `carrentalapp-data`.

### Option 2: Plain Docker CLI

Build image:

```bash
docker build -t carrentalapp:latest .
```

Run container:

```bash
docker run --name carrentalapp -p 8080:8080 \
  -e ASPNETCORE_ENVIRONMENT=Development \
  -e ConnectionStrings__DefaultConnection="Data Source=/app/data/CarRentalApp.db" \
  -v carrentalapp-data:/app/data \
  carrentalapp:latest
```

Then open `http://localhost:8080`.

## Run Tests

```bash
dotnet test tests/CarRental.Application.UnitTests/CarRental.Application.UnitTests.csproj
```

## Seed Data

Seed inserts cars for cities:

- Berlin
- Munich
- Hamburg
- Frankfurt
- Stuttgart

Brands/models used per city:

- VW Golf
- BMW 3 Series
- Audi A4
- Tesla Model 3
- Mercedes C-Class

## Known Repository Notes

- The solution file currently includes only `CarRental.Application.UnitTests` under `tests`.
- `tests/CarRental.Domain.UnitTests` and `tests/CarRental.IntegrationTests` project references appear to use outdated paths and may require correction before running.

## Future Improvements

- Add migrations workflow and migration commands to documentation.
- Replace `FeaturesJson` with a typed value object/entity.
- Add full integration tests for API endpoints.
- Add authentication/authorization for booking operations.
