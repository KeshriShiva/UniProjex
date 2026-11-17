# UniProjex — University Project Repository & Management Portal

A full-stack academic project network built around the supplied University Projects Database workbook.

## What is included

- **Frontend:** React + JavaScript + Vite, responsive dashboard and repository UI.
- **Backend:** ASP.NET Core 8 Web API in **C#**.
- **Database:** EF Core + SQLite by default (SQL Server provider is included and configurable); 303 project records are seeded automatically from the richer v2 Excel workbook.
- **Authentication:** registration/login, BCrypt password hashing, signed JWT bearer tokens, Student and Admin roles.
- **Project workflows:** search, pagination, university/category/status filters, project detail view, authenticated submission, edit/delete ownership checks.
- **Analytics:** live project counts by status and category, institution count and recent projects.
- **Scoped chatbot:** backend-mediated Gemini API integration with a built-in UniProjex-only fallback when no API key is configured.

## Data source and scope

The canonical source is `University_Projects_Database-v2.xlsx`, specifically the `Project Repository` sheet. It contains 303 unique IDs, 102 universities, 9 categories and 6 statuses. The v2 workbook also includes Project Brief and Project Link, which are absent from the older workbook. The app seeds all eight repository fields into the database. The Excel file's `Analytics Dashboard` is not hard-coded as live analytics; dashboard figures are calculated from the database.

The product represented by this dataset is a **centralized inter-university project repository and management portal**.

## Run locally

Prerequisites: .NET 8 SDK, Node.js 20+ and npm.

### 1. Start the API

```bash
cd backend
dotnet restore
dotnet run --urls http://localhost:5080
```

On first start, EF Core creates `uniprojex.db`, imports the seed JSON and creates a demo administrator.

API health check: `http://localhost:5080/api/health`

### 2. Start the frontend

In a second terminal:

```bash
cd frontend
npm install
cp .env.example .env
npm run dev
```

Open `http://localhost:5173`.

If your API runs on another address, set `VITE_API_URL` in `frontend/.env`.

### 3. Demo administrator

- Email: `admin@uniprojex.local`
- Password: `ChangeMe!2026`

Change these before using the application with real users. Set `UNIPROJEX_ADMIN_EMAIL` and `UNIPROJEX_ADMIN_PASSWORD` environment variables before the first API startup to override the demo account. The seed admin is only created if that email does not already exist.

### 4. Enable Gemini chatbot (optional)

Create an API key in Google AI Studio and set it only on the backend:

```bash
# PowerShell
$env:Gemini__ApiKey="YOUR_KEY"
$env:Gemini__Model="gemini-3.8-flash"
dotnet run --urls http://localhost:5080
```

Or set `Gemini:ApiKey` in a local, untracked `appsettings.Development.json`. Never put the key in frontend code or commit it. The API has a deterministic built-in FAQ fallback if the key is absent or Gemini fails. Gemini free-tier model access and rate limits vary by model and can change; free-tier content may also be used to improve Google's products. Check Google's live pricing/availability and data-use terms before relying on a specific model or sending personal/confidential data.

## Architecture decision: why not both MERN and MEAN?

MERN and MEAN are two alternative JavaScript full-stack combinations (React vs Angular). Using both as parallel application stacks would duplicate frontend and backend responsibilities and make this system harder to maintain. This implementation uses React for the UI and the explicitly required ASP.NET Core/C# backend as the system of record. It uses JavaScript throughout the frontend. If a later requirement mandates Angular or a separate Node.js microservice, it can be added behind the same API boundary; it is not needed for the current requirements.

## Important production hardening

This is a complete local-development baseline, not a production-certified deployment. Before public deployment:
- replace the development JWT signing key with a high-entropy secret stored in a secret manager;
- switch `DatabaseProvider` to `SqlServer`, set `ConnectionStrings:SqlServer`, and use versioned EF Core migrations for managed SQL Server;
- enforce HTTPS, configure production CORS origins, rate-limit authentication/chat endpoints, and add email verification/password reset;
- add audit logs, moderation/approval workflow and automated tests;
- add pagination limits and upload validation if file uploads are introduced;
- use a managed identity/secret store for Gemini and verify applicable privacy/data-use terms.

## Project structure

```text
UniProjex/
├── backend/
│   ├── Controllers/       # Auth, projects, dashboard, chatbot endpoints
│   ├── Data/              # EF Core context and imported seed data
│   ├── Models/            # Entities and request contracts
│   ├── Program.cs
│   └── UniProjex.Api.csproj
├── frontend/
│   ├── src/App.jsx        # Application pages, workflows and interactions
│   ├── src/styles.css
│   └── package.json
└── docs/
```

## API routes

| Method | Route | Purpose |
|---|---|---|
| POST | `/api/auth/register` | Create Student account |
| POST | `/api/auth/login` | Authenticate and return JWT |
| GET | `/api/projects` | Search/filter/paginate repository |
| GET | `/api/projects/{id}` | Project detail |
| POST | `/api/projects` | Authenticated project submission |
| PUT | `/api/projects/{id}` | Owner/Admin edit |
| DELETE | `/api/projects/{id}` | Owner/Admin delete |
| GET | `/api/projects/meta/options` | Filter options |
| GET | `/api/dashboard/summary` | Live analytics |
| POST | `/api/chat` | UniProjex-scoped chatbot |

