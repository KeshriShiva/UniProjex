# Architecture and data dictionary

## Logical architecture

```text
Browser
  React + JavaScript (Vite)
      | HTTPS/JSON + JWT bearer token
      v
  ASP.NET Core 8 Web API (C#)
      | Entity Framework Core
      v
  SQLite (default development) / SQL Server (configurable)

  Chat endpoint -- server-side Gemini API (optional)
               \-- local scoped FAQ fallback
```

## Repository fields

| Workbook field | API/database field | Meaning |
|---|---|---|
| Project ID | `Id` | Stable imported ID (new submissions receive generated IDs) |
| University Name | `UniversityName` | Institution associated with the project |
| Project Title | `Title` | Project name |
| Project Brief | `Brief` | Short project description |
| Project Link | `ProjectLink` | External repository/reference URL |
| Category | `Category` | Project domain |
| Status | `Status` | Current lifecycle state |
| Start Date | `StartDate` | Project start date |

## Access model

- Anonymous visitors can view and search project records and analytics.
- Registration creates a `Student` role.
- Authenticated users can submit records.
- A user can edit/delete their own submissions; `Admin` can edit/delete any record.
- Passwords are stored as BCrypt hashes; the API issues signed JWTs.

## Lifecycle values in supplied data

`Proposed`, `UnderReview`, `Approved`, `InProgress`, `Completed`, `Rejected`.

The supplied workbook does not define a formal approval policy, reviewer assignment model, file-upload model, or university-level tenant permissions. Those are future product requirements, not assumptions encoded into this implementation.
