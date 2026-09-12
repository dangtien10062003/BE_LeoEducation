# Recruitment integration

Admin route: `/recruitment`. Public landing routes: `/careers` and `/careers/:id`.
Both frontends must point to this .NET API. The Express backend in the landing repository is a legacy prototype, not the shared admin API.

## Database

Use the existing PostgreSQL/Supabase connection in `.env`:

```
Database__UseInMemory=false
ConnectionStrings__DefaultConnection=Host=YOUR_HOST;Port=5432;Database=postgres;Username=YOUR_USER;Password=YOUR_PASSWORD;SSL Mode=Require
```

Do not use a SQL Server `Server=...;Trusted_Connection=...` string. Do not send passwords in chat or commit `.env`.

Run `dotnet ef database update -- --Database:UseInMemory=false --Database:AutoMigrate=false` after reviewing pending migrations. The `AddRecruitment` migration adds only `RecruitmentJobs`, `JobApplications`, a foreign key and the unique job/email index. It leaves existing tables alone. If the Supabase schema was created manually, reconcile its migration history before running earlier pending migrations.

Import the four labeled sample jobs exported from the SQL Server prototype with:

```
dotnet run --no-launch-profile -- --Database:UseInMemory=false --Database:AutoMigrate=false --Recruitment:ImportFile=recruitment-sample.json
```

The importer skips titles that already exist, does not copy candidate personal information, and refuses to run against in-memory storage. Importing does not start the HTTP server.

## Local runtime

Start .NET on a free port, for example `dotnet run --no-launch-profile --urls http://localhost:5080`.
In both frontend `.env.development.local` files use `VITE_API_BASE_URL=/api` and `VITE_API_PROXY_TARGET=http://localhost:5080`. Restart Vite after configuration changes. Keep Clerk authority and authorized parties configured as before; the management endpoints use the existing bearer authentication, not the prototype admin secret.

## API

- Public: `GET /api/recruitment/jobs`, `GET /api/recruitment/jobs/{id}`, `POST /api/recruitment/jobs/{id}/applications`.
- Authenticated: `GET/POST /api/recruitment/admin/jobs`, `PUT /api/recruitment/admin/jobs/{id}`.
- Authenticated: `GET /api/recruitment/admin/applications?page=1&jobId=1&status=new`, `PATCH /api/recruitment/admin/applications/{id}` with `{ "status": "reviewing" }`.

Public lists exclude drafts and expired jobs. Forms use HTTPS CV links. Application statuses are `new`, `reviewing`, `interview`, `hired`, `rejected`. Job content requires all three translations (`vi`, `en`, `zh`). Responses preserve the existing landing API contract.
