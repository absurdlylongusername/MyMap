// Minimal API wrapper; keep existing proxy semantics intact.
export async function apiGet(path: string, init?: RequestInit) {
 const res = await fetch(path, {
 ...init,
 credentials: "include",
 });
 // capture X-Data-Version for UI consumption if needed
 const version = res.headers.get("X-Data-Version") ?? undefined;
 const data = await res.json();
 return { data, version, status: res.status } as const;
}

export async function apiPost(path: string, body: unknown, init?: RequestInit) {
 const res = await fetch(path, {
 method: "POST",
 headers: { "Content-Type": "application/json", ...(init?.headers ?? {}) },
 body: JSON.stringify(body),
 credentials: "include",
 ...init,
 });
 const version = res.headers.get("X-Data-Version") ?? undefined;
 const data = await res.json().catch(() => undefined);
 return { data, version, status: res.status } as const;
}
