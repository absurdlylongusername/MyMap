# Phase5 TODOs

- Auth form: POST /auth/login-cookie; handle200/400; redirect to /map.
- Map canvas: Leaflet with tile layer; debounce moveend ? GET /api/features?bbox=…
- Clustering: pick client-side cluster layer; popup: name, category, updated_at.
- Polygon draw: Leaflet-Geoman; POST /api/features/query with GeoJSON.
- Version header: read X-Data-Version from api.ts; show in FooterStatus and Data info modal.
- Data info modal: GET /api/datasets/version; then GET /api/datasets/version/{v}`; show lineage.
