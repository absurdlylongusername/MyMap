# Phase5 Acceptance — Step3 (Auth)

- `/login` page renders a form.
- Submitting valid credentials POSTs `/auth/login-cookie` and redirects to `/map`.
- On invalid credentials, an inline error message is shown; no redirect.
- Cookies are set (Secure/HttpOnly/SameSite=None) by the API; subsequent API requests include them.
