# Testing

Run `dotnet test backend/AutoMaster.Tests/AutoMaster.Tests.csproj` for order completion regressions. Run `dotnet build backend/AutoMaster.API/AutoMaster.API.csproj` and `npm --prefix frontend run build`. Run `npm --prefix frontend run lint` separately; the current code has ESLint failures and this gate is not passing.

The tests use EF Core InMemory for sequential transitions. They do not verify PostgreSQL isolation, parallel requests, API authorization, UI behavior, migrations, or Docker startup. Use an isolated PostgreSQL database for those checks. Do not run reset tests against a database with real data.
