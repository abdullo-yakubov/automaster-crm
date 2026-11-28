# Known limitations

- Full solution build includes Blazor WebAssembly admin tasks and was blocked by the current sandbox task-server restriction. API project and React build were checked separately.
- React ESLint has 15 errors and 5 warnings in the current checkout.
- PostgreSQL integration, concurrent order completion, inventory races, restore, browser journeys, load tests, and deployment have not been verified.
- Order number generation uses `CountAsync() + 1`, which is unsafe under concurrent creation.
- A sequential duplicate-completion bug was fixed, but concurrent completion still needs an atomic database guard.
- The diagnostic feature in `AiService.cs` uses hand-written rules. No machine-learning model is present.
- No production administrator provisioning process exists after demo seeding is disabled.
- Real screenshots and measured performance results are pending.
