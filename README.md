# RaceDay API (Part 2)

Race and Running Event Management System.The next one is a RaceDay – Race and Running Event Management System.

 Project Overview

As part of the project, RaceDay is a system of race and running events management. The system is designed to provide a tool for managing races, race organisers, race participants, race categories, race enrollements and race results in a single central application.

The project was a phased development. In Part 1, the database was designed, and entities as well as the relationships between the entities were identified. The database structure was also created in SQL. That was the basis of Part 2, which involved the elaboration of an ASP.A web application service (also known as a Web API) that is developed using .NET Core and that communicates with the database and that users can interact with the system at specific endpoints.

The system has two roles of users:

- Organiser – Can handle races and record participants' scores.
- Participant: Can register and log in the system, view available information and enroll in events, and view their results.

It also contains authentication, role based access control, password hashing, API documentation, unit testing and automated testing using GitHub Actions.


In Part 1, the RaceDay system's foundation was created in the database. The design of the database was done using an Entity Relationship Diagram (ERD) to define the entities, attributes of the entities, primary key, foreign key and the relationship.
The database was created to have a consistent structure and ensure referential integrity of the information.

 Database Entities

The database contains objects for the key parts of the system:

User: Stores user account information, such as information that is needed for user authentication and the role of the user.
- Profiles: Stores extra information about a user.
- **Events**: Stores information related to events to be held (including name, description, date, location, distance, event type).
A set of categories with which to organise events or participants according to the system's needs.
- Enrolments: Records participants who enrol in events.
- **Results:** Records participant's race result for storage, which is accessible to organisers, depending on the permissions of the participants.
The application's data model was built on top of the database schema and relationships, which were created in SQL.

Keys and Relationships

Each table has a Primary key that is used to uniquely identify each record. Foreign keys are used to establish relationship between tables and ensure referential integrity.
For instance, enrolments are connected to events, and results are connected to participants and events.
These relationships minimize the duplication of data and ensure consistency in related data.

 Part 2 – ASP.NET Core Web API

Part 2 extends the database set up in Part 1, adding an application programming interface (API).
APIs are used for connection between various parts of software systems. The Web API gets HTTP requests, processes the request operations, communicates with the database, and sends back HTTP responses in RaceDay.
We have used C# and ASP on the backend.NET Core Web API.

Main API Features

The application offers API endpoints for:

- Registration and login of users.
- User profiles.
- Event management.
- Category management.
- Participant enrolments.
- Result management.
- Getting the results of a participant.

The API will provide the intended 41 endpoints for these functions.

Registration and Login

Registration and authentication of the user to the application are possible. The system documents the role the user is registering as—either Organiser or Participant—when they register. An organiser may need to provide an invitation code for registration.
Before storing passwords, they are prehashed with the Bcrypt hash. This means that the application only remembers the hash of a password, not the actual password.
Once user successfully logs in, session management keeps track of authenticated user's identity and role.

Role-Based Access Control

Organiser and Participant functionality separation in the system.
Authenticated users must have the appropriate role to access protected operations. An organiser can, for instance, store the results and a participant can fetch the results from the dedicated results endpoint.
To ensure that authentication and role restrictions are enforced, a SessionAuthorize is used.
This can reduce access to sensitive information and operations by those not authorized to have access to it.

 Technology and Tools used.
C#
The back end logic, entities, authentication, business rules and API operations are written using C#.

ASP.NET Core Web API
ASP.To build the HTTP API, use the NET Core framework. It provides a set of rules for receiving, processing and answering requests.

SQL Server
The RaceDay data is stored and managed in a relational database management system, SQL Server.

ntity Framework Core
An object-relational mapper (ORM) is called Entity Framework Core (EF Core). It lets the application handle the records in the database using C# objects instead of having to manually write all of the database operations in SQL code.

The project adopts the Code-First way of solving the problem, which means that the model defines the changes to make in the database, based on the entity classes and database context.

AppDbContext
The `AppDbContext` encapsulates the relationship between the application's entity models and the database and Entity Framework Core. It's the specification of the database sets and enables database operations.

EF Core Migrations
Migration records changes to the database model and offer a method for applying these changes to the database schema.

Docker

Docker containerizes SQL Server, giving a consistent environment to develop and test with a database.
Swagger and OpenAPI
Swagger is an interactive interface to view and test the API endpoints. It assists the developers with submitting requests, monitoring the responses, and grasping the API's behavior.
<img width="935" height="354" alt="image" src="https://github.com/user-attachments/assets/239c1bf5-b782-4ea6-97f3-b76e3546d7d5" />


The purpose, inputs to and result of the endpoints are documented using XML comments and response codes.
Before storing passwords they are hashed with BCrypt. In the event of a password database leak, it helps to protect user credentials.

xUnit and Unit Testing
Application features are tested by the unit tests. There are 41 tests for this project: registration, login, event management, enrolments, access restrictions (both successful and unsuccessful).

Git and GitHub
Version control is used with git to keep track of changes to the source code over time. The project has its repository at GitHub, which also stores the committed source code.

ASP.NET Core 8 Web API · Entity Framework Core (Code-First) · SQL Server · session authentication · Swagger · xUnit · GitHub Actions
<img width="931" height="361" alt="image" src="https://github.com/user-attachments/assets/8ed692f1-8f02-489d-8a58-89f1b70fd98c" />


Run it
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



Try it in Swagger
1. POST /api/auth/register (Organiser needs `organiserInviteCode` = value in appsettings, default `RACEDAY-ORG-2026`).
2. POST /api/auth/login - the session cookie is stored by the browser, so later calls are authenticated.
3. Use the other endpoints. Log out and log in as the other role to see 403 responses.
4. <img width="914" height="370" alt="image" src="https://github.com/user-attachments/assets/32ad88af-a3dc-4c8a-940f-19982b88e22b" />


Tests
    dotnet test RaceDay.Tests/RaceDay.Tests.csproj
    <img width="937" height="468" alt="image" src="https://github.com/user-attachments/assets/5f6a31dd-ceea-4da9-b5ba-bc3d2b78a8a6" />


Run with Docker

Requires Docker Desktop.

    docker compose up --build
    <img width="927" height="496" alt="image" src="https://github.com/user-attachments/assets/bbbc0e3c-d628-4ff1-be85-f758196d0800" />


Swagger UI: http://localhost:8080/swagger

The database migration is applied automatically when the API starts.
Stop everything with `docker compose down`.
<img width="942" height="484" alt="image" src="https://github.com/user-attachments/assets/32fa31ca-c697-4159-bc63-1008bfed934f" />


Tests and CI

    dotnet test RaceDay.Tests/RaceDay.Tests.csproj
  <img width="920" height="427" alt="image" src="https://github.com/user-attachments/assets/a886b46b-bb93-4d01-bcc1-615d5d1db7c3" />
GitHub Actions restores, builds and runs the unit tests on every push to `main`.

. What is the system doing?What is happening in the system?

The primary application process is:
As an Organiser or Participant, a user registers.
The system verifies the registration information and takes the password and hashes it.
All the user information is saved in the SQL Server database.
The user logs in using his/her credentials.
The system creates an authenticated session with the identity and role of the user.
The user makes a request to the appropriate API endpoints.
In cases where authentication and/or permissions are required, the API will verify them.
The related Database operations are performed by Entity Framework Core.
The API sends back an HTTP response with the result of the request.
This will help to divide the system into three parts, namely the database, the application logic, and the API operations, which can make the system easier to maintain and test.

6. API Documentation and Testing

The API documentation can be found via Swagger at:
/swagger
With Swagger, developers can examine the list of available endpoints, view their expected input and output, and send in test requests.

The project includes 41 Unit Tests for some of the most important functionality such as:
Registration and login.
Event management.
Enrolment operations.
Different user roles' access restrictions.
Success and failure scenarios.
Authorized and unauthenticated users operations.
The tests verify the proper functioning of API and the rules of access to the application.

7. Continuous Integration

For automation of the build and testing process, the project utilizes GitHub Actions.
The definition of the workflow is in:
.github/workflows/ci.yml
The work flow is created to build the application and execute the automatic tests when it is configured to run on a GitHub event. A green check mark in the GitHub Actions interface will be a successful run.
An action completed with success.An action that was successfully completed.
Below this heading, put a screenshot of your successful GitHub Actions workflow run.
on the other hand, a successful build is displayed with a green checkmark.Conversely, if the build is successful, it will be shown with a green checkmark.
The screen shot must have the successful workflow run for the part-2 branch.

8. Project Structure

The main project files and folders contain:
RaceDay.Api/ – The ASP is contained in this.A project built for the NET Core Web API.
Entities – Object instances.Association – Relationships between entities.
AppDbContext – Describes the EF Core's database context.
Migrations/ – Provides Entity Framework Core migrations.
Unit test project – It includes the automatic tests for the application.
\.github/workflows/ci.yml – A GitHub Actions workflow definition file.
docs/part2-notes.md – Describes additional notes for the Part 2 requirements and schema.
Each file location is dependent on the final repository structure.

9. Database Schema Note

The following fields were added to the Events entity for the Part 2 requirements: DistanceKm and EventType.
The race distance is stored in these fields, in kilometres, and the type of event is stored in these fields. They were introduced to meet the Part 2 requirements and these are documented in docs/part2-notes.md.
Reviewing the final schema should be done in conjunction with the original Part 1 ERD and SQL implementation, as the database model needs to be reviewed in conjunction with both.

10. Conclusion

The RaceDay project is an example of developing a database-driven application from the ground up, starting with the design of the database and ending with a developed backend Web API.
In Part 1, the data base structure, data base entities, keys, and relationships were set. In Part 2, this was continued with the creation of API endpoints, Authentications, Session management, role-based access control, Password hashing, and Database integration with Entity Framework Core.
Interactive API documentation, unit testing of key application behaviour, and build and test automation via GitHub Actions are all features of Swagger.
All these aspects are showcased in one project to illustrate database design, backend development, security, software testing, version control, and continuous integration.
