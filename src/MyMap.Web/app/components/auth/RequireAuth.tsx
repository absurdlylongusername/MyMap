import { useEffect } from "react";
import { useNavigate } from "react-router";

export function RequireAuth({ children }: { children: React.ReactNode }) {
  const navigate = useNavigate();
  useEffect(() => {
    (async () => {
      try {
        const res = await fetch("/api/datasets/version", { credentials: "include" });
        if (res.status === 401) navigate("/login");
      } catch {
        // network errors -> keep page but you can surface a banner later
      }
    })();
  }, [navigate]);
  return <>{children}</>;
}
