# Setup

Install .NET SDK 10, Node.js 22+, and PostgreSQL. Create a disposable local database. Supply `ConnectionStrings__DefaultConnection` and `Jwt__Key` through local environment variables, never commit them. The JWT key must be at least 32 UTF-8 bytes. The API rejects the committed `CHANGE_ME` connection string.

Run `dotnet restore backend/AutoMaster.slnx`, then `dotnet run --project backend/AutoMaster.API`. In another terminal run `npm --prefix frontend ci` and `npm --prefix frontend run dev`. Set `VITE_API_URL` to the actual API URL ending in `/api` when it differs from `http://localhost:5050/api`.

Synthetic accounts and demo data are opt-in: set `Seed__DemoData=true` in Development only. The reset route requires separate `Seed__AllowDemoReset=true`; it removes records, so use it only on disposable test data. A production account provisioning procedure is not implemented.
