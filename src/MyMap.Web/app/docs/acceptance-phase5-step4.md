# Phase5 Acceptance — Step4 (Routing/Auth fixes)

- Root `/` redirects to `/login` (no more flaky index page).
- Vite proxy no longer rewrites `/api` and `/auth`, so `/auth/login-cookie` hits the API.
- `/map` is protected: unauthenticated users are redirected to `/login` via a lightweight ping.
