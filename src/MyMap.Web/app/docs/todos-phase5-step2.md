# Phase5 TODO — Step2

- Auth form: POST /auth/login-cookie, handle 200/400, redirect to /map.
- Implement bbox fetch: debounce moveend, call GET /api/features, render markers.
- Add React state to show X-Data-Version in footer from last response.
- Add category filter wiring (Sidebar control ? query param).
- Add polygon draw tool (leaflet-geoman) and POST /api/features/query.
- Add Data info modal to display lineage endpoints.
- Protect /map route: if API returns401 on a ping (e.g., GET /api/datasets/version), redirect to /login.
