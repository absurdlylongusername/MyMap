# Phase5 Acceptance (UI) — Step2

- SSR-safe Leaflet: no "window is not defined" crash.
- Map loads only on the client via lazy import; SSR renders null for the map shell.
- App runs under Aspire; navigating to /map shows the base map.
