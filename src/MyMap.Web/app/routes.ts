import { type RouteConfig, route } from "@react-router/dev/routes";

export default [
  route("/", "routes/redirect-to-login.tsx"),
  route("/login", "routes/login.tsx"),
  route("/map", "routes/map.tsx"),
] satisfies RouteConfig;
