# API contracts (frontend view)

- GET `/api/features?bbox=west,south,east,north&category=&limit=`
 - returns: `FeatureDto[]` where item `{ id, name, category, lat, lon, updatedAtUtc }`
 - headers: `X-Data-Version`
- POST `/api/features/query` with GeoJSON Polygon/MultiPolygon body + optional `?category=&limit=`
 - returns: `FeatureDto[]`
 - headers: `X-Data-Version`
- GET `/api/datasets/version`
 - returns: `{ activeVersion: string }`
- GET `/api/datasets/version/{version}`
 - returns: `{ version, source, pulledAtUtc, transformsJson }` or404
- POST `/auth/login-cookie` with `{ email, password }`
 - returns200 on success; cookie is set
