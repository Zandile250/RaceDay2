# RaceDay API (Part 2)

ASP.NET Core 8 Web API · Entity Framework Core (Code-First) · SQL Server · session authentication · Swagger · xUnit · GitHub Actions

## Run it
1. Open the folder in Visual Studio. Check the connection string in `RaceDay.Api/appsettings.json`.
2. Create the database (Package Manager Console, default project `RaceDay.Api`):
       Add-Migration InitialCreate
       Update-Database
   or from a terminal inside `RaceDay.Api`:
       dotnet tool install --global dotnet-ef
       dotnet ef migrations add InitialCreate
       dotnet ef database update
3. Run `RaceDay.Api` and open `/swagger`.

## Try it in Swagger
1. POST /api/auth/register (Organiser needs `organiserInviteCode` = value in appsettings, default `RACEDAY-ORG-2026`).
2. POST /api/auth/login - the session cookie is stored by the browser, so later calls are authenticated.
3. Use the other endpoints. Log out and log in as the other role to see 403 responses.

## Tests
    dotnet test RaceDay.Tests/RaceDay.Tests.csproj

## Run with Docker

Requires Docker Desktop.

    docker compose up --build

Swagger UI: http://localhost:8080/swagger

The database migration is applied automatically when the API starts.
Stop everything with `docker compose down`.

## Tests and CI

    dotnet test RaceDay.Tests/RaceDay.Tests.csproj

GitHub Actions restores, builds and runs the unit tests on every push to `main`.
