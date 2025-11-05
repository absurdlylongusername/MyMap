# MyMap - Nimbus Maps Technical Test Submission

Hi, hello there! Welcome to my repo.

This is my submission for a technical test by Nimbus Maps.

The brief I was given:

**The task:**  

Build a simple containerized SaaS application that:

- Reads data from a backend database and presents it on a map
- Allows users to login, browse and interact with the data
- Uses .NET and React in the tech stack
- Can be cloned from GitHub and run locally or (bonus points) is deployed live somewhere

Expected time spent should be 4-8 hours on this. Use AI to generate the design, code and docs ("vibe code").

# My submission - MyMap (Geospatial SaaS Demo)

A small, production flavoured demo in the style of Nimbus Maps. It focuses on geospatial fundamentals and clean architecture with .NET9, EF Core + PostGIS, React + Leaflet, and .NET Aspire for orchestration and observability.

## What this application is

Map-centric web app where authenticated users can:
- Sign in (cookie auth via ASP.NET Core Identity API endpoints)
- View places of interest on a map from a real database
- Filter by category
- Draw a polygon and "search inside"
- See dataset version used for results and view a lineage manifest

Geospatial principles showcased:
- SRID4326 (WGS84 degrees)
- BBox queries and polygon predicates (`Intersects`, `Within`)
- Spatial indexing (GiST/R-tree)
- Data governance: single active `data_version`, lineage manifest, `X-Data-Version` header

## Tech & architecture (high-level)
- Backend: .NET9 minimal API, ASP.NET Core Identity (cookie), Identity API endpoints, EF Core9 + Npgsql + NetTopologySuite
- Storage: PostgreSQL + PostGIS (container orchestrated by Aspire)
- Frontend: React + Vite + TypeScript + React Router (dev/SSR), Leaflet for maps
- Orchestration/observability: .NET Aspire (`AppHost` + `ServiceDefaults`)
- Mono-repo layout:
 - `src/MyMap.AppHost` (Aspire AppHost, orchestration)
 - `src/MyMap.ServiceDefaults` (OTel, health, timeouts)
 - `src/MyMap.ApiService` (ASP.NET Core API)
 - `src/MyMap.Web` (React app, Vite)
 - `src/data` (seed files)
 - `src/docs` (contracts, schema, runbooks)

### Data model
- `pois(id, name, category, geom geometry(Point,4326), updated_at_utc, data_version)`
- `dataset_meta(id=1, active_version)` (single row)
- `dataset_versions(version, source, pulled_at_utc, transforms_json)`

### API contracts (summary)
- `GET /api/features?bbox=west,south,east,north&category=&limit=` ? `[ FeatureDto ]`, header `X-Data-Version`
- `POST /api/features/query` (GeoJSON Polygon/MultiPolygon + optional category/limit) ? `[ FeatureDto ]`, header `X-Data-Version`
- `GET /api/datasets/version` ? `{ activeVersion }`
- `GET /api/datasets/version/{v}` ? manifest for a version
- `POST /auth/login-cookie` ? sets cookie on200

## What's implemented so far
- Aspire `AppHost` runs:
 - PostGIS container (`postgis/postgis:16-3.4`) with named volume `mymap-pgdata` (persistent)
 - API service (health checks, OpenAPI in dev)
 - Vite frontend (proxied to API; appears in Aspire Dashboard)
- API:
 - Identity cookie auth (Identity API endpoints + explicit `/auth/login-cookie`)
 - EF Core schema with migrations, Npgsql + NTS, snake_case
 - Idempotent seed initializer (checks `dataset_meta.active_version`, loads CSV for `Seed.Version`, stamps `data_version`, upserts lineage, creates GiST index)
 - `X-Data-Version` header via endpoint filter per request
 - Endpoints implemented: `/api/features`, `/api/features/query`, `/api/datasets/version`, `/api/datasets/version/{v}`
 - Dev-only Postgres logging toggle via options
- Frontend:
 - Routes: `/login`, `/map`; root `/` redirects to `/login`
 - Login form posts `/auth/login-cookie`, redirects to `/map` on success
 - SSR?safe Leaflet mount (lazy client import)
 - `RequireAuth` guard for `/map` (pings `/api/datasets/version`, redirects on401)
 - React Query provider wired; Vite proxy forwards `/api/*` and `/auth/*`

## What's left to do (core UI flows)
- BBox flow: debounce map `moveend`, `GET /api/features`, render clustered markers
- Category filter: wire dropdown ? query param for both bbox and polygon queries
- Polygon draw: integrate leaflet?geoman, `POST /api/features/query`, overlay selection, clear action
- Versioning UX: read `X-Data-Version` into footer; "Data info" modal to show lineage (`/api/datasets/version`, `/api/datasets/version/{v}`)
- Sign out action (Identity sign?out endpoint) + redirect

Optional next steps
- Health endpoints `/live`, `/ready`; rate limiting for heavy queries; request logging enrichment; small test plan doc for manual checks
- Azure deployment (Static Web Apps + App Service + Azure Database for PostgreSQL) or Neon for a free DB

## Running locally (after clone)

**Prereqs: [Install Aspire prereqs](https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/setup-tooling)**

1) Clone and restore
```
git clone https://github.com/absurdlylongusername/MyMap
cd MyMap
```

2) Check configuration
- `src/MyMap.AppHost/appsettings.json` ? Postgres settings and volume name
- `src/MyMap.ApiService/appsettings.json`:
 - `Seed.Enabled`: `true|false`
 - `Seed.Version`: e.g. `2025-10-30`
 - `Seed.DataDir`: path to your local `src/data` folder
   - NOTE: current value is absolute to a local machine. Update it or disable seeding until you add data.
 - `src/MyMap.ApiService/appsettings.Development.json`:
 - `TestUserOptions`: default user (`test@nimbus.local` / `Test123!`)
 - You may also need to set `"Parameters:postgres-password": "postgres"` in `MyMap.AppHost` `secrets.json` or in `appsettings.{Environment}.json`.

3) Seed data (optional first run)
- Put CSV(s) in `src/data/` named `pois_<version>.csv` with header 
  - `name,category,lat,lon,updated_at_utc`
- Ensure `Seed.Enabled=true`, set `Seed.Version`, update `Seed.DataDir` path.

4) Run everything via Aspire
```
dotnet run --project src/MyMap.AppHost
```
Or just click run in VS or Rider.

- Aspire Dashboard opens; wait until `frontend` and `apiservice` healthy. Postgres persists in volume `mymap-pgdata`.

5) Open the app
- Use the frontend endpoint shown in Aspire Dashboard (HTTPS).
- Sign in with the test user or create your own.

Notes on cookies/HTTPS
- Cookies are `Secure` + `SameSite=None`. Use the HTTPS frontend URL so login works.

## Developer workflows
- Restart one service: keep AppHost running; restart `apiservice` or `frontend` from Aspire Dashboard, or `dotnet watch` per project.
- Reseed data:
 - Bump `Seed.Version` (with matching CSV) for idempotent reseed, or
 - Remove volume `mymap-pgdata` then run once with seeding enabled.
- Tests: `dotnet test src/MyMap.ApiService.Tests`

## Troubleshooting
- Login fails or 401s persist - ensure HTTPS frontend; verify cookie present.
- Seeding skipped - check `Seed.Enabled`, `Seed.Version`, `Seed.DataDir`; inspect API logs.
- CORS error - use the SPA URL from Aspire; don't hit API from a different origin.

## Repository structure
```
src/
 MyMap.AppHost/ # Aspire orchestration
 MyMap.ServiceDefaults/ # Service defaults (OTel, health)
 MyMap.ApiService/ # ASP.NET Core API (Identity, EF + NTS)
 MyMap.Web/ # React + Vite frontend
 data/ # Seed CSVs (local)
 docs/ # Contracts, runbooks
```

## Scope & security
- Demo grade cookie auth; no SSO/tenancy/billing
- Same origin dev via Vite proxy (avoid permissive CORS)

---
Use Aspire Dashboard to observe requests, DB calls, and health. This keeps runs reproducible, seeding idempotent, and DB persistent across restarts.

