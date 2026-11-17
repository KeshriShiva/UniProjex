# UniProjex API

ASP.NET Core 8 Web API in C#. SQLite is the zero-configuration development database. SQL Server support is included; set `DatabaseProvider` to `SqlServer` and configure `ConnectionStrings:SqlServer`.

## Run
1. Install the .NET 8 SDK.
2. From `backend/`: `dotnet restore`
3. `dotnet run --urls http://localhost:5080`
4. API health: `http://localhost:5080/api/health`

The database is created automatically and seeded from `Data/projects.seed.json` on first run. The seed file contains the 303 rows from the richer `University_Projects_Database-v2.xlsx`.

## Demo administrator
- Email: `admin@uniprojex.local`
- Password: `ChangeMe!2026`

Change these immediately. Set `UNIPROJEX_ADMIN_EMAIL` and `UNIPROJEX_ADMIN_PASSWORD` before first startup to override. Also replace `Jwt:Key` with a long random secret before deployment.

## Gemini chatbot (optional)
Set `Gemini:ApiKey` in appsettings.Development.json or the `Gemini__ApiKey` environment variable. The key is used only on the server and is never sent to the browser. If no key is configured or Gemini is unavailable, the chatbot uses a built-in, project-scoped FAQ response. Check Google's current Gemini API pricing and free-tier quotas before production use; free access and limits can change.

## API
- `POST /api/auth/register`, `POST /api/auth/login`
- `GET /api/projects?q=&university=&category=&status=&page=&pageSize=`
- `GET /api/projects/{id}`, `POST /api/projects`, `PUT /api/projects/{id}`, `DELETE /api/projects/{id}`
- `GET /api/projects/meta/options`
- `GET /api/dashboard/summary`
- `POST /api/chat` with `{ "message": "..." }`

All project mutations require a bearer token. Users can modify their own submitted projects; Admin can modify any project.
