# RaceDay API (Part 2)

ASP.NET Core 8 Web API · Entity Framework Core (Code-First) · SQL Server · session authentication · Swagger · xUnit · GitHub Actions
<img width="931" height="361" alt="image" src="https://github.com/user-attachments/assets/8ed692f1-8f02-489d-8a58-89f1b70fd98c" />


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
4. <img width="918" height="386" alt="image" src="https://github.com/user-attachments/assets/6757711e-2efd-4591-bf7c-49077251e836" />


5. <img width="906" height="406" alt="image" src="https://github.com/user-attachments/assets/bbcdd053-e2fd-41d8-84e4-8e2d233424e6" />


## Try it in Swagger
1. POST /api/auth/register (Organiser needs `organiserInviteCode` = value in appsettings, default `RACEDAY-ORG-2026`).
2. POST /api/auth/login - the session cookie is stored by the browser, so later calls are authenticated.
3. Use the other endpoints. Log out and log in as the other role to see 403 responses.
4. <img width="914" height="370" alt="image" src="https://github.com/user-attachments/assets/32ad88af-a3dc-4c8a-940f-19982b88e22b" />


## Tests
    dotnet test RaceDay.Tests/RaceDay.Tests.csproj
    <img width="937" height="468" alt="image" src="https://github.com/user-attachments/assets/5f6a31dd-ceea-4da9-b5ba-bc3d2b78a8a6" />


## Run with Docker

Requires Docker Desktop.

    docker compose up --build
    <img width="927" height="496" alt="image" src="https://github.com/user-attachments/assets/bbbc0e3c-d628-4ff1-be85-f758196d0800" />


Swagger UI: http://localhost:8080/swagger

The database migration is applied automatically when the API starts.
Stop everything with `docker compose down`.
<img width="942" height="484" alt="image" src="https://github.com/user-attachments/assets/32fa31ca-c697-4159-bc63-1008bfed934f" />


## Tests and CI

    dotnet test RaceDay.Tests/RaceDay.Tests.csproj
    <img width="922" height="485" alt="image" src="https://github.com/user-attachments/assets/95f55dc1-5aaa-4438-9f2c-7f2eeb2b4cd4" />
    <img width="917" height="482" alt="image" src="https://github.com/user-attachments/assets/a6f4f0d7-5fea-4697-8a65-a5b77d5afcfc" />



GitHub Actions restores, builds and runs the unit tests on every push to `main`.
