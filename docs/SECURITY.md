# Security

The API checks for a sufficiently long JWT signing key and refuses placeholder database configuration. Controllers are authenticated by default, with role attributes. The Development demo seeder creates accounts with public fixed passwords; it is now gated to Development plus `Seed:DemoData`. The destructive demo reset route is gated to Development plus `Seed:AllowDemoReset`.

Use environment-specific secrets and an isolated database. Rotate any demo credentials before real use. Test role enforcement, token lifetime and refresh, rate limits, object-level authorization, audit trails, and SQL permissions before deployment. The rule-based diagnostic assistant is advisory.
